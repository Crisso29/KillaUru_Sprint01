"""FR-007 — ingesta de grabaciones: verificar, SHA-256, catalogo y subir a R2."""
from __future__ import annotations

import pathlib
import tarfile
import tempfile

import numpy as np
import pandas as pd

from killauru_ml.comun.huella import sha256_archivo
from killauru_ml.lectura.catalogo import FilaCatalogo, agregar_fila
from killauru_ml.lectura.grabacion import GrabacionError, leer_grabacion
from killauru_ml.lectura.integridad import verificar_integridad


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
    """Verifica la grabación, calcula SHA-256 y registra en el catálogo."""
    kwargs = {"ruta": ruta_catalogo} if ruta_catalogo else {}
    motivos: list[str] = []

    try:
        grabacion = leer_grabacion(carpeta)
    except GrabacionError as exc:
        fila = FilaCatalogo(
            grabacion_id=carpeta.name, recorrido="", celular="", vehiculo="",
            sentido="", horas=0.0, hz_efectivo=0.0, huecos_pct=0.0,
            sha256="", estado="NoApta", motivo=f"MetaIncompleta: {exc}",
        )
        agregar_fila(fila, **kwargs)
        return fila

    meta = grabacion.meta
    t_ns = grabacion.acelerometro["t_ns"].to_numpy(dtype=np.int64)
    t_giro_ns = grabacion.giroscopio["t_ns"].to_numpy(dtype=np.int64)
    precision_m = grabacion.gps["precision_m"].to_numpy() if "precision_m" in grabacion.gps.columns else None
    t_gps = grabacion.gps["t_ns"].to_numpy(dtype=np.int64) if "t_ns" in grabacion.gps.columns else None

    resultado = verificar_integridad(t_ns, t_giro_ns, precision_m, t_gps)
    for m in resultado.motivos:
        motivos.append(m.value)

    with tempfile.TemporaryDirectory() as tmp:
        tar_path = _tar_sin_video(carpeta, pathlib.Path(tmp))
        sha = sha256_archivo(tar_path)

    horas = _horas_en_marcha(grabacion.gps)
    estado = resultado.estado.value
    tramo = meta.get("tramo", {})

    fila = FilaCatalogo(
        grabacion_id=meta.get("grabacionId", carpeta.name),
        recorrido=meta.get("recorrido", ""),
        celular=meta.get("celular", {}).get("alias", ""),
        vehiculo=meta.get("vehiculo", {}).get("alias", ""),
        sentido=tramo.get("sentido", ""),
        horas=horas,
        hz_efectivo=resultado.hz_efectivo,
        huecos_pct=resultado.huecos_pct,
        sha256=sha,
        estado=estado,
        motivo=",".join(motivos),
    )
    agregar_fila(fila, **kwargs)

    if subir and estado == "Apta":
        import subprocess
        script = pathlib.Path(__file__).parents[3] / "scripts" / "subir_grabacion.sh"
        subprocess.run(["bash", str(script), str(carpeta)], check=True)

    return fila
