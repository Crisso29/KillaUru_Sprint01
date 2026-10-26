# Formato de una grabación (salida del grabador, entrada de `ingerir`)

Una carpeta por celular y recorrido: `<AAAAMMDD>-<recorrido>-<celular>/`. Texto UTF-8, separador coma, punto decimal, encabezado en la primera línea. El grabador escribe por bloques de ~1 s (lo que entrega el lote de sensores) y hace `flush` en cada bloque.

## `meta.json`

```json
{
  "grabacionId": "20261026-R03-C2", "recorrido": "R03", "grabadorVersion": "1.0.0",
  "celular": { "alias": "C2", "marcaModelo": "Samsung Galaxy A34", "ramGb": 4, "anioLanzamiento": 2023, "android": "14" },
  "vehiculo": { "alias": "V1", "categoria": "Liviano" },
  "soporte": { "tipo": "rejilla", "posicion": "centro-tablero" },
  "tramo": { "kmInicio": 268.0, "kmFin": 335.073, "sentido": "VinchosAyacucho" },
  "fecha": "2026-10-26", "esCelularVideo": false,
  "parametros": { "samplingPeriodUs": 20000, "maxReportLatencyUs": 1000000, "gpsIntervaloMs": 1000 }
}
```

`categoria` solo admite `Liviano` (RN-18, S-06); `soporte.tipo` ∈ {`rejilla`, `parabrisas`, `tablero`, `cd`}. No hay campos para nombres, placas ni identificadores del aparato (FR-005).

## Archivos de datos

| Archivo | Encabezado | Unidades |
|---|---|---|
| `acelerometro.csv` | `t_ns,x,y,z` | ns (`elapsedRealtimeNanos`), m/s² con gravedad (`TYPE_ACCELEROMETER`) |
| `giroscopio.csv` | `t_ns,x,y,z` | ns, rad/s (`TYPE_GYROSCOPE`) |
| `gps.csv` | `t_ns,utc_ms,lat,lon,precision_m,velocidad_ms,rumbo` | `utc_ms` = `Location.time` (hora del GPS, RF-03) |
| `marcas.csv` | `t_ns,origen` | `copiloto` o `sincronizacion` |
| `video.mp4`, `video.json` | `{ "t_ns_primer_cuadro": …, "fps": 30 }` | solo con `esCelularVideo = true` |

## Verificaciones de `ingerir` (FR-007)

1. Están los cinco archivos (y los dos de video si `esCelularVideo`); `meta.json` cumple este formato.
2. `t_ns` crece en cada archivo; frecuencia efectiva ≥ 45 Hz; huecos > 200 ms en ≤ 1 % del tiempo.
3. Hay GPS con `precision_m` ≤ 20 m (RN-11) en ≥ 80 % del tiempo en marcha; si no → `SinGps`.
4. Se detecta el golpe de sincronización en los primeros 2 min (aviso, no error).
5. Se calcula el SHA-256 del `.tar` sin video y se escribe la fila del catálogo.
