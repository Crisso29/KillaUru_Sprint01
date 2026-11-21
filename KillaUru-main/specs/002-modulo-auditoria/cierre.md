# Cierre de spec 002 — Auditoría

**Revisado:** 2026-11-15. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 35/38 tareas marcadas; 3 pendientes en tasks.md.
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Depende de 001; la revisión de consumidores incluye 003/004.

## Evidencia disponible

Implementación y pruebas de transacción, inmutabilidad, API y contratos. 22 casos de módulo aprobados en la revisión local anterior.

## Qué falta localmente

T038 requiere registrar la revisión de emisores. Existen ReglasIdentidadTests y ReglasAnomaliasTests; Mapa/Dashboard no mostraron referencias a auditoría en la inspección de fuentes. Conservar revisión explícita al cerrar la tarea.

## Qué falta en el entorno desplegado

Activar y disparar alertas Grafana; ejecutar y cronometrar EQ-13; acreditar auditoría en la misma transacción bajo fallos.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T027 | Desplegado/campo | Reglas de alerta en Grafana para `killauru.seguridad.acciones_riesgo` (exportación > 0 en 5 min, rol > 0 en 5 min, bloqueos ≥ 5 en 1 h) y documentarlas |
| T031 | Desplegado/campo | Simulacro EQ-13 en «killauru-pruebas»; anotar tiempos |
| T038 | Local | Revisión de código: Identidad (003) y Anomalías (004) **no** llaman `RegistrarAccionRiesgo` ni registran descartes de ingesta; Mapa y Dashboard no auditan lecturas |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
