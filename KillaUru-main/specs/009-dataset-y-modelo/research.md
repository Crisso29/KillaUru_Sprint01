# Research: Dataset y modelo de detección

> Docs fija el **qué** (HU-28, RF-01/02/03/33, RN-01…RN-04, RN-18, RNF-04, RNF-35, ADR-004, S-06) y algunas piezas del **cómo** (MLP cuantizado, LiteRT, prefiltro de energía, sensores en lote, manifiesto + SHA-256 en R2, modelo inicial en el APK). Todo lo demás es **Decisión SDD** y está aquí con sus alternativas.

## R-001 · Herramienta de grabación: APK aparte (Decisión SDD)

- **Decision**: «KillaUru Grabador», un proyecto Android pequeño en `ml/grabador/` (applicationId `pe.killauru.grabador`), instalado solo en los celulares del equipo por cable. Sin permiso `INTERNET`: los archivos se copian por USB (`adb pull` o MTP). Usa los **mismos parámetros de captura** que la app (R-002) y tiene un modo *benchmark* (R-015).
- **Rationale**: HU-28 empieza en la **semana 1** y es la tarea con más riesgo; la app 008 todavía no existe. Un APK que graba señal cruda, GPS y video no debe llegar a los conductores (R-06): separarlo garantiza que el APK de producción no tenga ese código. Sin internet, el grabador no puede filtrar nada.
- **Alternatives considered**: (a) modo «grabación» o *flavor* de 008 — reutiliza la captura, pero obliga a esperar la app y mezcla código sensible con el de producción; (b) apps de terceros (phyphox, Physics Toolbox) — frecuencia y lote no controlados, formatos distintos, sin video sincronizado y, en algunas, publicidad o red; (c) registrador externo (Arduino + IMU) — no mide lo que mide un celular en soporte.
- **Riesgo**: la captura del grabador y la de 008 podrían diferir. Se mitiga con el contrato `p1` (remuestreo a 50 Hz en ambos lados), los vectores de prueba (FR-025) y, en el piloto, una grabación de control con la app 008 cuando exista.

## R-002 · Frecuencia, lote y formato de archivo (Decisión SDD sobre RF-01)

- **Decision**: 50 Hz **pedidos** (`samplingPeriodUs = 20 000`) y `maxReportLatencyUs = 1 000 000`, igual que la app (RF-01, `diseño-interno` §7.4). Se guarda la hora real de cada muestra (`event.timestamp`, base `elapsedRealtimeNanos`); el GPS guarda `elapsedRealtimeNanos` y la hora UTC del fix, así sensores y GPS comparten reloj. El pipeline **remuestrea** con interpolación lineal a una malla exacta de 50 Hz (la app hace lo mismo). Formato: CSV plano por sensor (texto, se escribe por bloques; si el celular se apaga solo se pierde el último bloque); `ingerir` lo comprime al subirlo. Grabación apta: frecuencia efectiva ≥ 45 Hz y huecos > 200 ms en ≤ 1 % del tiempo.
- **Rationale**: entrenar con lo mismo que verá el modelo en producción evita la diferencia entrenamiento/uso; Android no garantiza 50 Hz exactos, por eso se remuestrea en ambos lados.
- **Alternatives considered**: grabar a la frecuencia máxima (200–500 Hz) y diezmar — más información, pero los filtros internos del sensor cambian con la frecuencia y el modelo vería otra señal; Parquet o binario — el celular no lo escribe fácil y un archivo a medio escribir se corrompe; GZIP en el celular — un corte deja el archivo ilegible.

## R-003 · Seguridad del conductor en campo (Decisión SDD sobre HU-01 y ADR-004)

