# Data Model: Dataset y modelo de detección

No hay tablas SQL: los datos son archivos versionados. Lo marcado **(git)** se sube al repositorio; **(R2 privado)** vive en el *bucket* `killauru-datos`; **(caché)** se regenera en `ml/.cache/`.

## 1. Grabación (R2 privado) — detalle en `contracts/formato-grabacion.md`

Carpeta `<grabacion_id>/`, con `grabacion_id = <AAAAMMDD>-<recorrido>-<celular>` (p. ej. `20261026-R03-C2`).

| Archivo | Columnas / campos | Notas |
|---|---|---|
| `meta.json` | `grabacionId`, `recorrido`, `celular{alias, marcaModelo, ramGb, anioLanzamiento, android}`, `vehiculo{alias, categoria: "Liviano"}`, `soporte{tipo, posicion}`, `tramo{kmInicio, kmFin, sentido}`, `fecha`, `grabadorVersion`, `esCelularVideo`, `parametros{samplingPeriodUs, maxReportLatencyUs}` | Sin IMEI, `ANDROID_ID`, nombres ni placas (FR-005) |
| `acelerometro.csv` | `t_ns, x, y, z` (m/s²) | `t_ns` = `event.timestamp` (base `elapsedRealtimeNanos`) |
| `giroscopio.csv` | `t_ns, x, y, z` (rad/s) | ídem |
| `gps.csv` | `t_ns, utc_ms, lat, lon, precision_m, velocidad_ms, rumbo` | `utc_ms` = hora del fix GPS (RF-03) |
| `marcas.csv` | `t_ns, origen` | `origen` ∈ {`copiloto`, `sincronizacion`} |
| `video.mp4` + `video.json` | `video.json`: `t_ns_primer_cuadro`, `fps` | Solo el celular de video; se borra a los 90 días del congelamiento (R-004) |

## 2. Catálogo de grabaciones — `ml/datos/catalogo.csv` (git)

| Columna | Tipo | Regla |
|---|---|---|
| `grabacion_id` | texto | único |
| `recorrido` | texto (`R01`…) | unidad de partición (R-008) |
| `celular` / `vehiculo` | alias | sin identificadores reales |
| `sentido` | `AyacuchoVinchos` \| `VinchosAyacucho` | — |
| `horas` | decimal | duración con RN-03 cumplida |
| `hz_efectivo` | decimal | ≥ 45 para `Apta` |
| `huecos_pct` | decimal | % del tiempo en huecos > 200 ms; ≤ 1 para `Apta` |
| `sha256` | 64 hex | huella del `.tar` de la carpeta sin video |
| `estado` | `Apta` \| `NoApta` | — |
| `motivo` | texto | `FrecuenciaBaja`, `Huecos`, `MetaIncompleta`, `SoporteInestable`, `SinGps` |

## 3. Etiquetas (git)

`ml/datos/etiquetas/<grabacion_id>.csv`

| Columna | Tipo | Regla (FR-009) |
|---|---|---|
| `t_pico_ns` | entero | dentro de la grabación |
| `tipo` | `Bache` \| `Hundimiento` \| `Normal` | exactamente RN-01 |
| `subtipo` | `Rompemuelles` \| `Junta` \| `Baden` \| `Riel` \| `Ripio` \| `CambioPavimento` \| vacío | solo con `Normal` |
| `severidad` | 1–5 | obligatoria con Bache/Hundimiento; vacía con Normal |
| `fuente` | `video` \| `copiloto` \| `patron` | — |
| `etiquetador` | alias (`E1`, `E2`) | — |
| `bache_patron` | id o vacío | si viene, la severidad debe ser la del patrón |

`ml/datos/baches-patron.csv`: `id, km, sentido, tipo, profundidad_cm, ancho_cm, severidad, foto` (la foto vive en R2 privado, sin personas ni placas).

Doble etiquetado: `ml/datos/etiquetas/doble/<grabacion_id>.csv` con el mismo formato; `acuerdo` empareja eventos a ± 250 ms.

## 4. Preprocesamiento `p1` y vector de entrada (contrato con 008)

Pasos por ventana (25 muestras, 500 ms), con el contexto de 2 s (100 muestras) que termina en la ventana:

1. Remuestreo lineal de acelerómetro y giroscopio a la malla de 50 Hz.
2. `g = media(acelerómetro, 100 muestras)`; `ĝ = g/|g|`.
3. `a_v = a·ĝ − |g|`; `a_h = |a − (a·ĝ)ĝ|`; `w = |ω|` (por muestra de la ventana).

| # | Nombre | Definición (sobre las 25 muestras de la ventana) |
|---|---|---|
| 0 | `v_media` | media de `a_v` |
| 1 | `v_desv` | desviación estándar poblacional de `a_v` |
| 2 | `v_min` | mínimo de `a_v` |
| 3 | `v_max` | máximo de `a_v` |
| 4 | `v_pap` | `v_max − v_min` (lo usa el prefiltro) |
| 5 | `v_rms` | raíz de la media de `a_v²` |
| 6 | `v_curtosis` | curtosis de Pearson − 3 (0 si `v_desv` < 1e-6) |
| 7 | `v_cruces` | cruces por cero de `a_v − v_media` |
| 8 | `v_jerk` | máx \|Δ`a_v`\| × 50 (m/s³) |
| 9 | `v_razon` | \|`v_min`\| / (`v_max` + 1e-6) |
| 10 | `v_orden` | +1 si el máximo ocurre antes que el mínimo; −1 si no |
| 11 | `h_desv` | desviación de `a_h` |
| 12 | `h_max` | máximo de `a_h` |
| 13 | `h_pap` | pico a pico de `a_h` |
| 14 | `h_rms` | rms de `a_h` |
| 15 | `w_desv` | desviación de `w` |
| 16 | `w_max` | máximo de `w` |
| 17 | `w_rms` | rms de `w` |
| 18 | `v_energia_rel` | `v_rms` / (rms de `a_v` en los 2 s de contexto + 1e-6) |
| 19 | `velocidad_kmh` | velocidad del último fix GPS anterior al fin de la ventana × 3,6 |

