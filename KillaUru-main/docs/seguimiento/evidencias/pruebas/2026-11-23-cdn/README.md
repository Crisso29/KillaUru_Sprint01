# CDN del mapa: de los datos del corredor al mapa sin señal

> Generación de los datos con el contrato `specs/008-app-android/contracts/cdn.openapi.yaml`, CDN local con nginx y
> recorrido en el emulador. [Volver a evidencias](../../README.md).

| Dato | Valor |
|---|---|
| Fechas | 2026-11-22 y 2026-11-23 |
| Datos | Eje canónico `backend/db/datos/corredor_pe28a.geojson`; mapa base extraído de la compilación **20261120** de Protomaps (OSM, esquema 4.15.2) |
| CDN | Local: nginx 1.27 en Docker (`Range`, `ETag`), en `http://10.0.2.2:8090/mapa/`. **No es R2**: la publicación real es `app-datos-r2` (T127, la corre el autor) |
| Emulador | `pixel_7_-_api_35` (Android 15), APK de depuración con `-Pkillauru.cdnUrlDepuracion` |
| Pruebas | App: 391 JVM, lint y detekt sin hallazgos. Herramientas CDN: 6 (`test_datos_cdn.py`). Backend: tramo demo 5 (`test_construir_tramo_demo.py`) |

## Datos generados (versión 1)

| Archivo | Tamaño | Comprobación |
|---|---|---|
| `corredor-pe28a.v1.pmtiles` | 2 362 855 B (2,3 MB), zoom 0–14 | ≤ 80 MB (RNF-15); `pmtiles show` |
| `corredor_pe28a.v1.json` | 243 545 B, 6 689 vértices, 335,073 km | esquema `Corredor`; km de `km_referencia.csv` < 1 m |
| `tramo_demo.v1.geojson` | 6 212 B, **115 vértices**, margen 200 m | toda la vía del km 268 al final dentro; km 258 fuera |
| `estilo.v1.json` | 3 453 B | lee el PMTiles de la misma versión; atribución OSM |
| `manifiesto.json` | 1 110 B | esquema `ManifiestoMapa` (validado contra el YAML) |

CDN local: el manifiesto responde **200 con ETag** y **304** con `If-None-Match`; el PMTiles responde **206** con
`Content-Range` a `Range: bytes=1000000-`.

## En el emulador

| Qué | Prototipo | Real |
|---|---|---|
| Paso 4 «Mapa sin conexión», descargando | [prototipo 05](prototipo-05-mapa-sin-conexion.png) | [46 % · 2,5 MB](real-paso4-descargando-46.png) |
| Paso 4 terminado | — | [100 % · Listo](real-paso4-listo.png) |
| Mapa sin señal (PMTiles del celular) | [prototipo 07](prototipo-07-mapa.png) | [v1](real-mapa-v1-vias-doradas.png) · [v2 tras actualizar](real-mapa-v2-actualizado.png) |
| «Aún no tienes el mapa» | [prototipo 13](prototipo-13-sin-mapa.png) | `MapaUiTest` (2 casos) |

- **Actualización real v1 → v2**: con v1 instalada se publicó v2 en la CDN local; al abrir la app, `IniciarApp` la
  descargó, la activó y borró los archivos de v1 (registro de WorkManager: `Worker result SUCCESS`).
- **Corte a mitad de descarga**: con la CDN a 40 KB/s se detuvo el contenedor al 26 %; el celular conservó
  `corredor-pe28a.v2.pmtiles.parcial` con 860 160 B. La continuación en el emulador **no se llegó a ver**: el sistema
  detuvo el emulador por falta de memoria. La reanudación con `Range` está probada en `DescargaReanudableTest` (6) y la
  CDN local respondió 206 con `curl`.

## Fallos encontrados y corregidos (cada uno con su prueba)

- Generador, flujo de publicación y app usaban **tres formatos distintos** del manifiesto y del corredor; ninguno era el
  del contrato, y la prueba de contrato de la app usaba un JSON escrito a mano.
- El flujo pasaba las claves de R2 con nombres que el CLI `aws` no lee, y subía a `/corredor/` sin PMTiles ni estilo.
- `km_referencia.csv` de las herramientas tenía coordenadas inventadas; se usa el del backend (20 puntos reales).
- El polígono demo del backend era un cuadrilátero de 5 puntos; ahora es el real (006 T002). La regla «200 m y ≤ 60
  vértices» era imposible en esta vía (mínimo 111): el autor eligió 200 m y ≤ 120 (R-010).
- **La app enviaba el token y el seudónimo del conductor a la CDN pública** (compartía el cliente de la API).
- Nadie pedía el manifiesto ni programaba la descarga; tampoco la sincronización ni la purga (`IniciarApp` no existía).
- `enDemo` nunca se calculaba: cerrar sesión habría borrado también las anomalías del demo.
- `CargadorCorredor` y `CargadorZonaDemo` leían `assets` que no existían.
- Una carga vieja del mapa podía pisar a la nueva y apuntar a archivos ya borrados (visto en el emulador).
- En las pruebas, hilos de Room y la `Application` real seguían vivos entre pruebas.

## Lo que queda, con su motivo

- **Publicar en R2**: correr `app-datos-r2` con `ensayo: true` y luego real (T127), con los secretos del autor.
- **T129** (numerada T128 por error hasta el 2026-11-23): sombreado del tramo demo y aviso fuera del demo (prototipo 08).
- **Panel**: apuntar `VITE_URL_ESTILO_MAPA` al estilo publicado en `/mapa/estilo.vN.json`.
- Encima del mapa: estado de la detección, velocidad, hito y alertas (prototipos 07, 10–12): llegan con la detección.
