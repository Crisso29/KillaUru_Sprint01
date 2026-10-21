---

description: "Tareas del módulo Dashboard"
---

# Tasks: Módulo Dashboard

**Input**: `specs/007-modulo-dashboard/` · **Requiere**: 001 (fases 1–6), 003 (`IIdentidadConsultas`) y 004 (US7, `ResumenDelDiaAsync`)

**Tests**: obligatorias y **primero** (constitución VII; las fórmulas se prueban con valores calculados a mano).

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `backend/`. Módulo: `src/Modules/KillaUru.Dashboard/` (`Da/`). Pruebas: `tests/KillaUru.Dashboard.Tests/` (`DaT/`) y `tests/KillaUru.Integracion.Tests/Dashboard/` (`IntDa/`). **Ningún `.cs` > 100 líneas.**

> **Recortable**: HU-13 es *Should* y es la 4.ª en el orden de recorte. Si se recorta, no se crea el módulo (el esquema `dashboard` queda vacío).

## Phase 1: Setup

- [ ] T001 Migración `V0007__dashboard.sql` (`data-model.md` §1) — `db/migraciones/V0007__dashboard.sql`
- [ ] T002 [P] `DashboardOptions` con `ValidateOnStart` y su sección en `appsettings.json` — `Da/Application/DashboardOptions.cs` · (ADR-012)
- [ ] T003 [P] Semilla de 60 días con 2 huecos — `db/semillas/dashboard-60-dias.sql`

## Phase 2: Foundational — dominio

### Pruebas (primero; deben fallar)

- [ ] T004 [P] `DiaLocalTests`: 2026-10-21 04:59Z → hoy = 20 oct; 05:00Z → 21 oct; `DiasFaltantes` con 0, 1 y 2 huecos — `DaT/Domain/DiaLocalTests.cs` · (FR-002)
- [ ] T005 [P] `ProgramaDiarioTests`: 05:29:59Z → hoy 05:30Z; 05:30:01Z → mañana 05:30Z — `DaT/Domain/ProgramaDiarioTests.cs` · (FR-001)
- [ ] T006 [P] `CalculadoraIndicadoresTests`: escenarios 5–9 del quickstart; variación en %, absoluta y en puntos según `research.md` R-003; redondeo a 1 decimal — `DaT/Domain/CalculadoraIndicadoresTests.cs` · (FR-007…FR-010, FR-012)

### Implementación

- [ ] T007 [P] `Indicador`, `DiaIndicadores`, `DiaLocal`, `ProgramaDiario` — `Da/Domain/`
- [ ] T008 [P] `IndicadorResumen` y `CalculadoraIndicadores` — `Da/Domain/` · (FR-007…FR-010)

**Checkpoint**: `KillaUru.Dashboard.Tests` (dominio) en verde.

---

## Phase 3: User Story 1 — Calcular y guardar los indicadores de cada día (P1) 🎯 MVP

### Pruebas

- [ ] T009 [P] [US1] `CalcularIndicadoresUseCaseTests` (dobles de `IAnomaliaLectura`, `IIdentidadConsultas`, repositorio): escenarios 1–4 del quickstart; métrica `indicadores` solo si todo salió bien — `DaT/Application/CalcularIndicadoresUseCaseTests.cs` · (FR-002, FR-003, FR-005)
- [ ] T010 [US1] `IndicadoresRepositorioTests` (Testcontainers): día completo en una transacción; `ON CONFLICT DO NOTHING`; `DiasCompletosAsync` solo cuenta días con 9 filas; `LeerAsync` con huecos; purga — `IntDa/IndicadoresRepositorioTests.cs` · (FR-003; R-004)

### Implementación

- [ ] T011 [US1] Puerto `IIndicadoresRepositorio`, `DashboardDbContext`, `IndicadorDiarioConfig` e `IndicadoresRepositorio` — `Da/Application/Puertos/`, `Da/Infrastructure/`
- [ ] T012 [US1] `CalcularIndicadoresUseCase` (días faltantes → contratos → guardar → purga → métrica) — `Da/Application/CalcularIndicadoresUseCase.cs` · (FR-002…FR-005)
- [ ] T013 [US1] `TareaIndicadoresDiarios` (al arrancar y a las 00:30 Lima con `Task.Delay(…, TimeProvider)`) — `Da/Infrastructure/TareaIndicadoresDiarios.cs` · (FR-001)
- [ ] T014 [US1] `AddDashboardModule` — `Da/DashboardModule.cs`
- [ ] T015 [US1] `TareaIndicadoresTests` (Testcontainers + `FakeTimeProvider` + Anomalías e Identidad reales con datos sembrados): el día guardado coincide con un conteo a mano — `IntDa/TareaIndicadoresTests.cs` · (SC-001, SC-004)

## Phase 4: User Story 2 — Ver el resumen en el panel (P1)

- [ ] T016 [P] [US2] `ConsultarResumenUseCaseTests`: lee `dias × 2 + 7` días en una sola llamada; `eventosPorDia` alineado con el periodo anterior; sin datos → `null` — `DaT/Application/ConsultarResumenUseCaseTests.cs` · (FR-006…FR-012, SC-003)
- [ ] T017 [US2] `ConsultarResumenUseCase` — `Da/Application/ConsultarResumenUseCase.cs`
- [ ] T018 [US2] `DashboardController` (`[Authorize(SoloAdministrador)]`, `dias` ∈ {7, 14, 30}) y DTOs con XML y `<example>` — `Da/Adapters/` · (FR-006, FR-011; Constitución IX)
- [ ] T019 [US2] `DashboardApiTests`: escenario 11; cuerpo con los 6 indicadores y el enlace — `IntDa/DashboardApiTests.cs`

## Phase 5: User Story 3 — Conservar 2 años (P3)

- [ ] T020 [US3] Prueba del escenario 10 (ya implementado en T012) — `IntDa/IndicadoresRepositorioTests.cs` · (FR-005)

## Phase 6: Polish

- [ ] T021 [P] `README.md` del módulo (fórmulas, hora del cálculo, por qué se guardan conteos) — `Da/README.md` · (RNF-19)
- [ ] T022 [P] Prueba de privacidad del esquema `dashboard` (quickstart §4) — `IntDa/EsquemaPrivacidadTests.cs` · (SC-002, RN-17)
- [ ] T023 Prueba de contrato con `contracts/dashboard.openapi.yaml` en verde
- [ ] T024 Marcar `checklists/trazabilidad.md` y llevar los «Pendientes de confirmar» de `research.md` al autor

## Dependencies & Execution Order

Phase 1 → Phase 2 → US1 → US2 → US3 → Polish. US1 puede avanzar con dobles antes de que 003 y 004 terminen; T015 necesita ambos reales.
