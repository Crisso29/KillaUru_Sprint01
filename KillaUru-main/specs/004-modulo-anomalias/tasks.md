---

description: "Tareas del módulo Anomalías"
---

# Tasks: Módulo Anomalías

**Input**: `specs/004-modulo-anomalias/` · **Requiere**: 001 (fases 1–6) y 002 (fases 1–3)

**Tests**: obligatorias y **primero** (constitución VII: es el módulo con más reglas).

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `backend/`. Módulo: `src/Modules/KillaUru.Anomalias/` (`An/`). Pruebas: `tests/KillaUru.Anomalias.Tests/` (`AnT/`) y `tests/KillaUru.Integracion.Tests/Anomalias/` (`IntAn/`). **Ningún `.cs` > 100 líneas.**

## Phase 1: Setup

- [ ] T001 Migración `V0004__anomalias.sql` (4 tablas e índices de `data-model.md` §1) — `db/migraciones/V0004__anomalias.sql`
- [ ] T002 Obtener la polilínea PE-28A de OpenStreetMap (Pisco → Ayacucho), revisarla a mano, simplificarla sin perder curvas (tolerancia ≤ 5 m) y generar `V0005__corredor_via_datos.sql`; guardar el GeoJSON fuente en `db/datos/corredor_pe28a.geojson` · (RN-13)
- [ ] T003 [P] `AnomaliasOptions` (POCO, valores de la spec) y su sección en `appsettings.json` — `An/Domain/AnomaliasOptions.cs` · (ADR-012)

## Phase 2: Foundational — contratos y políticas puras (bloquean a 005, 006 y 007)

### Contratos

- [ ] T004 [P] `EventoReportado`, `ContextoReporte` (con `RecibidoUtc`), `ResultadoProceso`, `MotivoDescarte`, `TipoAnomalia`, `EstadoAnomalia`, `AnomaliaCambiada`, tipos de `Lecturas/` — `An/Contracts/` · (FR-001, FR-029; R-001, R-002)
- [ ] T005 [P] `IAnomaliasService` e `IAnomaliaLectura` con los comentarios de `contratos-internos.md` — `An/Contracts/`

### Pruebas del dominio (primero; deben fallar)

- [ ] T006 [P] `CorredorViaTests`: un punto sobre la vía → 0 m; a 49/50/51 m; km acumulado correcto en 3 vértices conocidos; curva cerrada (distancia por la vía ≠ línea recta); `Celdas` contiene la celda de todo punto a ≤ 50 m — `AnT/Domain/CorredorViaTests.cs` · (FR-003)
- [ ] T007 [P] `ReporteAnomaliaTests` (mapeo): campo faltante → `EsquemaInvalido`; tipo `Normal` o desconocido → `TipoDesconocido` — `AnT/Domain/ReporteAnomaliaTests.cs` · (R-002)
- [ ] T008 [P] `ReglasCalidadTests`: escenarios 1–4 del quickstart (contra `recibidoUtc`) — `AnT/Domain/ReglasCalidadTests.cs` · (FR-001, FR-002)
- [ ] T009 [P] `PoliticaPlausibilidadTests`: escenario 9; descuento de precisiones; tope 120 / 121 — `AnT/Domain/PoliticaPlausibilidadTests.cs` · (FR-004, FR-013)
- [ ] T010 [P] `PoliticaConfirmacionTests`: escenarios 5–8; `EsVehiculoNuevo` (seudónimo distinto y ≥ 3 min) — `AnT/Domain/PoliticaConfirmacionTests.cs` · (FR-010, FR-011)
- [ ] T011 [P] `ReglaPersistenciaTests`: escenario 11 (22:00 Lima) y ventana de 15 días — `AnT/Domain/ReglaPersistenciaTests.cs` · (FR-016)
- [ ] T012 [P] `AnomaliaReportesTests`: nace activa/pendiente; mediana; `Activa` no vuelve a pendiente; `vence_utc` según la tabla §3.3; escenarios 12 y 13; `ultima_deteccion` no retrocede — `AnT/Domain/AnomaliaReportesTests.cs` · (FR-008…FR-012, FR-017, FR-019)
- [ ] T013 [P] `AnomaliaModeracionTests`: cada acción desde cada uno de los 5 estados (20 casos) contra la tabla de RN-07; `MantenerDescartada` limpia marcas; «Por revisar» con 3 vehículos en 7 días (escenario 16) — `AnT/Domain/AnomaliaModeracionTests.cs` · (FR-020, FR-025, FR-026)
- [ ] T014 [P] `AnomaliaTransicionesTests`: una sola transición por operación (nace leve → `null→Activa`; nace grave y se confirma → `null→Pendiente`, `Pendiente→Activa`; renace leve → `Expirada→Activa`) — `AnT/Domain/AnomaliaTransicionesTests.cs` · (FR-015)

