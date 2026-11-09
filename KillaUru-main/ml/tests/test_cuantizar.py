"""T046 / FR-018 — cuantización int8: archivo creado, tamaño y equivalencia."""
import numpy as np

from killauru_ml.comun.semillas import fijar_semillas
from killauru_ml.modelo.arquitectura import construir_modelo
from killauru_ml.modelo.cuantizar import cuantizar, inferir_tflite

_N_FEATURES = 20
_N_CLASES = 3
_TFLITE_MAX_MB = 5.0  # puerta.yaml TamanoMaximoModeloMb


def _modelo_listo():
    """Modelo mínimamente entrenado para representar datasets reales."""
    fijar_semillas(2828)
    modelo = construir_modelo(_N_CLASES)
    rng = np.random.default_rng(2828)
    X = rng.random((30, _N_FEATURES)).astype(np.float32)
    y = rng.integers(0, _N_CLASES, 30)
    modelo.compile(optimizer="adam", loss="sparse_categorical_crossentropy")
    modelo.fit(X, y, epochs=1, verbose=0)
    return modelo


def test_cuantizar_crea_archivo_tflite(tmp_path) -> None:
    # Arrange
    modelo = _modelo_listo()
    ruta = tmp_path / "modelo.tflite"
    X_rep = np.random.default_rng(1).random((20, _N_FEATURES)).astype(np.float32)
    # Act
    ruta_salida = cuantizar(modelo, X_rep, ruta)
    # Assert
    assert ruta_salida.exists()
    assert ruta_salida.suffix == ".tflite"


def test_modelo_tflite_bajo_5_mb(tmp_path) -> None:
    # Arrange
    modelo = _modelo_listo()
    X_rep = np.random.default_rng(1).random((20, _N_FEATURES)).astype(np.float32)
    # Act
    ruta = cuantizar(modelo, X_rep, tmp_path / "modelo.tflite")
    # Assert — puerta.yaml TamanoMaximoModeloMb=5.0
    tamano_mb = ruta.stat().st_size / (1024 * 1024)
    assert tamano_mb < _TFLITE_MAX_MB


def test_inferencia_tflite_coherente_con_keras(tmp_path) -> None:
    # Arrange
    fijar_semillas(2828)
    modelo = _modelo_listo()
    X_rep = np.random.default_rng(2).random((20, _N_FEATURES)).astype(np.float32)
    ruta = cuantizar(modelo, X_rep, tmp_path / "m.tflite")
    X_test = np.random.default_rng(3).random((5, _N_FEATURES)).astype(np.float32)
    # Act
    pred_keras = modelo.predict(X_test, verbose=0)
    pred_tflite = inferir_tflite(ruta, X_test)
    # Assert — DeltaConfidenceP99Maximo=0.05 (puerta.yaml)
    delta_max = float(np.abs(pred_keras - pred_tflite).max())
    assert delta_max < 0.05
