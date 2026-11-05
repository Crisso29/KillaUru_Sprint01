"""data-model §4 — 20 features float32 por ventana de 500 ms a 50 Hz."""
from __future__ import annotations

import numpy as np

_N_FEATURES = 20


def extraer_features(
    a_v: np.ndarray,
    vel: np.ndarray | None = None,
) -> np.ndarray:
    """Extrae 20 features float32 de la aceleración vertical de una ventana.

    Args:
        a_v: 25 muestras de aceleración vertical (m/s²)
        vel: 25 muestras de velocidad (m/s); si None, features 18-19 = 0
    Returns:
        ndarray shape (20,) dtype float32
    """
    a = a_v.astype(np.float64)

    # --- estadísticos básicos (0-9) ---
    mean = np.mean(a)
    std = np.std(a)
    mn = np.min(a)
    mx = np.max(a)
    rms = float(np.sqrt(np.mean(a ** 2)))
    peak_abs = float(np.max(np.abs(a)))
    skew = _skewness(a, std)
    kurt = _kurtosis(a, std)
    iqr = float(np.percentile(a, 75) - np.percentile(a, 25))
    p95 = float(np.percentile(np.abs(a), 95))

    # --- forma (10-14) ---
    zero_cross = float(np.mean(np.diff(np.sign(a)) != 0))
    peak_peak = float(mx - mn)
    crest = float(peak_abs / rms) if rms > 0 else 0.0
    energy = float(np.mean(a ** 2))
    waveform_len = float(np.mean(np.abs(np.diff(a))))

    # --- espectro (15-17) ---
    fft_mag = np.abs(np.fft.rfft(a))[1:]  # excluir componente DC
    spec_energy = float(np.sum(fft_mag ** 2) / len(a)) if len(fft_mag) > 0 else 0.0
    spec_peak = float(np.argmax(fft_mag) / len(fft_mag)) if len(fft_mag) > 0 else 0.0
    low = float(np.sum(fft_mag[:3] ** 2))
    total = float(np.sum(fft_mag ** 2)) + 1e-10
    spec_ratio = low / total

    # --- velocidad (18-19) ---
    if vel is not None:
        v = vel.astype(np.float64)
        vel_mean = float(np.mean(v))
        vel_max = float(np.max(v))
    else:
        vel_mean = 0.0
        vel_max = 0.0

    return np.array(
        [
            mean, std, mn, mx, rms, peak_abs, skew, kurt, iqr, p95,
            zero_cross, peak_peak, crest, energy, waveform_len,
            spec_energy, spec_peak, spec_ratio,
            vel_mean, vel_max,
        ],
        dtype=np.float32,
    )


def _skewness(a: np.ndarray, std: float | None = None) -> float:
    s = std if std is not None else float(np.std(a))
    if s == 0:
        return 0.0
    return float(np.mean(((a - np.mean(a)) / s) ** 3))


def _kurtosis(a: np.ndarray, std: float | None = None) -> float:
    s = std if std is not None else float(np.std(a))
    if s == 0:
        return 0.0
    return float(np.mean(((a - np.mean(a)) / s) ** 4) - 3.0)
