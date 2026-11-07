"""FR-034 — estado del pipeline: grabaciones ingresadas, versión de dataset."""
from __future__ import annotations

import csv
import pathlib

_RUTA_CATALOGO = pathlib.Path(__file__).parent.parent / "datos" / "catalogo.csv"
_RUTA_PARTICION = pathlib.Path(__file__).parent.parent / "config" / "particion.yaml"


def obtener_estado(
    ruta_catalogo: pathlib.Path | None = None,
    ruta_particion: pathlib.Path | None = None,
) -> dict:
    """Devuelve métricas de progreso del pipeline.

    Returns:
        {
          "grabaciones": int,
          "grabaciones_aptas": int,
          "version_particion": str,
          "estado_particion": str,
        }
    """
    cat = ruta_catalogo or _RUTA_CATALOGO
    total, aptas = _contar_catalogo(cat)

    part = ruta_particion or _RUTA_PARTICION
    version_part, estado_part = _leer_particion(part)

    return {
        "grabaciones": total,
        "grabaciones_aptas": aptas,
        "version_particion": version_part,
        "estado_particion": estado_part,
    }


def _contar_catalogo(ruta: pathlib.Path) -> tuple[int, int]:
    if not ruta.exists():
        return 0, 0
    total = 0
    aptas = 0
    try:
        with ruta.open(newline="", encoding="utf-8") as f:
            reader = csv.DictReader(f)
            for fila in reader:
                total += 1
                if fila.get("estado", "").strip() == "Apta":
                    aptas += 1
    except Exception:
        pass
    return total, aptas


def _leer_particion(ruta: pathlib.Path) -> tuple[str, str]:
    if not ruta.exists():
        return "—", "—"
    try:
        import yaml  # noqa: PLC0415
        datos = yaml.safe_load(ruta.read_text())
        return str(datos.get("Version", "—")), str(datos.get("Estado", "—"))
    except Exception:
        return "—", "—"
