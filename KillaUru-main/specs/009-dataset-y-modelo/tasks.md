---

description: "Tareas del dataset y el modelo de detección (HU-28)"
---

# Tasks: Dataset y modelo de detección

**Input**: `specs/009-dataset-y-modelo/` · **Requiere**: nada del backend para empezar (semana 1, HU-28); la polilínea de 004 (`backend/db/datos/corredor_pe28a.geojson`, D16) para el kilometraje; el grabador no depende de 008.

**Tests**: obligatorias y **primero** (constitución VII): cada tarea de prueba debe fallar antes de su implementación. Valores límite en todas las reglas con número (0,899/0,900; 199/200; 49,9/50,0 ms; 0.74/0.75/0.76; 44,9/45 Hz; 249/250 ms).

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a la raíz del repositorio. Paquete: `ml/killauru_ml/` (`KM/`). Pruebas: `ml/tests/` (`MT/`). Grabador: `ml/grabador/app/src/` (`GR/`, paquete `pe/killauru/grabador/`). **Ningún `.py` ni `.kt` > 100 líneas.**

## Phase 1: Setup

- [ ] T001 `pyproject.toml` (paquete `killauru_ml`, ruff, pytest), `requirements.in` (Python 3.11, TensorFlow 2.18 CPU, `ai-edge-litert`, NumPy, pandas, scikit-learn, PyYAML, matplotlib, jsonschema, pytest, ruff) y `requirements.lock` con `pip-compile --generate-hashes`; `.gitignore` (`.cache/`, `crudos/`, `*.mp4`) — `ml/pyproject.toml`, `ml/requirements.in`, `ml/requirements.lock`, `ml/.gitignore` · (FR-016; R-019)
- [ ] T002 [P] Configuración con la fuente de cada valor en comentario: `captura.yaml` (RF-01, RF-02, RN-03, `diseño-interno` §7.4), `etiquetado.yaml`, `entrenamiento.yaml` (semilla 2828), `puerta.yaml` (`data-model.md` §9) — `ml/config/` · (FR-012, FR-017, FR-028; constitución III)
- [ ] T003 [P] `test_limite_lineas.py`: recorre `ml/**/*.py`, `ml/**/*.kt` y `ml/**/*.kts`; falla con ruta y conteo si alguno pasa de 100 líneas — `ml/tests/test_limite_lineas.py` · (FR-033)
- [ ] T004 [P] `ml-ci.yml` (PR que toca `ml/**`): instala el *lock*, `ruff check`, `pytest`, límite de 100 líneas y corrida de humo `todo` con el dataset sintético dos veces comparando la huella de pesos — `.github/workflows/ml-ci.yml` · (FR-016, FR-033; D10)
- [ ] T005 [P] `test_comun.py`: lee YAML y falla si falta una clave; misma semilla → mismos números; SHA-256 de un archivo conocido; `entorno` devuelve versiones de Python y TensorFlow — `MT/test_comun.py` · (FR-016)
- [ ] T006 `comun/config.py`, `semillas.py` (Python, NumPy, `keras.utils.set_random_seed`, `enable_op_determinism`), `huella.py`, `entorno.py` — `KM/comun/` · (FR-016; R-019)
- [ ] T007 `sinteticos.py`: generador con semilla de grabaciones falsas (bache con rebote, hundimiento sin rebote, rompemuelles, pista lisa, celular inclinado, huecos y GPS) en el formato de `contracts/formato-grabacion.md` — `MT/sinteticos.py` · (FR-003, FR-016)

## Phase 2: Foundational — lectura, integridad y señal

### Pruebas (primero; deben fallar)

