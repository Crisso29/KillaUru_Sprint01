"""FR-017 — entrenamiento del MLP con early stopping y semilla fija."""
from __future__ import annotations

import keras
import numpy as np

from killauru_ml.modelo.arquitectura import construir_modelo


def entrenar(
    X_train: np.ndarray,
    y_train: np.ndarray,
    X_val: np.ndarray,
    y_val: np.ndarray,
    epocas: int = 200,
    lote: int = 64,
    capas_ocultas: list[int] | None = None,
    paciencia: int = 20,
) -> tuple[keras.Sequential, keras.callbacks.History]:
    """Entrena el MLP con Adam, sparse_categorical_crossentropy y early stopping.

    Args:
        X_train / y_train: datos de entrenamiento (float32 / int)
        X_val / y_val: datos de validación
        epocas: máximo de épocas (config/entrenamiento.yaml Epocas=200)
        lote: tamaño de lote (config/entrenamiento.yaml TamanioLote=64)
        capas_ocultas: arquitectura; por defecto [32, 16]
        paciencia: épocas sin mejora para early stopping
    Returns:
        (modelo entrenado, objeto History de Keras)
    """
    n_clases = int(np.max(np.concatenate([y_train, y_val]))) + 1
    modelo = construir_modelo(n_clases, capas_ocultas)
    modelo.compile(
        optimizer="adam",
        loss="sparse_categorical_crossentropy",
        metrics=["accuracy"],
    )

    cb_early = keras.callbacks.EarlyStopping(
        monitor="val_loss",
        patience=paciencia,
        restore_best_weights=True,
    )

    historia = modelo.fit(
        X_train,
        y_train,
        validation_data=(X_val, y_val),
        epochs=epocas,
        batch_size=lote,
        callbacks=[cb_early],
        verbose=0,
    )
    return modelo, historia
