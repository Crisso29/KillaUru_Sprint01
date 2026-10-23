---

description: "Tareas del módulo Auditoría"
---

# Tasks: Módulo Auditoría

**Input**: `specs/002-modulo-auditoria/` · **Requiere**: fases 1–6 de `001-plataforma-backend`

**Tests**: obligatorias; primero la prueba (debe fallar), luego el código.

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `backend/`. Módulo: `src/Modules/KillaUru.Auditoria/` (abreviado `Aud/`). **Ningún `.cs` > 199 líneas.**

## Phase 1: Setup

- [x] T001 Migración `V0002__auditoria.sql` (tabla, `CHECK`, 3 índices, `REVOKE`) de `data-model.md` §1 — `db/migraciones/V0002__auditoria.sql` · (FR-008, FR-009: sin purga ni borrado, FR-010)
- [x] T002 [P] Referencias del proyecto (`KillaUru.Shared`, EF Core, Npgsql) — `Aud/KillaUru.Auditoria.csproj`

## Phase 2: Foundational (contratos públicos que otros módulos esperan)

- [x] T003 [P] `AccionAuditada` (14 códigos fijos) — `Aud/Contracts/AccionAuditada.cs` · (FR-005)
- [x] T004 [P] `ActorAuditoria` con `Sistema` — `Aud/Contracts/ActorAuditoria.cs` · (FR-004)
- [x] T005 [P] `IAuditoriaService` con el comentario XML de `contracts/contratos-internos.md` — `Aud/Contracts/IAuditoriaService.cs` · (FR-001)

**Checkpoint**: Identidad (003) y Anomalías (004) ya pueden compilar contra el contrato.

---

## Phase 3: User Story 1 — Ninguna acción sensible sin rastro (P1) 🎯 MVP

### Pruebas (primero)

- [x] T006 [P] [US1] `RegistroAuditoriaTests`: crea con datos válidos; rechaza acción fuera del enum; rechaza cada clave prohibida (también en mayúsculas); usa la hora del `FakeTimeProvider` — `tests/KillaUru.Auditoria.Tests/Domain/RegistroAuditoriaTests.cs` · (FR-004…FR-006)
- [x] T007 [P] [US1] `CatalogoAccionesTests`: las 14 acciones tienen categoría; solo `RolCambiado`, `DatasetExportado` y `CuentaBloqueada` son de riesgo — `tests/KillaUru.Auditoria.Tests/Domain/CatalogoAccionesTests.cs` · (FR-005, FR-013)
- [ ] T008 [P] [US1] `RegistrarAuditoriaUseCaseTests`: sin transacción unida → `InvalidOperationException`; guarda con el repositorio; registra `AlConfirmar` con `RegistrarAccionRiesgo` solo para las 3 de riesgo (no la llama directamente); contexto prohibido → excepción — `tests/KillaUru.Auditoria.Tests/Application/RegistrarAuditoriaUseCaseTests.cs` · (FR-002, FR-003, FR-013)
- [x] T009 [US1] `TransaccionCompartidaTests` (Testcontainers): (a) un `DbContext` de prueba abre transacción, `Unir`, inserta una fila propia, registra auditoría y confirma → existen ambas; (b) igual pero el registro falla (contexto prohibido) → no existe ninguna; (c) `RegistrarAsync` sin transacción unida → `InvalidOperationException` y ninguna fila; (d) `RegistrarLecturaAsync` sin transacción → la fila queda confirmada; (e) `RolCambiado` con `RevertirAsync` → no se emite la métrica; con `ConfirmarAsync` → se emite una vez — `tests/KillaUru.Integracion.Tests/Auditoria/TransaccionCompartidaTests.cs` · (FR-002, FR-003, FR-013, FR-017, RNF-31)
- [x] T010 [US1] `InmutabilidadTests`: con el rol `killauru_app`, `UPDATE`, `DELETE` y `TRUNCATE` sobre `auditoria.registro` fallan con `permission denied` — `tests/KillaUru.Integracion.Tests/Auditoria/InmutabilidadTests.cs` · (FR-008)

### Implementación

- [ ] T011 [P] [US1] `CategoriaAuditoria`, `CatalogoAcciones`, `ClavesProhibidas` — `Aud/Domain/` · (FR-005, FR-006)
- [x] T012 [US1] `RegistroAuditoria` con `Crear(...)` → `ResultadoRegistro` — `Aud/Domain/RegistroAuditoria.cs` · (FR-004…FR-006)
- [x] T013 [US1] Puerto `IRegistroAuditoriaRepositorio` — `Aud/Application/Puertos/IRegistroAuditoriaRepositorio.cs`
- [x] T014 [US1] `RegistrarAuditoriaUseCase` (exige transacción unida, métrica con `AlConfirmar`) y `AuditoriaService` (los dos métodos) — `Aud/Application/` · (FR-001, FR-002, FR-013)
- [x] T036 [P] [US1] `RegistrarLecturaUseCaseTests`: guarda y confirma en transacción propia; `DatasetExportado` emite la métrica después de confirmar; contexto prohibido → excepción — `tests/KillaUru.Auditoria.Tests/Application/RegistrarLecturaUseCaseTests.cs` · (FR-017, FR-013)
- [x] T037 [US1] `RegistrarLecturaUseCase` — `Aud/Application/RegistrarLecturaUseCase.cs` · (FR-017, D15)
- [x] T015 [US1] `AuditoriaDbContext` y `RegistroAuditoriaConfiguracion` (`jsonb`, `smallint`, esquema `auditoria`) — `Aud/Infrastructure/` · (ADR-012)
- [x] T016 [US1] `FabricaContextoAuditoria` (`CrearUnido` sobre `IContextoTransaccion`, `CrearPropio` solo para lecturas) — `Aud/Infrastructure/FabricaContextoAuditoria.cs` · (FR-002, FR-017, R-001)
- [x] T017 [US1] `RegistroAuditoriaRepositorio.AgregarEnTransaccionAsync` y `AgregarConTransaccionPropiaAsync` — `Aud/Infrastructure/RegistroAuditoriaRepositorio.cs` · (FR-002, FR-017)
- [x] T018 [US1] `AddAuditoriaModule` registra todo (*scoped*) — `Aud/AuditoriaModule.cs`

