---

description: "Tareas del módulo Consulta de Mapa"
---

# Tasks: Módulo Consulta de Mapa

**Input**: `specs/006-modulo-consulta-mapa/` · **Requiere**: 001 (fases 1–6, más `Celda.Limites` y las pruebas de carga de `backend/tests/carga/`) y 004 (US7, con `CeldasDelCorredor` y `KmVia` en `AnomaliaVisible`/`AnomaliaCambio`)

**Tests**: obligatorias y **primero** (constitución VII; el recorte anónimo es un escenario crítico: EQ-11).

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `backend/`. Módulo: `src/Modules/KillaUru.Mapa/` (`Ma/`). Pruebas: `tests/KillaUru.Mapa.Tests/` (`MaT/`) y `tests/KillaUru.Integracion.Tests/Mapa/` (`IntMa/`). **Ningún `.cs` > 199 líneas.**

## Phase 1: Setup

- [ ] T001 [P] `MapaOptions` con `ValidateOnStart` y su sección en `appsettings.json` — `Ma/Application/MapaOptions.cs` · (ADR-012)
- [ ] T002 Armar el polígono demo desde la polilínea PE-28A (límites `demo_km_inicio` → `demo_km_fin` del GeoJSON de 004; km 268 → 335,073 al mostrar, margen 200 m, ≤ 60 vértices); guardarlo en `appsettings.json` y en `db/datos/tramo_demo.geojson` · (RN-08; R-010)
- [ ] T003 [P] Semilla `mapa-roles.sql` con los 4 grupos del quickstart — `db/semillas/mapa-roles.sql`

## Phase 2: Foundational — dominio

### Pruebas (primero; deben fallar)

- [ ] T004 [P] `BboxTests`: parseo válido; 3 números, texto, `min > max`, fuera de rango → `null`; `Intersecta` en bordes compartidos — `MaT/Domain/BboxTests.cs` · (R-003)
- [ ] T005 [P] `PoligonoGeoTests` y `ZonaDemoTests`: punto dentro, fuera y sobre un vértice; celda `Dentro`, `Borde` (una esquina adentro, o un lado cruzado sin esquinas adentro) y `Fuera` — `MaT/Domain/` · (FR-003, FR-004)
- [ ] T006 [P] `PoliticaAlcanceTests`: las 8 filas de `data-model.md` §1 — `MaT/Domain/PoliticaAlcanceTests.cs` · (FR-002, FR-011)

### Implementación

- [ ] T007 [P] `Alcance`, `Bbox`, `PoligonoGeo`, `ZonaDemo`, `PlanConsulta`, `PoliticaAlcance` — `Ma/Domain/`
- [ ] T008 [P] `ParametrosMapaDto` (con XML y `<example>`), `ConsultaMapa`, `ResultadoMapa` — `Ma/Adapters/Dtos/`, `Ma/Application/`

**Checkpoint**: `KillaUru.Mapa.Tests` (dominio) en verde y cobertura del dominio ≥ 90 % (coverlet, como el CI de 001).

---

## Phase 3: User Story 2 — El conductor ve el corredor completo (P1) 🎯 MVP

### Pruebas

- [ ] T009 [P] [US2] `CalculadorETagTests`: mismas versiones → mismo `ETag`; una celda +1 → distinto; otro instante de arranque → distinto; otro alcance → distinto — `MaT/Application/CalculadorETagTests.cs` · (FR-008; R-008)
- [ ] T010 [P] [US2] `SerializadorGeoJsonTests`: *feature* con los 7 campos (incluido `kmVia`) y `[lon, lat]`; `venceUtc: null` en un estado final; con `estado` solo si se pide; baja con `geometry: null` y sin `kmVia`; colección vacía válida — `MaT/Infrastructure/SerializadorGeoJsonTests.cs` · (FR-005; R-006, R-009, R-011)
- [ ] T011 [P] [US2] `ConsultarPorCeldasUseCaseTests` (dobles): `bbox` → solo celdas del corredor; sin `bbox` → todas; 304 sin llamar a la fuente; orden ascendente de celdas — `MaT/Application/ConsultarPorCeldasUseCaseTests.cs` · (FR-003, FR-006, FR-008)

### Implementación

- [ ] T012 [US2] Puertos `IFuenteCeldas` e `IVersionesCelda`; `CorredorMapa` y `CalculadorETag` — `Ma/Application/`
- [ ] T013 [US2] `SerializadorGeoJson`, `CeldaGeoJson`, `FuenteCeldasAnomalias`, `FuenteCeldasCacheada`, `VersionesCelda` — `Ma/Infrastructure/` · (FR-006, FR-015)
- [ ] T014 [US2] `InvalidarCeldasMapa` (versión++ y `RemoveByTagAsync`) — `Ma/Infrastructure/InvalidarCeldasMapa.cs` · (FR-007)
- [ ] T015 [US2] `ConsultarPorCeldasUseCase` y `ConsultarMapaUseCase` (despachador) — `Ma/Application/` · (FR-002, FR-006, FR-008)
- [ ] T016 [US2] `AnomaliasController`: lee parámetros, `If-None-Match`, escribe el cuerpo en el `PipeWriter`, `ETag` y `Cache-Control` por rol — `Ma/Adapters/AnomaliasController.cs` · (FR-001, FR-009)
- [ ] T017 [US2] `AddMapaModule` (HybridCache con `SizeLimit`, *Decorator*, suscripción) — `Ma/MapaModule.cs` · (FR-014: sin `DbContext` ni SQL; lo vigila `ReglasDeModulosTests` de 001)
- [ ] T018 [US2] `CacheEtagApiTests`: escenarios 3, 4, 11 y 12 del quickstart — `IntMa/CacheEtagApiTests.cs` · (SC-004, SC-005)
- [ ] T033 [P] [US2] `MetricasMapa` (medidor `KillaUru.Mapa`, contador `killauru.mapa.cache{resultado}`) contado en `FuenteCeldasCacheada` (`acierto`/`fallo`) y en `ConsultarPorCeldasUseCase` (`no_modificado`); prueba `MetricasMapaTests` con `MeterListener`: solo esos 3 valores de etiqueta — `Ma/Infrastructure/MetricasMapa.cs`, `MaT/Infrastructure/MetricasMapaTests.cs` · (FR-016; R-013)

