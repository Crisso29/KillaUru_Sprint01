# Cómo leer la revisión de arquitectura

**Guía actualizada: 2026-11-21.** Esta carpeta conserva la revisión de diseño del
21 de octubre y sus decisiones. No contiene el estado actual de todas las pruebas.

**Esta carpeta se conserva por trazabilidad.** No hay que ejecutar todos sus informes
como si fueran tareas abiertas. Para orientarte entre documentación actual e histórica,
consulta [el índice de documentación](../../README.md#revisiones-de-arquitectura).

Para trabajar hoy, abre primero el [índice de pruebas](../../guias/pruebas.md),
el [estado de implementación](../../seguimiento/estado-implementacion.md) o el
[plan de cierre](../../guias/cierre-y-despliegue.md). Cada spec tiene `tasks.md`
para tareas y `cierre.md` para evidencia y pendientes.

## Qué leer según tu pregunta

| Pregunta | Documento | Cómo interpretarlo |
|---|---|---|
| ¿Por qué se eligió esta solución? | [Decisiones del autor](decisiones.md) | Acuerdos D1–D17 de octubre; contrastar su desarrollo con la referencia normativa |
| ¿Qué cambios se planificaron entonces? | [Plan de cambios](plan-de-cambios.md) | Relación entre decisiones, hallazgos y cambios; no es el backlog vigente |
| ¿Qué detectó cada área? | Informes de la tabla siguiente | Diagnóstico histórico, no prueba de que el problema continúe abierto |
| ¿Por qué una propuesta no se aplicó? | Registros de propuestas no aplicadas | Motivo histórico; puede haberse descartado, reemplazado o condicionado |
| ¿Qué falta ahora? | [Cierre por spec](../../../specs/README.md) | Usar tareas y evidencia actual, no contar los hallazgos antiguos como pendientes |

## Informes históricos por área

| Área | Informe | Qué explica |
|---|---|---|
| Arquitectura | [Revisión de arquitectura](revision-arquitecto.md) | Capas, límites de módulos, contratos y dependencias |
| Base de datos | [Revisión de base de datos](revision-dba.md) | PostgreSQL, SQLite, persistencia, índices y consistencia |
| QA | [Revisión de pruebas](revision-qa.md) | Escenarios, aceptación, rendimiento y cobertura previstos |
| Seguridad | [Revisión de seguridad](revision-seguridad.md) | Acceso, privacidad, amenazas y controles |
| Operación | [Revisión DevOps](revision-devops.md) | CI/CD, respaldos, observabilidad y despliegue |
| Producto | [Revisión de producto](revision-producto.md) | Historias, requisitos, alcance y coherencia del prototipo |

Las referencias `archivo:línea` se conservan como evidencia de la revisión original.
Las líneas pueden haber cambiado; para localizar pruebas actuales usa `pruebas.md`
de la spec, que enlaza directamente los archivos existentes.

## Propuestas no aplicadas en aquella revisión

| Ámbito | Registro | Seguimiento actual |
|---|---|---|
| Plataforma y auditoría | [001–002](no-aplicado-001-002.md) | [001](../../../specs/001-plataforma-backend/cierre.md) · [002](../../../specs/002-modulo-auditoria/cierre.md) |
| Identidad | [003](no-aplicado-003.md) | [Cierre 003](../../../specs/003-modulo-identidad-accesos/cierre.md) |
| Anomalías | [004](no-aplicado-004.md) | [Cierre 004](../../../specs/004-modulo-anomalias/cierre.md) |
| Ingesta | [005](no-aplicado-005.md) | [Cierre 005](../../../specs/005-modulo-ingesta/cierre.md) |
| Mapa y dashboard | [006–007](no-aplicado-006-007.md) | [006](../../../specs/006-modulo-consulta-mapa/cierre.md) · [007](../../../specs/007-modulo-dashboard/cierre.md) |
| Análisis del sistema | [Docs 01](no-aplicado-docs01.md) | [Referencia normativa](../../../referencia/docs) |
| Arquitectura y diseño | [Docs 02–04](no-aplicado-docs02-04.md) | [Constitución](../../../.specify/memory/constitution.md) |

## Regla para mantener esta carpeta

Conservar el motivo y la fecha originales. Una resolución nueva se registra con su
fecha, spec, tarea y evidencia; no se reescribe el informe antiguo para que parezca
una revisión reciente. Si cambia una regla de dominio o arquitectura, actualizar
primero su documentación normativa y después las specs afectadas.

El orden para investigar un caso es: **hallazgo → decisión → requisito vigente →
tarea → prueba → resultado**. Ni una decisión aceptada ni un archivo de test
constituyen, por sí solos, evidencia de ejecución satisfactoria.
