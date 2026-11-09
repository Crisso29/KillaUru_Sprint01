"""FR-003, FR-004, FR-005, FR-006 — lectura y validación de una carpeta de grabación."""
from __future__ import annotations

import json
import pathlib
from dataclasses import dataclass, field

import pandas as pd

_ARCHIVOS_REQUERIDOS = ["meta.json", "acelerometro.csv", "giroscopio.csv", "gps.csv", "marcas.csv"]
_CAMPOS_PROHIBIDOS = {"imei", "android_id", "placa", "nombre"}


class GrabacionError(ValueError):
    """Error en el formato o el contenido de una grabación."""


@dataclass
class Grabacion:
    meta: dict
    acelerometro: pd.DataFrame
    giroscopio: pd.DataFrame
    gps: pd.DataFrame
    marcas: pd.DataFrame
    ruta: pathlib.Path = field(repr=False)


def _validar_meta(meta: dict) -> None:
    prohibidos = _CAMPOS_PROHIBIDOS & {k.lower() for k in meta}
    if prohibidos:
        raise GrabacionError(f"Campos personales prohibidos en meta.json: {sorted(prohibidos)}")

    categoria = meta.get("vehiculo", {}).get("categoria", "")
    if categoria != "Liviano":
        raise GrabacionError(f"categoria debe ser 'Liviano', se encontró '{categoria}' (RN-18)")


def _validar_archivos(carpeta: pathlib.Path) -> None:
    for nombre in _ARCHIVOS_REQUERIDOS:
        if not (carpeta / nombre).exists():
            raise GrabacionError(f"Archivo requerido ausente: {nombre}")


def leer_grabacion(carpeta: pathlib.Path) -> Grabacion:
    """Lee y valida una carpeta de grabación; lanza GrabacionError si hay problemas."""
    _validar_archivos(carpeta)

    meta = json.loads((carpeta / "meta.json").read_text(encoding="utf-8"))
    _validar_meta(meta)

    acelerometro = pd.read_csv(carpeta / "acelerometro.csv")
    giroscopio = pd.read_csv(carpeta / "giroscopio.csv")
    gps = pd.read_csv(carpeta / "gps.csv")
    marcas = pd.read_csv(carpeta / "marcas.csv")

    return Grabacion(
        meta=meta,
        acelerometro=acelerometro,
        giroscopio=giroscopio,
        gps=gps,
        marcas=marcas,
        ruta=carpeta,
    )
