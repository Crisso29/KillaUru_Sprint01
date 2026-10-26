# Feature Specification: Dataset y modelo de detección

**Feature Branch**: `009-dataset-y-modelo`

**Created**: 2026-10-22

**Status**: Draft

**Input**: User description: "HU-28 de KillaUru: grabar y etiquetar recorridos reales en Ayacucho–Vinchos con vehículos livianos y el celular en soporte, en al menos 3 celulares; entrenar con ellos un MLP cuantizado (LiteRT/TFLite) que clasifique ventanas de 500 ms en Bache, Hundimiento o Normal con severidad 1–5; demostrar RNF-35 sobre un conjunto de prueba separado y RNF-04 en ≥ 3 celulares; publicar el modelo con su manifiesto y su matriz de confusión por `modeloVersion` (ADR-004), con el modelo inicial dentro del APK."

---

## 0. Ficha de la feature

| Campo | Valor |
|---|---|
| Qué es | El **dataset propio** (protocolo de campo, grabador, etiquetado, partición) y el **pipeline reproducible en Python** (`ml/`) que entrena, evalúa, cuantiza, mide y publica el modelo del Detector, con su manifiesto en R2 |
| Qué **no** es | No es la app: el Detector, el Actualizador de modelo (RF-33), el servicio de sensores (RF-01) y la batería (RNF-15) son de **008-app-android**, que **consume** el contrato de esta feature (`contracts/contratos-internos.md`). No toca el backend (001–007) |
| Componentes C4 | App → **Detector** (lo alimenta: modelo y preprocesamiento) y **Actualizador** (lo alimenta: manifiesto); `cdn.killauru.pe` (Cloudflare R2) |
| Dónde vive | `ml/` en la raíz del repositorio (pipeline, configuración, etiquetas, modelos versionados, grabador Android aparte) y `.github/workflows/` (CI de `ml/` y publicación manual) |
| Depende de | Nada del backend. Usa R2 (ADR-010), GitHub Actions (D10) y la polilínea `db/datos/corredor_pe28a.geojson` de 004 (para pasar GPS → km, D16) |
| La usan | 008-app-android (modelo inicial en el APK, contrato de entradas y salidas, vectores de prueba, manifiesto); 004 solo recibe `modeloVersion` en cada evento (ADR-014) |
| Prioridad | *Must* (HU-28), **no recortable**; RF-33 (canal de actualización) es *Should* recortable: si se recorta, el manifiesto en R2 se publica después sin cambiar nada de esta feature |
| Riesgo | **El mayor del plan**: sin modelo que pase la puerta no hay detección (HU-01, HU-02). Empieza en la semana 1 y dura ~2 semanas (HU-28, R-09, R-10) |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historias | HU-28 (principal); HU-01, HU-02, HU-26 (lo que el modelo debe permitir) | `02-historias-de-usuario.md` |
| Requisitos funcionales | RF-01 (50 Hz, sensores), RF-02 (ventana de 500 ms, tipo, severidad, confidence), RF-03 (`modeloVersion`, hora GPS), RF-33 (manifiesto, SHA-256, versión mínima de la app), RF-34 (solo Liviano) | `03-requisitos-funcionales.md` |
| Reglas de negocio | RN-01 (Bache, Hundimiento, Normal), RN-02 (severidad 1–5), RN-03 (> 5 km/h durante ≥ 10 s), RN-04 y RN-11 (confidence ≥ 0.75), RN-18 (solo liviano con soporte) | `09-reglas-de-negocio.md` |
| Atributos de calidad | RNF-04 (< 50 ms p95, gama media, ≥ 3 celulares), RNF-35 (*precision* y *recall* ≥ 0,90 por clase; severidad ± 1 en ≥ 80 %; ≥ 200 ventanas por clase; ≥ 3 celulares; matriz por `modeloVersion`), RNF-15 (batería), RNF-34 (secretos) | `04-atributos-de-calidad.md` |
| Restricciones y supuestos | S-06 (liviano + soporte), S-07, R-05 (`cdn.killauru.pe`), R-06 (Ley 29733), R-09 (una persona), R-10 (6–8 semanas), RS-07 (rompemuelles), RS-08 (soporte no comprobable) | `05-restricciones.md` |
| Drivers | DA-03, DA-04, DA-18 | `06-driver-arquitectonicos.md` |
| Decisiones | ADR-004 (MLP cuantizado, modelo inicial en el APK, manifiesto + SHA-256 en R2, catálogo = etiquetas, matriz de confusión), ADR-010 (R2), ADR-014 (`JSONB` acepta varias versiones) | `07-decisiones-arquitectonicas.md` |
| Escenarios | EQ-03 (rendimiento y precisión en móvil) | `08-escenarios-y-atributos-de-calidad.md` |
| Diseño | Detector y Actualizador; R2 con modelos y manifiestos | `componentes-arquitectonicos.md` §5, `Nivel2`/`Nivel3` |
| Diseño | Prefiltro de energía, sensores en lote, intérprete reutilizado | `diseño-interno-de-modulos.md` §7.4; `enfoque-arquitectonico.md` §9 |
| Decisiones del autor | D1 (agregados también en docs), D10 (GitHub Actions), D16 (datos del corredor), D17 (orden de las specs) | `specs/revision/decisiones.md` |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Grabar recorridos reales sin poner en riesgo al conductor (Priority: P1)