## Phase 4: User Story 1 — El anónimo ve solo el tramo demo (P1)

- [ ] T019 [P] [US1] `FuenteCeldasAnomaliasTests`: con alcance `Anonimo`, una celda de borde filtra punto por punto; una celda `Fuera` no se consulta — `MaT/Infrastructure/FuenteCeldasAnomaliasTests.cs` · (FR-004)
- [ ] T020 [US1] Recorte anónimo en `CorredorMapa` (celdas `Dentro` ∪ `Borde`) y en `FuenteCeldasAnomalias` — `Ma/Application/CorredorMapa.cs`, `Ma/Infrastructure/FuenteCeldasAnomalias.cs` · (FR-003, FR-004)
- [ ] T021 [US1] `RecorteRolApiTests`: escenarios 1 y 2 del quickstart; `bbox` sin el demo → colección vacía; 100 % de respuestas anónimas dentro del polígono — `IntMa/RecorteRolApiTests.cs` · (SC-003, EQ-11, RNF-27)

## Phase 5: User Story 4 — El administrador ve todo y filtra (P1)

- [ ] T022 [P] [US4] `ConsultarFiltradoUseCaseTests` (dobles): administrador sin `estado` → los 5 estados; conductor → `[Activa]`; anónimo → `ZonaDemo.Contiene`; `bbox` ausente → envolvente del corredor — `MaT/Application/ConsultarFiltradoUseCaseTests.cs` · (FR-010, FR-011; R-004, R-005)
- [ ] T023 [US4] `ConsultarFiltradoUseCase` — `Ma/Application/ConsultarFiltradoUseCase.cs`
- [ ] T024 [US4] `FiltrosApiTests`: escenarios 5–8 y 13 del quickstart (`estado` con token de conductor y sin token → 403); sin `ETag`; cabeceras por rol — `IntMa/FiltrosApiTests.cs` · (RF-17, FR-011; T-SEG20)

## Phase 6: User Story 3 — Copia offline con `updatedSince` (P1)

- [ ] T025 [P] [US3] `ConsultarCambiosUseCaseTests`: alta visible; baja sin estado; anónimo: ni altas ni bajas fuera del polígono demo, en cualquier estado (T-SEG16); administrador sin bajas; `generadoUtc` del `TimeProvider`; futuro → inválido — `MaT/Application/ConsultarCambiosUseCaseTests.cs` · (FR-012, FR-013; R-006)
- [ ] T026 [US3] `ConsultarCambiosUseCase` — `Ma/Application/ConsultarCambiosUseCase.cs`
- [ ] T027 [US3] `CambiosApiTests`: escenarios 9 y 10 del quickstart — `IntMa/CambiosApiTests.cs` · (RF-14, RF-26)

## Phase 7: Polish

- [ ] T028 [P] `README.md` del módulo (tabla por rol, caché, `ETag`, `updatedSince`, ejemplos) — `Ma/README.md` · (RNF-19)
- [ ] T029 Prueba de contrato con `contracts/mapa.openapi.yaml` en verde
- [ ] T030 Regla de caché de Cloudflare (cachear `GET /api/v1/anomalies` solo sin `Authorization` ni cookie `ku_acceso`, respetando `Cache-Control` del origen) documentada en `deploy/runbooks/monitoreo.md` y comprobada como en el quickstart §4; la tasa de aciertos en el borde se lee en las analíticas de Cloudflare y se anota en ese *runbook* · (ADR-006, SC-002; R-013)
- [ ] T031 Medir SC-001 con las pruebas de carga de 001 (`tests/carga/carga-combinada.k6.js`: 300 consultando el mapa + 300 sincronizando, 10 min; y `picos.k6.js`), fuera de la VM contra el entorno de pruebas; anotar el p95 del mapa. 006 no tiene script propio · (RNF-01, EQ-14; D11)
- [ ] T032 Marcar `checklists/trazabilidad.md` (los pendientes de `research.md` ya fueron confirmados por el autor el 2026-10-21)

## Dependencies & Execution Order

Phase 1 → Phase 2 → US2 → US1 → (US4, US3 en paralelo) → Polish. US2 va primero porque construye la caché y el `ETag` que US1 reutiliza con el alcance `Anonimo`. Antes de T012, 004 debe tener `CeldasDelCorredor` (su T042) y 001 `Celda.Limites` (su T016).
