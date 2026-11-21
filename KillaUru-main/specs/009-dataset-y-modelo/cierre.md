# Cierre de spec 009 — Dataset y modelo

**Revisado:** 2026-11-15. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 46/74 tareas marcadas; 28 pendientes en tasks.md.
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Pipeline independiente de campo; entrega artefactos y manifiesto a 008.

## Evidencia disponible

Funciones de señal, partición, entrenamiento, conversión y pipeline sintético con tests. README creado; T072 documental cerrada. Hay 128 casos descubiertos, con verificación y corrección local documentadas.

Tareas documentales cerradas en esta revisión: T072. Eso no certifica ejecución funcional.
## Qué falta localmente

El CLI solo despacha parte de las etapas. todo sin --sintetico y puerta devuelven 2; los subcomandos declarados no equivalen a implementación completa. Faltan calibración/decisión/informes, equivalencia/vectores/benchmark/versionado, integración del CLI y publicación.

## Qué falta en el entorno desplegado

Campaña y etiquetado; evaluación sin fuga sobre datos reales; tres teléfonos; modelo aprobado; simulacro R2/rollback/inmutabilidad; paquete para APK. No saltar puerta ni publicar sintéticos como producción.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T004 | Local | `ml-ci.yml` (PR que toca `ml/**`): instala el *lock*, `ruff check`, `pytest`, límite de 200 líneas y corrida de humo `todo` con el dataset sintético dos veces comparando la huella de pesos |
| T019 | Local | `Metadatos.kt`, `MainActivity.kt`, `PantallaGrabador.kt`: iniciar y detener solo con velocidad GPS < 1 km/h; botón «Marca» grande para el copiloto |
| T021 | Desplegado/campo | Piloto de 20 min con 3 celulares → `ingerir` → filas `Apta`; ajustar el protocolo si algo falla |
| T022 | Desplegado/campo | Campaña: ≥ 10 recorridos en ambos sentidos con 3 celulares a la vez y ≥ 20 baches patrón medidos |
| T023 | Desplegado/campo | Borrado de video a los 90 días del congelamiento (lista de grabaciones con video y fecha límite; lo corre el autor) |
| T032 | Desplegado/campo | Etiquetar la campaña y el 10 % doble; `acuerdo` en verde |
| T040 | Local | `test_decision.py`: confidence 0.74 → Normal, 0.75 y 0.76 → anomalía; temperatura aplicada antes del softmax; `ConfidenceMinimo` leído de `puerta.yaml` |
| T043 | Local | `test_calibracion.py`: la temperatura baja la NLL de validación en un modelo sintético sobreconfiado y no lee prueba; `test_linea_base.py`: la línea base de umbrales clasifica las señales sintéticas |
| T046 | Local | `calibracion.py` y `linea_base.py` |
| T047 | Local | `decision.py`, `metricas.py`, `matriz.py` (incluye por celular, «baches no vistos», balanceado y `argmax` sin umbral) |
| T048 | Local | `informe.py`: `informe.json`, `informe.md` y matriz CSV/JSON con commit, `datasetVersion`, huella, semilla y entorno |
| T051 | Local | `test_equivalencia.py`: tipo igual 98,9 % falla y 99 % pasa; Δ confidence p99 0.051 falla; un modelo con un peso alterado falla |
| T052 | Local | `test_vectores_prueba.py`: ≥ 60 casos, 20 por clase, con casos cerca del umbral del prefiltro y de 0.75; recomputar características y salidas con el pipeline da lo guardado |
| T053 | Local | `test_benchmark.py`: p95 49,9 ms pasa y 50,0 no; celular de 5 GB de RAM o de hace 5 años no cuenta como gama media; menos de 3 celulares válidos → falla |
| T054 | Local | `PercentilesTest` (JVM): p50/p95/máx de una lista conocida |
| T056 | Local | `equivalencia.py` (intérprete `ai-edge-litert`) y `vectores_prueba.py` |
| T057 | Local | `BenchmarkModelo.kt` (un intérprete, 1 hilo, 100 + 1 000), `Percentiles.kt` y `PantallaBenchmark.kt` (escribe el JSON) |
| T058 | Local | `benchmark.py`: valida los JSON de `benchmark/` (gama media, ≥ 3 celulares, p95) |
| T059 | Desplegado/campo | Medir la candidata en ≥ 3 celulares de gama media |
| T062 | Local | `test_versiones.py`: cambia el catálogo → MAYOR; cambia `preprocesamientoVersion` → MENOR y sube `versionMinimaApp`; mismo contrato → PARCHE; `ds-<n>` y su huella |
| T063 | Local | `versiones.py` |
| T065 | Local | `publicar_r2.sh`: falla si la ruta de la versión existe; sube `.tflite`, matriz e informe con `immutable`; verifica con `curl` + `sha256sum` desde `cdn.killauru.pe`; sube el manifiesto al final; modo `--volver-atras <versión>` |
| T066 | Local | `publicar-modelo.yml` (`workflow_dispatch`: `version`, `volver_atras`; entorno `modelos`; secretos `KU_R2_MODELOS_ACCESS_KEY_ID`, `KU_R2_MODELOS_SECRET_ACCESS_KEY`, `KU_R2_ENDPOINT`): *lock* → `puerta` → `publicar_r2.sh` |
| T067 | Desplegado/campo | Simulacro en el prefijo `modelo-prueba/`: publicar una versión sintética, publicar otra y volver atrás; comprobar que los archivos de la primera no cambiaron |
| T068 | Desplegado/campo | Aprobar `1.0.0` y dejar `killauru-1.0.0.tflite` + `manifiesto.json` + `vectores-prueba.json` listos para el APK de 008; publicar en R2 con el workflow (si RF-33 se recorta, la publicación puede esperar) |
| T071 | Desplegado/campo | Bitácora con los controles del día 7 (conteos) y del día 11 (validación ≥ 0,85) y la decisión tomada (seguir, pasadas dirigidas o plan B) |
| T073 | Desplegado/campo | Correr el quickstart de punta a punta con el dataset sintético y luego con `ds-1` |
| T074 | Local | Marcar la trazabilidad y llevar a docs lo que el autor confirme (D1, `research.md` «Para llevar a docs») |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