Todo en `float32`. La normalización está **dentro** del modelo (R-011): la app entrega estos 20 valores crudos.

## 5. Ventana del dataset (caché, `ventanas-<ds>.npz`)

`x[20]`, `tipo`, `severidad`, `grabacion_id`, `recorrido`, `celular`, `km` (redondeado a 10 m), `sentido`, `conjunto` (`entrenamiento` \| `validacion` \| `prueba`), `zona_gris` (excluida), `aumentada` (solo entrenamiento), `huella` (SHA-256 de `x` + `grabacion_id` + índice, para la prueba de no-fuga). **Sin** latitud ni longitud (FR-005).

## 6. Partición — `ml/config/particion.yaml` (git)

```yaml
datasetVersion: ds-1
congelada: true            # una vez true, cualquier cambio exige ds-2
entrenamiento: [R01, R02, R04, R05, R07, R08]
validacion:    [R03, R09]
prueba:        [R06, R10]
celularNoVisto: null       # p. ej. C4: todas sus grabaciones van solo a prueba (R-008)
```

Validaciones (`particion.py`, `tamano.py`): cada recorrido del catálogo en exactamente un conjunto; tamaños de FR-014; ≥ 3 celulares y ambos sentidos en prueba.

## 7. Estados de una versión del modelo

| Desde | Hacia | Disparador |
|---|---|---|
| (nace) | `Candidata` | `todo --version X` termina y escribe `informe.json` |
| `Candidata` | `Aprobada` | `puerta --version X` termina con código 0 y escribe `manifiesto.json` |
| `Candidata` | `Rechazada` | la puerta falla (queda en el registro; la versión no se reutiliza) |
| `Aprobada` | `Publicada` | `publicar-modelo.yml` reemplaza el manifiesto en R2 |
| `Publicada` | `Retirada` | otro manifiesto la reemplaza (vuelta atrás o versión nueva); sus archivos se conservan |

`ml/modelos/registro-evaluaciones.csv`: `fecha_utc, version, datasetVersion, commit, conjunto (prueba), resultado (Aprobada/Rechazada), motivo`.

## 8. Informe — `ml/modelos/<versión>/informe.json` (git, generado)

```json
{
  "modeloVersion": "1.0.0", "datasetVersion": "ds-1", "huellaDataset": "<sha256>",
  "commit": "<sha git>", "semilla": 2828, "preprocesamientoVersion": "p1",
  "entorno": { "python": "3.11.x", "tensorflow": "2.18.x" },
  "tamanos": { "prueba": { "Normal": 41250, "Bache": 640, "Hundimiento": 231 }, "celularesPrueba": ["C1", "C2", "C3"] },
  "metricas": {
    "precision": { "Normal": 0.998, "Bache": 0.93, "Hundimiento": 0.91 },
    "recall":    { "Normal": 0.999, "Bache": 0.92, "Hundimiento": 0.90 },
    "severidadMasMenos1": 0.86
  },
  "diagnostico": { "porCelular": {}, "bachesNoVistos": {}, "balanceado": {}, "argmaxSinUmbral": {}, "lineaBase": {}, "tasaPasoPrefiltroNormal": 0.18 },
  "calibracion": { "temperatura": 1.35 }, "umbralPrefiltro": 1.2,
  "equivalencia": { "tipoIgual": 0.997, "deltaConfidenceP99": 0.03, "severidadIgual": 0.99 },
  "tamanoBytes": 9216, "cumpleRnf35": true
}
```

Los números del ejemplo son ilustrativos. `matriz-confusion.csv/json`: filas = real, columnas = predicho por la decisión del sistema, orden `Normal, Bache, Hundimiento`, conteos absolutos.

## 9. Puerta — `ml/config/puerta.yaml` (git)

```yaml
rnf35: { precisionMinima: 0.90, recallMinimo: 0.90, severidadMasMenos1: 0.80, ventanasPruebaPorClase: 200, celulares: 3 }   # RNF-35
rnf04: { latenciaP95MaximaMs: 50, celulares: 3, ramMaximaGb: 4, antiguedadMaximaAnios: 4 }                                  # RNF-04
adr004: { tamanoMaximoMb: 5, clases: [Normal, Bache, Hundimiento] }                                                         # ADR-004, RN-01
equivalencia: { tipoIgual: 0.99, deltaConfidenceP99: 0.05, severidadIgual: 0.98 }                                           # R-014
etiquetado: { kappaMinimo: 0.80, severidadMasMenos1: 0.90 }                                                                 # R-005
confidenceMinimo: 0.75                                                                                                      # RN-04 (no va al manifiesto)
```

Regla de comparación: «≥» para mínimos (0,900 pasa, 0,899 no) y «<» estricto para la latencia (49,9 pasa, 50,0 no).