- **Decision**: el conductor **nunca** toca un celular con el vehículo en movimiento. Se inicia y se detiene la grabación **estacionado** (antes de salir y al llegar), en un lugar seguro fuera de la calzada. En marcha, solo un copiloto puede pulsar «Marca» (botón grande que guarda la hora); sin copiloto no hay marcas y el etiquetado sale del video. Un celular (el «de video», con la cámara hacia la vía) graba video; los tres se sincronizan por la hora GPS y con un **golpe de sincronización** (tres golpes con los nudillos al tablero, con el vehículo detenido, al inicio). Velocidad: la habitual y legal de la vía; **nunca** se busca un bache a propósito a más velocidad ni se esquiva tarde por el estudio. Los baches patrón se miden a pie solo con el vehículo fuera de la calzada y con chaleco. Checklist de salida en `protocolo-campo.md`: soportes firmes, celulares cargando (S-07), espacio libre, ruta y horario de día.
- **Rationale**: ADR-004 parte de que «el conductor no puede tocar el celular mientras maneja»; el estudio no puede contradecir el producto.
- **Alternatives considered**: marcas por voz (ruido y distracción); botón Bluetooth en el volante (otro aparato, también distrae); inicio automático por velocidad (innecesario: se graba todo el viaje y el pipeline aplica RN-03).

## R-004 · Privacidad del dataset (Decisión SDD sobre R-06 y RNF-10)

- **Decision**: (1) sin IMEI, `ANDROID_ID`, cuentas, nombres ni placas: celular y vehículo se identifican con un alias (`C1`, `V1`) y su modelo comercial; (2) grabador sin internet; (3) latitud y longitud solo en el crudo privado: el dataset derivado guarda **km** (redondeado a 10 m), sentido y velocidad; (4) crudo y video en el *bucket* R2 **privado** `killauru-datos`, nunca en git ni en `cdn`; (5) el video se borra a los **90 días** de congelar la versión del dataset (las etiquetas no lo necesitan para reproducir el entrenamiento); (6) quien maneja o acompaña firma un consentimiento corto (en `protocolo-campo.md`) aunque sea del equipo.
- **Rationale**: el video puede mostrar personas y placas (dato personal); una trayectoria con hora identifica a quien manejó. La política de privacidad declara «mejorar el modelo de detección» como finalidad (R-06), pero estos datos son del equipo, no de los conductores de la app.
- **Pendiente de confirmar por el autor**: ver el punto 3 al final.

## R-005 · Cómo se etiqueta (Decisión SDD sobre RN-01)

- **Decision**: `candidatos` propone un pico por evento (aceleración vertical sobre `UmbralCandidatoMs2`, bajo a propósito); el etiquetador abre el video en ese segundo y llena la hoja `etiquetas/<grabacion>.csv` (instante del pico, tipo, subtipo, severidad, fuente). Lo que no es candidato es Normal. Rompemuelles, juntas, badenes, rieles, ripio y cambios de pavimento son **Normal con subtipo** (RN-01 no los incluye; RS-07): no se crea una clase nueva. El umbral de candidatos se valida con los baches patrón: ninguna pasada sobre un patrón puede quedar fuera. Doble etiquetado del 10 % (otra persona o la misma ≥ 7 días después, sin ver la primera hoja): κ de Cohen del tipo ≥ 0.80 y severidad ± 1 en ≥ 90 %.
- **Rationale**: con una sola persona (R-09), etiquetar 200 000 ventanas a mano es imposible; etiquetar eventos y heredar a ventanas sí. Los subtipos de Normal son los «negativos difíciles» que más falsos baches producen.
- **Alternatives considered**: Label Studio (más instalación y servidor; el flujo es simple); etiquetar solo con marcas del copiloto (no siempre hay copiloto); etiquetado automático por umbral (sería entrenar con la línea base).

## R-006 · Criterios de severidad (Decisión SDD sobre RN-02)

- **Decision**: la severidad es del **bache**, no de la pasada: el significado de RN-02 para un vehículo liviano a velocidad normal. Guía (orientativa, en `guia-etiquetado.md`, con fotos): 1 = desnivel < 2 cm o fisura; 2 = 2–4 cm, se siente sin frenar; 3 = 4–7 cm o ancho > 40 cm, conviene reducir; 4 = 7–12 cm o borde vivo, hay que frenar; 5 = > 12 cm o que ocupa el carril, riesgo de perder el control. Un **catálogo de ≥ 20 baches patrón** medidos (profundidad y ancho con regla, km y sentido) fija la severidad de todas sus pasadas; los demás se etiquetan por comparación visual con los patrones. El modelo recibe la velocidad como entrada (R-010) para separar «bache fuerte a poca velocidad» de «bache leve a mucha».
- **Rationale**: RN-02 define la severidad por su efecto en el conductor; una severidad por pasada mezclaría la velocidad con el bache y bajaría el acuerdo entre etiquetadores.
- **Alternatives considered**: severidad por la aceleración pico (circular: el modelo aprendería un umbral); por la reacción del conductor (subjetiva y peligrosa de provocar).

