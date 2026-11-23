"""FR-022, R-014, ADR-004 — conversión a TFLite int8 completa y la inferencia con el mismo runtime que la app.

Pesos y activaciones en int8 (corre más rápido en gama media); la entrada `caracteristicas` y las salidas
`tipo_logits` y `severidad` quedan en `float32` para que la app no maneje escalas. TensorFlow avisa «Statistics for
quantized inputs were expected»: es justamente por dejar la entrada en float32, y es lo buscado.
"""
from __future__ import annotations

import pathlib
from collections.abc import Iterator

import keras
import numpy as np
import tensorflow as tf

_MAXIMO_CALIBRACION = 500    # R-014: ≤ 500 ventanas de entrenamiento


def cuantizar(modelo: keras.Model, X_calibracion: np.ndarray, ruta_salida: pathlib.Path) -> pathlib.Path:
    """Convierte el modelo a `.tflite` int8 calibrando con ventanas de **entrenamiento**; devuelve la ruta escrita."""
    convertidor = tf.lite.TFLiteConverter.from_keras_model(modelo)
    convertidor.optimizations = [tf.lite.Optimize.DEFAULT]
    convertidor.representative_dataset = lambda: _muestras(X_calibracion[:_MAXIMO_CALIBRACION])
    convertidor.target_spec.supported_ops = [tf.lite.OpsSet.TFLITE_BUILTINS_INT8]
    convertidor.inference_input_type = tf.float32
    convertidor.inference_output_type = tf.float32
    ruta_salida.parent.mkdir(parents=True, exist_ok=True)
    ruta_salida.write_bytes(convertidor.convert())
    return ruta_salida


def inferir_tflite(ruta: pathlib.Path, X: np.ndarray) -> dict[str, np.ndarray]:
    """Ejecuta el `.tflite` ventana a ventana con `ai-edge-litert` (RNF-04) y lee las salidas por nombre.

    Devuelve {"tipo_logits": (n, 3), "severidad": (n, 1)}.
    """
    from ai_edge_litert.interpreter import Interpreter  # noqa: PLC0415 (solo lo carga quien infiere)

    firma = Interpreter(model_path=str(ruta)).get_signature_runner()
    resultados = [firma(caracteristicas=fila.reshape(1, -1).astype(np.float32)) for fila in X]
    return {nombre: np.concatenate([resultado[nombre] for resultado in resultados]) for nombre in ("tipo_logits",
                                                                                                  "severidad")}


def _muestras(X: np.ndarray) -> Iterator[list[np.ndarray]]:
    for fila in X:
        yield [fila.reshape(1, -1).astype(np.float32)]
