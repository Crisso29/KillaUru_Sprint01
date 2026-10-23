# Contratos de la feature 009 (lo que consume 008-app-android)

> Este archivo **define** el contrato del modelo y del manifiesto. 008-app-android (escrita en paralelo) lo consume: lee `modelo/manifiesto.json` (008 FR-070, FR-071), toma del manifiesto la ventana, la frecuencia y la normalización, y funde ventanas positivas seguidas (008 FR-014). Si alguna de las dos necesita cambiarlo, se versiona (`preprocesamientoVersion`, `modeloVersion` MENOR o MAYOR, `versionMinimaApp`) y se actualizan las dos specs a la vez.

## 1. Lo que 009 **ofrece**

| Artefacto | Dónde | Para qué (en 008) |
|---|---|---|
| `killauru-<versión>.tflite` | `ml/modelos/<versión>/` (git) y `cdn.killauru.pe/modelo/<versión>/` | Modelo inicial en `assets/` del APK (ADR-004 punto 1) y descarga por el Actualizador (RF-33) |
| `manifiesto.json` | ídem; el vigente en `cdn.killauru.pe/modelo/manifiesto.json` (la ruta que lee 008 FR-070) | El APK lleva el de su modelo inicial; el Actualizador compara versión, `versionMinimaApp` y SHA-256 |
| `vectores-prueba.json` | `ml/modelos/<versión>/` | Prueba unitaria del preprocesamiento y del Detector en Kotlin |
| `contracts/manifiesto-modelo.schema.json` | esta carpeta | Validar el manifiesto antes de usarlo |
| Parámetros de captura | `ml/config/captura.yaml` | Mismo muestreo y lote que el grabador |

## 2. Firma del modelo

| Tensor | Nombre | Forma | Tipo | Contenido |
|---|---|---|---|---|
| Entrada | `caracteristicas` | `[1, 20]` | `float32` | Las 20 entradas de `data-model.md` §4, **sin normalizar** (la normalización está en el grafo) |
| Salida 0 | `tipo_logits` | `[1, 3]` | `float32` | *Logits* en el orden `Normal`, `Bache`, `Hundimiento` |
| Salida 1 | `severidad` | `[1, 1]` | `float32` | Valor continuo; la app usa `clamp(round(x), 1, 5)` |

## 3. Decisión del Detector (la misma que se evalúa en 009, R-012)

```text
cada 500 ms (25 muestras remuestreadas a 50 Hz) con RN-03 y RN-18 cumplidas:
  contexto = últimos 2 s de acelerómetro (100 muestras)
  calcular a_v de la ventana                                   # data-model §4, pasos 1–3
  si (max(a_v) − min(a_v)) < manifiesto.prefiltro.umbralPicoAPicoVerticalMs2 → Normal (no se llama al modelo)
  x = características p1 (20 valores)
  logits, sev = modelo(x)                                      # intérprete reutilizado, buffers preasignados
  p = softmax(logits / manifiesto.calibracion.temperatura)
  tipo = argmax(p)
  si tipo ≠ Normal y p[tipo] ≥ ConfidenceMinimo (RN-04, configuración de la app):
      evento(tipo, severidad = clamp(round(sev), 1, 5), confidence = p[tipo], modeloVersion = manifiesto.modeloVersion)
```

- Normal **nunca** se guarda ni se envía (RN-01).
- `ConfidenceMinimo` no viene del manifiesto (R-013).
- Ventanas **sin solapamiento** (R-007). Si un golpe cae entre dos ventanas y ambas salen positivas, 008 las funde en un solo evento (008 FR-014, `FusionVentanasMs`).
- `ventanaMs` (500), `frecuenciaHz` (50) y `normalizacion` (`dentroDelModelo`) vienen en el manifiesto: la app **no** vuelve a normalizar las entradas.

## 4. Vectores de prueba (`vectores-prueba.json`)

```json
{ "modeloVersion": "1.0.0", "preprocesamientoVersion": "p1", "tolerancias": { "caracteristica": 1e-4, "logit": 0.02, "severidad": 0.02 },
  "casos": [ { "id": "bache-007", "acelerometro2s": [[0.12, 9.71, 0.33], "... 100 filas"], "giroscopioVentana": [[0.01, 0.0, 0.02], "... 25 filas"],
               "velocidadKmh": 47.5, "pasaPrefiltro": true, "caracteristicas": [/* 20 */], "tipoLogits": [-2.1, 3.4, 0.2], "severidad": 3.2,
               "decision": { "tipo": "Bache", "severidad": 3, "confidence": 0.93 } } ] }
```

Al menos 60 casos (20 por clase, incluidos Normal con subtipo y casos cerca del umbral del prefiltro y de 0.75). La señal ya viene remuestreada a 50 Hz.

## 5. Captura que 008 debe respetar (igual que el grabador)

| Parámetro | Valor | Fuente |
|---|---|---|
| Acelerómetro y giroscopio | `samplingPeriodUs = 20 000` (50 Hz) | RF-01 |
| Entrega en lote | `maxReportLatencyUs ≈ 1 000 000` | `diseño-interno` §7.4 |
| Reloj | `event.timestamp` y `Location.elapsedRealtimeNanos` | R-002 |
| Remuestreo | lineal a malla de 50 Hz | R-002 |
| GPS | 1 Hz, velocidad del último fix | `diseño-interno` §7.4; R-007 |

## 6. Lo que 009 **usa**

| De | Qué |
|---|---|
| 004 (D16) | `backend/db/datos/corredor_pe28a.geojson`: polilínea para pasar GPS → km y sentido al etiquetar |
| 001 (D10) | Convenciones de GitHub Actions y manejo de secretos (RNF-34) |
| Cloudflare R2 (ADR-010) | *Bucket* público `cdn.killauru.pe` (prefijo `modelo/`) y *bucket* privado `killauru-datos` |
