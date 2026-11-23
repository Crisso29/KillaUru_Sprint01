"""T042 / FR-019, R-011 — arquitectura: normalización dentro, ≤ 2 capas ocultas y dos salidas. Patrón AAA.

Usa TensorFlow: unitario aquí significa «una pieza», no «sin librerías».
"""
import numpy as np
import pytest

from killauru_ml.modelo.arquitectura import construir_modelo
from killauru_ml.modelo.entrenar import Conjunto, pesos_de_muestra

_X = np.random.default_rng(2828).normal(5.0, 2.0, (200, 20)).astype(np.float32)


def test_entrada_caracteristicas_y_salidas_tipo_logits_y_severidad() -> None:
    # Arrange y Act
    modelo = construir_modelo(_X)
    # Assert — los nombres son el contrato con la app y el manifiesto
    assert (modelo.layers[0].name, tuple(modelo.input_shape)) == ("caracteristicas", (None, 20))
    assert dict(zip(modelo.output_names, modelo.output_shape, strict=True)) == {
        "tipo_logits": (None, 3), "severidad": (None, 1)}


def test_la_normalizacion_va_dentro_y_se_adapta_con_entrenamiento() -> None:
    # Arrange — R-011: la app entrega las 20 características crudas
    modelo = construir_modelo(_X)
    # Act
    normalizacion = modelo.get_layer("normalizacion")
    # Assert
    np.testing.assert_allclose(np.ravel(normalizacion.mean), _X.mean(axis=0), rtol=1e-5)


def test_mas_de_2_capas_ocultas_es_error() -> None:
    # Arrange — ADR-004: MLP pequeño (< 1 ms por ventana)
    capas = [32, 16, 8]
    # Act y Assert
    with pytest.raises(ValueError, match="2 capas ocultas"):
        construir_modelo(_X, capas_ocultas=capas)


def test_tipo_logits_son_logits_y_no_probabilidades() -> None:
    # Arrange — la temperatura (FR-020) se aplica a los logits antes del softmax
    modelo = construir_modelo(_X)
    # Act
    logits = modelo.predict(_X[:5], verbose=0)["tipo_logits"]
    # Assert — un softmax sumaría 1 en cada fila
    assert not np.allclose(logits.sum(axis=1), 1.0)


def test_pesos_de_clase_inversos_a_la_frecuencia_y_severidad_solo_en_anomalias() -> None:
    # Arrange — 6 Normal, 2 Bache, 1 Hundimiento
    tipos = np.array([0] * 6 + [1] * 2 + [2])
    conjunto = Conjunto(np.zeros((9, 20), dtype=np.float32), tipos, np.array([0.0] * 6 + [3, 4, 2]))
    # Act
    pesos = pesos_de_muestra(conjunto)
    # Assert — peso = n / (clases · n_clase): 9/18, 9/6, 9/3; la severidad de Normal pesa 0 (máscara)
    np.testing.assert_allclose(pesos["tipo_logits"][[0, 6, 8]], [0.5, 1.5, 3.0])
    np.testing.assert_array_equal(pesos["severidad"], [0.0] * 6 + [1.0] * 3)