Como equipo de desarrollo, quiero grabar acelerómetro, giroscopio y GPS en recorridos reales de Ayacucho–Vinchos con varios celulares en soporte, sin que nadie toque un celular mientras el vehículo se mueve, para tener datos de la vía real (HU-28, S-06).

**Why this priority**: Sin datos no hay modelo; la grabación es lo que más tarda y depende del clima y del vehículo.

**Independent Test**: Un recorrido corto de prueba (piloto de 20 min) con 3 celulares; el comando `ingerir` lo acepta y reporta frecuencia real, huecos y metadatos completos.

**Acceptance Scenarios**:

1. **Given** el vehículo estacionado y 3 celulares fijos en sus soportes, **When** se inicia la grabación, **Then** cada celular graba acelerómetro y giroscopio a 50 Hz pedidos con entrega en lote de ~1 s y GPS a 1 Hz, en un servicio en primer plano, sin ninguna acción posterior hasta volver a estacionar (RF-01, `diseño-interno` §7.4, HU-01 «0 toques»).
2. **Given** un copiloto, **When** pasa por un bache, **Then** puede pulsar «Marca» en uno de los celulares; **Given** que no hay copiloto, **Then** no se marca nada en marcha y se etiqueta después con el video (R-003).
3. **Given** una grabación terminada, **When** se copia por cable y se ingiere, **Then** queda en el catálogo con su SHA-256 y sus metadatos (celular, vehículo, soporte, tramo, sentido) y sin ningún dato personal (R-004).
4. **Given** un celular que entrega menos de 45 Hz efectivos o con huecos de más de 200 ms en más del 1 % de la grabación, **When** se ingiere, **Then** se marca `NoApta` con el motivo y no entra al dataset.

---

### User Story 2 - Etiquetar ventanas con criterios repetibles (Priority: P1)

Como equipo, quiero etiquetar cada evento como Bache, Hundimiento o Normal y su severidad 1–5 con una guía escrita, para que dos etiquetadores lleguen a la misma etiqueta (RN-01, RN-02).

**Why this priority**: Una etiqueta mala no se arregla con más entrenamiento; RNF-35 se mide contra estas etiquetas.

**Independent Test**: Sobre una grabación de prueba, `candidatos` lista los picos, el etiquetador llena la hoja, `validar-etiquetas` la acepta y `acuerdo` calcula la concordancia con una segunda pasada.

**Acceptance Scenarios**:

1. **Given** una grabación ingerida, **When** se generan candidatos, **Then** aparece una fila por pico de aceleración vertical con su hora, km de la vía, velocidad y el segundo del video donde verlo.
2. **Given** un candidato, **When** se etiqueta, **Then** queda con `tipo` ∈ {Bache, Hundimiento, Normal}, `subtipo` (p. ej. `Rompemuelles`, `Junta`, `Ripio` para Normal), `severidad` 1–5 solo para Bache y Hundimiento, `fuente` y etiquetador; cualquier otro valor lo rechaza `validar-etiquetas` (RN-01, RN-02).
3. **Given** el 10 % de los eventos etiquetados dos veces, **When** se mide el acuerdo, **Then** κ de Cohen del tipo ≥ 0.80 y severidad dentro de ± 1 en ≥ 90 %; si no, se corrige la guía y se vuelve a etiquetar (R-005).
4. **Given** un bache patrón medido (profundidad, ancho, km), **When** se etiquetan sus pasadas, **Then** todas llevan la severidad del catálogo de baches patrón (R-006).

---

### User Story 3 - Separar entrenamiento, validación y prueba sin fuga (Priority: P1)

Como equipo, quiero que el conjunto de prueba nunca se use para entrenar ni para ajustar nada, para que el resultado de RNF-35 sea creíble (HU-28: «se separa un conjunto de prueba que no se usa para entrenar»).

