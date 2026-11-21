# Cierre de spec 007 — Dashboard

**Revisado:** 2026-11-15. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 24/26 tareas marcadas; 2 pendientes en tasks.md.
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Depende de 001/003/004; se verifica también después de 005.

## Evidencia disponible

Cálculo programado, persistencia agregada y API administrativa presentes; 23 casos de módulo aprobados. README con fórmulas y horarios actualizado; T021 cerrada.

Tareas documentales cerradas en esta revisión: T021. Eso no certifica ejecución funcional.
## Qué falta localmente

Completar contrato dedicado T023 y revisar trazabilidad T024. La integración existente necesita ejecutarse con PostgreSQL en el commit candidato.

## Qué falta en el entorno desplegado

Verificar cálculo diario/recuperación, datos nulos y consistencia con lotes/anomalías reales. Confirmar rol administrativo y ausencia de datos identificables.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T023 | Local | Prueba de contrato con `contracts/dashboard.openapi.yaml` en verde |
| T024 | Local | Marcar `checklists/trazabilidad.md` (los pendientes de `research.md` ya fueron confirmados por el autor el 2026-10-21) |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