- [ ] T008 [P] `test_grabacion.py`: lee una carpeta sintética; `categoria = Pesado` → error (RN-18); `meta.json` con un campo no previsto (`imei`, `placa`, `nombre`) → error — `MT/test_grabacion.py` · (FR-003, FR-004, FR-005, FR-006)
- [ ] T009 [P] `test_integridad.py`: 44,9 Hz → `NoApta(FrecuenciaBaja)`, 45,0 → `Apta`; huecos 1,00 % pasa y 1,01 % no; hueco de 200 ms no cuenta y 201 ms sí; GPS con precisión > 20 m en > 20 % → `SinGps` — `MT/test_integridad.py` · (FR-007)
- [ ] T010 [P] `test_senal.py`: remuestreo lineal exacto en una rampa; gravedad = media de 100 muestras; `a_v` igual con el celular vertical o inclinado 30°; giro > 30° entre ventanas → `SoporteInestable`; RN-03 con 4,9/5,0/5,1 km/h y 9/10 s; GPS → km y sentido sobre una polilínea sintética — `MT/test_senal.py` · (FR-012, FR-017)

### Implementación

- [ ] T011 [P] `grabacion.py` e `integridad.py` — `KM/lectura/` · (FR-003, FR-004, FR-005, FR-007)
- [ ] T012 [P] `remuestreo.py`, `gravedad.py`, `componentes.py`, `condicion_velocidad.py`, `kilometraje.py` (lee `backend/db/datos/corredor_pe28a.geojson`) — `KM/senal/` · (FR-012, FR-017; D16)
- [ ] T013 `catalogo.py` e `ingerir.py` (verifica, calcula SHA-256, escribe la fila, sube) + `subir_grabacion.sh` (rclone al *bucket* privado) + encabezado de `catalogo.csv` — `KM/lectura/`, `ml/scripts/subir_grabacion.sh`, `ml/datos/catalogo.csv` · (FR-005, FR-007)
- [ ] T014 `__main__.py`: despacha `ingerir`, `candidatos`, `validar-etiquetas`, `acuerdo`, `ventanas`, `particion`, `entrenar`, `evaluar`, `cuantizar`, `puerta`, `estado`, `todo` — `KM/__main__.py` · (FR-016)

**Checkpoint**: `pytest ml/tests` en verde; una grabación sintética entra al catálogo.

---

## Phase 3: User Story 1 — Grabar recorridos sin riesgo (P1) 🎯 semana 1

### Pruebas

- [ ] T015 [P] [US1] `EscritorCsvTest` y `MetadatosTest` (JVM): encabezados y unidades de `formato-grabacion.md`; *flush* por bloque; `meta.json` sin identificadores del aparato; `categoria` solo `Liviano` — `GR/test/java/pe/killauru/grabador/` · (FR-003, FR-004, FR-005)

### Implementación

- [ ] T016 [US1] Protocolo de campo: seguridad (R-003: iniciar y detener estacionado, solo el copiloto marca, golpe de sincronización, velocidad legal, medición de patrones fuera de la calzada), montaje de 3 soportes, checklist de salida, plan de ~10 recorridos (R-020), consentimiento y privacidad (R-004) — `ml/protocolo/protocolo-campo.md` · (FR-002, FR-005, FR-006)
- [ ] T017 [US1] Proyecto Gradle del grabador: `applicationId pe.killauru.grabador`, `minSdk 26`, **sin** `INTERNET`; permisos de servicio en primer plano, ubicación y cámara — `ml/grabador/settings.gradle.kts`, `ml/grabador/build.gradle.kts`, `ml/grabador/gradle/libs.versions.toml`, `ml/grabador/app/build.gradle.kts`, `GR/main/AndroidManifest.xml` · (FR-001, FR-005; R-001)
- [ ] T018 [US1] `SensoresGrabacion.kt` (20 000 µs, `maxReportLatencyUs` 1 000 000), `GpsGrabacion.kt` (1 Hz, `LocationManagerCompat`), `EscritorCsv.kt`, `ServicioGrabacion.kt` — `GR/main/java/pe/killauru/grabador/` · (FR-001, FR-003; R-002)
- [ ] T019 [US1] `Metadatos.kt`, `MainActivity.kt`, `PantallaGrabador.kt`: iniciar y detener solo con velocidad GPS < 1 km/h; botón «Marca» grande para el copiloto — `GR/main/java/pe/killauru/grabador/` · (FR-002, FR-004)
- [ ] T020 [US1] `CamaraGrabacion.kt` (CameraX 720p, hora del primer cuadro en `elapsedRealtimeNanos`) solo si `esCelularVideo` — `GR/main/java/pe/killauru/grabador/CamaraGrabacion.kt` · (FR-003)
- [ ] T021 [US1] Piloto de 20 min con 3 celulares → `ingerir` → filas `Apta`; ajustar el protocolo si algo falla — `ml/datos/catalogo.csv` · (FR-006, FR-007)
- [ ] T022 [US1] Campaña: ≥ 10 recorridos en ambos sentidos con 3 celulares a la vez y ≥ 20 baches patrón medidos — `ml/datos/catalogo.csv`, `ml/datos/baches-patron.csv` · (FR-006, FR-010; R-006, R-020)
- [ ] T023 [US1] Borrado de video a los 90 días del congelamiento (lista de grabaciones con video y fecha límite; lo corre el autor) — `ml/scripts/borrar_videos.sh` · (FR-005; R-004)

