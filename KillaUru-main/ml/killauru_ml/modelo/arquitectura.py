"""FR-017 — arquitectura MLP: capas ocultas configurables, salida softmax."""
from __future__ import annotations

import keras

_N_FEATURES = 20   # data-model §4
_CAPAS_DEFAULT = [32, 16]  # config/entrenamiento.yaml CapasOcultas


def construir_modelo(
    n_clases: int = 3,
    capas_ocultas: list[int] | None = None,
    n_features: int = _N_FEATURES,
) -> keras.Sequential:
    """Construye el MLP base de KillaUru.

    Args:
        n_clases: número de clases (Normal/Bache/Hundimiento = 3)
        capas_ocultas: unidades por capa; por defecto [32, 16]
        n_features: tamaño del vector de entrada (20)
    Returns:
        Modelo Keras Sequential listo para compilar.
    """
    if capas_ocultas is None:
        capas_ocultas = _CAPAS_DEFAULT

    capas = [keras.Input(shape=(n_features,))]
    for unidades in capas_ocultas:
        capas.append(keras.layers.Dense(unidades, activation="relu"))
    capas.append(keras.layers.Dense(n_clases, activation="softmax"))

    return keras.Sequential(capas)
