"""FR-017 — estimación de la gravedad como media de los últimos 2 s (100 muestras)."""
import numpy as np

_MUESTRAS_GRAVEDAD = 100  # 2 s × 50 Hz (data-model §4, paso 2)


def estimar_gravedad(acelerometro: np.ndarray) -> np.ndarray:
    """Media de las últimas `_MUESTRAS_GRAVEDAD` filas del acelerómetro (3-vector)."""
    ventana = acelerometro[-_MUESTRAS_GRAVEDAD:]
    return ventana.mean(axis=0).astype(np.float32)
