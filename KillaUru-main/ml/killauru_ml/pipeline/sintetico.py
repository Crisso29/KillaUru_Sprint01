"""Pipeline end-to-end con datos sintéticos (CI y pruebas de integración)."""
from __future__ import annotations

import json
import pathlib

import numpy as np


_CLASES = ["Normal", "Bache", "Hundimiento"]
_N_TRAIN, _N_VAL, _N_TEST = 300, 60, 60
_N_FEATURES = 20
_EPOCAS_CI = 5
_LOTE_CI = 32


def ejecutar_todo_sintetico(version: str) -> int:
    """Entrena, cuantiza y verifica un modelo con datos completamente sintéticos.

    Retorna 0 si la puerta se aprueba, 2 si no (esperado con datos aleatorios).
    """
    from killauru_ml.comun.semillas import fijar_semillas
    from killauru_ml.comun.huella import sha256_archivo
    from killauru_ml.modelo.entrenar import entrenar
    from killauru_ml.modelo.cuantizar import cuantizar
    from killauru_ml.modelo.puerta import verificar_puerta_modelo
    from killauru_ml.modelo.manifiesto import generar_manifiesto, guardar_manifiesto
    from killauru_ml.evaluacion.metricas import calcular_metricas
    from killauru_ml.evaluacion.registro import RegistroEntrenamiento, guardar_registro

    fijar_semillas(2828)
    rng = np.random.default_rng(2828)

    X_tr = rng.random((_N_TRAIN, _N_FEATURES)).astype(np.float32)
    y_tr = np.array([i % 3 for i in range(_N_TRAIN)])
    X_val = rng.random((_N_VAL, _N_FEATURES)).astype(np.float32)
    y_val = np.array([i % 3 for i in range(_N_VAL)])
    X_test = rng.random((_N_TEST, _N_FEATURES)).astype(np.float32)
    y_test_lbl = [_CLASES[i % 3] for i in range(_N_TEST)]

    print(f"[sintetico] entrenando {version} ({_EPOCAS_CI} épocas)…")
    modelo, historia = entrenar(X_tr, y_tr, X_val, y_val, epocas=_EPOCAS_CI, lote=_LOTE_CI)

    ruta_base = pathlib.Path("modelos") / version
    ruta_base.mkdir(parents=True, exist_ok=True)
    ruta_tflite = ruta_base / f"killauru-{version}.tflite"
    cuantizar(modelo, X_tr[:20], ruta_tflite)

    pred_arr = modelo.predict(X_test, verbose=0)
    pred_lbl = [_CLASES[int(i)] for i in pred_arr.argmax(axis=1)]
    metricas = calcular_metricas(y_test_lbl, pred_lbl, _CLASES)
    print(json.dumps(metricas, indent=2))

    sha = sha256_archivo(ruta_tflite)
    manifiesto = generar_manifiesto(version, sha, _CLASES)
    guardar_manifiesto(manifiesto, ruta_base / "manifiesto.json")

    n_ep = len(historia.history["loss"])
    registro = RegistroEntrenamiento.nueva(
        version=version,
        semilla=2828,
        epocas_ejecutadas=n_ep,
        perdida_entrenamiento=float(historia.history["loss"][-1]),
        perdida_validacion=float(historia.history["val_loss"][-1]),
        precision_por_clase={f"p_{c}": metricas[f"precision_{c}"] for c in _CLASES},
        recall_por_clase={f"r_{c}": metricas[f"recall_{c}"] for c in _CLASES},
        ruta_modelo_tflite=str(ruta_tflite),
        sha256_tflite=sha,
        version_dataset="sintetico",
    )
    guardar_registro(registro, ruta_base / "registro.json")

    resultado = verificar_puerta_modelo(ruta_tflite, metricas)
    print(resultado.resumen())
    return 0 if resultado.aprobado else 2