**Why this priority**: Una fuga infla las métricas y el modelo falla en la vía.

**Independent Test**: La prueba de no-fuga sobre la partición declarada pasa; si se mueve una grabación de un recorrido de prueba a entrenamiento, falla.

**Acceptance Scenarios**:

1. **Given** `config/particion.yaml`, **When** se arma el dataset, **Then** cada **recorrido** (con **todos** los celulares que iban en ese vehículo) cae en un solo conjunto, y ningún par (recorrido, celular) aparece en dos (R-008).
2. **Given** la partición, **When** se verifica, **Then** el conjunto de prueba tiene ≥ 200 ventanas por clase, ≥ 3 celulares distintos y los dos sentidos de la vía; si no, el pipeline se detiene antes de entrenar (RNF-35).
3. **Given** el entrenamiento, **When** se ajustan la normalización, el umbral del prefiltro y la temperatura, **Then** solo se usan datos de entrenamiento y validación (R-012, R-013).
4. **Given** una versión candidata, **When** se evalúa sobre prueba, **Then** queda anotado en `ml/modelos/registro-evaluaciones.csv`; el conjunto de prueba se usa **una sola vez** por versión candidata.

---

### User Story 4 - Entrenar y evaluar el modelo contra RNF-35 (Priority: P1)

Como equipo, quiero un pipeline de un comando que entrene el MLP y produzca su matriz de confusión y su informe, para saber si el modelo sirve y poder repetirlo (HU-28, RNF-35, ADR-004).

**Why this priority**: Es la evidencia que pide HU-28 y la base de la puerta de publicación.

**Independent Test**: Con el dataset sintético de las pruebas, `python -m killauru_ml todo --version 0.0.1` produce informe y matriz; dos corridas con la misma semilla dan las mismas métricas y la misma huella de pesos.

**Acceptance Scenarios**:

1. **Given** una versión del dataset, **When** se entrena, **Then** el informe registra `datasetVersion`, semilla, commit, versiones del entorno y `preprocesamientoVersion` (R-016, R-019).
2. **Given** el modelo cuantizado, **When** se evalúa sobre prueba con la **decisión del sistema** (prefiltro → modelo → confidence ≥ 0.75), **Then** se reportan *precision* y *recall* por clase, severidad ± 1, la matriz de confusión, el desglose por celular y la comparación con la línea base de umbrales (R-012).
3. **Given** las métricas, **When** alguna clase queda por debajo de 0,90 en *precision* o *recall*, o la severidad ± 1 por debajo de 80 %, **Then** el informe marca `cumpleRnf35 = false`.

---

### User Story 5 - Cuantizar, comprobar equivalencia y medir en celulares (Priority: P1)

Como equipo, quiero exportar el modelo a LiteRT cuantizado, comprobar que da lo mismo que el de Python y medir su latencia en ≥ 3 celulares de gama media, para que lo que se mide sea lo que se entrega (RNF-04, ADR-004).

**Why this priority**: La puerta se evalúa sobre el archivo `.tflite` que viaja en el APK, no sobre el modelo de Python.

**Independent Test**: `equivalencia` compara Python y TFLite sobre prueba; el modo *benchmark* del grabador mide el `.tflite` en un celular.

**Acceptance Scenarios**:

1. **Given** el modelo entrenado, **When** se cuantiza a int8 con entrada y salida `float32`, **Then** el `.tflite` pesa < 5 MB (ADR-004) y el tipo coincide con el de Python en ≥ 99 % de las ventanas de prueba (R-014).
2. **Given** el `.tflite`, **When** se exportan los vectores de prueba, **Then** cada vector trae la señal cruda, las características esperadas y las salidas esperadas, para que 008 compruebe su preprocesamiento en Kotlin (R-007).
3. **Given** 3 celulares de gama media (≤ 4 GB de RAM y procesador de los últimos 4 años), **When** corre el *benchmark* (100 de calentamiento + 1 000 medidas, 1 hilo), **Then** el p95 de cada uno es < 50 ms y queda en `benchmark/<celular>.json` (RNF-04, R-015).

---

### User Story 6 - No publicar nada que no pase la puerta (Priority: P1)

Como responsable del producto, quiero que una versión del modelo solo llegue a los celulares si cumple RNF-35 y RNF-04, con su manifiesto y su matriz de confusión publicados, para no repartir un modelo peor (ADR-004, RF-33).

**Why this priority**: Un modelo malo llena el mapa de falsos baches o los esconde.

