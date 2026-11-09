"""FR-017 — remuestreo lineal de acelerómetro/giroscopio a malla uniforme de 50 Hz."""
import numpy as np


def remuestrear(
    t_orig: np.ndarray,
    valores: np.ndarray,
    t_meta: np.ndarray,
) -> np.ndarray:
    """Interpola linealmente `valores` de los instantes `t_orig` a la malla `t_meta`."""
    return np.interp(t_meta, t_orig, valores).astype(np.float32)
