"""FR-012, RN-03, FR-007 — qué ventanas sirven para el dataset.

Una ventana es apta si el vehículo lleva > 5 km/h sostenidos ≥ 10 s al terminar la ventana (RN-03; la app tampoco
clasifica fuera de esa condición) y si no tiene un hueco de muestreo > 200 ms (el mismo criterio que la integridad).
"""
from __future__ import annotations

import numpy as np

from killauru_ml.lectura.integridad import HUECO_MAXIMO_NS
from killauru_ml.senal.condicion_velocidad import VELOCIDAD_MINIMA_KMH, cumple_rn03

_NS_POR_S = 1_000_000_000
_SIN_MARCHA = -1


def a_kmh(velocidad_ms: np.ndarray) -> np.ndarray:
    """m/s → km/h redondeado a 3 decimales: el GPS no es más preciso, y así 5 km/h guardado como `float32` sigue
    siendo 5 y no 5,0000002 (que pasaría la regla por error)."""
    return np.round(np.asarray(velocidad_ms, dtype=np.float64) * 3.6, 3)


def inicio_de_racha(t_ns: np.ndarray, kmh: np.ndarray) -> np.ndarray:
    """Para cada muestra, la hora en que empezó la marcha actual sobre 5 km/h; `_SIN_MARCHA` si no va sobre 5."""
    en_marcha = kmh > VELOCIDAD_MINIMA_KMH
    inicios = np.full(len(t_ns), _SIN_MARCHA, dtype=np.int64)
    for indice in np.flatnonzero(en_marcha):
        seguia = indice > 0 and en_marcha[indice - 1]
        inicios[indice] = inicios[indice - 1] if seguia else t_ns[indice]
    return inicios


def es_apta(t_ns: np.ndarray, kmh: np.ndarray, racha: np.ndarray, muestras: slice, t_fin_ns: int) -> bool:
    """La ventana cumple RN-03 de principio a fin (una sola racha) y no tiene huecos."""
    primera, ultima = muestras.start, muestras.stop - 1
    if racha[primera] == _SIN_MARCHA or racha[primera] != racha[ultima]:
        return False
    sostenida_s = (t_fin_ns - int(racha[ultima])) / _NS_POR_S
    sin_hueco = int(np.max(np.diff(t_ns[muestras]))) <= HUECO_MAXIMO_NS
    return sin_hueco and cumple_rn03(float(np.min(kmh[muestras])), sostenida_s)