**Independent Test**: `puerta` sobre un informe que no cumple termina con código ≠ 0 y el workflow de publicación se detiene sin tocar R2.

**Acceptance Scenarios**:

1. **Given** una versión que cumple RNF-35, RNF-04, equivalencia, tamaño y catálogo, **When** se corre la puerta, **Then** genera `manifiesto.json` con versión, tamaño, SHA-256, calibración, umbral del prefiltro, versión mínima de la app, fecha y métricas (R-017).
2. **Given** la puerta en verde, **When** el autor lanza a mano `publicar-modelo.yml`, **Then** sube primero el `.tflite`, la matriz y el informe a `modelo/<versión>/`, los descarga desde `cdn.killauru.pe` para comprobar el SHA-256 y **solo después** reemplaza `modelo/manifiesto.json` (R-017, R-018).
3. **Given** una versión que no cumple, **When** se lanza el workflow, **Then** falla en el paso de la puerta y R2 no cambia.
4. **Given** una versión publicada con un problema, **When** se vuelve atrás, **Then** el workflow republica el manifiesto de la versión anterior; los archivos de una versión nunca se sobrescriben.
5. **Given** la v1.0 de la app, **When** se arma el APK, **Then** lleva dentro el `.tflite` y el `manifiesto.json` de la versión aprobada, aunque RF-33 se haya recortado (ADR-004 punto 1).

---

### User Story 7 - Seguir el riesgo y activar el plan B a tiempo (Priority: P2)

Como única persona del equipo, quiero saber al día 7 y al día 11 si voy a llegar, para decidir a tiempo y no descubrirlo en la semana 8 (HU-28, R-09, R-10).

**Why this priority**: Es el mayor riesgo del plan.

**Independent Test**: `python -m killauru_ml estado` muestra horas grabadas, ventanas etiquetadas por clase y conjunto contra las metas, y las métricas de validación de la última corrida.

**Acceptance Scenarios**:

1. **Given** el día 7 de la feature, **When** Hundimiento tiene menos del 60 % de la meta, **Then** se programan pasadas dirigidas por los km donde el catálogo ubicó hundimientos (R-020).
2. **Given** el día 11, **When** la validación queda por debajo de 0,85 en alguna clase, **Then** se activa el plan B que haya elegido el autor (R-021).

### Edge Cases

- **Celular que se cae o gira en el soporte**: cambio brusco de la gravedad estimada (> 30° entre ventanas): ese tramo se marca `SoporteInestable` y se excluye (RS-08).
- **Muestreo irregular**: Android no garantiza 50 Hz exactos; se remuestrea a una malla de 50 Hz (R-002). Huecos > 200 ms: las ventanas que los tocan se excluyen.
- **Sin GPS** (quebradas): sin velocidad ni km; esas ventanas se excluyen, igual que la app no detecta sin GPS (RF-36 «Sin GPS»).
- **Detenido o lento**: ventanas que no cumplen RN-03 (> 5 km/h durante ≥ 10 s) no entran al dataset, igual que la app no las clasifica.
- **Evento en el borde de una ventana**: la ventana que contiene el pico toma la etiqueta; las ventanas vecinas a menos de 250 ms del pico que no lo contienen son **zona gris** y se excluyen del entrenamiento y de la evaluación (R-007).
- **Dos eventos en la misma ventana**: gana el de mayor severidad; a igual severidad, Bache.
- **Rompemuelles, juntas de puente, badenes y ripio**: son `Normal` con subtipo (RN-01 no los incluye; RS-07). Se conservan aparte para evaluar una clase futura.
- **Mismo bache en entrenamiento y prueba** (recorridos distintos sobre la misma vía): es el uso real; se informa además la métrica sobre «baches no vistos» como diagnóstico (R-008).
- **Pocos hundimientos**: pasadas dirigidas y los 3 celulares a la vez; si aun así no hay 200 en prueba, la puerta no pasa y aplica el plan B (R-020, R-021).
- **Celular que no es de gama media**: puede grabar, pero no cuenta para el *benchmark* de RNF-04.
- **Recorrido con otro vehículo no liviano o celular en la mano**: no se graba (S-06, RN-18).
- **Video con personas o placas**: solo para etiquetar; nunca sale del *bucket* privado y se borra a los 90 días de congelar la versión del dataset (R-004).

## Requirements *(mandatory)*

### Functional Requirements

**A. Recolección en campo (HU-28, S-06, RN-18, RF-01)**