### Implementación del dominio

- [ ] T015 [P] `GeodesiaLocal` y `CorredorVia` (con índice por rejilla) — `An/Domain/` · (FR-003; R-003)
- [ ] T016 [P] `ReporteAnomalia` — `An/Domain/ReporteAnomalia.cs`
- [ ] T017 [P] `ReglasCalidad` — `An/Domain/ReglasCalidad.cs` · (FR-001, FR-002)
- [ ] T018 [P] `PoliticaPlausibilidad` — `An/Domain/PoliticaPlausibilidad.cs` · (FR-004, FR-013)
- [ ] T019 [P] `PoliticaConfirmacion` (código de referencia de `diseño-interno` §6.2) — `An/Domain/PoliticaConfirmacion.cs` · (FR-010, FR-011)
- [ ] T020 [P] `ReglaPersistencia` (código de referencia de `enfoque` §5) — `An/Domain/ReglaPersistencia.cs` · (FR-016)
- [ ] T021 [P] `EstadoPersistencia`, `AccionAdmin` (4 acciones), `Transicion`, `ResultadoModeracion`, `ReglasAnomalia` — `An/Domain/`
- [ ] T022 `Anomalia` en 4 archivos `partial` (`Anomalia.cs`, `.Reportes.cs`, `.Moderacion.cs`, `.Transiciones.cs`) — `An/Domain/` · (FR-008…FR-020, FR-025)

**Checkpoint**: `KillaUru.Anomalias.Tests` en verde; cobertura del dominio ≥ 90 % (SC-006). 005 puede empezar.

---

## Phase 3: User Story 1 — Solo los eventos de calidad cuentan (P1)

- [ ] T023 [US1] `EvaluarIntrinsecaUseCase` (mapea `EventoReportado` → `ReporteAnomalia` → `ReglasCalidad`) + prueba — `An/Application/Procesamiento/` · (FR-001)
- [ ] T024 [US1] `CargadorCorredor` (carga al arrancar + `IHealthCheck`) + prueba de integración — `An/Infrastructure/CargadorCorredor.cs`, `IntAn/CargadorCorredorTests.cs` · (FR-003)

## Phase 4: User Story 2 — Agrupar, confirmar y publicar (P1) 🎯 MVP

### Pruebas

- [ ] T025 [P] [US2] `ProcesarReporteUseCaseTests` (dobles): idempotencia; descarte → métrica y sin auditoría; publica `AnomaliaCambiada` solo tras confirmar; tope → `Crear(porTope: true)` — `AnT/Application/ProcesarReporteUseCaseTests.cs` · (FR-005, FR-006, FR-013, FR-014)
- [ ] T026 [US2] `AnomaliaRepositorioTests` (Testcontainers): `MasCercanaAsync` (mismo tipo, ≤ 100 m, la más cercana), `ReportesDesdeAsync`, `AnteriorDelSeudonimoAsync`, `GuardarAsync` con transiciones — `IntAn/AnomaliaRepositorioTests.cs` · (FR-008)
- [ ] T027 [US2] `ConcurrenciaTests`: escenario 15 con 8 trabajadores; mismo evento dos veces en paralelo; punto en esquina de 4 celdas sin interbloqueo — `IntAn/ConcurrenciaTests.cs` · (FR-006, FR-007, SC-002)