**Checkpoint**: ≥ 10 recorridos `Apta` en el catálogo, con SHA-256, en R2 privado.

---

## Phase 4: User Story 2 — Etiquetar con criterios repetibles (P1)

### Pruebas

- [ ] T024 [P] [US2] `test_candidatos.py`: ninguna pasada sintética sobre un patrón queda fuera; cada fila trae hora, km, velocidad y segundo del video — `MT/test_candidatos.py` · (FR-008)
- [ ] T025 [P] [US2] `test_validar_etiquetas.py`: Normal con severidad → error; Bache sin severidad → error; severidad 0 y 6 → error, 1 y 5 pasan; subtipo con Bache → error; tipo `Rompemuelles` → error (es subtipo); patrón con otra severidad → error — `MT/test_validar_etiquetas.py` · (FR-009, FR-010)
- [ ] T026 [P] [US2] `test_acuerdo.py`: emparejamiento a ± 250 ms; κ 0.79 falla y 0.80 pasa; severidad ± 1 en 89 % falla y 90 % pasa — `MT/test_acuerdo.py` · (FR-011)
- [ ] T027 [P] [US2] `test_ventaneo.py`: 25 muestras sin solapamiento; el pico en la última muestra pertenece a esa ventana; vecina a 249 ms = zona gris y a 250 ms no; dos eventos → mayor severidad (empate → Bache); fuera de RN-03 o con hueco → excluida; la salida no tiene latitud ni longitud; aumento ± 200 ms solo en entrenamiento — `MT/test_ventaneo.py` · (FR-005, FR-012)

### Implementación

- [ ] T028 [US2] Guía de etiquetado: RN-01 con firmas, subtipos de Normal (RS-07), tabla de severidad de R-006 con fotos de los patrones, zona gris, doble etiquetado — `ml/protocolo/guia-etiquetado.md` · (FR-009, FR-010, FR-011)
- [ ] T029 [US2] `candidatos.py` — `KM/etiquetado/candidatos.py` · (FR-008)
- [ ] T030 [US2] `validar_etiquetas.py` y `acuerdo.py` (κ con scikit-learn) — `KM/etiquetado/` · (FR-009, FR-010, FR-011)
- [ ] T031 [US2] `ventaneo.py`, `etiqueta_ventana.py`, `dataset.py` (escribe `ventanas-<ds>.npz` en la caché) — `KM/ventanas/` · (FR-012)
- [ ] T032 [US2] Etiquetar la campaña y el 10 % doble; `acuerdo` en verde — `ml/datos/etiquetas/`, `ml/datos/etiquetas/doble/` · (FR-009, FR-010, FR-011)

---

## Phase 5: User Story 3 — Partir sin fuga (P1)

### Pruebas

