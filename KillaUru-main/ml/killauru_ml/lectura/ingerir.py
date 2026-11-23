"""FR-007 — ingesta de grabaciones: verificar, SHA-256, catalogo y subir a R2."""
from __future__ import annotations

import pathlib
import subprocess
import tarfile
import tempfile

import numpy as np
import pandas as pd

from killauru_ml.comun.huella import sha256_archivo
from killauru_ml.lectura.catalogo import FilaCatalogo, agregar_fila
from killauru_ml.lectura.grabacion import Grabacion, GrabacionError, leer_grabacion
from killauru_ml.lectura.integridad import ResultadoIntegridad, verificar_integridad


def _horas_en_marcha(gps: pd.DataFrame) -> float:
    """Duración total con velocidad > 0, en horas."""
    if gps.empty or "velocidad_ms" not in gps.columns:
        return 0.0
    duracion_s = float(gps["velocidad_ms"].gt(0).sum()) / 1.0  # 1 fix/s
    return duracion_s / 3600.0


def _tar_sin_video(carpeta: pathlib.Path, destino: pathlib.Path) -> pathlib.Path:
    """Crea un .tar de la carpeta sin video.mp4; devuelve la ruta del .tar."""
    tar_path = destino / f"{carpeta.name}.tar"
    with tarfile.open(tar_path, "w") as tar:
        for archivo in sorted(carpeta.iterdir()):
            if archivo.name not in {"video.mp4"}:
                tar.add(archivo, arcname=archivo.name)
    return tar_path


def ingerir(
    carpeta: pathlib.Path,
    ruta_catalogo: pathlib.Path | None = None,
    subir: bool = False,
) -> FilaCatalogo:
    """Verifica la grabación, calcula SHA-256 y registra en el catálogo; si es apta y se pide, la sube a R2."""
    opciones_catalogo = {"ruta": ruta_catalogo} if ruta_catalogo else {}
    try:
        grabacion = leer_grabacion(carpeta)
    except GrabacionError as error:
        fila = _fila_no_legible(carpeta, error)
    else:
        fila = _fila_verificada(carpeta, grabacion)
    agregar_fila(fila, **opciones_catalogo)
    if subir and fila.estado == "Apta":
        _subir_a_r2(carpeta)
    return fila


def _fila_no_legible(carpeta: pathlib.Path, error: GrabacionError) -> FilaCatalogo:
    """Fila NoApta para una carpeta que ni siquiera se puede leer (meta.json o CSV ausentes o inválidos)."""
    return FilaCatalogo(
        grabacion_id=carpeta.name, recorrido="", celular="", vehiculo="", sentido="", horas=0.0, hz_efectivo=0.0,
        huecos_pct=0.0, sha256="", estado="NoApta", motivo=f"MetaIncompleta: {error}",
    )


def _fila_verificada(carpeta: pathlib.Path, grabacion: Grabacion) -> FilaCatalogo:
    """Fila con la integridad medida, la huella sin el video y los alias (nunca datos personales) del meta.json."""
    resultado = _verificar(grabacion)
    meta = grabacion.meta
    return FilaCatalogo(
        grabacion_id=meta.get("grabacionId", carpeta.name),
        recorrido=meta.get("recorrido", ""),
        celular=meta.get("celular", {}).get("alias", ""),
        vehiculo=meta.get("vehiculo", {}).get("alias", ""),
        sentido=meta.get("tramo", {}).get("sentido", ""),
        horas=_horas_en_marcha(grabacion.gps),
        hz_efectivo=resultado.hz_efectivo,
        huecos_pct=resultado.huecos_pct,
        sha256=_huella_sin_video(carpeta),
        estado=resultado.estado.value,
        motivo=",".join(motivo.value for motivo in resultado.motivos),
    )


def _verificar(grabacion: Grabacion) -> ResultadoIntegridad:
    gps = grabacion.gps
    return verificar_integridad(
        grabacion.acelerometro["t_ns"].to_numpy(dtype=np.int64),
        grabacion.giroscopio["t_ns"].to_numpy(dtype=np.int64),
        gps["precision_m"].to_numpy() if "precision_m" in gps.columns else None,
        gps["t_ns"].to_numpy(dtype=np.int64) if "t_ns" in gps.columns else None,
    )


def _huella_sin_video(carpeta: pathlib.Path) -> str:
    with tempfile.TemporaryDirectory() as temporal:
        return sha256_archivo(_tar_sin_video(carpeta, pathlib.Path(temporal)))


def _subir_a_r2(carpeta: pathlib.Path) -> None:
    """Delega en scripts/subir_grabacion.sh (las credenciales de R2 nunca pasan por Python)."""
    script = pathlib.Path(__file__).parents[3] / "scripts" / "subir_grabacion.sh"
    subprocess.run(["bash", str(script), str(carpeta)], check=True)