## R-007 · Ventaneo, gravedad y contrato del preprocesamiento `p1` (Decisión SDD sobre RF-02)

- **Decision**: ventanas de 25 muestras (500 ms) **sin solapamiento**, igual que la app; gravedad = media del acelerómetro de los **últimos 2 s** (100 muestras, incluida la ventana), sin estado entre ventanas; componente vertical `a_v = a·ĝ − |g|`, horizontal `a_h = |a − (a·ĝ)ĝ|`, giro `w = |ω|`; velocidad = la del **último fix** GPS anterior al fin de la ventana (la app tampoco tiene otra). Etiqueta: la ventana que contiene el pico; vecinas a < 250 ms que no lo contienen = zona gris, excluidas; dos eventos en una ventana → el de mayor severidad (a igual, Bache). En entrenamiento se agregan copias con la ventana desplazada ± 200 ms alrededor del pico (aumento), nunca en validación ni prueba. Los vectores de prueba (FR-025) fijan el contrato con 008.
- **Rationale**: sin solapamiento, un bache produce un solo evento en la app; el aumento enseña al modelo a reconocer el golpe en cualquier lugar de la ventana. Una gravedad sin estado se replica exacta en Kotlin.
- **Alternatives considered**: ventanas solapadas al 50 % (doble costo y eventos duplicados en la app); sensor `TYPE_GRAVITY` (no existe igual en todos los celulares); filtro IIR (depende del estado inicial y complica la equivalencia).

## R-008 · Partición por recorrido y fuga de datos (Decisión SDD sobre HU-28)

- **Decision**: unidad de partición = **recorrido** (un viaje continuo con todos los celulares del vehículo). Los celulares de un mismo recorrido ven el mismo bache al mismo tiempo: si se separan, el modelo «ve» la prueba. La asignación se escribe a mano en `config/particion.yaml` antes de entrenar (≈ 60 % entrenamiento, 15 % validación, 25 % prueba, por horas), con ambos sentidos y ≥ 3 celulares en prueba, y se congela con la versión del dataset. «Por celular»: (a) si hay un 4.º celular, sus grabaciones van **solo** a prueba como «celular no visto»; (b) en validación se hace un diagnóstico dejando un celular fuera. «Baches no vistos»: métrica de prueba solo con eventos a > 20 m de cualquier evento de entrenamiento; es diagnóstico, no puerta. Prueba de no-fuga (`no_fuga.py`): recorridos y pares (recorrido, celular) disjuntos; normalización, prefiltro y temperatura ajustados sin prueba; ninguna huella de ventana de prueba en entrenamiento.
- **Rationale**: es la fuga más fácil de cometer con 3 celulares grabando a la vez. Partir por bloques de km haría imposible juntar 200 hundimientos en prueba.
- **Alternatives considered**: partición aleatoria por ventana (fuga segura); por bloques geográficos (pocos hundimientos por bloque; queda como diagnóstico); por celular solamente (contradice «≥ 3 celulares en prueba» con 3 celulares).

## R-009 · Distribución del conjunto de prueba (Decisión SDD; se pide confirmar)

- **Decision propuesta**: la puerta usa **todas** las ventanas válidas de los recorridos de prueba (distribución natural: muchas más Normal que anomalías), porque es lo que vive el mapa; además se informa una submuestra balanceada como diagnóstico.
- **Rationale**: con un conjunto balanceado la *precision* de Bache sale inflada; en la vía, cada falso bache llega al mapa.
- **Alternatives considered**: conjunto balanceado (200 por clase exactas): más fácil de cumplir, menos fiel. **Pendiente de confirmar por el autor** (punto 2).

## R-010 · Características en lugar de señal cruda (Decisión SDD sobre ADR-004)