- **FR-001**: Un **grabador** Android aparte (`ml/grabador/`, nunca distribuido a conductores) MUST grabar acelerómetro y giroscopio con 50 Hz pedidos y entrega en lote de ~1 s, y GPS a 1 Hz, en un servicio en primer plano, con los mismos parámetros de captura que la app. *(RF-01, `diseño-interno` §7.4; R-001, R-002)*
- **FR-002**: El protocolo MUST impedir que el conductor toque un celular con el vehículo en movimiento: la grabación se inicia y se detiene con el vehículo estacionado; en marcha solo el copiloto puede pulsar «Marca»; sin copiloto no hay marcas. *(HU-01, ADR-004 contexto; R-003)*
- **FR-003**: Cada grabación MUST guardarse como una carpeta con `meta.json`, `acelerometro.csv`, `giroscopio.csv`, `gps.csv`, `marcas.csv` y, solo en el celular de video, `video.mp4` + `video.json`, con todas las horas en `elapsedRealtimeNanos` y la hora UTC tomada del GPS. *(RF-03; R-002; `data-model.md` §1)*
- **FR-004**: `meta.json` MUST traer: alias del celular, marca y modelo comercial, RAM, año de lanzamiento, versión de Android, alias del vehículo y categoría `Liviano`, tipo y posición del soporte, tramo (km inicial y final), sentido, fecha y versión del grabador. *(S-06, RNF-04, RNF-35)*
- **FR-005**: El dataset MUST NOT contener datos personales: ni IMEI, ni `ANDROID_ID`, ni cuentas, nombres o placas; el grabador no tiene permiso de internet; la ubicación solo sirve para calcular km y sentido (el dataset derivado guarda km y velocidad, no latitud ni longitud); el crudo y el video viven solo en un *bucket* R2 privado y el video se borra a los 90 días de congelar la versión del dataset. *(R-06, RNF-10, ADR-007; R-004)*
- **FR-006**: Las grabaciones MUST hacerse solo con vehículos livianos y el celular en soporte fijo, con **≥ 3 celulares distintos** (los tres a la vez en el mismo vehículo), en ambos sentidos del tramo Ayacucho–Vinchos (km 268–335.073). *(HU-28, S-06, RN-18, RNF-35)*
- **FR-007**: `ingerir` MUST verificar cada grabación (archivos completos, frecuencia efectiva ≥ 45 Hz, huecos > 200 ms en ≤ 1 % del tiempo, `meta.json` válido), calcular su SHA-256, registrarla en `ml/datos/catalogo.csv` con estado `Apta` o `NoApta` y motivo, y subirla al *bucket* privado. *(HU-28; R-002, R-016)*

**B. Etiquetado (RN-01, RN-02)**

- **FR-008**: `candidatos` MUST listar los picos de aceleración vertical por encima de un umbral bajo (`UmbralCandidatoMs2`), con hora, km, velocidad y segundo del video, sin perder ninguna pasada de los baches patrón. *(HU-28; R-005)*
- **FR-009**: Cada etiqueta MUST tener `tipo` ∈ {Bache, Hundimiento, Normal}, `subtipo` opcional solo para Normal, `severidad` 1–5 solo para Bache y Hundimiento, `fuente` (`video`, `copiloto`, `patron`) y etiquetador; `validar-etiquetas` rechaza cualquier otro valor. *(RN-01, RN-02; R-005)*
- **FR-010**: La severidad MUST asignarse con la guía de `ml/protocolo/guia-etiquetado.md` (tabla de RN-02 + medidas de referencia); las pasadas de un bache patrón llevan la severidad del catálogo `baches-patron.csv`. *(RN-02; R-006)*
- **FR-011**: Al menos el 10 % de los eventos MUST etiquetarse dos veces (otra persona, o la misma ≥ 7 días después), con κ del tipo ≥ 0.80 y severidad ± 1 en ≥ 90 %; si no se cumple, la versión del dataset no se congela. *(RNF-35; R-005)*
- **FR-012**: El ventaneo MUST producir ventanas de 500 ms (25 muestras a 50 Hz) sin solapamiento, solo donde se cumple RN-03; la ventana que contiene el pico toma su etiqueta; las vecinas a < 250 ms que no lo contienen son zona gris y se excluyen; el resto es Normal. *(RF-02, RN-03; R-007)*

**C. Partición y tamaño (HU-28, RNF-35)**

