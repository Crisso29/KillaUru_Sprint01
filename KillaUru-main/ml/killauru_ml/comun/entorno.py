"""FR-016, R-019 — información del entorno de ejecución para el informe."""
import sys


def versiones() -> dict[str, str]:
    """Devuelve versiones de Python, TensorFlow y NumPy."""
    import numpy as np
    import tensorflow as tf

    return {
        "python": sys.version.split()[0],
        "tensorflow": tf.__version__,
        "numpy": np.__version__,
    }
