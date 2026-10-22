---

description: "Tareas del módulo Ingesta"
---

# Tasks: Módulo Ingesta

**Input**: `specs/005-modulo-ingesta/` · **Requiere**: 001 (fases 1–6) y 004 (Phase 2 para empezar con dobles; US2 para integrar de verdad)

**Tests**: obligatorias y **primero** (constitución VII; el contrato del lote y la prueba de reinicio son críticos: EQ-02, EQ-09).

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `backend/`. Módulo: `src/Modules/KillaUru.Ingesta/` (`In/`). Pruebas: `tests/KillaUru.Ingesta.Tests/` (`InT/`) y `tests/KillaUru.Integracion.Tests/Ingesta/` (`IntIn/`). **Ningún `.cs` > 100 líneas.**

## Phase 1: Setup

- [ ] T001 Migración `V0006__ingesta.sql` (tabla e índices de `data-model.md` §1: `lote_id` NULLable, `CHECK (motivo BETWEEN 1 AND 8)`, `ix_recibido_purga`) — `db/migraciones/V0006__ingesta.sql` · (FR-017, FR-025; D3, T-BD06, T-BD-CHK)
- [ ] T002 [P] `IngestaOptions` con `ValidateOnStart` y su sección en `appsettings.json` — `In/Application/IngestaOptions.cs` · (ADR-012)
- [ ] T003 [P] Prueba que `Ingesta:PurgaSeudonimoHoras` = `Anomalias:PurgaSeudonimoHoras` en `appsettings.json` — `IntIn/ConfiguracionTests.cs` · (plan, Complexity Tracking)
- [ ] T004 [P] Semillas: lotes de ejemplo sobre la polilínea, sin `loteId` y con un solo seudónimo (`lote-50-buenos.json`, `lote-con-errores.json`) — `db/semillas/lotes/` · (D2, D3)

## Phase 2: Foundational — dominio mínimo y tipos del lote

### Pruebas (primero; deben fallar)

- [ ] T005 [P] `LoteAceptadoTests`: separa aceptados y rechazados; repetidos en el lote → `RepetidosEnLote` (R-003); `ComoParametros` sin seudónimo en el payload — `InT/Application/LoteAceptadoTests.cs` · (FR-006, FR-008)
- [ ] T006 [P] `HoraDeDeteccionTests`: 14:37:12Z → [14:00Z, 15:00Z); 22:59:59 Lima cae en el bloque 03:00Z — `InT/Domain/HoraDeDeteccionTests.cs` · (RN-15a; R-007)
- [ ] T007 [P] `LectorEventoTests`: campo faltante → `null`; `"severidad": "alta"` → `null` sin excepción; `eventoId` faltante o no UUID → `id: null` con su `indice` (luego `EsquemaInvalido`) — `InT/Application/LectorEventoTests.cs` · (FR-002, FR-023; R-001, R-002)
- [ ] T038 [P] `SeudonimoDelLoteTests`: cabecera válida gana; sin cabecera, el primer evento con seudónimo válido; seudónimo inválido no fija el del lote; otro seudónimo válido → `SeudonimoInconsistente`; sin `eventoId` se evalúa antes (`EsquemaInvalido`) — `InT/Application/SeudonimoDelLoteTests.cs` · (FR-024; D2, R-010)

### Implementación

- [ ] T008 [P] `EstadoRecepcion` y `HoraDeDeteccion` (sin tipos de Anomalías) — `In/Domain/` · (D4)
- [ ] T037 [P] `LoteEntrante`, `LectorEvento`, `SeudonimoDelLote`, `LoteAceptado`, `ResultadoLote` + `Rechazo(Id?, Indice, Motivo)`, `LoteRecibido(Guid?)`, `EventoPendiente` — `In/Application/Recepcion/`, `In/Application/Procesamiento/` · (FR-002, FR-006, FR-023, FR-024; D4)
- [ ] T009 [P] `LoteDto` (sin `loteId`), `RespuestaLoteDto`, `RechazoDto` (`id` nullable, `indice`) con XML y `<example>`; solo usan Application — `In/Adapters/Dtos/` · (FR-006, FR-025, Constitución IX; D4, D5)

**Checkpoint**: `KillaUru.Ingesta.Tests` en verde; cobertura del dominio ≥ 90 %.

---

## Phase 3: User Story 1 — Enviar un lote y saber qué pasó con cada evento (P1) 🎯 MVP

### Pruebas