- **Decision**: entrada de 20 valores: 19 características de la ventana (`data-model.md` §4: estadísticos de `a_v`, `a_h` y `w`, curtosis, cruces por cero, *jerk*, razón y orden de mínimo y máximo, energía relativa al contexto de 2 s) + velocidad en km/h.
- **Rationale**: con ~2 000–4 000 anomalías etiquetadas, un MLP sobre 150 valores crudos sobreajusta; las características capturan la «firma» de RN-01 (golpe con rebote vs caída sin rebote: orden y razón de picos). Además el prefiltro reutiliza el pico a pico vertical.
- **Alternatives considered**: señal cruda (25 × 6) en el MLP; CNN 1D (no es MLP, ADR-004); calcular las características dentro del grafo TFLite (cuantizar curtosis y razones en int8 es frágil).

## R-011 · Arquitectura, salidas y entrenamiento (Decisión SDD sobre ADR-004)

- **Decision**: Keras 3 sobre TensorFlow 2.18: `Normalization` (adaptada con entrenamiento) → `Dense(32, relu)` → `Dense(16, relu)` → dos cabezas: `tipo_logits` (3) y `severidad` (1, lineal). Pérdida: entropía cruzada con pesos de clase (inverso de la frecuencia) + 0,5 × Huber de severidad **solo** en anomalías (máscara). Adam, parada temprana con validación, ≤ 200 épocas. Exportación con `model.export()` (SavedModel) → `TFLiteConverter.from_saved_model` (Keras 3 recomienda ese camino). Línea base: reglas de umbral sobre pico a pico y orden (ADR-004 la descartó por baja precisión; aquí sirve de piso: el MLP debe superarla).
- **Rationale**: ~1 500 parámetros: < 1 ms por ventana y < 100 KB; regresión para la severidad porque la meta es «± 1».
- **Alternatives considered**: scikit-learn `MLPClassifier` (sin conversión a TFLite); PyTorch + `ai-edge-torch` (más dependencias para el mismo MLP); severidad como 5 clases (pierde el orden).

## R-012 · Qué se evalúa: la decisión del sistema (Decisión SDD sobre RNF-35)

- **Decision**: la clase predicha es la que **emitiría la app**: si el prefiltro descarta → Normal; si no, `tipo = argmax(softmax(logits/T))` y, si `tipo ≠ Normal` y su confidence < `ConfidenceMinimo` (0.75, RN-04) → Normal. Se evalúa el **`.tflite` cuantizado**. La severidad ± 1 se mide sobre las anomalías reales que el sistema detectó con el tipo correcto (las demás ya penalizan el *recall*). También se informa el `argmax` sin umbral, como diagnóstico.
- **Rationale**: RNF-35 habla del modelo que clasifica en el celular; medir otra cosa daría una nota que nadie vive.

## R-013 · Calibración y umbrales por versión (Decisión SDD sobre ADR-004)

- **Decision**: temperatura `T` ajustada en validación (minimiza la log-verosimilitud negativa) y publicada en el manifiesto; `umbralPrefiltro` por versión, también en el manifiesto. `ConfidenceMinimo` **no** va en el manifiesto: es RN-04/RN-11 y el servidor rechaza < 0.75 igual (RN-11); la calibración hace que 0.75 signifique lo mismo en todas las versiones.
- **Alternatives considered**: hornear `T` en el grafo (la app no tendría que aplicarla, pero recalibrar exigiría reconvertir); umbral de confidence por versión (choca con RN-04 y RN-11, que son únicos).

## R-014 · Cuantización y equivalencia (Decisión SDD sobre ADR-004)

- **Decision**: int8 completa (`Optimize.DEFAULT` + `representative_dataset` de ≤ 500 ventanas de entrenamiento + `TFLITE_BUILTINS_INT8`), con entrada y salida `float32` para que la app no maneje escalas. Equivalencia contra el modelo de Python sobre prueba: tipo igual ≥ 99 %, |Δ confidence| p99 ≤ 0.05, severidad redondeada igual ≥ 98 %. La puerta usa las métricas del `.tflite`, no las de Python.
- **Alternatives considered**: solo rango dinámico (pesos int8, activaciones float; también vale, más simple, pero ADR-004 habla de cuantizado y el int8 completo es el que corre más rápido en gama media); `float16` (no reduce cómputo en CPU).