- [ ] T033 [P] [US3] `test_particion.py`: recorrido en dos conjuntos → error; recorrido del catálogo sin conjunto → error; 199 ventanas de Hundimiento en prueba → error y 200 pasa; 2 celulares en prueba → error; un solo sentido → error; validación 99/100; entrenamiento 399/400; `celularNoVisto` solo en prueba; cambiar una partición congelada cambia la huella — `MT/test_particion.py` · (FR-013, FR-014)
- [ ] T034 [P] [US3] `test_no_fuga.py`: pares (recorrido, celular) disjuntos; la normalización, el prefiltro y la temperatura ajustados con datos de prueba se detectan y fallan; ninguna huella de ventana de prueba en entrenamiento — `MT/test_no_fuga.py` · (FR-013, FR-015)

### Implementación

- [ ] T035 [US3] `particion.py`, `tamano.py`, `no_fuga.py` — `KM/particion/` · (FR-013, FR-014, FR-015)
- [ ] T036 [US3] Declarar y congelar `ds-1` (antes de entrenar) — `ml/config/particion.yaml` · (FR-013, FR-014; R-008)
- [ ] T037 [US3] `registro.py` (agrega una fila por cada uso de la prueba y se niega a repetir una versión) — `KM/evaluacion/registro.py`, `ml/modelos/registro-evaluaciones.csv` · (FR-015)

---

## Phase 6: User Story 4 — Entrenar y evaluar contra RNF-35 (P1)

### Pruebas

- [ ] T038 [P] [US4] `test_caracteristicas.py`: 20 valores `float32` en el orden de `data-model.md` §4; curtosis 0 con señal constante; orden +1/−1; razón; energía relativa; velocidad del último fix — `MT/test_caracteristicas.py` · (FR-017)
- [ ] T039 [P] [US4] `test_prefiltro.py`: el umbral deja pasar ≥ 99 % de las anomalías de entrenamiento (98,9 % falla); ventana bajo el umbral → Normal sin llamar al modelo — `MT/test_prefiltro.py` · (FR-018)
- [ ] T040 [P] [US4] `test_decision.py`: confidence 0.74 → Normal, 0.75 y 0.76 → anomalía; temperatura aplicada antes del softmax; `ConfidenceMinimo` leído de `puerta.yaml` — `MT/test_decision.py` · (FR-020, FR-021)
- [ ] T041 [P] [US4] `test_metricas.py`: *precision* y *recall* por clase en una matriz conocida; 0,899 no cumple y 0,900 sí; severidad ± 1 solo sobre anomalías detectadas con el tipo correcto (79 %/80 %); matriz en orden `Normal, Bache, Hundimiento` — `MT/test_metricas.py` · (FR-021)
- [ ] T042 [P] [US4] `test_entrenamiento.py`: dos corridas con la semilla 2828 → misma huella de pesos; la pérdida de severidad ignora Normal; la red tiene ≤ 2 capas ocultas y salidas `tipo_logits` (3) y `severidad` (1) — `MT/test_entrenamiento.py` · (FR-016, FR-019)
- [ ] T043 [P] [US4] `test_calibracion.py`: la temperatura baja la NLL de validación en un modelo sintético sobreconfiado y no lee prueba; `test_linea_base.py`: la línea base de umbrales clasifica las señales sintéticas — `MT/test_calibracion.py`, `MT/test_linea_base.py` · (FR-020, FR-021)

### Implementación

- [ ] T044 [US4] `caracteristicas.py` y `prefiltro.py` — `KM/caracteristicas/` · (FR-017, FR-018)
- [ ] T045 [US4] `red.py` (Normalization → 32 → 16 → dos cabezas) y `entrenamiento.py` (pesos de clase, máscara, parada temprana) — `KM/modelo/` · (FR-019; R-011)
- [ ] T046 [US4] `calibracion.py` y `linea_base.py` — `KM/modelo/` · (FR-020, FR-021; R-013)
- [ ] T047 [US4] `decision.py`, `metricas.py`, `matriz.py` (incluye por celular, «baches no vistos», balanceado y `argmax` sin umbral) — `KM/evaluacion/` · (FR-021; R-012)
- [ ] T048 [US4] `informe.py`: `informe.json`, `informe.md` y matriz CSV/JSON con commit, `datasetVersion`, huella, semilla y entorno — `KM/evaluacion/informe.py` · (FR-016, FR-022)
- [ ] T049 [US4] Etapa `todo --version X` (ventanas → partición → entrenar → calibrar → cuantizar → evaluar el `.tflite` → informe) — `KM/__main__.py` · (FR-016, FR-021)

