"""FR-028, data-model §7 — etapa `puerta`: Candidata → Aprobada (escribe el manifiesto) o Rechazada.

La comparten la CLI (`puerta --version X`) y `todo`: lee `modelos/<versión>/evidencia-puerta.json`, aplica las 7
condiciones y solo si todas pasan escribe `manifiesto.json`. Código 0 = aprobada, 1 = rechazada.
"""
from __future__ import annotations

import pathlib

from killauru_ml.modelo.manifiesto import DatosManifiesto, generar_manifiesto, guardar_manifiesto
from killauru_ml.modelo.puerta import (
    EvidenciaPuerta,
    UmbralesPuerta,
    celulares_validos,
    verificar_puerta,
)

APROBADA, RECHAZADA = 0, 1


def ejecutar_puerta(carpeta_version: pathlib.Path, umbrales: UmbralesPuerta) -> int:
    evidencia = EvidenciaPuerta.leer(carpeta_version / "evidencia-puerta.json")
    resultado = verificar_puerta(evidencia, umbrales)
    print(f"Puerta {evidencia.version}: {resultado.resumen()}")
    if not resultado.aprobado:
        return RECHAZADA
    manifiesto = generar_manifiesto(_datos_manifiesto(evidencia, umbrales), pathlib.Path(evidencia.ruta_tflite))
    guardar_manifiesto(manifiesto, carpeta_version / "manifiesto.json")
    return APROBADA


def _datos_manifiesto(evidencia: EvidenciaPuerta, umbrales: UmbralesPuerta) -> DatosManifiesto:
    """El manifiesto publica la latencia de los celulares que cuentan para RNF-04."""
    latencias = {medicion.alias: medicion.latencia_p95_ms for medicion in celulares_validos(evidencia, umbrales)}
    return DatosManifiesto(
        version=evidencia.version, dataset_version=evidencia.dataset_version,
        version_minima_app=evidencia.version_minima_app, temperatura=evidencia.temperatura,
        umbral_prefiltro=evidencia.umbral_prefiltro, precision=evidencia.precision, recall=evidencia.recall,
        severidad_mas_menos_1=evidencia.severidad_mas_menos_1, latencias_p95_ms=latencias,
    )
