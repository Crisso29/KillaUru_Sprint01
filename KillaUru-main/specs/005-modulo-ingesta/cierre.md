# Cierre de spec 005 — Ingesta

**Revisado:** 2026-11-21. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 29/40 tareas marcadas; 11 pendientes en tasks.md.
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Depende de 001/004.

## Evidencia disponible

Recepción HTTP y durable, cola, trabajadores, barredor y purgas presentes; 46 casos del módulo aprobados. README actualizado; T033 documental cerrada.

Tareas documentales cerradas en esta revisión: T033. Eso no certifica ejecución funcional.

**2026-11-21 — T034 cerrada:** [ContratoIngestaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/005-modulo-ingesta/contratos/ContratoIngestaTests.cs), 4/4 aprobados con Testcontainers.
Al escribirla en rojo aparecieron tres diferencias con el contrato, ya corregidas: faltaba el `operationId`
`recibirLote`, no se declaraba el 413 y `Lote`/`Evento`/`RespuestaLote` no publicaban requeridos, límites ni catálogos
(se completan en `Adapters/Documentacion/EsquemasIngesta.cs`).
**2026-11-21 — T012 bloque C (en rojo 0/6, luego 6/6):** errores reales de la recepción, corregidos:
- **300 KB → 500 en producción.** Con Kestrel real, `RequestSizeLimit` cortaba `[FromBody]` y la excepción acababa en `error_interno`. Ahora `LectorCuerpoLote` responde 413 `lote_demasiado_grande` con el tope de `Ingesta:TamanoMaximoCuerpoKb`, que antes estaba fijo en el código ([research R-002, adenda](research.md)).
- **Cuerpo no JSON → `datos_invalidos`** (validación genérica de MVC) en vez de `lote_invalido`.
- **Errores en `application/json` con solo `{codigo}`**, en vez de ProblemDetails con `type`, `title`, `status`, `codigo` y `correlacionId`. La app ya esperaba ProblemDetails (`ProblemaDto`), y `codigo` sigue en el primer nivel.
- **Soporte de pruebas:** `FabricaApi` no quitaba las purgas de Ingesta. Una prueba de 003 con el reloj en 2031 arrancaba la purga y borraba filas procesadas de otras pruebas (el #1 vio 37 de 50). Se quitan como ya se hacía con Anomalías; las purgas se prueban aparte en T030.
**2026-11-21 — T012 cerrada:** 17/17 escenarios del quickstart con HTTP y PostgreSQL real, en 4 archivos
(ver [pruebas.md](pruebas.md)). Se marcaron en `checklists/trazabilidad.md` CHK002, 004, 007, 008, 012, 024, 027 y 028;
CHK011 y CHK023 siguen abiertos porque también dependen de T027 (reinicio).
## Qué falta localmente

Faltan el repositorio PostgreSQL aislado (T011, T020), procesamiento con Anomalías real (T024), reinicio, purgas y salud. El E2E compartido no cubre esas ausencias.

## Qué falta en el entorno desplegado

Concilación tras corte/reinicio; ninguna pérdida o duplicado; p95, retraso de cola, picos/resistencia/caos; alerta de pendientes antiguos y procedimiento de reproceso.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T003 | Local | Prueba que `Ingesta:PurgaSeudonimoHoras` = `Anomalias:PurgaSeudonimoHoras` en `appsettings.json` |
| T004 | Local | Semillas: lotes de ejemplo sobre la polilínea, sin `loteId` y con un solo seudónimo (`lote-50-buenos.json`, `lote-con-errores.json`) |
| T011 | Local | `RecepcionEventosPostgresTests` (Testcontainers): un solo `INSERT`; devuelve solo los nuevos; reinsertar = 0 nuevos |
| T020 | Local | `PendientesRepositorioTests` (Testcontainers): `RecibidosDelLoteAsync` usa `ix_recibido_lote` (también con `lote_id IS NULL`); `ContarEnLaHoraAsync` con límites de la hora; `MarcarAsync`; `BarridoPendientesPostgres` |
| T024 | Local | `ProcesamientoTests` de punta a punta con Anomalías real: lote → filas `Procesado`/`Descartado` y anomalías creadas |
| T027 | Local | `ReinicioTests` (EQ-09): aceptar 500 eventos, detener el *host* a mitad del procesamiento, arrancar otro y verificar conciliación exacta y 0 duplicados en ≤ 2 min |
| T030 | Local | `PurgasTests`: escenarios 15 y 16 (seudónimo y `lote_id` en `NULL`, hora redondeada; filas `Recibido` > 30 días se conservan); `payload` sin seudónimo; esquema sin cuentas, correos ni tokens |
| T032 | Local | `SaludIngestaTests`: escenario 14; con la cola al día `/health/ready` = 200 |
| T040 | Desplegado/campo | Alerta de Grafana «pendiente más viejo de Ingesta > 72 h» (riesgo de R-008 frente a RNF-25) y paso de reproceso manual, en el runbook de monitoreo de 001 |
| T035 | Desplegado/campo | Medir SC-001 y SC-002 con los scripts de 001 (`tests/carga/carga-combinada.k6.js`, `picos.k6.js`, `estres.k6.js`, `resistencia.k6.js`, `caos.sh`), fuera de la VM contra el entorno de pruebas, con `killauru.ingesta.retraso_cola_s`; Ingesta no tiene script propio |
| T036 | Local | Marcar `checklists/trazabilidad.md` (los pendientes de `research.md` ya están confirmados por el autor) |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
