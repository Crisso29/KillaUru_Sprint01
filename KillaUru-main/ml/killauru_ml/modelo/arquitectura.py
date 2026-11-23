"""FR-019, R-011, ADR-004 — MLP de KillaUru: normalización dentro → 32 → 16 → dos cabezas.

- `caracteristicas` (20 valores crudos `p1`): la app no normaliza; la capa `normalizacion` lo hace dentro del grafo,
  adaptada solo con entrenamiento (FR-015).
- `tipo_logits` (3, orden de `CLASES`): logits sin softmax, porque la temperatura se aplica antes (FR-020).
- `severidad` (1, lineal): valor continuo que la app redondea y acota a 1–5 (RN-02).
"""
from __future__ import annotations

import keras
import numpy as np

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.features.features import NOMBRES

CAPAS_OCULTAS = (32, 16)      # config/entrenamiento.yaml CapasOcultas
_MAXIMO_CAPAS_OCULTAS = 2     # FR-019


def construir_modelo(X_entrenamiento: np.ndarray, capas_ocultas: tuple[int, ...] | list[int] = CAPAS_OCULTAS
                     ) -> keras.Model:
    """MLP con la normalización ya adaptada a `X_entrenamiento`, listo para compilar."""
    if len(capas_ocultas) > _MAXIMO_CAPAS_OCULTAS:
        raise ValueError(f"FR-019 permite como máximo {_MAXIMO_CAPAS_OCULTAS} capas ocultas: {list(capas_ocultas)}")
    entrada = keras.Input(shape=(len(NOMBRES),), name="caracteristicas")
    normalizacion = keras.layers.Normalization(name="normalizacion")
    normalizacion.adapt(np.asarray(X_entrenamiento, dtype=np.float32))
    oculta = normalizacion(entrada)
    for numero, unidades in enumerate(capas_ocultas, start=1):
        oculta = keras.layers.Dense(unidades, activation="relu", name=f"oculta_{numero}")(oculta)
    tipo_logits = keras.layers.Dense(len(CLASES), name="tipo_logits")(oculta)
    severidad = keras.layers.Dense(1, name="severidad")(oculta)
    return keras.Model(entrada, {"tipo_logits": tipo_logits, "severidad": severidad}, name="killauru")
