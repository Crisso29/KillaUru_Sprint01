"""FR-029, RN-04 — manifiesto JSON del modelo publicado."""
from __future__ import annotations

import json
import pathlib
from datetime import datetime, timezone


def generar_manifiesto(
    version: str,
    sha256: str,
    clases: list[str],
    fecha_utc: str | None = None,
) -> dict:
    """Genera el dict del manifiesto de un modelo publicado.

    IMPORTANTE: confidence_minimo NO se incluye aquí.
    Vive en la config de la app (RN-04 / ConfidenceMinimo=0.75).
    """
    return {
        "version": version,
        "sha256": sha256,
        "clases": clases,
        "fecha_utc": fecha_utc or datetime.now(timezone.utc).isoformat(),
    }


def guardar_manifiesto(manifiesto: dict, ruta: pathlib.Path) -> None:
    """Escribe el manifiesto como JSON. Falla si ya existe (FR-031)."""
    if ruta.exists():
        raise FileExistsError(
            f"El manifiesto {ruta} ya existe; "
            "las versiones publicadas no se sobreescriben (FR-031)"
        )
    ruta.parent.mkdir(parents=True, exist_ok=True)
    ruta.write_text(json.dumps(manifiesto, indent=2, ensure_ascii=False))
