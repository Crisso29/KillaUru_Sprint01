"""Pipeline de punta a punta con datos sintéticos (CI y pruebas e2e).

Recorre las etapas reales (entrenar → cuantizar → evaluar el `.tflite` → evidencia → puerta) con datos aleatorios y
semilla fija. No mide calidad: comprueba que las piezas encajan y que el resultado es reproducible. Sin benchmark,
acuerdo, equivalencia ni partición reales, la puerta **debe** rechazar: por eso `todo --sintetico` devuelve 2 y no
escribe manifiesto. Solo lo importa la etapa `todo`, así que cargar TensorFlow aquí no retrasa las demás.
"""
from __future__ import annotations

import pathlib
from datetime import UTC, datetime

import keras
import numpy as np

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.comun.huella import sha256_archivo
from killauru_ml.comun.semillas import fijar_semillas
from killauru_ml.evaluacion.metricas import calcular_metricas, severidad_mas_menos_1
from killauru_ml.evaluacion.registro import RegistroEntrenamiento, guardar_registro
from killauru_ml.modelo.cuantizar import cuantizar, inferir_tflite
from killauru_ml.modelo.entrenar import Conjunto, entrenar
from killauru_ml.modelo.etapa_puerta import APROBADA, ejecutar_puerta
from killauru_ml.modelo.puerta import EvidenciaPuerta, UmbralesPuerta

_SEMILLA = 2828
_TAMANOS = {"entrenamiento": 300, "validacion": 60, "prueba": 60}
_EPOCAS_CI, _LOTE_CI = 5, 32
_NO_APROBADA = 2


def ejecutar_todo_sintetico(version: str) -> int:
    """0 si la puerta aprueba; 2 si no (lo esperado con datos aleatorios)."""
    fijar_semillas(_SEMILLA)
    generador = np.random.default_rng(_SEMILLA)
    datos = {nombre: _conjunto(generador, cantidad) for nombre, cantidad in _TAMANOS.items()}
    print(f"[sintetico] entrenando {version} ({_EPOCAS_CI} épocas)…")
    modelo, historia = entrenar(datos["entrenamiento"], datos["validacion"], epocas=_EPOCAS_CI, lote=_LOTE_CI)
    carpeta = pathlib.Path("modelos") / version
    ruta_tflite = cuantizar(modelo, datos["entrenamiento"].X, carpeta / f"killauru-{version}.tflite")
    evidencia = _evidencia(version, ruta_tflite, datos["prueba"])
    evidencia.guardar(carpeta / "evidencia-puerta.json")
    guardar_registro(_registro(version, historia, evidencia), carpeta / "registro.json")
    return 0 if ejecutar_puerta(carpeta, UmbralesPuerta()) == APROBADA else _NO_APROBADA


def _conjunto(generador: np.random.Generator, cantidad: int) -> Conjunto:
    """Clases repartidas por igual; severidad 0 en Normal y 1–5 en anomalías. El orden de llamadas fija el resultado."""
    tipos = np.arange(cantidad) % len(CLASES)
    X = generador.random((cantidad, 20)).astype(np.float32)
    severidad = np.where(tipos == 0, 0, generador.integers(1, 6, cantidad)).astype(np.float32)
    return Conjunto(X, tipos, severidad)


def _evidencia(version: str, ruta_tflite: pathlib.Path, prueba: Conjunto) -> EvidenciaPuerta:
    """R-012: se evalúa el `.tflite` cuantizado, no el modelo de Python. Lo no medido queda vacío."""
    salidas = inferir_tflite(ruta_tflite, prueba.X)
    reales = [CLASES[indice] for indice in prueba.tipo]
    predichos = [CLASES[indice] for indice in salidas["tipo_logits"].argmax(axis=1)]
    metricas = calcular_metricas(reales, predichos, list(CLASES))
    return EvidenciaPuerta(
        version=version, dataset_version="sintetico", version_minima_app="1.0.0", ruta_tflite=str(ruta_tflite),
        temperatura=1.0, umbral_prefiltro=0.0, clases=list(CLASES),
        precision={clase: metricas[f"precision_{clase}"] for clase in CLASES},
        recall={clase: metricas[f"recall_{clase}"] for clase in CLASES},
        severidad_mas_menos_1=severidad_mas_menos_1(reales, predichos, list(prueba.severidad),
                                                    list(salidas["severidad"].ravel())),
        anio_medicion=datetime.now(UTC).year,
    )


def _registro(version: str, historia: keras.callbacks.History, evidencia: EvidenciaPuerta) -> RegistroEntrenamiento:
    """Registro inmutable del entrenamiento (FR-031): pérdidas finales, métricas por clase y huella del .tflite."""
    perdidas = historia.history
    return RegistroEntrenamiento.nueva(
        version=version, semilla=_SEMILLA, epocas_ejecutadas=len(perdidas["loss"]),
        perdida_entrenamiento=float(perdidas["loss"][-1]), perdida_validacion=float(perdidas["val_loss"][-1]),
        precision_por_clase=evidencia.precision, recall_por_clase=evidencia.recall,
        ruta_modelo_tflite=evidencia.ruta_tflite, sha256_tflite=sha256_archivo(pathlib.Path(evidencia.ruta_tflite)),
        version_dataset=evidencia.dataset_version,
    )