### Implementación

- [ ] T028 [US2] Puertos `IAnomaliaRepositorio` e `ITransaccionAnomalias` (`An/Application/Puertos/`), `AnomaliasDbContext` y 4 configuraciones (`geography`, `timestamptz[]`, enums `smallint`) — `An/Infrastructure/`
- [ ] T029 [US2] `TransaccionAnomalias` (bloqueos ordenados + `IContextoTransaccion.Unir`) — `An/Infrastructure/TransaccionAnomalias.cs` · (FR-007; R-004)
- [ ] T030 [US2] `AnomaliaRepositorio` (las 5 operaciones; `km_via` calculado con `CorredorVia`) — `An/Infrastructure/AnomaliaRepositorio.cs` · (FR-008)
- [ ] T031 [US2] `ProcesarReporteUseCase` (algoritmo de `enfoque` §6 pasos 0–6, con `RecibidoUtc` del contexto) y `AnomaliasService` — `An/Application/Procesamiento/` · (FR-006…FR-015)
- [ ] T032 [US2] `AddAnomaliasModule` (todo menos tareas y administración) — `An/AnomaliasModule.cs`

**Checkpoint**: escenarios 5–10, 14 y 15 del quickstart en verde. 005 puede integrarse.

---

## Phase 5: User Story 3 — Clasificar y caducar (P1)

- [ ] T033 [P] [US3] Puerto `ITareasRepositorio` y `TareasRepositorio.CaducarAsync` (CTE de R-008) — `An/Application/Puertos/`, `An/Infrastructure/TareasRepositorio.cs` · (FR-018)
- [ ] T034 [US3] `CaducarAnomaliasUseCase` (publica `AnomaliaCambiada` por fila tras el commit; métrica de tarea) + prueba unitaria — `An/Application/Tareas/` · (FR-018, FR-024)
- [ ] T035 [US3] `TareaCaducidad` (`PeriodicTimer` 60 min sobre `TimeProvider`) — `An/Infrastructure/Tareas/TareaCaducidad.cs` · (FR-018)
- [ ] T036 [US3] `TareasCaducidadTests` (Testcontainers + `FakeTimeProvider`): escenario 17; `SKIP LOCKED` no espera a un trabajador; transición con `ocurrida_utc = vence_utc` — `IntAn/TareasCaducidadTests.cs` · (SC-007)

## Phase 6: User Story 4 — Privacidad y retención (P1)

- [ ] T037 [P] [US4] `TareasRepositorio.PurgarSeudonimosAsync` y `PurgarHistoricoAsync` (lotes de 5 000) — `An/Infrastructure/TareasRepositorio.cs` · (FR-021, FR-022)
- [ ] T038 [US4] `PurgarSeudonimosUseCase` y `PurgarHistoricoUseCase` + `TareaPurgaSeudonimos` (60 min) y `TareaPurgaHistorica` (diaria) — `An/Application/Tareas/`, `An/Infrastructure/Tareas/` · (FR-021, FR-022, FR-024)
- [ ] T039 [US4] `PurgasTests`: escenario 18 (recibido hace 72 h 1 s → purgado; detectado hace días pero recibido hace 1 h → se conserva); 2 años exactos — `IntAn/PurgasTests.cs` · (SC-005)
- [ ] T040 [US4] Auditoría del esquema: ninguna columna de cuenta, correo o token en `anomalias` — `IntAn/EsquemaPrivacidadTests.cs` · (FR-023)

