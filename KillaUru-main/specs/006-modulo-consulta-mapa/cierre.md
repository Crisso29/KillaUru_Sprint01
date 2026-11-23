# Cierre de spec 006 — Consulta de mapa

**Revisado:** 2026-11-23. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 23/33 tareas marcadas; 10 pendientes en tasks.md (T002 cerrada el 2026-11-22: polígono demo real de 115 vértices, R-010 revisado; T021 y T024 cerradas el 2026-11-23 con pruebas de integración y mutaciones).
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Depende de 001/004 y del ciclo de cambios originado por 005.

## Evidencia disponible

GET /api/v1/anomalies, consultas por celdas/filtros/cambios, caché y métricas implementadas; 44 casos de módulo aprobados. README actualizado; T028 cerrada.

Tareas documentales cerradas en esta revisión: T028. Eso no certifica ejecución funcional.

**2026-11-21 — T029 cerrada:** [ContratoMapaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/006-modulo-consulta-mapa/contratos/ContratoMapaTests.cs), 5/5 aprobados con Testcontainers.
Al escribirla en rojo aparecieron diferencias con el contrato, ya corregidas:
- Documentación: faltaban el `operationId` `consultarMapa`, el 401, los esquemas GeoJSON (la respuesta son bytes de la caché) y el catálogo de `tipo`. Ahora se publican desde `Adapters/Documentacion/`.
- **Comportamiento:** `?severidad=4,5` y `?estado=Activa,PendienteConfirmacion` respondían 400. Así los envía el panel (`filtros.ts`, `peticion.ts`), y sus pruebas no lo detectaban porque usan MSW. `ParametrosMapaDto` acepta ahora listas separadas por comas; una lista mal formada responde 400 `datos_invalidos` ([ParametrosMapaDtoTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Adapters/ParametrosMapaDtoTests.cs)).
## Qué falta localmente

Faltan geometría demo/semilla, tests ETag, GeoJSON, fuente de celdas y métricas, y API por rol/filtros/cambios. MetricasMapa existe: T033 está incompleta por validación, no por ausencia total del componente.

## Qué falta en el entorno desplegado

Validar recorte anónimo punto por punto, invalidación y 304, caché de Cloudflare solo anónima y p95 bajo carga combinada.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T003 | Local | Semilla `mapa-roles.sql` con los 4 grupos del quickstart |
| T009 | Local | `CalculadorETagTests`: mismas versiones → mismo `ETag`; una celda +1 → distinto; otro instante de arranque → distinto; otro alcance → distinto |
| T010 | Local | `SerializadorGeoJsonTests`: *feature* con los 7 campos (incluido `kmVia`) y `[lon, lat]`; `venceUtc: null` en un estado final; con `estado` solo si se pide; baja con `geometry: null` y sin `kmVia`; colección vacía válida |
| T018 | Local | `CacheEtagApiTests`: escenarios 3, 4, 11 y 12 del quickstart |
| T033 | Local | `MetricasMapa` (medidor `KillaUru.Mapa`, contador `killauru.mapa.cache{resultado}`) contado en `FuenteCeldasCacheada` (`acierto`/`fallo`) y en `ConsultarPorCeldasUseCase` (`no_modificado`); prueba `MetricasMapaTests` con `MeterListener`: solo esos 3 valores de etiqueta |
| T019 | Local | `FuenteCeldasAnomaliasTests`: con alcance `Anonimo`, una celda de borde filtra punto por punto; una celda `Fuera` no se consulta |
| T027 | Local | `CambiosApiTests`: escenarios 9 y 10 del quickstart |
| T030 | Desplegado/campo | Regla de caché de Cloudflare (cachear `GET /api/v1/anomalies` solo sin `Authorization` ni cookie `ku_acceso`, respetando `Cache-Control` del origen) documentada en `deploy/runbooks/monitoreo.md` y comprobada como en el quickstart §4; la tasa de aciertos en el borde se lee en las analíticas de Cloudflare y se anota en ese *runbook* |
| T031 | Desplegado/campo | Medir SC-001 con las pruebas de carga de 001 (`tests/carga/carga-combinada.k6.js`: 300 consultando el mapa + 300 sincronizando, 10 min; y `picos.k6.js`), fuera de la VM contra el entorno de pruebas; anotar el p95 del mapa. 006 no tiene script propio. **Ejecución local previa (2026-11-21):** [progresiva hasta 2 000](../../docs/seguimiento/evidencias/carga/2026-11-21-local-progresiva-2000/README.md); no cierra la tarea (falta la VM). |
| T032 | Local | Marcar `checklists/trazabilidad.md` (los pendientes de `research.md` ya fueron confirmados por el autor el 2026-10-21) |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
