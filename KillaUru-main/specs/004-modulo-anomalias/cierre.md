# Cierre de spec 004 — Anomalías

**Revisado:** 2026-11-15. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 56/58 tareas marcadas; 2 pendientes en tasks.md.
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Depende de 001/002; coopera con 005/006/007.

## Evidencia disponible

Dominio, procesamiento, moderación, geometría y exportaciones implementados; 100 casos de módulo aprobados en la revisión anterior.

## Qué falta localmente

Ejecutar integración PostgreSQL del candidato. T054 es el escenario completo de abuso y tráfico legítimo, no solo las pruebas parciales existentes.

## Qué falta en el entorno desplegado

Medir p95 con la cola real de 005 y 300 dispositivos; EQ-12 completo; migraciones, geometría y privacidad; confirmar propagación hacia mapa/dashboard.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T053 | Desplegado/campo | Medir p95 del procesamiento con 300 dispositivos (junto con 005) ≤ 100 ms con el histograma `killauru.anomalias.proceso_ms`. **2026-11-22:** k6 local confirma el riesgo y localiza causas probables; ver [DT-01](../../docs/seguimiento/deuda-tecnica.md) y 005/T041 |
| T054 | Desplegado/campo | Escenario EQ-12 completo (ataque + subida legítima de 5 h + lluvia de 600 eventos/h) |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
