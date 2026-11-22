# Carga progresiva hasta 2 000 usuarios · local · 2026-11-21

> Evidencia de atributos de calidad (rendimiento y disponibilidad, AC-06). [Estado del proyecto](../../../estado-implementacion.md) · [Cómo se ejecuta](../../../../../backend/tests/carga/README.md)

| Campo | Valor |
|---|---|
| Script | `backend/tests/carga/progresiva-2000.k6.js` (escalones 100 → 300 → 500 → 1 000 → 1 500 → 2 000; 60 s de subida + 120 s sostenidos) |
| Entorno | `killauru-local` (`backend/deploy/docker-compose.local.yml`): API 1,25 CPU / 3 GB, PostgreSQL 0,75 CPU / 3 GB, `ASPNETCORE_ENVIRONMENT=Pruebas` |
| Máquina | PC del autor: Windows 10, 8 núcleos, 24 GB; Docker 29.8.2; k6 v2.0.0-rc1. **k6 corre en la misma máquina** |
| Código | `e549393` + cambios sin commit del mismo día (entorno local, script y corrección de `comun.js`) |
| Hora | 23:04 a 23:24 (UTC−5), 18 min 30 s |
| Archivos | [`resumen.txt`](resumen.txt) y [`resumen.json`](resumen.json) (k6), [`recursos.csv`](recursos.csv) (CPU y memoria cada 15 s). La consola de k6 (240 KB de barras de progreso) solo queda en `backend/TestResults/` del PC |

**Alcance:** evidencia **local, misma máquina**. No es una medición de producción. Se repetirá en `killauru-pruebas` (VM).

## Resultado frente a las metas documentadas (escalón de 300, RNF-03)

| Meta | Medido | Resultado |
|---|---|---|
| RNF-02 · lote p95 < 300 ms | 89,5 ms | ✅ |
| RNF-01 · mapa p95 < 250 ms (en la API) | 7,5 ms | ✅ |
| Errores 5xx < 1 % | 0 en toda la prueba (102 894 peticiones) | ✅ |
| Sin 429 (D11) | 0 | ✅ |
| RNF-23 · retraso de cola ≤ 30 s (p95) | **No lo mide este script.** El procesamiento va más lento que la llegada (ver abajo) | ⚠️ Probablemente no se cumple |

## Por escalón

«Lotes» es aproximado: el resumen de k6 no separa peticiones por ruta, así que se calcula como el total menos
las consultas al mapa esperadas (VU/2 × 24 por escalón). La versión actual del script ya cuenta `peticiones{ruta}` exactas.

| Usuarios | Peticiones | Lotes (aprox.) | 503 con Retry-After | % de lotes con 503 | Lote p95 | Mapa p95 | Otros 5xx |
|---:|---:|---:|---:|---:|---:|---:|---:|
| 100 | 1 299 | 99 | 0 | 0 % | 56,9 ms | 4,2 ms | 0 |
| 300 | 3 900 | 300 | 0 | 0 % | 89,5 ms | 7,5 ms | 0 |
| 500 | 6 496 | 496 | 0 | 0 % | 114,9 ms | 9,9 ms | 0 |
| 1 000 | 12 994 | 994 | 478 | 48 % | 71,0 ms | 9,2 ms | 0 |
| 1 500 | 19 470 | 1 470 | 1 123 | 76 % | 61,0 ms | 10,8 ms | 0 |
| 2 000 | 25 937 | 1 937 | 1 822 | 94 % | 69,8 ms | 31,5 ms | 0 |

Desde 1 000 usuarios el p95 de lotes baja porque los 503 se responden sin guardar nada y pesan en el percentil.

## Lectura

1. **El mapa aguanta 2 000 usuarios** con un p95 de 31,5 ms, muy lejos de la meta de 250 ms (caché por celdas, ADR-006).
2. **La recepción se defiende como dice DA-05.** Cuando la cola se llena (capacidad 2 000 lotes), responde 503 con
   `Retry-After` y **no hay ningún 500**. La app reintenta después; no se pierde nada.
3. **No se perdió ni duplicó ningún evento aceptado.** En `ingesta.evento_recibido` hay **160 100 filas = 3 202 lotes con 200 × 50**.
4. **El cuello de botella es el procesamiento en segundo plano, no la API.** Medido después de la prueba, sin carga:
   **77 eventos/s** con la API al 55 % de CPU, es decir, sin saturar. En el escalón de 300 llegan unos 125 eventos/s
   (150 celulares × 50 eventos por minuto), así que la cola **crece desde los 300 usuarios**. Al terminar quedaban
   63 388 pendientes y `/health/ready` respondía 503, como dispone RF-28 (pendientes de más de 5 min).
   Esto pone en riesgo **RNF-23**. Hay que medirlo con `killauru.ingesta.retraso_cola_s` y averiguar la causa
   (número de trabajadores, transacción por evento o bloqueos) antes de afirmarla.
5. **CPU:** la API llegó al 127 % (su tope de 1,25 CPU) y PostgreSQL al 79 % (tope de 0,75) en los escalones altos.
6. **29 conexiones TCP cortadas** (0,03 %), todas entre 23:21:48 y 23:23:48 (escalón de 2 000 y bajada),
   una cada ~5 s. La API no se reinició ni registró errores en ese tramo. Lo más probable es que sea el reenvío de
   puertos de Docker Desktop en Windows con 2 000 conexiones; está **sin confirmar** y se revisará en la VM.
   508 eventos se descartaron por `VelocidadImposible`, lo esperado porque k6 salta entre puntos lejanos de la vía.
