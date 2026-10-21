---

description: "Tareas del módulo Ingesta"
---

# Tasks: Módulo Ingesta

**Input**: `specs/005-modulo-ingesta/` · **Requiere**: 001 (fases 1–6) y 004 (Phase 2 para empezar con dobles; US2 para integrar de verdad)

**Tests**: obligatorias y **primero** (constitución VII; el contrato del lote y la prueba de reinicio son críticos: EQ-02, EQ-09).

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `backend/`. Módulo: `src/Modules/KillaUru.Ingesta/` (`In/`). Pruebas: `tests/KillaUru.Ingesta.Tests/` (`InT/`) y `tests/KillaUru.Integracion.Tests/Ingesta/` (`IntIn/`). **Ningún `.cs` > 100 líneas.**

## Phase 1: Setup

- [ ] T001 Migración `V0006__ingesta.sql` (tabla e índices de `data-model.md` §1) — `db/migraciones/V0006__ingesta.sql`
- [ ] T002 [P] `IngestaOptions` con `ValidateOnStart` y su sección en `appsettings.json` — `In/Application/IngestaOptions.cs` · (ADR-012)
- [ ] T003 [P] Prueba que `Ingesta:PurgaSeudonimoHoras` = `Anomalias:PurgaSeudonimoHoras` en `appsettings.json` — `IntIn/ConfiguracionTests.cs` · (plan, Complexity Tracking)
- [ ] T004 [P] Semillas: lotes de ejemplo sobre la polilínea (`lote-50-buenos.json`, `lote-con-errores.json`) — `db/semillas/lotes/`

## Phase 2: Foundational — dominio mínimo

### Pruebas (primero; deben fallar)

- [ ] T005 [P] `LoteAceptadoTests`: separa aceptados y rechazados; repetidos en el lote → `RepetidosEnLote` (R-003); `ComoParametros` sin seudónimo en el payload — `InT/Domain/LoteAceptadoTests.cs` · (FR-006, FR-008)
- [ ] T006 [P] `HoraDeDeteccionTests`: 14:37:12Z → [14:00Z, 15:00Z); 22:59:59 Lima cae en el bloque 03:00Z — `InT/Domain/HoraDeDeteccionTests.cs` · (RN-15a; R-007)
- [ ] T007 [P] `EventoDtoLectorTests`: campo faltante → `null`; `"severidad": "alta"` → `null` sin excepción; `eventoId` inválido → lo señala para el 400 — `InT/Adapters/EventoDtoLectorTests.cs` · (FR-002, FR-003; R-001, R-002)

### Implementación

- [ ] T008 [P] `LoteEntrante`, `LoteAceptado`, `ResultadoLote`, `EstadoRecepcion`, `HoraDeDeteccion` — `In/Domain/`
- [ ] T009 [P] `LoteDto`, `EventoDtoLector`, `RespuestaLoteDto`, `RechazoDto` (con XML y `<example>`) — `In/Adapters/Dtos/` · (FR-002, Constitución IX)

**Checkpoint**: `KillaUru.Ingesta.Tests` en verde.

---

## Phase 3: User Story 1 — Enviar un lote y saber qué pasó con cada evento (P1) 🎯 MVP

### Pruebas

- [ ] T010 [P] [US1] `RecibirLoteEventosUseCaseTests` (dobles de `IAnomaliasService`, `IRecepcionEventos`, `IColaLotes`): vacío/51 → `Invalido`; sin capacidad → `Saturado` sin insertar; una sola `recibidoUtc` para todo el lote; listas disjuntas; métricas de lote y de rechazo por motivo — `InT/Application/RecibirLoteEventosUseCaseTests.cs` · (FR-003…FR-010)
- [ ] T011 [US1] `RecepcionEventosPostgresTests` (Testcontainers): un solo `INSERT`; devuelve solo los nuevos; reinsertar = 0 nuevos — `IntIn/RecepcionEventosPostgresTests.cs` · (FR-005)
- [ ] T012 [US1] `RecepcionApiTests` (`WebApplicationFactory`): escenarios 1–11 del quickstart — `IntIn/RecepcionApiTests.cs` · (SC-004)

### Implementación

- [ ] T013 [US1] Puertos `IRecepcionEventos` e `IColaLotes` — `In/Application/Puertos/`
- [ ] T014 [US1] `IngestaDbContext`, `EventoRecibidoConfig` y `RecepcionEventosPostgres` (SQL de `diseño-interno` §6.4) — `In/Infrastructure/` · (FR-005)
- [ ] T015 [US1] `RecibirLoteEventosUseCase` (esqueleto de `enfoque` §6, con `recibidoUtc`) — `In/Application/Recepcion/` · (FR-004…FR-010)
- [ ] T016 [US1] `SyncController`: `[RequestSizeLimit]`, 403 al administrador, `con_sesion` por rol, mapeo de `ResultadoLote` a 200/400/503 con `Retry-After` — `In/Adapters/SyncController.cs` · (FR-001, FR-007; R-004, R-006)
- [ ] T017 [US1] `ColaLotesChannel` (acotado, `HayCapacidad`, `TryEscribir`, `LeerAsync`) — `In/Infrastructure/ColaLotesChannel.cs` · (FR-011)
- [ ] T018 [US1] `AddIngestaModule` (recepción) — `In/IngestaModule.cs`