- [ ] T010 [P] [US1] `RecibirLoteEventosUseCaseTests` (dobles de `IAnomaliasService`, `IRecepcionEventos`, `IColaLotes`): vacío/51 → `Invalido`; sin capacidad → `Saturado` sin insertar; una sola `recibidoUtc` y un `loteId` UUIDv7 generado por el servidor; evento sin `eventoId` → `Rechazo(null, indice, EsquemaInvalido)`; `SeudonimoInconsistente`; listas disjuntas; métricas de lote y de rechazo por motivo — `InT/Application/RecibirLoteEventosUseCaseTests.cs` · (FR-003…FR-010, FR-023…FR-025)
- [ ] T011 [US1] `RecepcionEventosPostgresTests` (Testcontainers): un solo `INSERT`; devuelve solo los nuevos; reinsertar = 0 nuevos — `IntIn/RecepcionEventosPostgresTests.cs` · (FR-005)
- [ ] T012 [US1] `RecepcionApiTests` (`WebApplicationFactory`): escenarios 1–12 y 17–21 del quickstart — `IntIn/RecepcionApiTests.cs` · (SC-004, FR-001, FR-007, FR-023…FR-025)

### Implementación

- [ ] T013 [US1] Puertos `IRecepcionEventos` e `IColaLotes` — `In/Application/Puertos/`
- [ ] T014 [US1] `IngestaDbContext`, `EventoRecibidoConfig` y `RecepcionEventosPostgres` (SQL de `diseño-interno` §6.4) — `In/Infrastructure/` · (FR-005)
- [ ] T015 [US1] `RecibirLoteEventosUseCase` (esqueleto de `enfoque` §6: `loteId` UUIDv7, `recibidoUtc`, orden de R-010) — `In/Application/Recepcion/` · (FR-004…FR-010, FR-023…FR-025)
- [ ] T016 [US1] `SyncController` (sin tipos de Domain): `[RequestSizeLimit]`, 403 al administrador (`01-actores.md` §4), `con_sesion` = `ConductorRegistrado` y *claim* `verificado = true`, cabecera `X-KillaUru-Seudonimo` → `LoteEntrante`, mapeo de `ResultadoLote` a 200/400/503 con `Retry-After` — `In/Adapters/SyncController.cs` · (FR-001, FR-007; R-004, R-006, R-013; D4, D8)
- [ ] T017 [US1] `ColaLotesChannel` (acotado, `HayCapacidad`, `TryEscribir`, `LeerAsync`) — `In/Infrastructure/ColaLotesChannel.cs` · (FR-011)
- [ ] T018 [US1] `AddIngestaModule` (recepción) — `In/IngestaModule.cs`

**Checkpoint**: la app puede enviar lotes y recibir el resultado por evento.

---

## Phase 4: User Story 3 — Procesar en segundo plano sin saturarse (P1)

### Pruebas

- [ ] T019 [P] [US3] `ProcesarLoteUseCaseTests` (dobles): orden por `detectado_utc`; contexto con `ConSesion`, conteo y `RecibidoUtc`; marca `Procesado` ante `Procesado` y `YaProcesado`, `Descartado` con motivo; `RegistrarRetrasoCola(ahora − recibido)` al marcar; excepción → no marca, sin métrica, y sigue con el siguiente — `InT/Application/ProcesarLoteUseCaseTests.cs` · (FR-012…FR-014, FR-026; R-005, R-015)
- [ ] T020 [US3] `PendientesRepositorioTests` (Testcontainers): `RecibidosDelLoteAsync` usa `ix_recibido_lote` (también con `lote_id IS NULL`); `ContarEnLaHoraAsync` con límites de la hora; `MarcarAsync`; `BarridoPendientesPostgres` — `IntIn/PendientesRepositorioTests.cs` · (FR-013, FR-016; R-007, R-011)

### Implementación

- [ ] T021 [US3] Puertos `IPendientesRepositorio` e `IBarridoPendientes`; `PendientesRepositorio` y `BarridoPendientesPostgres` (partidos para no pasar de 100 líneas) — `In/Application/Puertos/`, `In/Infrastructure/` · (FR-013, FR-016; T-ARQ08)
- [ ] T022 [US3] `ArmadorContexto` y `ProcesarLoteUseCase` (con `RegistrarRetrasoCola`) — `In/Application/Procesamiento/` · (FR-012…FR-015, FR-026; D11)
- [ ] T023 [US3] `ProcesadorEventosBackgroundService` (N = `Trabajadores` o núcleos; un *scope* por lote) — `In/Infrastructure/ProcesadorEventosBackgroundService.cs` · (FR-012)
- [ ] T024 [US3] `ProcesamientoTests` de punta a punta con Anomalías real: lote → filas `Procesado`/`Descartado` y anomalías creadas — `IntIn/ProcesamientoTests.cs` · (FR-013, FR-014)

## Phase 5: User Story 2 — Ningún evento aceptado se pierde (P1)