---

## Phase 7: User Story 5 — Cuantizar, equivalencia y *benchmark* (P1)

### Pruebas

- [ ] T050 [P] [US5] `test_cuantizacion.py`: `.tflite` < 5 MB; entrada `caracteristicas [1,20] float32`; salidas `tipo_logits [1,3]` y `severidad [1,1]` `float32`; operaciones int8 — `MT/test_cuantizacion.py` · (FR-019, FR-023)
- [ ] T051 [P] [US5] `test_equivalencia.py`: tipo igual 98,9 % falla y 99 % pasa; Δ confidence p99 0.051 falla; un modelo con un peso alterado falla — `MT/test_equivalencia.py` · (FR-024)
- [ ] T052 [P] [US5] `test_vectores_prueba.py`: ≥ 60 casos, 20 por clase, con casos cerca del umbral del prefiltro y de 0.75; recomputar características y salidas con el pipeline da lo guardado — `MT/test_vectores_prueba.py` · (FR-025)
- [ ] T053 [P] [US5] `test_benchmark.py`: p95 49,9 ms pasa y 50,0 no; celular de 5 GB de RAM o de hace 5 años no cuenta como gama media; menos de 3 celulares válidos → falla — `MT/test_benchmark.py` · (FR-026)
- [ ] T054 [P] [US5] `PercentilesTest` (JVM): p50/p95/máx de una lista conocida — `GR/test/java/pe/killauru/grabador/PercentilesTest.kt` · (FR-026)

### Implementación

- [ ] T055 [US5] `cuantizacion.py` (`model.export` → `TFLiteConverter.from_saved_model`, int8 completa, I/O `float32`, ≤ 500 ventanas representativas) — `KM/exportacion/cuantizacion.py` · (FR-023; R-014)
- [ ] T056 [US5] `equivalencia.py` (intérprete `ai-edge-litert`) y `vectores_prueba.py` — `KM/exportacion/` · (FR-024, FR-025)
- [ ] T057 [US5] `BenchmarkModelo.kt` (un intérprete, 1 hilo, 100 + 1 000), `Percentiles.kt` y `PantallaBenchmark.kt` (escribe el JSON) — `GR/main/java/pe/killauru/grabador/` · (FR-026; R-015)
- [ ] T058 [US5] `benchmark.py`: valida los JSON de `benchmark/` (gama media, ≥ 3 celulares, p95) — `KM/publicacion/benchmark.py` · (FR-026)
- [ ] T059 [US5] Medir la candidata en ≥ 3 celulares de gama media — `ml/modelos/<versión>/benchmark/<alias>.json` · (FR-026; RNF-04)

---

## Phase 8: User Story 6 — Puerta y publicación (P1)

### Pruebas

- [ ] T060 [P] [US6] `test_puerta.py`: falla (código ≠ 0) por cada condición sola: una clase bajo 0,90; severidad bajo 0,80; < 200 por clase; latencia; equivalencia; tamaño; clases ≠ RN-01; acuerdo; partición; pasa con todo en verde — `MT/test_puerta.py` · (FR-028)
- [ ] T061 [P] [US6] `test_manifiesto.py`: valida contra `contracts/manifiesto-modelo.schema.json`; SHA-256 y tamaño = archivo; no trae `ConfidenceMinimo`; URLs en `cdn.killauru.pe/modelo/<versión>/` — `MT/test_manifiesto.py` · (FR-029)
- [ ] T062 [P] [US6] `test_versiones.py`: cambia el catálogo → MAYOR; cambia `preprocesamientoVersion` → MENOR y sube `versionMinimaApp`; mismo contrato → PARCHE; `ds-<n>` y su huella — `MT/test_versiones.py` · (FR-027)

