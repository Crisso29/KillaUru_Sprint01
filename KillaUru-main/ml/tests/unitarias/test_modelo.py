"""T042 / FR-017 — arquitectura MLP: forma, capas, softmax."""
import numpy as np

from killauru_ml.modelo.arquitectura import construir_modelo

_N_FEATURES = 20
_N_CLASES = 3


def test_input_shape_es_20() -> None:
    # Arrange + Act
    modelo = construir_modelo(_N_CLASES)
    # Assert — entrada (None, 20)
    assert tuple(modelo.input_shape) == (None, _N_FEATURES)


def test_output_shape_es_n_clases() -> None:
    # Arrange + Act
    modelo = construir_modelo(_N_CLASES)
    # Assert — salida (None, 3)
    assert tuple(modelo.output_shape) == (None, _N_CLASES)


def test_salida_softmax_suma_uno() -> None:
    # Arrange
    modelo = construir_modelo(_N_CLASES)
    rng = np.random.default_rng(2828)
    X = rng.random((4, _N_FEATURES)).astype(np.float32)
    # Act
    pred = modelo.predict(X, verbose=0)
    # Assert — cada fila suma ≈ 1 (softmax)
    sumas = pred.sum(axis=1)
    np.testing.assert_allclose(sumas, np.ones(4), atol=1e-5)


def test_capas_ocultas_config_32_16() -> None:
    # Arrange + Act
    modelo = construir_modelo(_N_CLASES, capas_ocultas=[32, 16])
    # Assert — hay capas Dense con 32 y 16 unidades (ocultas)
    unidades_ocultas = [
        capa.units for capa in modelo.layers
        if hasattr(capa, "units") and capa.units != _N_CLASES
    ]
    assert 32 in unidades_ocultas
    assert 16 in unidades_ocultas


def test_capas_ocultas_personalizadas() -> None:
    # Arrange + Act
    modelo = construir_modelo(2, capas_ocultas=[64, 32, 16])
    # Assert — 3 capas ocultas
    ocultas = [capa for capa in modelo.layers if hasattr(capa, "units") and capa.units != 2]
    assert len(ocultas) == 3