- **FR-013**: La partición MUST declararse a mano en `ml/config/particion.yaml` **por recorrido** antes de entrenar y congelarse con la versión del dataset; todos los celulares de un recorrido van al mismo conjunto. *(HU-28; R-008)*
- **FR-014**: La verificación de la partición MUST exigir en prueba ≥ 200 ventanas por clase, ≥ 3 celulares y ambos sentidos; en validación ≥ 100 por clase; en entrenamiento ≥ 400 por clase de anomalía. Si no se cumple, el pipeline se detiene. *(RNF-35; R-020)*
- **FR-015**: La normalización, el umbral del prefiltro, la temperatura y la selección de hiperparámetros MUST ajustarse solo con entrenamiento y validación; la prueba se evalúa una vez por versión candidata y queda en `registro-evaluaciones.csv`. *(HU-28; R-008, R-012)*

**D. Pipeline, modelo y evaluación (ADR-004, RNF-35)**

- **FR-016**: El pipeline MUST vivir en `ml/` como paquete `killauru_ml` ejecutable por etapas (`python -m killauru_ml <etapa>`) y de punta a punta (`todo`), con semillas fijas, operaciones deterministas y entorno fijado en `ml/requirements.lock` con *hashes*. *(HU-28; R-019)*
- **FR-017**: El preprocesamiento `p1` MUST: remuestrear a 50 Hz; estimar la gravedad con la media de los últimos 2 s de acelerómetro; separar componente vertical y horizontal; calcular las 19 características de `data-model.md` §4 y agregar la velocidad del último fix GPS. Es el mismo algoritmo que implementa 008. *(RF-01, RF-02; R-007, R-010)*
- **FR-018**: El prefiltro de energía MUST descartar como Normal, sin pasar por el modelo, las ventanas con pico a pico vertical menor que `umbralPrefiltro`, elegido en entrenamiento para perder ≤ 1 % de las ventanas de anomalía. *(`diseño-interno` §7.4, RNF-15; R-022)*
- **FR-019**: El modelo MUST ser un MLP (normalización dentro del grafo, ≤ 2 capas ocultas) con dos salidas: `tipo_logits` (3, en el orden `Normal`, `Bache`, `Hundimiento`) y `severidad` (1 valor continuo que la app redondea y acota a 1–5). *(ADR-004, RN-01, RN-02; R-011)*
- **FR-020**: La confidence MUST ser `softmax(tipo_logits / temperatura)`, con la temperatura ajustada en validación; el umbral de confidence no vive en el modelo ni en el manifiesto: es RN-04 (`ConfidenceMinimo`), leído de configuración. *(RN-04, RN-11; R-013)*
- **FR-021**: La evaluación MUST usar la **decisión del sistema** (prefiltro → modelo → tipo con confidence ≥ `ConfidenceMinimo`, si no Normal) sobre el **modelo cuantizado**, y reportar *precision* y *recall* por clase, severidad ± 1 (sobre las anomalías detectadas con el tipo correcto), matriz de confusión, desglose por celular, «baches no vistos» y la línea base de umbrales. *(RNF-35, EQ-03; R-012)*
- **FR-022**: Cada versión MUST tener `ml/modelos/<versión>/` con `informe.json`, `informe.md`, `matriz-confusion.csv` y `matriz-confusion.json`, generados (nunca escritos a mano). *(HU-28, ADR-004)*

**E. Cuantización, equivalencia y *benchmark* (ADR-004, RNF-04)**

- **FR-023**: La exportación MUST pasar por SavedModel y `TFLiteConverter` con cuantización int8 completa, entrada y salida `float32` y un conjunto representativo de ≤ 500 ventanas de entrenamiento; el archivo pesa < 5 MB. *(ADR-004; R-014)*
- **FR-024**: La prueba de equivalencia MUST comparar el modelo de Python y el `.tflite` sobre prueba: tipo igual en ≥ 99 %, |Δ confidence| ≤ 0.05 en el p99 y severidad redondeada igual en ≥ 98 %. *(ADR-004; R-014)*
- **FR-025**: Cada versión MUST exportar `vectores-prueba.json` (≥ 60 casos, 20 por clase, con 2 s de señal cruda, características y salidas esperadas) para la prueba de preprocesamiento de 008. *(RF-02; R-007)*
- **FR-026**: El modo *benchmark* del grabador MUST medir el `.tflite` (100 de calentamiento + 1 000 inferencias, 1 hilo, intérprete reutilizado) en ≥ 3 celulares de gama media y guardar p50, p95, máximo y datos del celular en `ml/modelos/<versión>/benchmark/<alias>.json`. *(RNF-04, EQ-03; R-015)*

**F. Versionado, puerta y publicación (ADR-004, RF-33)**

