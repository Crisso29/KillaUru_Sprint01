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
