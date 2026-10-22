# Implementation Plan: Dataset y modelo de detección

**Branch**: `009-dataset-y-modelo` | **Date**: 2026-10-22 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/009-dataset-y-modelo/spec.md`

## Summary

Tres piezas pequeñas y separadas. (1) Un **grabador** Android aparte (`ml/grabador/`, Kotlin, sin permiso de internet) que graba sensores a 50 Hz con los parámetros de la app, GPS a 1 Hz y, en un celular, video; además trae un modo *benchmark* para medir el `.tflite` (RNF-04). (2) Un **pipeline en Python** (`ml/killauru_ml/`) que ingiere y verifica grabaciones, propone candidatos para etiquetar, arma ventanas de 500 ms con el preprocesamiento `p1`, verifica la partición por recorrido, entrena un MLP (TensorFlow/Keras), calibra, cuantiza a LiteRT int8, comprueba la equivalencia y evalúa la **decisión del sistema** contra RNF-35. (3) Una **puerta** y un **workflow manual** de GitHub Actions que publica en R2 el `.tflite`, la matriz y el manifiesto solo si todo cumple. El dataset crudo vive en un *bucket* R2 privado; en git solo van el catálogo con SHA-256, las etiquetas, la partición, la configuración y los modelos versionados (pesan KB).

## Technical Context

**Language/Version**: Python 3.11 (pipeline) · Kotlin + Android SDK 35, `minSdk` 26 (grabador)

**Primary Dependencies**: TensorFlow 2.18 CPU (Keras 3, `TFLiteConverter`), `ai-edge-litert` (intérprete para la equivalencia), NumPy, pandas, scikit-learn (métricas, κ), PyYAML, jsonschema (manifiesto), matplotlib (figura de la matriz), pytest, ruff; versiones exactas en `ml/requirements.lock` (R-019). Grabador: `SensorManager`, `LocationManagerCompat`, CameraX, LiteRT (`com.google.ai.edge.litert`)

**Storage**: git (`ml/datos/catalogo.csv`, `etiquetas/`, `baches-patron.csv`, `config/`, `modelos/<versión>/`) · R2 privado `killauru-datos` (grabaciones crudas y video) · R2 público `cdn.killauru.pe/modelo/` (artefactos publicados) · caché local `ml/.cache/` (ignorada por git)

**Testing**: pytest con datos **sintéticos** generados con semilla (`ml/tests/sinteticos.py`): ventaneo, etiquetas, partición y no-fuga, características, métricas con valores límite, puerta, manifiesto, determinismo y equivalencia; `ml-ci.yml` en cada PR que toque `ml/`

**Target Platform**: computadora del autor (Windows/Linux, CPU) y GitHub Actions `ubuntu-24.04`; celulares Android de gama media

**Performance Goals**: inferencia p95 < 50 ms (RNF-04; un MLP de ~1 500 parámetros tarda < 1 ms); entrenamiento < 10 min en CPU

**Constraints**: archivos `.py` y `.kt` ≤ 100 líneas; sin notebooks en el pipeline; costo cero (R2 gratis, Actions gratis); ningún dato personal en el dataset derivado; el conjunto de prueba se usa una vez por candidata

**Scale/Scope**: ~10 recorridos × 3 celulares ≈ 30 grabaciones; ~30 h-celular; ~200 000 ventanas, de ellas ~2 000–4 000 de anomalía (R-020)

## Constitution Check

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Cada FR cita HU/RF/RN/RNF/ADR; lo que docs no fija es «Decisión SDD» en `research.md`; los umbrales de la puerta salen de RNF-35, RNF-04 y ADR-004 | ✅ |
| II. Monolito modular | No aplica: `ml/` no es código del backend ni agrega módulos o contenedores; no toca `backend/` | N/A |
| III. Calibrable | Todos los umbrales en `ml/config/*.yaml` con su fuente; `ConfidenceMinimo` = RN-04 leído de configuración, nunca en el modelo | ✅ |
| IV. Datos confiables | Grabaciones con SHA-256 en el catálogo; artefactos publicados inmutables; manifiesto al final | ✅ |
| V. Privacidad | Grabador sin internet; sin identificadores; km en lugar de coordenadas en el derivado; video privado con retención de 90 días (R-004) | ✅ |
| VI. Acceso y rastro | Publicación solo por workflow manual con secretos de GitHub; cada publicación queda en el historial de Actions | ✅ |
| VII. Pruebas primero | pytest antes del código, con valores límite (0,899/0,900; 199/200; 49,9/50 ms; 0.74/0.75) | ✅ (ver propuesta de enmienda) |
| VIII. Código pequeño | `.py` y `.kt` ≤ 100 líneas con prueba propia; funciones ≤ 20 líneas; nombres de negocio en español | ✅ (la prueba de 001 no cubre `ml/`: ver Complexity Tracking) |
| IX. API documentada | No hay API; el contrato es el esquema del manifiesto y la firma del modelo (`contracts/`) | N/A |
| X. Costo cero | CPU local, R2 dentro del plan gratis (< 10 GB), Actions gratis; sin servicios de ML pagados | ✅ |

## Project Structure

### Documentation

```text
specs/009-dataset-y-modelo/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/contratos-internos.md · contracts/manifiesto-modelo.schema.json · contracts/formato-grabacion.md
└── checklists/requirements.md · checklists/trazabilidad.md
```

### Source Code — árbol exacto (cada `.py` y `.kt` ≤ 100 líneas)

```text
.github/workflows/
├── ml-ci.yml                                  # PR que toca ml/: ruff, pytest, límite de 100 líneas, humo determinista
└── publicar-modelo.yml                        # manual (workflow_dispatch: version): puerta → R2 → verificación → manifiesto

ml/
├── README.md                                  # cómo grabar, etiquetar, entrenar y publicar
├── pyproject.toml                             # paquete killauru_ml, ruff, pytest
├── requirements.in · requirements.lock        # entorno fijado con hashes (pip-tools)
├── .gitignore                                 # .cache/, crudos/, *.mp4
├── protocolo/
│   ├── protocolo-campo.md                     # seguridad, montaje, recorridos, consentimiento, checklist de salida
│   ├── guia-etiquetado.md                     # RN-01 + RN-02 con criterios, fotos de referencia, zona gris
│   └── bitacora.md                            # controles de los días 7 y 11 y decisiones (R-020, R-021)
├── config/
│   ├── captura.yaml                           # 50 Hz, 500 ms, lote 1 s, RN-03, gravedad 2 s (contrato p1)
│   ├── etiquetado.yaml                        # umbral de candidatos, zona gris, acuerdo mínimo
│   ├── particion.yaml                         # recorridos por conjunto (congelado con el dataset)
│   ├── entrenamiento.yaml                     # semilla, capas, épocas, pesos de clase
│   └── puerta.yaml                            # RNF-35, RNF-04, ADR-004, equivalencia (con fuente)
├── datos/
│   ├── catalogo.csv                           # una fila por grabación: id, recorrido, celular, sha256, estado, motivo
│   ├── baches-patron.csv                      # km, sentido, tipo, profundidad, ancho, severidad
│   └── etiquetas/<grabacion>.csv              # una hoja por grabación
├── killauru_ml/
│   ├── __init__.py · __main__.py              # __main__: despacha etapas (argparse)
│   ├── comun/ config.py · semillas.py · huella.py · entorno.py · versiones.py
│   ├── lectura/ grabacion.py · integridad.py · catalogo.py · ingerir.py
│   ├── senal/ remuestreo.py · gravedad.py · componentes.py · kilometraje.py · condicion_velocidad.py
│   ├── etiquetado/ candidatos.py · validar_etiquetas.py · acuerdo.py
│   ├── ventanas/ ventaneo.py · etiqueta_ventana.py · dataset.py
│   ├── caracteristicas/ caracteristicas.py · prefiltro.py
│   ├── particion/ particion.py · no_fuga.py · tamano.py
│   ├── modelo/ red.py · entrenamiento.py · calibracion.py · linea_base.py
│   ├── evaluacion/ decision.py · metricas.py · matriz.py · informe.py · registro.py
│   ├── exportacion/ cuantizacion.py · equivalencia.py · vectores_prueba.py
│   ├── publicacion/ puerta.py · manifiesto.py · benchmark.py
│   └── seguimiento/ estado.py
├── tests/
│   ├── sinteticos.py                          # señales de bache, hundimiento, normal y rompemuelles con semilla
│   ├── test_limite_lineas.py                  # .py y .kt de ml/ ≤ 100 líneas
│   └── test_<módulo>.py                       # una prueba por módulo (lista en tasks.md)
├── scripts/
│   ├── subir_grabacion.sh                     # rclone → R2 privado
│   ├── borrar_videos.sh                       # video a los 90 días del congelamiento (R-004)
│   └── publicar_r2.sh                         # lo usa publicar-modelo.yml
├── modelos/
│   ├── registro-evaluaciones.csv              # cada uso del conjunto de prueba
│   └── <versión>/ killauru-<versión>.tflite · manifiesto.json · informe.json · informe.md
│                  · matriz-confusion.csv · matriz-confusion.json · vectores-prueba.json · benchmark/<alias>.json
└── grabador/                                  # APK aparte, nunca para conductores (R-001)
    ├── settings.gradle.kts · build.gradle.kts · gradle/libs.versions.toml
    └── app/
        ├── build.gradle.kts                   # applicationId pe.killauru.grabador; sin INTERNET
        ├── src/main/
        │   ├── AndroidManifest.xml            # FOREGROUND_SERVICE, ubicación, cámara; sin INTERNET
        │   └── java/pe/killauru/grabador/
        │       ├── MainActivity.kt · PantallaGrabador.kt   # iniciar/detener (estacionado), «Marca», metadatos
        │       ├── ServicioGrabacion.kt       # servicio en primer plano
        │       ├── SensoresGrabacion.kt       # 20 000 µs, maxReportLatencyUs 1 000 000 (= app)
        │       ├── GpsGrabacion.kt            # 1 Hz, LocationManagerCompat
        │       ├── CamaraGrabacion.kt         # solo en el celular de video (CameraX, 720p)
        │       ├── EscritorCsv.kt · Metadatos.kt
        │       └── BenchmarkModelo.kt · Percentiles.kt · PantallaBenchmark.kt  # RNF-04 con vectores-prueba.json
        └── src/test/java/pe/killauru/grabador/   # EscritorCsvTest · MetadatosTest · PercentilesTest (JVM)
```

El paso GPS → km vive en `killauru_ml/senal/kilometraje.py` y lee `backend/db/datos/corredor_pe28a.geojson` (D16).

**Structure Decision**: `ml/` en la raíz, al lado de `backend/` y de la futura `android/` de 008, porque no es código del backend ni de la app. El grabador es un proyecto Gradle **aparte** dentro de `ml/` (R-001): HU-28 empieza en la semana 1, antes que la app, y así el APK de producción nunca lleva código para grabar señal cruda. El pipeline es un paquete con una carpeta por etapa para que cada archivo quede bajo 100 líneas y cada etapa se pruebe sola.

## Dependencias con otras features

| Con | Qué | Dirección |
|---|---|---|
| 008-app-android | Modelo inicial en el APK, firma del modelo, preprocesamiento `p1`, vectores de prueba, manifiesto, `versionMinimaApp` | 009 → 008 (`contracts/contratos-internos.md`) |
| 008-app-android | Parámetros de captura (50 Hz, lote 1 s, RN-03) iguales en grabador y app | 008 ↔ 009 (si 008 los cambia, sube `preprocesamientoVersion`) |
| 004 | `db/datos/corredor_pe28a.geojson` para km y sentido | 004 → 009 (D16) |
| 001 | Convenciones de GitHub Actions y secretos (D10, RNF-34) | 001 → 009 |

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| Un segundo proyecto Android (grabador) | HU-28 empieza en la semana 1, antes de la app; el APK de producción no debe grabar señal cruda ni video (R-06) | Un modo «grabación» en 008 obliga a esperar la app y a meter código sensible en el APK de los conductores (R-001) |
| Prueba de 100 líneas propia en `ml/` | La prueba de 001 solo recorre `backend/` (`.cs`, `.sql`, `.sh`, `.js`) | Ampliar la prueba de 001 toca otra feature; se propone la enmienda en `research.md` |
| Preprocesamiento implementado dos veces (Python y Kotlin) | El modelo recibe características, no señal cruda (R-010) | Meter el cálculo en el grafo TFLite complica la cuantización int8 de curtosis y razones; los vectores de prueba (FR-025) cierran la brecha |
