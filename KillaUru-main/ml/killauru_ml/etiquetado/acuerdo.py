"""FR-011 — acuerdo inter-etiquetador: κ de Cohen y severidad ±1."""
from __future__ import annotations

import warnings
from dataclasses import dataclass

from sklearn.exceptions import UndefinedMetricWarning
from sklearn.metrics import cohen_kappa_score

from killauru_ml.comun.catalogo import CLASES

_EMPAREJAMIENTO_NS = 250 * 1_000_000  # 250 ms en nanosegundos
_KAPPA_MINIMO = 0.80
_ACUERDO_SEVERIDAD_MINIMO = 0.90

Par = tuple[dict, dict]


class AcuerdoError(ValueError):
    pass


@dataclass
class ResultadoAcuerdo:
    kappa: float
    acuerdo_severidad: float
    aprobado: bool
    n_pares: int


def calcular_acuerdo(eventos1: list[dict], eventos2: list[dict]) -> ResultadoAcuerdo:
    """Calcula κ de Cohen y acuerdo de severidad ±1 entre dos listas de eventos.

    Empareja eventos dentro de ±250 ms (el más cercano primero). Sin pares no hay acuerdo que medir.
    """
    pares = _emparejar(eventos1, eventos2)
    if not pares:
        return ResultadoAcuerdo(kappa=0.0, acuerdo_severidad=0.0, aprobado=False, n_pares=0)
    kappa = _kappa(pares)
    acuerdo_severidad = _acuerdo_severidad(pares)
    aprobado = kappa >= _KAPPA_MINIMO and acuerdo_severidad >= _ACUERDO_SEVERIDAD_MINIMO
    return ResultadoAcuerdo(kappa=kappa, acuerdo_severidad=acuerdo_severidad, aprobado=aprobado, n_pares=len(pares))


def _kappa(pares: list[Par]) -> float:
    """κ de Cohen del tipo de anomalía.

    κ es indefinido (0/0) solo cuando los dos etiquetadores usaron siempre la misma única clase: eso es acuerdo total,
    así que vale 1. Antes salía `nan` y `nan >= 0,80` es falso: se rechazaba un acuerdo perfecto.
    """
    tipos1 = [primero["tipo"] for primero, _ in pares]
    tipos2 = [segundo["tipo"] for _, segundo in pares]
    with warnings.catch_warnings():
        # Ese caso indefinido es justo el que se resuelve con `replace_undefined_by`: el aviso no aporta nada.
        warnings.simplefilter("ignore", UndefinedMetricWarning)
        return float(cohen_kappa_score(tipos1, tipos2, labels=list(CLASES), replace_undefined_by=1.0))


def _acuerdo_severidad(pares: list[Par]) -> float:
    """Proporción de pares cuya severidad difiere en ±1 como máximo; sin severidades comparables vale 1."""
    con_severidad = [
        (primero["severidad"], segundo["severidad"])
        for primero, segundo in pares
        if primero.get("severidad") is not None and segundo.get("severidad") is not None
    ]
    if not con_severidad:
        return 1.0
    dentro = sum(1 for severidad1, severidad2 in con_severidad if abs(severidad1 - severidad2) <= 1)
    return dentro / len(con_severidad)


def _emparejar(eventos1: list[dict], eventos2: list[dict]) -> list[Par]:
    """Cada evento de `eventos1`, en orden de tiempo, toma el evento libre más cercano de `eventos2` dentro de 250 ms."""
    usados: set[int] = set()
    pares: list[Par] = []
    for evento in sorted(eventos1, key=lambda e: e["t_pico_ns"]):
        indice = _mas_cercano_libre(evento, eventos2, usados)
        if indice is not None:
            usados.add(indice)
            pares.append((evento, eventos2[indice]))
    return pares


def _mas_cercano_libre(evento: dict, candidatos: list[dict], usados: set[int]) -> int | None:
    """Índice del candidato libre más cercano en el tiempo, si está dentro de la ventana de 250 ms."""
    distancias = [
        (abs(evento["t_pico_ns"] - candidato["t_pico_ns"]), indice)
        for indice, candidato in enumerate(candidatos)
        if indice not in usados
    ]
    if not distancias:
        return None
    distancia, indice = min(distancias)
    return indice if distancia <= _EMPAREJAMIENTO_NS else None
