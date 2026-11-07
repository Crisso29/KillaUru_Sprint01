"""T038 / data-model §4 — extracción de 20 features float32 por ventana."""
import numpy as np
import pytest
from killauru_ml.features.features import extraer_features

_N = 25  # muestras por ventana (500 ms a 50 Hz)


def test_devuelve_exactamente_20_features() -> None:
    # Arrange
    a_v = np.zeros(_N, dtype=np.float32)
    # Act
    features = extraer_features(a_v)
    # Assert
    assert features.shape == (20,)


def test_dtype_es_float32() -> None:
    # Arrange
    a_v = np.ones(_N, dtype=np.float64)
    # Act
    features = extraer_features(a_v)
    # Assert
    assert features.dtype == np.float32


def test_señal_cero_produce_ceros_en_estadisticos() -> None:
    # Arrange — señal completamente plana
    a_v = np.zeros(_N, dtype=np.float32)
    # Act
    features = extraer_features(a_v)
    # Assert — mean, min, max, rms, peak_abs = 0
    assert features[0] == pytest.approx(0.0)   # mean
    assert features[2] == pytest.approx(0.0)   # min
    assert features[3] == pytest.approx(0.0)   # max
    assert features[4] == pytest.approx(0.0)   # rms
    assert features[5] == pytest.approx(0.0)   # peak_abs


def test_señal_constante_std_cero() -> None:
    # Arrange
    a_v = np.full(_N, 5.0, dtype=np.float32)
    # Act
    features = extraer_features(a_v)
    # Assert — desviación estándar = 0
    assert features[1] == pytest.approx(0.0, abs=1e-5)  # std


def test_señal_simetrica_skew_cercano_a_cero() -> None:
    # Arrange — rampa simétrica -12..12 (25 muestras)
    a_v = np.array(list(range(-12, 13)), dtype=np.float32)
    # Act
    features = extraer_features(a_v)
    # Assert — asimetría ≈ 0 en señal simétrica
    assert abs(features[6]) < 0.1  # skew


def test_pico_positivo_detectado_en_peak_abs() -> None:
    # Arrange — pico de +10 m/s² en el centro
    a_v = np.zeros(_N, dtype=np.float32)
    a_v[12] = 10.0
    # Act
    features = extraer_features(a_v)
    # Assert
    assert features[5] == pytest.approx(10.0, rel=1e-3)  # peak_abs


def test_sin_velocidad_ultimas_features_cero() -> None:
    # Arrange — sin vel → features 18 y 19 = 0
    a_v = np.ones(_N, dtype=np.float32)
    # Act
    features = extraer_features(a_v, vel=None)
    # Assert
    assert features[18] == pytest.approx(0.0)  # vel_mean
    assert features[19] == pytest.approx(0.0)  # vel_max