**Checkpoint**: la app puede enviar lotes y recibir el resultado por evento.

---

## Phase 4: User Story 3 — Procesar en segundo plano sin saturarse (P1)

### Pruebas

- [ ] T019 [P] [US3] `ProcesarLoteUseCaseTests` (dobles): orden por `detectado_utc`; contexto con `ConSesion`, conteo y `RecibidoUtc`; marca `Procesado` ante `Procesado` y `YaProcesado`, `Descartado` con motivo; excepción → no marca y sigue con el siguiente — `InT/Application/ProcesarLoteUseCaseTests.cs` · (FR-012…FR-014; R-005)
- [ ] T020 [US3] `PendientesRepositorioTests` (Testcontainers): `RecibidosDelLoteAsync` usa `ix_recibido_lote`; `ContarEnLaHoraAsync` con límites de la hora; `MarcarAsync` — `IntIn/PendientesRepositorioTests.cs` · (FR-013; R-007)

### Implementación

- [ ] T021 [US3] Puerto `IPendientesRepositorio` y `PendientesRepositorio` — `In/Application/Puertos/`, `In/Infrastructure/PendientesRepositorio.cs`
- [ ] T022 [US3] `ArmadorContexto` y `ProcesarLoteUseCase` — `In/Application/Procesamiento/` · (FR-012…FR-015)
- [ ] T023 [US3] `ProcesadorEventosBackgroundService` (N = `Trabajadores` o núcleos; un *scope* por lote) — `In/Infrastructure/ProcesadorEventosBackgroundService.cs` · (FR-012)
- [ ] T024 [US3] `ProcesamientoTests` de punta a punta con Anomalías real: lote → filas `Procesado`/`Descartado` y anomalías creadas — `IntIn/ProcesamientoTests.cs`

## Phase 5: User Story 2 — Ningún evento aceptado se pierde (P1)

- [ ] T025 [P] [US2] `BarrerPendientesUseCaseTests`: al arrancar encola todo; después, solo lo de más de 60 s; respeta la capacidad (lotes de 100 ids) — `InT/Application/BarrerPendientesUseCaseTests.cs` · (FR-016)
- [ ] T026 [US2] `BarrerPendientesUseCase` y `BarredorBackgroundService` (al arrancar + `PeriodicTimer` sobre `TimeProvider`) — `In/Application/Procesamiento/`, `In/Infrastructure/BarredorBackgroundService.cs` · (FR-016)
- [ ] T027 [US2] `ReinicioTests` (EQ-09): aceptar 500 eventos, detener el *host* a mitad del procesamiento, arrancar otro y verificar conciliación exacta y 0 duplicados en ≤ 2 min — `IntIn/ReinicioTests.cs` · (SC-003, RNF-06)

## Phase 6: User Story 4 — Privacidad y retención (P1)

- [ ] T028 [P] [US4] Puerto `IPurgasRecepcion` y `PurgasRecepcionPostgres` (lotes de 5 000; la de 30 días solo estados 1 y 2) — `In/Application/Puertos/`, `In/Infrastructure/PurgasRecepcionPostgres.cs` · (FR-017, FR-018; R-008)
- [ ] T029 [US4] `PurgarSeudonimosRecepcionUseCase` y `PurgarRecepcionUseCase` (métrica de tarea) + `TareaPurgaSeudonimosRecepcion` (60 min) y `TareaPurgaRecepcion` (diaria) — `In/Application/Tareas/`, `In/Infrastructure/Tareas/` · (FR-017, FR-018, FR-020)
- [ ] T030 [US4] `PurgasTests`: escenarios 15 y 16; `payload` sin seudónimo; esquema sin cuentas, correos ni tokens — `IntIn/PurgasTests.cs` · (SC-005, FR-019)

## Phase 7: User Story 5 — Salud de la cola (P2)

- [ ] T031 [US5] `ChequeoColaIngesta` (caché de 5 s) y `ObservarCola` (tamaño del `Channel` + antigüedad) — `In/Infrastructure/ChequeoColaIngesta.cs`, `In/Infrastructure/ColaLotesChannel.cs` · (FR-021, FR-022; R-009)
- [ ] T032 [US5] `SaludIngestaTests`: escenario 14; con la cola al día `/health/ready` = 200 — `IntIn/SaludIngestaTests.cs`

## Phase 8: Polish

- [ ] T033 [P] `README.md` del módulo (flujo, contrato del lote, parámetros, conciliación) — `In/README.md` · (RNF-19)
- [ ] T034 Prueba de contrato con `contracts/ingesta.openapi.yaml` en verde
- [ ] T035 Script de carga `tests/carga/lotes.k6.js` (eventos sobre la polilínea, velocidad plausible) y medición de SC-001 y SC-002 junto con 004 (T053) · (RNF-02, RNF-03, RNF-23)
- [ ] T036 Marcar `checklists/trazabilidad.md` y llevar los «Pendientes de confirmar» de `research.md` al autor

## Dependencies & Execution Order

Phase 1 → Phase 2 → US1 → US3 → US2 → (US4, US5 en paralelo) → Polish. US1 y US3 pueden avanzar con dobles de `IAnomaliasService` antes de que 004 termine su US2; T024, T027 y T035 necesitan Anomalías real.