- **FR-027**: `modeloVersion` MUST seguir versión semántica: MAYOR si cambia el catálogo o el contrato de salidas, MENOR si cambia el preprocesamiento o la arquitectura (sube `versionMinimaApp`), PARCHE si solo se reentrena; el dataset se versiona como `ds-<n>` con su huella SHA-256. *(ADR-004 punto 4, RF-33; R-016)*
- **FR-028**: La puerta MUST fallar (código ≠ 0) si no se cumple cualquiera de: RNF-35 sobre prueba, RNF-04 en ≥ 3 celulares de gama media, equivalencia, tamaño < 5 MB, clases = RN-01, acuerdo del etiquetado y verificación de la partición. *(RNF-35, RNF-04, ADR-004; R-017)*
- **FR-029**: El manifiesto MUST cumplir `contracts/manifiesto-modelo.schema.json`: versión, URL, tamaño, SHA-256, fecha UTC, `versionMinimaApp`, categoría, clases, `preprocesamientoVersion`, `ventanaMs`, `frecuenciaHz`, `normalizacion`, forma de entradas y salidas (que lee 008 FR-071), temperatura, `umbralPrefiltro`, `datasetVersion`, métricas y URL de la matriz. *(ADR-004, RF-33; R-017)*
- **FR-030**: El workflow manual `publicar-modelo.yml` MUST repetir la puerta y el SHA-256 en CI, subir los archivos inmutables de la versión, comprobarlos desde `cdn.killauru.pe` y recién entonces reemplazar el manifiesto; con las credenciales de R2 como secretos de GitHub. *(ADR-004, ADR-010, RNF-34, D10; R-018)*
- **FR-031**: Volver atrás MUST ser republicar el manifiesto de una versión anterior; los archivos de una versión publicada nunca se reemplazan. *(ADR-004; R-017)*
- **FR-032**: La versión aprobada para la v1.0 MUST quedar lista para el APK (`.tflite` + `manifiesto.json` en `ml/modelos/<versión>/`), aunque RF-33 se recorte. *(ADR-004 punto 1, RF-33)*

**G. Código y riesgo**

- **FR-033**: Ningún `.py` ni `.kt` de `ml/` MUST pasar de 200 líneas (lo comprueba `ml/tests/test_limite_lineas.py` en `ml-ci.yml`); las pruebas se escriben antes del código. *(Constitución VII, VIII; ver propuesta de enmienda en `research.md`)*
- **FR-034**: `estado` MUST mostrar horas grabadas, ventanas por clase y conjunto frente a las metas, y las últimas métricas de validación, para los puntos de control de los días 7 y 11. *(HU-28, R-10; R-020, R-021)*

### Reglas y parámetros (`ml/config/*.yaml`)

| Regla | Parámetro | Valor | Fuente |
|---|---|---|---|
| RF-01 | `FrecuenciaHz` | 50 | `03-requisitos-funcionales.md` |
| RF-02 | `VentanaMs` (25 muestras) | 500 | `03-requisitos-funcionales.md` |
| `diseño-interno` §7.4 | `LatenciaLoteUs` (`maxReportLatencyUs`) | 1 000 000 | `diseño-interno` §7.4 |
| RN-03 | `VelocidadMinimaKmh` / `DuracionSostenidaSegundos` | 5 / 10 | `09-reglas-de-negocio.md` |
| RN-04, RN-11 | `ConfidenceMinimo` | 0.75 | `09-reglas-de-negocio.md` |
| RNF-35 | `PrecisionMinimaPorClase` / `RecallMinimoPorClase` | 0,90 / 0,90 | `04-atributos-de-calidad.md` |
| RNF-35 | `SeveridadMasMenosUnoMinima` | 0,80 | `04-atributos-de-calidad.md` |
| RNF-35 | `VentanasPruebaMinimasPorClase` / `CelularesMinimos` | 200 / 3 | `04-atributos-de-calidad.md` |
| RNF-04 | `LatenciaP95MaximaMs` / `CelularesBenchmarkMinimos` | 50 / 3 | `04-atributos-de-calidad.md` |
| ADR-004 | `TamanoMaximoModeloMb` | 5 | `07-decisiones-arquitectonicas.md` |
| — | `VentanaGravedadSegundos` | 2 | Decisión SDD (R-007) |
| — | `ZonaGrisMs` | 250 | Decisión SDD (R-007) |
| — | `FrecuenciaEfectivaMinimaHz` / `HuecoMaximoMs` | 45 / 200 | Decisión SDD (R-002) |
| — | `PerdidaMaximaPrefiltro` | 1 % de las anomalías | Decisión SDD (R-022) |
| — | `VentanasValidacionMinimasPorClase` / `VentanasEntrenamientoMinimasAnomalia` | 100 / 400 | Decisión SDD (R-020) |
| — | `KappaMinimo` / `AcuerdoSeveridadMinimo` / `FraccionDobleEtiquetado` | 0.80 / 0,90 / 10 % | Decisión SDD (R-005) |
| — | `TipoIgualMinimo` / `DeltaConfidenceP99Maximo` / `SeveridadIgualMinima` | 99 % / 0.05 / 98 % | Decisión SDD (R-014) |
| — | `RetencionVideoDias` | 90 | Decisión SDD (R-004) |
| — | `Semilla` | 2828 | Decisión SDD (R-019) |

