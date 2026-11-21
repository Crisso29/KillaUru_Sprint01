# Cierre de spec 003 — Identidad y accesos

**Revisado:** 2026-11-15. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 65/65 tareas marcadas; 0 pendientes en tasks.md.
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Depende de 001/002; habilita sesiones y administración.

## Evidencia disponible

65 tareas marcadas y 89 casos de módulo aprobados en la revisión local anterior. La revisión histórica documenta integración con correo y anti-bot sustituidos.

## Qué falta localmente

No hay tareas abiertas en tasks.md, pero eso no cierra la activación desplegada. Revisar contrato, cobertura del commit candidato y suites PostgreSQL; conservar separados los resultados históricos y actuales.

## Qué falta en el entorno desplegado

Correo real y canje de enlaces; Turnstile; cookies/origen; creación privada del primer admin; renovación/revocación; restauración y carga representativa.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

No hay tareas abiertas. Las puertas de aceptación desplegada de arriba siguen pendientes.

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