- [ ] T025 [P] [US2] `BarrerPendientesUseCaseTests`: al arrancar encola todo; después, solo lo de más de 60 s; filas sin `lote_id` → aviso `LoteRecibido(null)`; respeta la capacidad (lotes de 100 ids) — `InT/Application/BarrerPendientesUseCaseTests.cs` · (FR-016; R-011)
- [ ] T026 [US2] `BarrerPendientesUseCase` y `BarredorBackgroundService` (al arrancar + `PeriodicTimer` de 60 s sobre `TimeProvider`) — `In/Application/Procesamiento/`, `In/Infrastructure/BarredorBackgroundService.cs` · (FR-016)
- [ ] T027 [US2] `ReinicioTests` (EQ-09): aceptar 500 eventos, detener el *host* a mitad del procesamiento, arrancar otro y verificar conciliación exacta y 0 duplicados en ≤ 2 min — `IntIn/ReinicioTests.cs` · (SC-003, RNF-06)

## Phase 6: User Story 4 — Privacidad y retención (P1)

- [ ] T028 [P] [US4] Puerto `IPurgasRecepcion` y `PurgasRecepcionPostgres` (SQL de `data-model.md` §3: la de 72 h pone `seudonimo`, `lote_id` en `NULL` y `recibido_utc = date_trunc('hour', recibido_utc)` en una sola sentencia con `ix_recibido_purga`; la de 30 días solo estados 1 y 2; lotes de 5 000) — `In/Application/Puertos/`, `In/Infrastructure/PurgasRecepcionPostgres.cs` · (FR-017, FR-018; D3, R-008)
- [ ] T029 [US4] `PurgarSeudonimosRecepcionUseCase` y `PurgarRecepcionUseCase` (métrica `purga-seudonimos-recepcion` / `purga-recepcion`) + `TareaPurgaSeudonimosRecepcion` (al arrancar y cada hora en :10) y `TareaPurgaRecepcion` (al arrancar y 03:00 hora local), ambas con `ProgramaReloj` — `In/Application/Tareas/`, `In/Infrastructure/Tareas/` · (FR-017, FR-018, FR-020; T-OPS05)
- [ ] T039 [P] [US4] `TareasPorRelojTests` (`FakeTimeProvider`): corren al arrancar; la horaria dispara en el minuto :10 y la diaria a las 03:00 hora local, también tras reinicios seguidos — `InT/Infrastructure/TareasPorRelojTests.cs` · (FR-017, FR-018, FR-020; T-OPS05)
- [ ] T030 [US4] `PurgasTests`: escenarios 15 y 16 (seudónimo y `lote_id` en `NULL`, hora redondeada; filas `Recibido` > 30 días se conservan); `payload` sin seudónimo; esquema sin cuentas, correos ni tokens — `IntIn/PurgasTests.cs` · (SC-005, FR-017…FR-019)

## Phase 7: User Story 5 — Salud de la cola (P2)

- [ ] T031 [US5] `ChequeoColaIngesta` (caché de 5 s) y `ObservarCola` (tamaño del `Channel` + antigüedad) — `In/Infrastructure/ChequeoColaIngesta.cs`, `In/Infrastructure/ColaLotesChannel.cs` · (FR-021, FR-022; R-009)
- [ ] T032 [US5] `SaludIngestaTests`: escenario 14; con la cola al día `/health/ready` = 200 — `IntIn/SaludIngestaTests.cs` · (FR-021)
- [ ] T040 [US5] Alerta de Grafana «pendiente más viejo de Ingesta > 72 h» (riesgo de R-008 frente a RNF-25) y paso de reproceso manual, en el runbook de monitoreo de 001 — `deploy/runbooks/monitoreo.md` · (FR-018, FR-022; SEG-05)

## Phase 8: Polish

- [ ] T033 [P] `README.md` del módulo (flujo, contrato del lote con `indice` y `SeudonimoInconsistente`, parámetros, horario de tareas, conciliación) — `In/README.md` · (RNF-19)
- [ ] T034 Prueba de contrato con `contracts/ingesta.openapi.yaml` en verde · (FR-001, FR-006)
- [ ] T035 Medir SC-001 y SC-002 con los scripts de 001 (`tests/carga/carga-combinada.k6.js`, `picos.k6.js`, `estres.k6.js`, `resistencia.k6.js`, `caos.sh`), fuera de la VM contra el entorno de pruebas, con `killauru.ingesta.retraso_cola_s`; Ingesta no tiene script propio · (RNF-02, RNF-03, RNF-23, FR-026; D11)
- [ ] T036 Marcar `checklists/trazabilidad.md` (los pendientes de `research.md` ya están confirmados por el autor)

## Dependencies & Execution Order

Phase 1 → Phase 2 → US1 → US3 → US2 → (US4, US5 en paralelo) → Polish. US1 y US3 pueden avanzar con dobles de `IAnomaliasService` antes de que 004 termine su US2; T024, T027 y T035 necesitan Anomalías real; T035 necesita los scripts de carga de 001.
