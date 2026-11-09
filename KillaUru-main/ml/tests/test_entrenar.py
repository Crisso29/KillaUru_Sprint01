"""T044 / FR-017 — entrenamiento: seed, convergencia, historia."""
import numpy as np

from killauru_ml.comun.semillas import fijar_semillas
from killauru_ml.modelo.entrenar import entrenar

_N_FEATURES = 20
_N_CLASES = 3


def _datos(n: int, seed: int = 0):
    rng = np.random.default_rng(seed)
    X = rng.random((n, _N_FEATURES)).astype(np.float32)
    y = rng.integers(0, _N_CLASES, n)
    return X, y


def test_entrenar_devuelve_modelo_e_historia() -> None:
    # Arrange
    fijar_semillas(2828)
    X_tr, y_tr = _datos(60)
    X_val, y_val = _datos(20, seed=1)
    # Act
    modelo, historia = entrenar(X_tr, y_tr, X_val, y_val, epocas=5, lote=16)
    # Assert
    assert modelo is not None
    assert "val_loss" in historia.history
    assert "loss" in historia.history


def test_historia_tiene_al_menos_una_epoca() -> None:
    # Arrange
    fijar_semillas(2828)
    X_tr, y_tr = _datos(30)
    X_val, y_val = _datos(10)
    # Act
    _, historia = entrenar(X_tr, y_tr, X_val, y_val, epocas=3, lote=16)
    # Assert
    assert 1 <= len(historia.history["loss"]) <= 3


def test_modelo_infiere_sin_error_post_entrenar() -> None:
    # Arrange
    fijar_semillas(2828)
    X_tr, y_tr = _datos(60)
    X_val, y_val = _datos(20)
    modelo, _ = entrenar(X_tr, y_tr, X_val, y_val, epocas=2, lote=16)
    X_test = np.random.rand(5, _N_FEATURES).astype(np.float32)
    # Act
    pred = modelo.predict(X_test, verbose=0)
    # Assert — forma correcta y suma softmax ≈ 1
    assert pred.shape == (5, _N_CLASES)
    np.testing.assert_allclose(pred.sum(axis=1), np.ones(5), atol=1e-4)


def test_misma_semilla_produce_mismos_pesos() -> None:
    # Arrange — reproducibilidad con seed 2828
    X_tr, y_tr = _datos(60, seed=7)
    X_val, y_val = _datos(20, seed=8)
    # Act
    fijar_semillas(2828)
    m1, _ = entrenar(X_tr, y_tr, X_val, y_val, epocas=3, lote=16)
    fijar_semillas(2828)
    m2, _ = entrenar(X_tr, y_tr, X_val, y_val, epocas=3, lote=16)
    # Assert — últimos pesos coinciden
    p1 = m1.layers[-1].get_weights()[0]
    p2 = m2.layers[-1].get_weights()[0]
    np.testing.assert_allclose(p1, p2, atol=1e-5)