## R-015 · *Benchmark* de RNF-04 (Decisión SDD)

- **Decision**: pantalla *benchmark* del grabador: carga el `.tflite` copiado por `adb push`, crea **un** intérprete LiteRT (CPU, 1 hilo, XNNPACK por defecto), 100 inferencias de calentamiento y 1 000 medidas con `System.nanoTime()` usando las características de `vectores-prueba.json`; guarda p50, p95, máximo, modelo del celular, RAM, año y versión de Android. Gama media = ≤ 4 GB de RAM y procesador lanzado en los últimos 4 años (RNF-04). Puerta: p95 < 50 ms en **cada uno** de ≥ 3 celulares de gama media.
- **Rationale**: `benchmark_model` de TensorFlow da promedio y desviación, no p95; una pantalla propia mide lo mismo que hará el Detector (intérprete reutilizado, `diseño-interno` §7.4). El tiempo del preprocesamiento en Kotlin lo mide 008.
- **Alternatives considered**: `benchmark_model` por `adb` (sin p95); Jetpack Microbenchmark dentro de 008 (la app aún no existe en la semana 2).

## R-016 · Versionado de datos y modelos (Decisión SDD sobre ADR-004 punto 4 y RF-33)

- **Decision**: `modeloVersion` semántica `MAYOR.MENOR.PARCHE` (patrón `^\d+\.\d+\.\d+$`, coherente con el `"1.2.0"` del contrato del lote de 005): MAYOR = cambia el catálogo (RN-01) o el contrato de salidas; MENOR = cambia `preprocesamientoVersion` o la arquitectura (sube `versionMinimaApp`, porque la app debe calcular otras entradas); PARCHE = reentrenamiento con más datos y el mismo contrato. Primera versión: `1.0.0`. Dataset: `ds-<n>` + huella SHA-256 de `catalogo.csv`, `etiquetas/`, `particion.yaml` y `etiquetado.yaml`. Crudo en R2 privado verificado contra el SHA-256 del catálogo. Cada informe guarda commit, `datasetVersion`, semilla y versiones del entorno.
- **Alternatives considered**: DVC (otra herramienta y otro remoto para ~30 carpetas); Git LFS (cuota gratis de 1 GB y el video no debe estar en GitHub); todo en git (pesa y expone datos).

## R-017 · Manifiesto, rutas en R2 y vuelta atrás (Decisión SDD sobre ADR-004 y RF-33)

- **Decision**: rutas en `cdn.killauru.pe`: `modelo/manifiesto.json` (puntero, `Cache-Control: max-age=300`; la ruta que ya lee 008 FR-070) y `modelo/<versión>/` con `killauru-<versión>.tflite`, `matriz-confusion.json` e `informe.json` (`max-age=31536000, immutable`). Hoy hay una sola categoría (campo `categoriaVehiculo = Liviano`); un modelo para otra categoría (ADR-004 punto 3) tendrá su propio manifiesto en `modelo/<categoria>/` sin tocar el actual. El manifiesto declara también `ventanaMs`, `frecuenciaHz` y `normalizacion = dentroDelModelo`, porque 008 toma esos datos del manifiesto (008 spec, «Reglas y parámetros»). Contenido: `contracts/manifiesto-modelo.schema.json`. Orden: archivos de la versión → verificación por la URL pública → manifiesto (si algo falla antes, el manifiesto viejo sigue vigente). Una ruta de versión nunca se sobrescribe (el script falla si ya existe); volver atrás = publicar de nuevo el manifiesto de la versión anterior (`ml/modelos/<anterior>/manifiesto.json`).
- **Rationale**: ADR-004 pide versión, URL y SHA-256; RF-33 pide versión mínima de la app; los umbrales por versión (R-013) y las métricas permiten al panel o a quien audite ver qué se publicó.

## R-018 · Publicación con GitHub Actions (Decisión SDD sobre D10 y RNF-34)

