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
| Exportación | incremento de DatasetExportado > 0 en 5 min | RNF-29, 002 |
| Cambio de rol | incremento de RolCambiado > 0 en 5 min | RNF-29, 002 |
| Bloqueos | incremento de CuentaBloqueada ≥ 5 en 1 h | RNF-29, 002 |

## Activación de alertas de Auditoría (002 T027)

Las reglas están en [alertas-auditoria.yaml](../grafana/alertas-auditoria.yaml). Falta importarlas y comprobar su entrega en Grafana Cloud.
En Explore, verificar el nombre Prometheus normalizado del contador OTLP y separar producción de pruebas con las etiquetas realmente disponibles.
Configurar el contacto privado de CRISOLOGO y la sustitución por CRISSO, evaluar cada minuto y conservar evidencia de la alerta recibida.

Las semillas SQL solo crean filas: no ejecutan IMetricas. Para probar alertas hay que producir acciones mediante
IAuditoriaService con el exportador activo (003/004 cuando estén implementados), esperar muestras del contador
y confirmar la entrega. Una transacción revertida no debe incrementarlo.
T027 y 001 T050 siguen abiertas hasta esa verificación externa.
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
