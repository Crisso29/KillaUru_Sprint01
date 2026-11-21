# Cierre de spec 006 — Consulta de mapa

**Revisado:** 2026-11-15. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 19/33 tareas marcadas; 14 pendientes en tasks.md.
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Depende de 001/004 y del ciclo de cambios originado por 005.

## Evidencia disponible

GET /api/v1/anomalies, consultas por celdas/filtros/cambios, caché y métricas implementadas; 44 casos de módulo aprobados. README actualizado; T028 cerrada.

Tareas documentales cerradas en esta revisión: T028. Eso no certifica ejecución funcional.
## Qué falta localmente

No se encontró suite dedicada de integración Mapa. Faltan geometría demo/semilla, tests ETag, GeoJSON, fuente de celdas y métricas, API por rol/filtros/cambios y contrato. MetricasMapa existe: T033 está incompleta por validación, no por ausencia total del componente.

## Qué falta en el entorno desplegado

Validar recorte anónimo punto por punto, invalidación y 304, caché de Cloudflare solo anónima y p95 bajo carga combinada.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T002 | Local | Armar el polígono demo desde la polilínea PE-28A (límites `demo_km_inicio` → `demo_km_fin` del GeoJSON de 004; km 268 → 335,073 al mostrar, margen 200 m, ≤ 60 vértices); guardarlo en `appsettings.json` y en `db/datos/tramo_demo.geojson` |
| T003 | Local | Semilla `mapa-roles.sql` con los 4 grupos del quickstart |
| T009 | Local | `CalculadorETagTests`: mismas versiones → mismo `ETag`; una celda +1 → distinto; otro instante de arranque → distinto; otro alcance → distinto |
| T010 | Local | `SerializadorGeoJsonTests`: *feature* con los 7 campos (incluido `kmVia`) y `[lon, lat]`; `venceUtc: null` en un estado final; con `estado` solo si se pide; baja con `geometry: null` y sin `kmVia`; colección vacía válida |
| T018 | Local | `CacheEtagApiTests`: escenarios 3, 4, 11 y 12 del quickstart |
| T033 | Local | `MetricasMapa` (medidor `KillaUru.Mapa`, contador `killauru.mapa.cache{resultado}`) contado en `FuenteCeldasCacheada` (`acierto`/`fallo`) y en `ConsultarPorCeldasUseCase` (`no_modificado`); prueba `MetricasMapaTests` con `MeterListener`: solo esos 3 valores de etiqueta |
| T019 | Local | `FuenteCeldasAnomaliasTests`: con alcance `Anonimo`, una celda de borde filtra punto por punto; una celda `Fuera` no se consulta |
| T021 | Local | `RecorteRolApiTests`: escenarios 1 y 2 del quickstart; `bbox` sin el demo → colección vacía; 100 % de respuestas anónimas dentro del polígono |
| T024 | Local | `FiltrosApiTests`: escenarios 5–8 y 13 del quickstart (`estado` con token de conductor y sin token → 403); sin `ETag`; cabeceras por rol |
| T027 | Local | `CambiosApiTests`: escenarios 9 y 10 del quickstart |
| T029 | Local | Prueba de contrato con `contracts/mapa.openapi.yaml` en verde |
| T030 | Desplegado/campo | Regla de caché de Cloudflare (cachear `GET /api/v1/anomalies` solo sin `Authorization` ni cookie `ku_acceso`, respetando `Cache-Control` del origen) documentada en `deploy/runbooks/monitoreo.md` y comprobada como en el quickstart §4; la tasa de aciertos en el borde se lee en las analíticas de Cloudflare y se anota en ese *runbook* |
| T031 | Desplegado/campo | Medir SC-001 con las pruebas de carga de 001 (`tests/carga/carga-combinada.k6.js`: 300 consultando el mapa + 300 sincronizando, 10 min; y `picos.k6.js`), fuera de la VM contra el entorno de pruebas; anotar el p95 del mapa. 006 no tiene script propio |
| T032 | Local | Marcar `checklists/trazabilidad.md` (los pendientes de `research.md` ya fueron confirmados por el autor el 2026-10-21) |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