## Phase 7: User Story 7 — Lecturas para Mapa y Dashboard (P1)

- [ ] T041 [P] [US7] Puerto `ILecturaAnomaliasRepositorio`, `LecturaAnomaliasRepositorio` (activas por celdas, filtrado, cambios) y `ResumenDiaRepositorio` (R-011) — `An/Application/Puertos/`, `An/Infrastructure/` · (FR-029)
- [ ] T042 [US7] `AnomaliaLectura` (implementa `IAnomaliaLectura`) — `An/Application/Lecturas/AnomaliaLectura.cs` · (FR-029)
- [ ] T043 [US7] `LecturasTests`: cada operación con datos sembrados; resumen con día local UTC−5 (reporte a las 23:30 Lima cuenta para ese día); `CeldasDelCorredor` vacío antes de cargar — `IntAn/LecturasTests.cs` · (US7)

**Checkpoint**: 006 y 007 pueden integrarse.

---

## Phase 8: User Story 5 — Moderación (P2)

- [ ] T044 [P] [US5] Pruebas de `ModerarAnomaliaUseCase` (bloqueo de celda, auditoría `AnomaliaModerada` en la misma transacción, transición con `actor_id`, `AnomaliaCambiada`, 409) y de `ListarPorRevisarUseCase`, `ListarPendientesUseCase`, `ObtenerDetalleUseCase`, `ListarTransicionesRecientesUseCase` — `AnT/Application/Administracion/` · (FR-025…FR-027)
- [ ] T045 [US5] Los 5 casos de uso — `An/Application/Administracion/` · (FR-025…FR-027)
- [ ] T046 [US5] `ModeracionController` y `DetalleAnomaliaController` (rutas con `{id:guid}` para no chocar con `/transiciones`) + DTOs documentados — `An/Adapters/` · (Constitución IX)
- [ ] T047 [US5] `ModeracionApiTests`: 403 a conductores; tabla completa de RN-07 por HTTP; listas y detalle con los campos del contrato — `IntAn/ModeracionApiTests.cs`

## Phase 9: User Story 6 — Exportación (P3, recortable)

- [ ] T048 [P] [US6] `IEscritorExportacion`, `EscritorCsv` y `EscritorGeoJson` (*streaming*) + pruebas: sin columna `seudonimo`; escapes CSV — `An/Application/Exportacion/`, `An/Infrastructure/Exportacion/` · (FR-028)
- [ ] T049 [US6] `ExportarDatasetUseCase` (audita `DatasetExportado` antes de escribir) y `ExportacionController` — `An/Application/Exportacion/`, `An/Adapters/ExportacionController.cs` · (FR-028)
- [ ] T050 [US6] `ExportacionApiTests` (CSV y GeoJSON, filtros, auditoría con filas) — `IntAn/ExportacionApiTests.cs`

## Phase 10: Polish

- [ ] T051 [P] `README.md` del módulo (reglas → clases, endpoints, ejemplos, tareas programadas) — `An/README.md` · (RNF-19)
- [ ] T052 Prueba de contrato con `contracts/anomalias.openapi.yaml` en verde
- [ ] T053 Medir p95 del procesamiento con 300 dispositivos (junto con 005) ≤ 100 ms · (SC-001, RNF-23)
- [ ] T054 Escenario EQ-12 completo (ataque + subida legítima de 5 h + lluvia de 600 eventos/h) — `IntAn/IntegridadDatoTests.cs` · (SC-003, SC-004)
- [ ] T055 Marcar `checklists/trazabilidad.md` y llevar los «Pendientes de confirmar» de `research.md` al autor

## Dependencies & Execution Order

Phase 1 → Phase 2 → US1 → US2 → (US3, US4, US7 en paralelo) → US5 → US6 → Polish. Al terminar la Phase 2 puede empezar 005 con dobles; al terminar US2 puede integrarse de verdad; al terminar US7 pueden empezar 006 y 007.