**Checkpoint**: T006–T010 y T036 en verde; cobertura del `Domain` de Auditoría ≥ 90 % (puerta del CI de 001). Las features 003 y 004 ya pueden auditar.

---

## Phase 4: User Story 2 — Consultar en modo solo lectura (P1)

### Pruebas (primero)

- [x] T019 [P] [US2] `ConsultarAuditoriaUseCaseTests`: valida `desde ≤ hasta` y `1 ≤ tamano ≤ 200`; aplica los filtros; registra `AccesoAdministrativoDatos` con los filtros en el contexto mediante `RegistrarLecturaUseCase` — `tests/KillaUru.Auditoria.Tests/Application/ConsultarAuditoriaUseCaseTests.cs` · (FR-011, FR-012)
- [ ] T020 [US2] `AuditoriaApiTests` (`WebApplicationFactory`): 401 sin token; 403 `sin_permiso` con `ConductorRegistrado` (regla única T-SEG20); 429 al superar la cuota del administrador; 200 con `Administrador`, orden descendente, paginación correcta, filtros combinados; 400 con fechas invertidas; no existen `PUT`, `PATCH` ni `DELETE` (405) — `tests/KillaUru.Integracion.Tests/Auditoria/AuditoriaApiTests.cs` · (FR-011, US2)

### Implementación

- [x] T021 [P] [US2] `FiltrosAuditoria` y `PaginaAuditoria` — `Aud/Application/` · (FR-011)
- [x] T022 [US2] `RegistroAuditoriaRepositorio.ConsultarAsync` (`AsNoTracking`, proyección, orden `ocurrido_utc DESC, id DESC`, `COUNT`) — `Aud/Infrastructure/RegistroAuditoriaRepositorio.cs` · (FR-011)
- [x] T023 [US2] `ConsultarAuditoriaUseCase` (se audita con `RegistrarLecturaUseCase`) — `Aud/Application/ConsultarAuditoriaUseCase.cs` · (FR-011, FR-012)
- [x] T024 [P] [US2] DTOs con comentarios XML y `<example>` — `Aud/Adapters/Dtos/` · (Constitución IX)
- [x] T025 [US2] `AuditoriaController` (`GET /api/v1/auditoria`, `SoloAdministrador`, ≤ 15 líneas por acción, atributos de documentación) — `Aud/Adapters/AuditoriaController.cs` · (FR-011)
- [ ] T026 [US2] Verificar que `ContratoOpenApiTests` (001) pasa con `contracts/auditoria.openapi.yaml` · (Constitución IX)

**Checkpoint**: quickstart §1 y §2 completos.

---

## Phase 5: User Story 3 — Alertas de riesgo e incidentes (P2)

- [ ] T027 [US3] Reglas de alerta en Grafana para `killauru.seguridad.acciones_riesgo` (exportación > 0 en 5 min, rol > 0 en 5 min, bloqueos ≥ 5 en 1 h) y documentarlas — `deploy/runbooks/monitoreo.md` · (FR-014, RNF-29)
- [ ] T028 [P] [US3] Procedimiento de incidentes: detección, contención, reconstrucción con consultas de auditoría, plantilla de notificación a la Autoridad y a titulares, canal oficial, persona de reemplazo, plazo de 48 h — `deploy/runbooks/incidentes-datos-personales.md` · (FR-015, RNF-29)
- [ ] T029 [P] [US3] Sección «Eliminaciones posteriores al respaldo» (consulta de `data-model.md` §6 si la BD anterior se puede leer; si no, riesgo aceptado y revisión manual de las solicitudes de eliminación del último día, R-009) — `deploy/runbooks/restaurar.md` · (FR-016, RN-10, D9)
- [ ] T030 [P] [US3] Semillas de riesgo para el entorno de pruebas — `db/semillas/auditoria-riesgo.sql`
- [ ] T031 [US3] Simulacro EQ-13 en «killauru-pruebas»; anotar tiempos — `deploy/runbooks/simulacros.md` · (SC-003)
- [ ] T035 [P] [US3] Agregar el simulacro EQ-13 **semestral** al calendario de verificaciones de `simulacros.md` (001 T081), no como tarea única — `deploy/runbooks/simulacros.md` · (FR-018, T-QA14)

---

## Phase 6: Polish

- [ ] T032 [P] `README.md` del módulo: propósito, catálogo, cómo auditar desde otro módulo (los 5 pasos de `contratos-internos.md`), endpoint y ejemplo — `Aud/README.md` · (RNF-19)
- [ ] T033 Prueba de rendimiento: 50 000 registros sembrados, consulta de un mes < 500 ms p95 — `tests/KillaUru.Integracion.Tests/Auditoria/RendimientoConsultaTests.cs` · (SC-004)
- [ ] T038 Revisión de código: Identidad (003) y Anomalías (004) **no** llaman `RegistrarAccionRiesgo` ni registran descartes de ingesta; Mapa y Dashboard no auditan lecturas · (FR-007, FR-013, D15)
- [ ] T034 Marcar `checklists/trazabilidad.md`

## Dependencies & Execution Order

Phase 1 → Phase 2 → US1 (Phase 3) → US2 (Phase 4) → US3 (Phase 5) → Polish. Al terminar la Phase 3, 003 y 004 pueden empezar.
