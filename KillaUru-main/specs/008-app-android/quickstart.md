# Quickstart: comprobar la app Android

> Vigencia documental (2026-11-15): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

## Requisitos previos

JDK 21 para ejecutar las herramientas (destino JVM 17), Android SDK (API 35) y un emulador o celular con Android 8.0+. Para el punta a punta: 003, 005 y 006 en el entorno de pruebas (R-11) y `BuildConfig.API_URL` apuntando a él. Para US8: Python 3.12 y la CLI `pmtiles`. La suite instrumentada y la validación de campo siguen pendientes; ver [cierre.md](cierre.md).

## 1. Pruebas automáticas

```powershell
cd app
./gradlew assembleDebug lint detekt               # build con advertencias como errores
./gradlew testDebugUnitTest                       # dominio, Robolectric, contrato (MockWebServer + YAML), Konsist
./gradlew koverVerify                             # ≥ 90 % de los paquetes dominio
python -I herramientas/verificar_lineas.py        # ningún .kt/.kts/.py/.sh > 199 líneas
./gradlew connectedDebugAndroidTest               # migraciones (MigrationTestHelper), benchmark, Compose en dispositivo
```

## 2. Escenarios que deben pasar

| # | Escenario | Esperado |
|---|---|---|
| 1 | Primer uso sin marcar una casilla | botón inactivo; ni mapa ni detección |
| 2 | Pesado / Moto | «Detección apagada (vehículo Pesado o Moto)»; mapa sí; alertas solo con cuenta |
| 3 | Reproductor: 6 km/h durante 9 s y luego 10 s | sensores apagados a los 9 s; encendidos a los 10 s |
| 4 | Ventana Bache 0.74 / 0.75 | no se guarda / se guarda `PENDING` con UUIDv7 y hora GPS |
| 5 | Batería 14 % sin cargar | «En pausa: batería baja»; no envía; alertas sí |
| 6 | 120 pendientes, red | lotes 50 · 50 · 20, más antiguos primero, un seudónimo cada uno |
| 7 | 200 con `{id: null, indice: 7}` | el evento 7 → `REJECTED_LOCAL`; nombrados → `SYNCED`; no mencionados → `PENDING` |
| 8 | 400 en un lote de 50 con 1 culpable | se parte hasta aislarlo; solo él `REJECTED_LOCAL` |
| 9 | 401 con renovación correcta / `cuenta_desactivada` | mismo lote reenviado con token nuevo / permiso borrado, copia recortada, lote sin token |
| 10 | 503 con `Retry-After: 120` | siguiente intento entre 120 y 150 s |
| 11 | 500 repetido | esperas 30 s, 2 min, 10 min, 30 min, 2 h (± 25 %) |
| 12 | Modo avión → red | primer lote en ≤ 60 s; *backoff* en 0 |
| 13 | Evento detectado hace 168 h | `EXPIRADO_LOCAL`, contador +1, no se envía |
| 14 | Cerrar la app con `ENVIANDO` y abrirla | vuelven a `PENDING` antes de enviar |
| 15 | Anónimo actualiza el mapa | `GET /api/v1/anomalies` sin `bbox` ni `updatedSince`, con `If-None-Match`; 304 conserva |
| 16 | Registrado actualiza | `updatedSince` = último `generadoUtc`; bajas borradas |
| 17 | Anónimo sale del demo | zona gris + aviso una vez por viaje |
| 18 | Descarga del mapa cortada al 62 % | sigue con `Range`; SHA-256 malo → no se activa |
| 19 | Liviano 60 km/h / Pesado 60 km/h | alerta a ~133 m / ~200 m por la vía, una por pasada |
| 20 | Tres baches en 200 m | una sola alerta «3 baches seguidos» |
| 21 | Login con clave mala, cuenta bloqueada | mismo mensaje genérico, sin cuenta regresiva |
| 22 | Cerrar sesión con 3 pendientes | demo, sin historial, 3 enviados sin token |
| 23 | Eliminar cuenta con contraseña mala / buena | nada borrado / limpieza igual a cerrar sesión |
| 24 | Migración con `PENDING` sembrados | 0 pérdidas |

## 3. Datos del corredor (US8)

Desde `KillaUru-main`, en una carpeta de salida (`$S`) y para la versión `$V`:

```powershell
python -m pip install -r herramientas/cdn/requisitos.txt
python -m unittest discover -s herramientas/cdn          # los archivos cumplen cdn.openapi.yaml
python herramientas/cdn/generar_corredor.py backend/db/datos/corredor_pe28a.geojson $V --salida $S
copy backend/db/datos/tramo_demo.geojson $S/tramo_demo.v$V.geojson
cd herramientas/cdn; python verificar_datos.py $S/corredor_pe28a.v$V.json $S/tramo_demo.v$V.geojson ../../backend/db/datos/km_referencia.csv; cd ../..
python herramientas/cdn/region_corredor.py $S/corredor_pe28a.v$V.json --salida region.geojson
bash herramientas/cdn/generar_pmtiles.sh region.geojson $S/corredor-pe28a.v$V.pmtiles 20261120   # requiere la CLI pmtiles
python herramientas/cdn/generar_estilo.py $V --base https://cdn.killauru.pe/mapa --salida $S/estilo.v$V.json
python herramientas/cdn/generar_manifiesto.py $S $V --base https://cdn.killauru.pe/mapa
```

El polígono demo lo genera el backend (`backend/db/datos/construir_tramo_demo.py`, 006 T002); la CDN solo lo copia.
Publicar: GitHub → Actions → `app-datos-r2` → `version`, `ensayo = true` (genera, verifica y guarda el resultado como
artefacto) y luego `ensayo = false` (sube a R2: archivos primero, manifiesto al final).

## 4. Campo (R-030)

Seguir `mediciones/informe-campo.md`: benchmark de inferencia en 3 celulares, 5 h en modo avión entre Ayacucho y Vinchos, 1 h con la pantalla apagada por celular para batería y datos, y conciliación de la cuenta de eventos de la app con `evento_recibido`.
