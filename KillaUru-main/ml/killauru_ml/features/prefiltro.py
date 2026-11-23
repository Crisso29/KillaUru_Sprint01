"""FR-018, RNF-15, `diseño-interno` §7.4 — prefiltro de energía.

Las ventanas con pico a pico vertical (`v_pap`) menor que el umbral se dan por Normal sin pasar por el modelo: ahorra
batería en la app, donde casi toda la vía es lisa. El umbral se elige con las anomalías de **entrenamiento** para
perder como mucho el 1 % de ellas, y el ajuste se registra para la prueba de no-fuga (FR-015).
"""
from __future__ import annotations

import math
from collections.abc import Callable

import numpy as np

from killauru_ml.features.features import INDICE_PICO_A_PICO
from killauru_ml.particion.no_fuga import RegistroAjuste, huella_ventana
from killauru_ml.ventanas.ventaneo import VentanaDataset

PERDIDA_MAXIMA = 0.01                       # config/entrenamiento.yaml PerdidaMaximaPrefiltro
Resultado = tuple[str, int | None, float]   # (tipo, severidad, confidence)


class PrefiltroError(ValueError):
    pass


def ajustar_prefiltro(ventanas_entrenamiento: list[VentanaDataset]) -> tuple[float, RegistroAjuste]:
    """Mayor umbral que deja fuera como mucho el 1 % de las anomalías; devuelve también qué ventanas se usaron."""
    anomalias = [ventana for ventana in ventanas_entrenamiento if ventana.tipo != "Normal"]
    pico_a_pico = np.sort([float(np.ptp(ventana.a_v)) for ventana in anomalias])
    umbral = float(pico_a_pico[math.floor(PERDIDA_MAXIMA * len(pico_a_pico))])
    verificar_umbral(pico_a_pico, umbral)
    return umbral, RegistroAjuste("prefiltro", frozenset(huella_ventana(ventana) for ventana in anomalias))


def verificar_umbral(pico_a_pico_anomalias: np.ndarray, umbral: float) -> None:
    """Lanza PrefiltroError si el umbral deja pasar menos del 99 % de las anomalías."""
    paso = float(np.mean(np.asarray(pico_a_pico_anomalias) >= umbral))
    if paso < 1.0 - PERDIDA_MAXIMA:
        raise PrefiltroError(f"El prefiltro deja pasar el {paso * 100:.2f} % de las anomalías; FR-018 exige 99 %"
                             .replace(".", ","))


def clasificar_con_prefiltro(vector: np.ndarray, umbral: float, modelo: Callable[[np.ndarray], Resultado]) -> Resultado:
    """Normal con confidence 1 si `v_pap` < umbral; si no, lo que diga el modelo."""
    if float(vector[INDICE_PICO_A_PICO]) < umbral:
        return "Normal", None, 1.0
    return modelo(vector)
