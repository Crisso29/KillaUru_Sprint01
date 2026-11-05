"""FR-007 — gestión de ml/datos/catalogo.csv (una fila por grabación)."""
from __future__ import annotations

import csv
import pathlib
from dataclasses import dataclass

_RUTA_CATALOGO = pathlib.Path(__file__).parents[3] / "datos" / "catalogo.csv"
_COLUMNAS = [
    "grabacion_id", "recorrido", "celular", "vehiculo", "sentido",
    "horas", "hz_efectivo", "huecos_pct", "sha256", "estado", "motivo",
]


@dataclass
class FilaCatalogo:
    grabacion_id: str
    recorrido: str
    celular: str
    vehiculo: str
    sentido: str
    horas: float
    hz_efectivo: float
    huecos_pct: float
    sha256: str
    estado: str   # Apta | NoApta
    motivo: str   # FrecuenciaBaja, Huecos, SinGps, MetaIncompleta, … o vacío


def agregar_fila(fila: FilaCatalogo, ruta: pathlib.Path = _RUTA_CATALOGO) -> None:
    """Añade una fila al catálogo; crea el archivo con cabecera si no existe."""
    nuevo = not ruta.exists() or ruta.stat().st_size == 0
    with ruta.open("a", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=_COLUMNAS)
        if nuevo:
            writer.writeheader()
        writer.writerow({
            "grabacion_id": fila.grabacion_id,
            "recorrido": fila.recorrido,
            "celular": fila.celular,
            "vehiculo": fila.vehiculo,
            "sentido": fila.sentido,
            "horas": f"{fila.horas:.4f}",
            "hz_efectivo": f"{fila.hz_efectivo:.2f}",
            "huecos_pct": f"{fila.huecos_pct:.4f}",
            "sha256": fila.sha256,
            "estado": fila.estado,
            "motivo": fila.motivo,
        })


def leer_catalogo(ruta: pathlib.Path = _RUTA_CATALOGO) -> list[FilaCatalogo]:
    """Lee todas las filas del catálogo."""
    if not ruta.exists():
        return []
    with ruta.open(newline="", encoding="utf-8") as f:
        return [
            FilaCatalogo(**{k: row[k] for k in _COLUMNAS})
            for row in csv.DictReader(f)
        ]
