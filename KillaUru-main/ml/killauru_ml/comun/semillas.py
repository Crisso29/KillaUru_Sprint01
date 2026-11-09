"""FR-016, R-019 — fijación de semillas para reproducibilidad total."""
import os
import random


def fijar_semillas(semilla: int = 2828) -> None:
    """Fija Python, NumPy, TensorFlow/Keras para operaciones deterministas."""
    os.environ["PYTHONHASHSEED"] = str(semilla)
    random.seed(semilla)

    import numpy as np
    np.random.seed(semilla)

    import tensorflow as tf
    tf.random.set_seed(semilla)
    # Keras 3: fija semilla global y activa determinismo de operadores
    import keras
    keras.utils.set_random_seed(semilla)
    tf.config.experimental.enable_op_determinism()
