# Pruebas de sistema · k6 (D11, CHK037)

Pruebas de carga, picos, estrés, resistencia y caos contra el entorno `killauru-pruebas`
(nunca contra producción). Se ejecutan con [k6](https://k6.io/) desde una máquina fuera de la VM.

## Prerequisitos

```sh
# Instalar k6 (Debian/Ubuntu)
sudo gpg --dearmor -o /usr/share/keyrings/k6-archive-keyring.gpg \
  < <(curl -fsSL https://dl.k6.io/key.gpg)
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" \
  | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update && sudo apt-get install k6

# Variable de entorno (apuntar al entorno de pruebas, no a producción)
export KU_URL_PRUEBAS=https://api-pruebas.killauru.pe
```

## Scripts disponibles

| Script | Tarea | Escenario | Duración |
|--------|-------|-----------|----------|
| `carga-combinada.k6.js` | T074 | 300 sincronizan + 300 miran el mapa | 10 min |
| `picos.k6.js` | T075 | 600 VUs sincronizan de golpe (feriado largo) | ~1.5 min |
| `estres.k6.js` | T075 | Escalera 100→1 200 VUs hasta el punto de quiebre | ~24 min |
| `resistencia.k6.js` | T076 | 150+150 VUs durante 4 horas | 4 h |
| `caos.k6.js` | T079 | 150 VUs válidos + 50 VUs de tráfico basura | 5 min |
| `identidad.k6.js` | SC-005 | 300 logins + renovación de token | ~3 min |
| `progresiva-2000.k6.js` | Sprint 2 | Escalones 100 → 300 → 500 → 1 000 → 1 500 → **2 000** usuarios concurrentes (mitad lotes, mitad mapa) | ~19 min |

`progresiva-2000` responde al pedido de «simular progresivamente hasta 2 000 usuarios virtuales concurrentes».
Su criterio de aprobación son las metas documentadas en el escalón de **300** (RNF-03): lote p95 < 300 ms y
mapa p95 < 250 ms. Por encima de 300 no hay meta escrita, así que se anota hasta dónde aguanta. Ahí un 503 con
`Retry-After` es la defensa esperada (DA-05) y cualquier otro 5xx cuenta en `errores_5xx`.
Cada petición lleva la etiqueta `escalon` (o `transicion` durante las subidas), para leer la latencia de cada nivel.
Guarda `resumen.txt` y `resumen.json` en la carpeta indicada con `-e SALIDA=...`.

Antes del 2026-11-21 **ningún script arrancaba**: `comun.js` leía el corredor como `FeatureCollection`
y `corredor_pe28a.geojson` es un `Feature`. Ya acepta ambas formas.

## Sin la VM: entorno local

Mientras no esté desplegado `killauru-pruebas`, se puede levantar [`killauru-local`](../../deploy/local/README.md)
con Docker en el PC (mismos recursos y configuración `Pruebas`) y apuntar k6 a `http://localhost:18080`,
que es el valor por defecto de `comun.js`. Los resultados se rotulan **«local, misma máquina»**
y se repiten en la VM antes de cerrar las tareas de carga.
`backend/TestResults/` se ignora en git: las ejecuciones que sirven de evidencia se copian a
[docs/seguimiento/evidencias](../../../docs/seguimiento/evidencias/README.md).
Primera ejecución: [progresiva hasta 2 000, local, 2026-11-21](../../../docs/seguimiento/evidencias/carga/2026-11-21-local-progresiva-2000/README.md).

```powershell
k6 run -e KU_URL_PRUEBAS=http://localhost:18080 -e SALIDA=<carpeta-de-resultados> progresiva-2000.k6.js
```

## Cómo correr cada prueba

```sh
# Carga combinada (línea base antes de un release)
k6 run -e KU_URL_PRUEBAS=$KU_URL_PRUEBAS carga-combinada.k6.js

# Picos (simular recuperación de señal masiva)
k6 run -e KU_URL_PRUEBAS=$KU_URL_PRUEBAS picos.k6.js

# Estrés (encontrar el punto de quiebre — k6 para solo cuando rompe)
k6 run -e KU_URL_PRUEBAS=$KU_URL_PRUEBAS estres.k6.js

# Resistencia (antes de una versión con cambios en el pipeline)
k6 run -e KU_URL_PRUEBAS=$KU_URL_PRUEBAS resistencia.k6.js

# Caos (verificar validación de entrada bajo mezcla de tráfico)
k6 run -e KU_URL_PRUEBAS=$KU_URL_PRUEBAS caos.k6.js

# Identidad (requiere archivo de cuentas sintéticas y confirmación explícita)
k6 run \
  -e BASE_URL=$KU_URL_PRUEBAS \
  -e USUARIOS_ARCHIVO=cuentas-pruebas.json \
  -e CONFIRMAR_PRUEBAS=si \
  identidad.k6.js
```

## Umbrales (thresholds)

Todos los scripts heredan `umbralesComunes` de `comun.js`:

| Métrica | Límite | Razón |
|---------|--------|-------|
| `http_req_failed` | < 1 % | Tolerancia de errores de red |
| `checks` | > 99 % | Las aserciones de negocio deben pasar casi siempre |
| `tasa_429` | = 0 % | D11: bajo carga normal los límites de cuota no deben activarse |

Scripts individuales añaden umbrales de latencia:

| Script | Métrica | Límite | Spec |
|--------|---------|--------|------|
| carga-combinada / resistencia | `http_req_duration{ruta:lote}` p95 | < 300 ms | RNF-02 |
| carga-combinada / resistencia | `http_req_duration{ruta:mapa}` p95 | < 250 ms | RNF-01 |
| estres | `http_req_duration` p95 | < 600 ms / 500 ms | 2× meta (abortOnFail) |
| caos | `errores_servidor` | = 0 % | D11: cero 5xx bajo entrada inválida |

## Cuándo ejecutar cada prueba

- **Antes de cada release**: carga-combinada + picos + caos
- **Cambios en el pipeline de procesamiento**: resistencia (buscar fugas de memoria en Grafana)
- **Cambios en rate limiting / cuotas**: carga-combinada (verificar que `tasa_429 == 0`)
- **Investigación del punto de quiebre**: estrés (una vez por sprint, no en cada release)
- **Cambios en auth / tokens**: identidad

## Datos de prueba

`comun.js` abre `../../db/datos/corredor_pe28a.geojson` con la polilínea de la PE-28A.
Este archivo debe existir en `backend/db/datos/` antes de ejecutar cualquier script.