### Key Entities

- **Grabación**: una carpeta de un celular en un recorrido (sensores, GPS, marcas, metadatos, video opcional), con SHA-256 y estado `Apta`/`NoApta`.
- **Recorrido**: un viaje continuo de un vehículo entre dos paradas, con todos los celulares que llevaba; es la **unidad de partición**.
- **Bache patrón**: bache o hundimiento medido a pie con el vehículo detenido en lugar seguro (km, profundidad, ancho, severidad asignada).
- **Etiqueta**: instante del pico, tipo, subtipo, severidad, fuente y etiquetador.
- **Ventana**: 25 muestras a 50 Hz con su contexto de 2 s, velocidad, km, sentido, celular, recorrido, etiqueta y conjunto.
- **Versión del dataset** (`ds-<n>`): huella del catálogo, las etiquetas, la partición y la configuración de etiquetado.
- **Versión del modelo** (`modeloVersion`): `.tflite`, informe, matriz, vectores de prueba, *benchmarks* y manifiesto; estados `Candidata` → `Aprobada` → `Publicada` → `Retirada` (`data-model.md` §7).
- **Manifiesto**: JSON en `cdn.killauru.pe/modelo/manifiesto.json` que apunta a la versión vigente.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El `.tflite` aprobado logra *precision* y *recall* ≥ 0,90 en Bache, Hundimiento y Normal, y severidad ± 1 en ≥ 80 %, sobre un conjunto de prueba con ≥ 200 ventanas por clase de ≥ 3 celulares (RNF-35, HU-28).
- **SC-002**: p95 de inferencia < 50 ms en **cada uno** de ≥ 3 celulares de gama media (RNF-04, EQ-03).
- **SC-003**: Dos corridas de `todo` con la misma versión del dataset y la misma semilla dan las mismas métricas (4 decimales) y la misma huella de pesos (FR-016).
- **SC-004**: 0 recorridos y 0 pares (recorrido, celular) en dos conjuntos (prueba de no-fuga en verde) (FR-013).
- **SC-005**: Equivalencia Python ↔ `.tflite` dentro de las tolerancias de FR-024 en el 100 % de las versiones aprobadas.
- **SC-006**: El SHA-256 del archivo descargado de `cdn.killauru.pe` coincide con el del manifiesto en el 100 % de las publicaciones; ninguna versión publicada sin informe ni matriz (ADR-004).
- **SC-007**: 0 columnas de latitud, longitud o identificadores de celular o de persona en el dataset derivado; 0 videos con más de 90 días desde el congelamiento (R-06).
- **SC-008**: HU-28 cerrada en ≤ 2 semanas de trabajo efectivo, con los puntos de control de los días 7 y 11 registrados (R-10, HU-28).

## Fuera de alcance

- El Detector, el Actualizador (descarga, verificación, activación con la detección detenida), el servicio de sensores, el prefiltro en Kotlin y la medición de batería en campo: **008-app-android**, que consume `contracts/contratos-internos.md`.
- Aceptar o rechazar eventos según `modeloVersion` en el servidor: 004/005 (ADR-014).
- Modelos para vehículos pesados o motos y la clase «Rompemuelles»: después de v1.0 (S-06, RS-07).
- Firmar el manifiesto: ADR-004 decide SHA-256 sobre HTTPS; se anota como mejora futura.

## Assumptions

- Se dispone de al menos 1 vehículo liviano, 3 celulares Android de gama media y 3 soportes; un segundo vehículo es deseable, no obligatorio.
- El tramo Ayacucho–Vinchos (km 268–335.073, ~67 km) tiene suficientes baches; los hundimientos son más escasos y pueden pedir pasadas dirigidas (R-020).
- El entrenamiento corre en la computadora del autor (CPU); un MLP de este tamaño entrena en minutos.
- La app 008 usa los mismos parámetros de captura (FR-001) y el preprocesamiento `p1` (FR-017); si 008 decide otra cosa, se versiona el preprocesamiento (FR-027).
