"""FR-018 — cuantización int8 con TFLiteConverter y ai-edge-litert."""
from __future__ import annotations

import pathlib

import numpy as np
import tensorflow as tf


def cuantizar(
    modelo,
    X_rep: np.ndarray,
    ruta_salida: pathlib.Path,
) -> pathlib.Path:
    """Convierte el modelo Keras a TFLite int8 usando dataset representativo.

    La cuantización es full-integer para los pesos; entrada y salida
    permanecen en float32 para compatibilidad directa con la app Android.

    Args:
        modelo: modelo Keras Sequential entrenado
        X_rep: muestras representativas (N, 20) float32 para calibración
        ruta_salida: destino del archivo .tflite
    Returns:
        ruta_salida tras escribir el archivo
    """
    def dataset_representativo():
        for i in range(len(X_rep)):
            yield [X_rep[i : i + 1].astype(np.float32)]

    converter = tf.lite.TFLiteConverter.from_keras_model(modelo)
    converter.optimizations = [tf.lite.Optimize.DEFAULT]
    converter.representative_dataset = dataset_representativo
    converter.target_spec.supported_ops = [tf.lite.OpsSet.TFLITE_BUILTINS_INT8]
    # Mantener float32 en entrada y salida: más simple de integrar en la app
    converter.inference_input_type = tf.float32
    converter.inference_output_type = tf.float32

    tflite_bytes = converter.convert()
    ruta_salida.parent.mkdir(parents=True, exist_ok=True)
    ruta_salida.write_bytes(tflite_bytes)
    return ruta_salida


def inferir_tflite(
    ruta: pathlib.Path,
    X: np.ndarray,
) -> np.ndarray:
    """Ejecuta inferencia TFLite sobre un lote X (N, 20) → (N, n_clases).

    Usa ai-edge-litert para coincidir con el runtime de producción (RNF-04).
    """
    from ai_edge_litert.interpreter import Interpreter  # noqa: PLC0415

    interp = Interpreter(model_path=str(ruta))
    interp.allocate_tensors()
    in_idx = interp.get_input_details()[0]["index"]
    out_idx = interp.get_output_details()[0]["index"]

    resultados = []
    for x in X:
        interp.set_tensor(in_idx, x.reshape(1, -1).astype(np.float32))
        interp.invoke()
        resultados.append(interp.get_tensor(out_idx)[0])
    return np.array(resultados, dtype=np.float32)
