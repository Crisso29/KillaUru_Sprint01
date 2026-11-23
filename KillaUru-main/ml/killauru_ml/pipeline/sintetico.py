"""Pipeline de punta a punta con datos sintéticos (CI y pruebas e2e).

Recorre las mismas etapas que el pipeline real (entrenar → cuantizar → métricas → manifiesto → registro → puerta) con
datos aleatorios y semilla fija. No mide calidad: comprueba que las piezas encajan. Solo lo importa la etapa `todo`
de la CLI, así que cargar TensorFlow aquí no retrasa las demás etapas.
"""
from __future__ import annotations

import json
import pathlib
from dataclasses import dataclass

import keras
import numpy as np

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.comun.huella import sha256_archivo
from killauru_ml.comun.semillas import fijar_semillas
from killauru_ml.evaluacion.metricas import calcular_metricas
from killauru_ml.evaluacion.registro import RegistroEntrenamiento, guardar_registro
from killauru_ml.modelo.cuantizar import cuantizar
from killauru_ml.modelo.entrenar import entrenar
from killauru_ml.modelo.manifiesto import generar_manifiesto, guardar_manifiesto
from killauru_ml.modelo.puerta import verificar_puerta_modelo

_SEMILLA = 2828
_N_TRAIN, _N_VAL, _N_TEST = 300, 60, 60
_N_FEATURES = 20
_EPOCAS_CI = 5
_LOTE_CI = 32
_MUESTRAS_CALIBRACION = 20


@dataclass(frozen=True)
class _DatosSinteticos:
    X_train: np.ndarray
    y_train: np.ndarray
    X_val: np.ndarray
    y_val: np.ndarray
    X_test: np.ndarray
    etiquetas_test: list[str]


def ejecutar_todo_sintetico(version: str) -> int:
    """Entrena, cuantiza y verifica un modelo con datos sintéticos.

    Retorna 0 si la puerta se aprueba, 2 si no (lo esperado con datos aleatorios).
    """
    fijar_semillas(_SEMILLA)
    datos = _datos(np.random.default_rng(_SEMILLA))
    print(f"[sintetico] entrenando {version} ({_EPOCAS_CI} épocas)…")
    modelo, historia = entrenar(datos.X_train, datos.y_train, datos.X_val, datos.y_val, epocas=_EPOCAS_CI, lote=_LOTE_CI)
    carpeta = pathlib.Path("modelos") / version
    ruta_tflite = cuantizar(modelo, datos.X_train[:_MUESTRAS_CALIBRACION], carpeta / f"killauru-{version}.tflite")
    metricas = _metricas(modelo, datos)
    print(json.dumps(metricas, indent=2))
    huella = sha256_archivo(ruta_tflite)
    guardar_manifiesto(generar_manifiesto(version, huella, list(CLASES)), carpeta / "manifiesto.json")
    guardar_registro(_registro(version, historia, metricas, ruta_tflite, huella), carpeta / "registro.json")
    resultado = verificar_puerta_modelo(ruta_tflite, metricas)
    print(resultado.resumen())
    return 0 if resultado.aprobado else 2


def _datos(generador: np.random.Generator) -> _DatosSinteticos:
    """Features aleatorias y clases repartidas por igual (0, 1, 2, 0, …). El orden de las llamadas fija el resultado."""
    def clases(cantidad: int) -> np.ndarray:
        return np.array([indice % len(CLASES) for indice in range(cantidad)])

    def features(cantidad: int) -> np.ndarray:
        return generador.random((cantidad, _N_FEATURES)).astype(np.float32)

    X_train, y_train = features(_N_TRAIN), clases(_N_TRAIN)
    X_val, y_val = features(_N_VAL), clases(_N_VAL)
    X_test = features(_N_TEST)
    return _DatosSinteticos(X_train, y_train, X_val, y_val, X_test, [CLASES[clase] for clase in clases(_N_TEST)])


def _metricas(modelo: keras.Sequential, datos: _DatosSinteticos) -> dict:
    probabilidades = modelo.predict(datos.X_test, verbose=0)
    predichas = [CLASES[int(indice)] for indice in probabilidades.argmax(axis=1)]
    return calcular_metricas(datos.etiquetas_test, predichas, list(CLASES))


def _registro(version: str, historia: keras.callbacks.History, metricas: dict, ruta_tflite: pathlib.Path,
              huella: str) -> RegistroEntrenamiento:
    """Registro inmutable del entrenamiento (FR-031): pérdidas finales, métricas por clase y huella del .tflite."""
    perdidas = historia.history
    return RegistroEntrenamiento.nueva(
        version=version,
        semilla=_SEMILLA,
        epocas_ejecutadas=len(perdidas["loss"]),
        perdida_entrenamiento=float(perdidas["loss"][-1]),
        perdida_validacion=float(perdidas["val_loss"][-1]),
        precision_por_clase={f"p_{clase}": metricas[f"precision_{clase}"] for clase in CLASES},
        recall_por_clase={f"r_{clase}": metricas[f"recall_{clase}"] for clase in CLASES},
        ruta_modelo_tflite=str(ruta_tflite),
        sha256_tflite=huella,
        version_dataset="sintetico",
    )
