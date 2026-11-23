"""T050 / FR-022, R-014, ADR-004 — el `.tflite` que carga la app: tamaño, nombres, formas, tipos e int8. Patrón AAA.

Se entrena y convierte una sola vez por módulo (fixture) porque convertir tarda; cada prueba mira una propiedad.
"""
import numpy as np
import pytest
from ai_edge_litert.interpreter import Interpreter

from killauru_ml.comun.semillas import fijar_semillas
from killauru_ml.modelo.cuantizar import cuantizar, inferir_tflite
from killauru_ml.modelo.entrenar import Conjunto, entrenar

_MAXIMO_BYTES = 5 * 1024 * 1024   # ADR-004, puerta.yaml TamanoMaximoModeloMb


def _conjunto(cantidad: int, semilla: int) -> Conjunto:
    generador = np.random.default_rng(semilla)
    tipos = np.arange(cantidad) % 3
    X = generador.normal(0.0, 1.0, (cantidad, 20)).astype(np.float32) + tipos[:, None].astype(np.float32)
    return Conjunto(X, tipos, np.where(tipos == 0, 0, 3).astype(np.float32))


@pytest.fixture(scope="module")
def convertido(tmp_path_factory):
    fijar_semillas(2828)
    entrenamiento = _conjunto(300, semilla=1)
    modelo, _ = entrenar(entrenamiento, _conjunto(60, semilla=2), epocas=20, lote=32)
    ruta = cuantizar(modelo, entrenamiento.X[:200], tmp_path_factory.mktemp("tflite") / "killauru-0.0.1.tflite")
    return modelo, ruta


def _firma(ruta):
    interprete = Interpreter(model_path=str(ruta))
    return interprete, interprete.get_signature_runner()


def test_el_archivo_pesa_menos_de_5_mb(convertido) -> None:
    # Arrange
    _, ruta = convertido
    # Act
    tamano = ruta.stat().st_size
    # Assert
    assert 0 < tamano < _MAXIMO_BYTES


def test_entrada_caracteristicas_1x20_float32(convertido) -> None:
    # Arrange
    _, ruta = convertido
    # Act
    entrada = _firma(ruta)[1].get_input_details()["caracteristicas"]
    # Assert — la app entrega float32 sin escalas
    assert (list(entrada["shape"]), entrada["dtype"]) == ([1, 20], np.float32)


def test_salidas_tipo_logits_1x3_y_severidad_1x1_float32(convertido) -> None:
    # Arrange
    _, ruta = convertido
    # Act
    salidas = _firma(ruta)[1].get_output_details()
    # Assert — la app las lee por nombre (contrato del manifiesto)
    assert {nombre: (list(detalle["shape"]), detalle["dtype"]) for nombre, detalle in salidas.items()} == {
        "tipo_logits": ([1, 3], np.float32), "severidad": ([1, 1], np.float32)}


def test_las_operaciones_internas_son_int8(convertido) -> None:
    # Arrange
    _, ruta = convertido
    interprete = _firma(ruta)[0]
    # Act
    tipos = {np.dtype(detalle["dtype"]) for detalle in interprete.get_tensor_details()}
    # Assert — pesos y activaciones cuantizados; solo los bordes quedan en float32
    assert np.dtype(np.int8) in tipos


def test_el_tflite_decide_igual_que_keras(convertido) -> None:
    # Arrange — R-014: tipo igual ≥ 99 % y |Δ confidence| p99 ≤ 0,05
    modelo, ruta = convertido
    X = _conjunto(200, semilla=3).X
    # Act
    keras_logits = modelo.predict(X, verbose=0)["tipo_logits"]
    tflite_logits = inferir_tflite(ruta, X)["tipo_logits"]
    # Assert
    probabilidades = [np.exp(logits) / np.exp(logits).sum(axis=1, keepdims=True) for logits in (keras_logits, tflite_logits)]
    assert np.mean(keras_logits.argmax(axis=1) == tflite_logits.argmax(axis=1)) >= 0.99
    assert np.percentile(np.abs(probabilidades[0] - probabilidades[1]).max(axis=1), 99) <= 0.05
