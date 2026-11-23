# Pruebas 009 — Dataset y modelo

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Tipo | Ubicación |
|---|---|
| Unitarias: señal, ventanas y reglas | [unitarias](../../ml/tests/unitarias/) |
| Integración: archivos, entrenamiento y conversión | [integracion](../../ml/tests/integracion/) |
| E2E del pipeline sintético | [e2e](../../ml/tests/e2e/) |
| Límites de código | [arquitectura](../../ml/tests/arquitectura/) |
| Preparación reutilizable | [sinteticos.py](../../ml/tests/sinteticos.py) |
| Captura y evaluación real | [Protocolo de campo](../../ml/protocolo/protocolo-campo.md) |

## Cómo ejecutar

Desde `KillaUru-main/ml/`, con Python 3.11 y las dependencias fijadas instaladas:

```powershell
python -m pytest tests/unitarias
python -m pytest tests/integracion
python -m pytest tests/e2e
python -m pytest tests/arquitectura
```

`python -m pytest` descubre todas las categorías.
La separación de particiones evita contaminación de la evaluación;
no equivale a una auditoría de ciberseguridad.

## Último resultado (2026-11-23)

**174 aprobadas, 0 fallidas, 0 omitidas** en 28 archivos: unitarias 120, integración 49, e2e 3, arquitectura 2.
Cobertura de `killauru_ml` **97 %** (líneas y ramas; CI exige 90 %). ruff sin hallazgos. Dos corridas de
`scripts/verificar_reproducibilidad.py` dan el mismo `.tflite` byte a byte. Detalle, defectos encontrados y auditoría de tareas en
[evidencias 2026-11-23-ml](../../docs/seguimiento/evidencias/pruebas/2026-11-23-ml/README.md).

Con cobertura: `python -m pytest --cov=killauru_ml --cov-branch`.

## Qué falta acreditar

La campaña de campo, el benchmark en dispositivos y la publicación final siguen sujetos
a los criterios de aceptación de la spec. Datos sintéticos y archivos TFLite generados
no prueban precisión real ni latencia en teléfonos. Carga HTTP no aplica a este pipeline;
el tiempo de entrenamiento y de inferencia se miden con sus propios escenarios.

## Separación de alcances del 21 de noviembre de 2026

La regla aislada conserva [test_no_fuga.py](../../ml/tests/unitarias/test_no_fuga.py); la colaboración entre
particionador y verificador está en [test_particion_sin_fuga.py](../../ml/tests/integracion/test_particion_sin_fuga.py).
No se eliminó el escenario ni se duplicó su ejecución.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.

<details>
<summary>Arquitectura (2 archivos)</summary>

- [test_funciones_cortas.py](../../ml/tests/arquitectura/test_funciones_cortas.py) — ninguna función de más de 20 líneas
- [test_limite_lineas.py](../../ml/tests/arquitectura/test_limite_lineas.py)

</details>

<details>
<summary>E2E sintético (1 archivo)</summary>

- [test_pipeline_sintetico.py](../../ml/tests/e2e/test_pipeline_sintetico.py)

</details>

<details>
<summary>Integración (11 archivos)</summary>

- [test_catalogo.py](../../ml/tests/integracion/test_catalogo.py) — orden de clases = APK = contrato de la CDN
- [test_cli.py](../../ml/tests/integracion/test_cli.py) — etapas de la línea de órdenes y sus códigos de salida
- [test_comun.py](../../ml/tests/integracion/test_comun.py)
- [test_cuantizar.py](../../ml/tests/integracion/test_cuantizar.py)
- [test_entrenar.py](../../ml/tests/integracion/test_entrenar.py)
- [test_estado.py](../../ml/tests/integracion/test_estado.py)
- [test_etapa_puerta.py](../../ml/tests/integracion/test_etapa_puerta.py) — `puerta --version`: 0 y manifiesto si aprueba; 1 y nada si no
- [test_grabacion.py](../../ml/tests/integracion/test_grabacion.py)
- [test_ingerir.py](../../ml/tests/integracion/test_ingerir.py) — catálogo, huella sin video y grabación ilegible
- [test_manifiesto.py](../../ml/tests/integracion/test_manifiesto.py)
- [test_particion_sin_fuga.py](../../ml/tests/integracion/test_particion_sin_fuga.py)

</details>

<details>
<summary>Unitarias (14 archivos)</summary>

- [test_acuerdo.py](../../ml/tests/unitarias/test_acuerdo.py)
- [test_candidatos.py](../../ml/tests/unitarias/test_candidatos.py)
- [test_features.py](../../ml/tests/unitarias/test_features.py)
- [test_integridad.py](../../ml/tests/unitarias/test_integridad.py)
- [test_metricas.py](../../ml/tests/unitarias/test_metricas.py)
- [test_modelo.py](../../ml/tests/unitarias/test_modelo.py)
- [test_no_fuga.py](../../ml/tests/unitarias/test_no_fuga.py)
- [test_particion.py](../../ml/tests/unitarias/test_particion.py)
- [test_prefiltro.py](../../ml/tests/unitarias/test_prefiltro.py)
- [test_puerta_completa.py](../../ml/tests/unitarias/test_puerta_completa.py)
- [test_senal.py](../../ml/tests/unitarias/test_senal.py)
- [test_tamano.py](../../ml/tests/unitarias/test_tamano.py) — FR-014: mínimos por clase en cada conjunto, ≥ 3 celulares y ambos sentidos en prueba
- [test_validar_etiquetas.py](../../ml/tests/unitarias/test_validar_etiquetas.py)
- [test_ventaneo.py](../../ml/tests/unitarias/test_ventaneo.py)

</details>
