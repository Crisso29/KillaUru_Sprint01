"""FR-029 — registro inmutable de cada entrenamiento (versión, métricas, rutas)."""
from __future__ import annotations

import json
import pathlib
from dataclasses import asdict, dataclass, field
from datetime import UTC, datetime


@dataclass
class RegistroEntrenamiento:
    version: str
    timestamp_utc: str
    semilla: int
    epocas_ejecutadas: int
    perdida_entrenamiento: float
    perdida_validacion: float
    precision_por_clase: dict[str, float] = field(default_factory=dict)
    recall_por_clase: dict[str, float] = field(default_factory=dict)
    ruta_modelo_keras: str = ""
    ruta_modelo_tflite: str = ""
    sha256_tflite: str = ""
    version_dataset: str = ""
    notas: str = ""

    @staticmethod
    def nueva(version: str, **kwargs) -> RegistroEntrenamiento:
        return RegistroEntrenamiento(
            version=version,
            timestamp_utc=datetime.now(UTC).isoformat(),
            **kwargs,
        )


def guardar_registro(registro: RegistroEntrenamiento, ruta: pathlib.Path) -> None:
    """Escribe el registro como JSON; falla si la ruta ya existe (FR-031)."""
    if ruta.exists():
        raise FileExistsError(
            f"El registro {ruta} ya existe; los registros son inmutables (FR-031)"
        )
    ruta.parent.mkdir(parents=True, exist_ok=True)
    ruta.write_text(json.dumps(asdict(registro), indent=2, ensure_ascii=False))


def leer_registro(ruta: pathlib.Path) -> RegistroEntrenamiento:
    """Lee un registro guardado previamente."""
    datos = json.loads(ruta.read_text())
    return RegistroEntrenamiento(**datos)