- **Decision**: `publicar-modelo.yml` con `workflow_dispatch` (entrada `version`, opción `volver_atras`), entorno de GitHub `modelos`: instala `requirements.lock`, corre `python -m killauru_ml puerta --version X` (recalcula SHA-256, tamaño, clases, métricas, *benchmarks* y equivalencia con el intérprete LiteRT), y `scripts/publicar_r2.sh` sube con `rclone` y verifica con `curl` + `sha256sum`. Secretos: `KU_R2_MODELOS_ACCESS_KEY_ID`, `KU_R2_MODELOS_SECRET_ACCESS_KEY` (token de R2 limitado al prefijo `modelo/` del *bucket* público) y `KU_R2_ENDPOINT`. `ml-ci.yml` corre en cada PR que toca `ml/`.
- **Rationale**: D10 eligió GitHub Actions; publicar a mano desde la laptop no deja rastro ni repite la puerta. RNF-34 ya lista «credenciales de R2»; los tres secretos nuevos se agregan a su inventario (ver «Para llevar a docs»).
- **Alternatives considered**: reentrenar en CI (el dataset crudo es privado y no hace falta: el `.tflite` está en git con su informe); publicar con `wrangler` (otra herramienta; `rclone` ya lo usa el backup de 001).

## R-019 · Entorno reproducible y semillas (Decisión SDD)

- **Decision**: Python 3.11; TensorFlow 2.18 (CPU) con Keras 3; NumPy, pandas, scikit-learn, PyYAML, jsonschema, matplotlib, `ai-edge-litert`, pytest y ruff en versiones compatibles, **resueltas una vez** con `pip-compile --generate-hashes` en `ml/requirements.lock` (T001) y cambiadas solo por PR. Semilla única `2828` para Python, NumPy y TensorFlow (`keras.utils.set_random_seed`) + `tf.config.experimental.enable_op_determinism()`; entrenamiento en CPU. El informe guarda `pip freeze` resumido.
- **Rationale**: SC-003 pide repetir el resultado; TF 2.18 aún trae `TFLiteConverter` (las versiones posteriores lo llevan a LiteRT y conviene no depender de ese cambio a mitad del proyecto).
- **Alternatives considered**: Conda (más pesado en Windows); Docker (útil para CI, innecesario para un MLP en CPU).

## R-020 · Tamaño mínimo, metas de campo y cronograma (Decisión SDD sobre HU-28 y R-10)

- **Decision**: metas por clase: prueba ≥ 200 (RNF-35), validación ≥ 100, entrenamiento ≥ 400 de cada anomalía (≈ 3 × prueba entre entrenamiento y validación). Normal sobra. Campaña: 3 celulares a la vez, ~10 recorridos de ~65 km (5 idas y vueltas, ~1,3 h cada uno) ≈ 13 h de manejo ≈ 39 h-celular; cada pasada por un hundimiento da 3 ventanas. Cronograma (días hábiles de la feature): 1–2 grabador + piloto de 20 min + `ingerir`; 3–6 campaña y baches patrón; 4–9 etiquetado (en paralelo); **día 7: control de conteos** (`estado`): si Hundimiento < 60 % de la meta → pasadas dirigidas por los km del catálogo; 8–11 entrenamiento y validación; **día 11: control de métricas** (validación ≥ 0,85 por clase o plan B); 12–14 prueba única, cuantización, *benchmark*, puerta y publicación.
- **Rationale**: HU-28 estima ~2 semanas y pide empezar en la semana 1.

## R-021 · Plan B si no hay modelo a tiempo (docs no lo define: propuesta)

