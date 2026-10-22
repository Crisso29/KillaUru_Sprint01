# Runbook · Monitoreo (HU-17, RNF-20…RNF-22, RNF-32)

## Qué llega a Grafana Cloud (plan gratuito, cuenta sin tarjeta)

| Fuente | Qué | Cómo |
|---|---|---|
| API | métricas de ASP.NET Core por `http.route`, `killauru.*` (lotes, rechazos, descartes, cola, retraso de cola, tareas, acciones de riesgo), trazas al 10 %, logs JSON | OpenTelemetry por OTLP (`KU_GRAFANA_OTLP_*`) |
| Alloy | disco, CPU, memoria de la VM; logs de Docker; `killauru_tarea_ultima_ok{tarea="backup"}` | `deploy/alloy/config.alloy` |
| UptimeRobot | `GET https://api.killauru.pe/health/ready` cada 5 min, página de estado pública | alerta por correo en ≤ 10 min |

## Alertas que se configuran (T050)

| Alerta | Condición | Nota |
|---|---|---|
| Mapa lento | p95 de `GET /api/v1/anomalies` > 250 ms durante 5 min | RNF-01 |
| Lote lento | p95 de `POST /api/v1/anomalies/batch` > 300 ms durante 5 min | RNF-02 |
| Resto | p95 > 250 ms; rutas de sesión > 500 ms | metas SDD |
| Errores | 5xx > 1 % durante 5 min | RNF-20 |
| Disco | > 80 % | RNF-22 |
| Tarea detenida | `killauru.tarea.ultima_ok` de cualquier tarea (incluido `backup`) con más de 26 h | RNF-22 |
| Plan gratuito | uso ≥ 80 % de cualquier cuota | RNF-32 |
| Acción de riesgo | cualquier `killauru.seguridad.acciones_riesgo` | RNF-29 |
| Costo | alerta de presupuesto de Oracle y Cloudflare a US$ 8 | RNF-16 (T057) |

## Calendario de tareas (`ProgramaReloj`, T-OPS05; todas corren también al arrancar)

| Hora (Lima) | Tarea | Módulo |
|---|---|---|
| cada hora :05 | `caducidad` | Anomalías |
| cada hora :10 | `purga-seudonimos`, `purga-seudonimos-recepcion` | Anomalías, Ingesta |
| 00:30 | `indicadores` | Dashboard |
| 02:00 | `backup` | contenedor `backup` |
| 03:00 | `purga-recepcion` | Ingesta |
| 03:30 | `purga-historica` | Anomalías |
| 03:45 | `purga-identidad` | Identidad |