### Implementación

- [ ] T063 [US6] `versiones.py` — `KM/comun/versiones.py` · (FR-027; R-016)
- [ ] T064 [US6] `puerta.py` (lee `puerta.yaml`, informe, *benchmarks*, equivalencia, acuerdo y partición; escribe `manifiesto.json` solo si pasa) y `manifiesto.py` — `KM/publicacion/` · (FR-028, FR-029)
- [ ] T065 [US6] `publicar_r2.sh`: falla si la ruta de la versión existe; sube `.tflite`, matriz e informe con `immutable`; verifica con `curl` + `sha256sum` desde `cdn.killauru.pe`; sube el manifiesto al final; modo `--volver-atras <versión>` — `ml/scripts/publicar_r2.sh` · (FR-030, FR-031; R-017)
- [ ] T066 [US6] `publicar-modelo.yml` (`workflow_dispatch`: `version`, `volver_atras`; entorno `modelos`; secretos `KU_R2_MODELOS_ACCESS_KEY_ID`, `KU_R2_MODELOS_SECRET_ACCESS_KEY`, `KU_R2_ENDPOINT`): *lock* → `puerta` → `publicar_r2.sh` — `.github/workflows/publicar-modelo.yml` · (FR-030; D10, RNF-34, R-018)
- [ ] T067 [US6] Simulacro en el prefijo `modelo-prueba/`: publicar una versión sintética, publicar otra y volver atrás; comprobar que los archivos de la primera no cambiaron — `ml/README.md` (registro del simulacro) · (FR-031)
- [ ] T068 [US6] Aprobar `1.0.0` y dejar `killauru-1.0.0.tflite` + `manifiesto.json` + `vectores-prueba.json` listos para el APK de 008; publicar en R2 con el workflow (si RF-33 se recorta, la publicación puede esperar) — `ml/modelos/1.0.0/` · (FR-022, FR-032; ADR-004)

---

## Phase 9: User Story 7 — Seguir el riesgo (P2)

- [ ] T069 [P] [US7] `test_estado.py`: con un catálogo y etiquetas sintéticas muestra horas, ventanas por clase y conjunto, porcentaje de la meta y la última validación — `MT/test_estado.py` · (FR-034)
- [ ] T070 [US7] `estado.py` — `KM/seguimiento/estado.py` · (FR-034; R-020)
- [ ] T071 [US7] Bitácora con los controles del día 7 (conteos) y del día 11 (validación ≥ 0,85) y la decisión tomada (seguir, pasadas dirigidas o plan B) — `ml/protocolo/bitacora.md` · (FR-034; R-021)

## Phase 10: Polish

- [ ] T072 [P] `README.md` de `ml/`: entorno, comandos por etapa, flujo grabar → publicar, dónde está cada dato, privacidad — `ml/README.md` · (FR-016)
- [ ] T073 Correr el quickstart de punta a punta con el dataset sintético y luego con `ds-1` — `specs/009-dataset-y-modelo/quickstart.md` · (FR-016, SC-003, SC-004)
- [ ] T074 Marcar la trazabilidad y llevar a docs lo que el autor confirme (D1, `research.md` «Para llevar a docs») — `specs/009-dataset-y-modelo/checklists/trazabilidad.md` · (FR-001…FR-034)

## Dependencies & Execution Order

Phase 1 → Phase 2 → **US1 empieza el día 1** (T015–T020 en 2 días; T021 piloto; T022 campaña días 3–6). US2 corre en paralelo con la campaña (días 4–9). US3 antes de entrenar. US4 → US5 → US6 (días 8–14). US7 desde el día 1 (T069–T070) y controles los días 7 y 11. 008 necesita de esta feature: el contrato (`contracts/`) desde el día 1 y `ml/modelos/1.0.0/` al final (T068).
