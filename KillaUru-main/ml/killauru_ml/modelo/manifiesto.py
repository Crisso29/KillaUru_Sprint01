"""FR-029, FR-031, R-017 — manifiesto del modelo según `contracts/manifiesto-modelo.schema.json`.

Lo escribe la puerta solo cuando la versión se aprueba (data-model §7) y lo lee la app (008 FR-071). El SHA-256 y el
tamaño salen del archivo, nunca se copian a mano. `ConfidenceMinimo` NO va aquí: es RN-04 de la app y del servidor.
"""
from __future__ import annotations

import json
import pathlib
from dataclasses import dataclass
from datetime import UTC, datetime

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.comun.huella import sha256_archivo
from killauru_ml.features.features import NOMBRES

_CDN = "https://cdn.killauru.pe/modelo"
# Lo que fija el preprocesamiento p1 (FR-017) y la arquitectura (FR-019): cambia solo con otra versión de p1
_FIJO = {
    "esquema": 1, "categoriaVehiculo": "Liviano", "clases": list(CLASES), "preprocesamientoVersion": "p1",
    "ventanaMs": 500, "frecuenciaHz": 50, "normalizacion": "dentroDelModelo",
    "entradas": {"nombre": "caracteristicas", "forma": [1, len(NOMBRES)], "tipo": "float32"},
    "salidas": [{"nombre": "tipo_logits", "forma": [1, len(CLASES)], "tipo": "float32"},
                {"nombre": "severidad", "forma": [1, 1], "tipo": "float32"}],
}


@dataclass(frozen=True)
class DatosManifiesto:
    """Lo que la puerta midió o ajustó para esta versión."""
    version: str
    dataset_version: str
    version_minima_app: str
    temperatura: float                    # R-013, ajustada en validación
    umbral_prefiltro: float               # FR-018, ajustado en entrenamiento
    precision: dict[str, float]
    recall: dict[str, float]
    severidad_mas_menos_1: float
    latencias_p95_ms: dict[str, float]    # benchmark por celular (≥ 3)


def generar_manifiesto(datos: DatosManifiesto, ruta_tflite: pathlib.Path, fecha_utc: str | None = None) -> dict:
    carpeta = f"{_CDN}/{datos.version}"
    return _FIJO | {
        "modeloVersion": datos.version, "url": f"{carpeta}/killauru-{datos.version}.tflite",
        "tamanoBytes": ruta_tflite.stat().st_size, "sha256": sha256_archivo(ruta_tflite),
        "fechaPublicacionUtc": fecha_utc or datetime.now(UTC).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "versionMinimaApp": datos.version_minima_app, "datasetVersion": datos.dataset_version,
        "calibracion": {"temperatura": datos.temperatura},
        "prefiltro": {"umbralPicoAPicoVerticalMs2": datos.umbral_prefiltro},
        "metricas": {"precision": datos.precision, "recall": datos.recall,
                     "severidadMasMenos1": datos.severidad_mas_menos_1,
                     "latenciaP95MsPorCelular": datos.latencias_p95_ms},
        "matrizConfusionUrl": f"{carpeta}/matriz-confusion.json", "informeUrl": f"{carpeta}/informe.json",
    }


def guardar_manifiesto(manifiesto: dict, ruta: pathlib.Path) -> None:
    """Escribe el manifiesto como JSON. Falla si ya existe (FR-031)."""
    if ruta.exists():
        raise FileExistsError(f"El manifiesto {ruta} ya existe; las versiones publicadas no se sobreescriben (FR-031)")
    ruta.parent.mkdir(parents=True, exist_ok=True)
    ruta.write_text(json.dumps(manifiesto, indent=2, ensure_ascii=False), encoding="utf-8")
