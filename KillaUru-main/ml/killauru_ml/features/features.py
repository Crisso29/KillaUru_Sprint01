"""data-model §4 — 20 features float32 por ventana de 500 ms a 50 Hz."""
from __future__ import annotations

import numpy as np

_N_FEATURES = 20


def extraer_features(
    a_v: np.ndarray,
    vel: np.ndarray | None = None,
) -> np.ndarray:
    """Extrae 20 features float32 de la aceleración vertical de una ventana.

    El orden es parte del contrato con el modelo y la app: 0–9 estadísticos, 10–14 forma, 15–17 espectro y
    18–19 velocidad.

    Args:
        a_v: 25 muestras de aceleración vertical (m/s²)
        vel: 25 muestras de velocidad (m/s); si None, features 18-19 = 0
    Returns:
        ndarray shape (20,) dtype float32
    """
    senal = a_v.astype(np.float64)
    valores = _estadisticos(senal) + _forma(senal) + _espectro(senal) + _velocidad(vel)
    return np.array(valores, dtype=np.float32)


def _estadisticos(senal: np.ndarray) -> list[float]:
    """0–9: media, desviación, mínimo, máximo, RMS, pico absoluto, asimetría, curtosis, IQR y percentil 95 absoluto."""
    desviacion = float(np.std(senal))
    return [
        float(np.mean(senal)), desviacion, float(np.min(senal)), float(np.max(senal)),
        _rms(senal), float(np.max(np.abs(senal))), _skewness(senal, desviacion), _kurtosis(senal, desviacion),
        float(np.percentile(senal, 75) - np.percentile(senal, 25)), float(np.percentile(np.abs(senal), 95)),
    ]


def _forma(senal: np.ndarray) -> list[float]:
    """10–14: cruces por cero, pico a pico, factor de cresta, energía y longitud de onda media."""
    rms = _rms(senal)
    pico_absoluto = float(np.max(np.abs(senal)))
    return [
        float(np.mean(np.diff(np.sign(senal)) != 0)),
        float(np.max(senal) - np.min(senal)),
        pico_absoluto / rms if rms > 0 else 0.0,
        float(np.mean(senal ** 2)),
        float(np.mean(np.abs(np.diff(senal)))),
    ]


def _espectro(senal: np.ndarray) -> list[float]:
    """15–17: energía espectral, posición relativa del pico y proporción de baja frecuencia (sin la componente DC)."""
    magnitudes = np.abs(np.fft.rfft(senal))[1:]
    if len(magnitudes) == 0:
        return [0.0, 0.0, 0.0]
    baja = float(np.sum(magnitudes[:3] ** 2))
    total = float(np.sum(magnitudes ** 2)) + 1e-10
    return [float(np.sum(magnitudes ** 2) / len(senal)), float(np.argmax(magnitudes) / len(magnitudes)), baja / total]


def _velocidad(vel: np.ndarray | None) -> list[float]:
    """18–19: velocidad media y máxima de la ventana; 0 si no hay GPS."""
    if vel is None:
        return [0.0, 0.0]
    velocidad = vel.astype(np.float64)
    return [float(np.mean(velocidad)), float(np.max(velocidad))]


def _rms(senal: np.ndarray) -> float:
    return float(np.sqrt(np.mean(senal ** 2)))


def _skewness(senal: np.ndarray, desviacion: float | None = None) -> float:
    desviacion = desviacion if desviacion is not None else float(np.std(senal))
    if desviacion == 0:
        return 0.0
    return float(np.mean(((senal - np.mean(senal)) / desviacion) ** 3))


def _kurtosis(senal: np.ndarray, desviacion: float | None = None) -> float:
    desviacion = desviacion if desviacion is not None else float(np.std(senal))
    if desviacion == 0:
        return 0.0
    return float(np.mean(((senal - np.mean(senal)) / desviacion) ** 4) - 3.0)