- **Búsqueda en docs**: no hay plan B; ADR-004 solo descarta «reglas manuales con umbrales fijos» por baja precisión y `02-historias` define el orden de recorte (HU-20 → HU-10 → RF-33 → HU-13 → HU-19 → HU-17).
- **Propuesta escalonada**: **B1 (día 7)**: faltan datos → pasadas dirigidas y, si hace falta, se recorta en el orden de docs (HU-20, HU-10, RF-33) para darle 1 semana más a HU-28 (la semana 3). **B2 (día 11)**: falla una clase → más negativos difíciles, ajustar pesos de clase y características (sin tocar la prueba). **B3 (fin de la semana 3)**: si la puerta sigue sin pasar, el autor elige: (a) salir con el mejor modelo marcado `1.0.0-preliminar` **solo en piloto cerrado** y declarar el desvío de RNF-35 en el informe; (b) publicar la **línea base de umbrales** empaquetada como `.tflite` con el mismo contrato (la app no cambia), con su matriz de confusión; (c) postergar la detección y sustentar con mapa, alertas y backend sobre datos sintéticos. Las tres mantienen el contrato de 008 y el servidor (que ya filtra con RN-11 y confirma lo grave con RN-06).
- **Pendiente de confirmar por el autor** (punto 1).

## R-022 · Prefiltro y batería (Decisión SDD sobre RNF-15 y `diseño-interno` §7.4)

- **Decision**: `umbralPrefiltro` = el mayor pico a pico vertical que deja pasar ≥ 99 % de las ventanas de anomalía de entrenamiento. El informe publica la **tasa de paso** en Normal (qué fracción de ventanas llega al modelo), para que 008 la use al perfilar la batería. No es puerta: RNF-15 se mide en campo en 008.

## Propuesta de enmienda a la constitución (no aplicada; la decide el autor)

> La constitución 1.1.0 está escrita para el backend .NET. No se edita aquí. Propuesta **MENOR → 1.2.0** (se amplían reglas, no se quita ninguna):

1. **VII. Pruebas primero**: agregar «En `ml/`, la pirámide es pytest: funciones puras del pipeline (ventaneo, etiquetas, partición, métricas, puerta) con valores límite → integración con datos sintéticos → determinismo y equivalencia. `ml-ci.yml` corre en cada PR que toca `ml/`; sin verde no se une a `main`. Las métricas del modelo se prueban con la puerta, no con cobertura».
2. **VIII. Código pequeño**: incluir `.py` y `.kt` (y `.kts` de Gradle) en el límite de 100 líneas, con la prueba `ml/tests/test_limite_lineas.py` (y la de 008 para `android/`); prohibir notebooks en el pipeline; equivalente a `BannedApiAnalyzers`: en `ml/`, prohibido `datetime.now()` en lógica (se pasa la fecha como argumento) y `random` sin semilla (regla de ruff + prueba).
3. **I. Docs manda**: «Un modelo solo se publica si su informe demuestra RNF-35 y RNF-04 (puerta); sus umbrales viven en `ml/config/puerta.yaml` citando la fuente».
4. **Restricciones**: fila nueva «ML: Python 3.11 + TensorFlow 2.18 → LiteRT int8; dataset crudo en R2 privado; publicación manual con GitHub Actions (R-018)».
5. **III**: aclarar que `TimeProvider` aplica al backend; en `ml/` y en la app rige el equivalente (fecha inyectada).

## Para llevar a docs (D1, si el autor lo confirma)

- `04-atributos-de-calidad.md` RNF-34: agregar `KU_R2_MODELOS_ACCESS_KEY_ID`, `KU_R2_MODELOS_SECRET_ACCESS_KEY` y `KU_R2_ENDPOINT` como secretos de GitHub.
- `10-glosario.md` «Manifiesto del modelo»: agregar versión mínima de la app, temperatura, umbral del prefiltro y métricas; ruta `modelo/manifiesto.json`.
- `07-decisiones-arquitectonicas.md` ADR-004: el grabador aparte, la puerta de publicación y el `.tflite` evaluado (no el de Python).
- `05-restricciones.md`: riesgo «sin modelo a tiempo» (R-021) con su mitigación.

## Pendientes de confirmar por el autor

1. **Plan B** (R-021): cuál de B3 (a), (b) o (c) se aplica si al final de la semana 3 la puerta no pasa.
2. **Distribución del conjunto de prueba** (R-009): natural (propuesta) o balanceada. Cambia qué significa «cumple RNF-35».
3. **Video de la vía** (R-004): aceptar grabar video con posibles personas y placas, solo para etiquetar, privado y borrado a los 90 días; sin video, el etiquetado depende de un copiloto en cada recorrido.
