---

description: "Tareas del módulo Anomalías"
---

# Tasks: Módulo Anomalías

**Input**: `specs/004-modulo-anomalias/` · **Requiere**: 001 (fases 1–6, incluido `Shared/Tareas/ProgramaReloj`) y 002 (fases 1–3)

**Tests**: obligatorias y **primero** (constitución VII: es el módulo con más reglas).

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `backend/`. Módulo: `src/Modules/KillaUru.Anomalias/` (`An/`). Pruebas: `tests/KillaUru.Anomalias.Tests/` (`AnT/`) y `tests/KillaUru.Integracion.Tests/Anomalias/` (`IntAn/`). **Ningún `.cs` > 199 líneas.**

## Phase 1: Setup

- [x] T001 Migración `V0004__anomalias.sql` (4 tablas, `anomalia.km_via`, CHECK e índices de `data-model.md` §1; sin `ix_anomalia_pendiente`) — `db/migraciones/V0004__anomalias.sql` · (FR-032; D13, T-BD-CHK)
- [x] T002 Obtener la polilínea PE-28A de OpenStreetMap (Pisco → Ayacucho), revisarla a mano, simplificarla sin perder curvas (tolerancia ≤ 5 m) y generar `V0005__corredor_via_datos.sql`; guardar el GeoJSON fuente en `db/datos/corredor_pe28a.geojson` (fuente única que reutilizará la app, D16) · (RN-13)
- [x] T003 [P] `AnomaliasOptions` (POCO, 25 valores de la spec) y su sección en `appsettings.json` — `An/Domain/AnomaliasOptions.cs` · (ADR-012)

## Phase 2: Foundational — contratos y políticas puras (bloquean a 005, 006 y 007)

### Contratos

- [x] T004 [P] `EventoReportado`, `ContextoReporte` (con `RecibidoUtc`), `ResultadoProceso`, `MotivoDescarte` (8 valores, `SeudonimoInconsistente = 8`), `TipoAnomalia`, `EstadoAnomalia`, `AnomaliaCambiada`, tipos de `Lecturas/` (con `KmVia`) — `An/Contracts/` · (FR-001, FR-029, FR-032, FR-034; R-001, R-002, R-015)
- [x] T005 [P] `IAnomaliasService` e `IAnomaliaLectura` con los comentarios de `contratos-internos.md` — `An/Contracts/`

### Pruebas del dominio (primero; deben fallar)

- [x] T006 [P] `CorredorViaTests`: un punto sobre la vía → 0 m; a 49/50/51 m; km acumulado correcto en 3 vértices conocidos; curva cerrada (distancia por la vía ≠ línea recta); `Celdas` contiene la celda de todo punto a ≤ 50 m — `AnT/Domain/CorredorViaTests.cs` · (FR-003)
- [x] T007 [P] `ReporteAnomaliaTests` (mapeo): campo faltante → `EsquemaInvalido`; tipo `Normal` o desconocido → `TipoDesconocido` — `AnT/Domain/CalidadTests.cs` · (R-002)
- [x] T008 [P] `ReglasCalidadTests`: escenarios 1–4 y 20 del quickstart (contra `recibidoUtc`; futuro 5 min − 1 s / 5 min / 5 min + 1 s) — `AnT/Domain/CalidadTests.cs` · (FR-001, FR-002; T-QA18)
- [x] T009 [P] `PoliticaPlausibilidadTests`: escenarios 9 y 19 (150 km/h pasa, 151 km/h → `VelocidadImposible`); descuento de precisiones; tope 120 / 121 — `AnT/Domain/PoliticasTests.cs` · (FR-004, FR-013; T-QA18)
- [x] T010 [P] `PoliticaConfirmacionTests`: escenarios 5–8; `EsVehiculoNuevo` (seudónimo distinto y ≥ 3 min) — `AnT/Domain/PoliticasTests.cs` · (FR-010, FR-011)
- [x] T011 [P] `ReglaPersistenciaTests`: escenario 11 (22:00 Lima) y ventana de 15 días — `AnT/Domain/PoliticasTests.cs` · (FR-016)
- [x] T012 [P] `AnomaliaReportesTests`: nace activa/pendiente con `KmVia` fijo; mediana; `Activa` no vuelve a pendiente; `vence_utc` según la tabla §3.3; escenarios 12, 13 y 21 (`Reparada` renace con reporte posterior a `estado_desde_utc` aunque no haya pasado su vencimiento anterior); `Descartada` absorbe y avanza `ultima_deteccion`; `ultima_deteccion` no retrocede — `AnT/Domain/AnomaliaTests.cs` · (FR-008…FR-012, FR-017, FR-019, FR-020, FR-032)
- [x] T013 [P] `AnomaliaModeracionTests`: cada acción desde cada uno de los 5 estados (20 casos) contra la tabla de RN-07 (`Moderar` → `true`/`false`); `MantenerDescartada` limpia marcas; «Por revisar» con 3 vehículos en 7 días (escenario 16) — `AnT/Domain/ModeracionTablaTests.cs` · (FR-020, FR-025, FR-026)
- [x] T014 [P] `AnomaliaTransicionesTests`: una sola transición por operación (nace leve → `null→Activa`; nace grave y se confirma → `null→Pendiente`, `Pendiente→Activa`; renace leve → `Expirada→Activa`) — `AnT/Domain/CicloVidaTests.cs` · (FR-015)

### Implementación del dominio

- [x] T015 [P] `GeodesiaLocal` y `CorredorVia` (con índice por rejilla) — `An/Domain/` · (FR-003; R-003)
- [x] T016 [P] `ReporteAnomalia` — `An/Domain/ReporteAnomalia.cs`
- [x] T017 [P] `ReglasCalidad` — `An/Domain/ReglasCalidad.cs` · (FR-001, FR-002)
- [x] T018 [P] `PoliticaPlausibilidad` — `An/Domain/PoliticaPlausibilidad.cs` · (FR-004, FR-013)
- [x] T019 [P] `PoliticaConfirmacion` (código de referencia de `diseño-interno` §6.2) — `An/Domain/PoliticaConfirmacion.cs` · (FR-010, FR-011)
- [x] T020 [P] `ReglaPersistencia` (código de referencia de `enfoque` §5) — `An/Domain/ReglaPersistencia.cs` · (FR-016)
- [x] T021 [P] `EstadoPersistencia`, `AccionAdmin` (4 acciones; se queda en Domain, D4), `Transicion`, `ReglasAnomalia` — `An/Domain/`
- [x] T022 `Anomalia` y sus colaboradores, una clase por archivo (`Anomalia.cs`, `ReglaRenacimiento.cs`, `TablaModeracion.cs`, `RegistroTransiciones.cs`; sin `partial`) — `An/Domain/` · (FR-008…FR-020, FR-025, FR-032; R-014)

**Checkpoint**: `KillaUru.Anomalias.Tests` en verde; cobertura del dominio ≥ 90 % (SC-006). 005 puede empezar.

---

## Phase 3: User Story 1 — Solo los eventos de calidad cuentan (P1)

- [x] T023 [US1] `EvaluarIntrinsecaUseCase` (mapea `EventoReportado` → `ReporteAnomalia` → `ReglasCalidad`) + prueba — `An/Application/Procesamiento/` · (FR-001)
- [x] T024 [US1] `CargadorCorredor` (carga al arrancar) y `CorredorHealthCheck` (comprobación `corredor` de `/health/ready` por el punto de extensión de 001) + prueba de integración (503 antes de cargar, 200 después) — `An/Infrastructure/`, `IntAn/DisponibilidadCorredorTests.cs` · (FR-003, FR-033; T-OPS19)

## Phase 4: User Story 2 — Agrupar, confirmar y publicar (P1) 🎯 MVP

### Pruebas

- [x] T025 [P] [US2] `ProcesarReporteUseCaseTests` (dobles): idempotencia; descarte → métrica y sin auditoría; publica `AnomaliaCambiada` solo tras confirmar; tope → `Crear(porTope: true)` y `RegistrarRetenidoPorTope()`; cada evento → `RegistrarDuracionProceso(d)`; nunca `RegistrarAccionRiesgo`; `KmVia` de la anomalía nueva = `KmSobreVia` del reporte — `AnT/Application/ProcesamientoUseCaseTests.cs` · (FR-005, FR-006, FR-013, FR-014, FR-031, FR-032; T-OPS14)
- [x] T026 [US2] `AnomaliaRepositoryTests` (Testcontainers): `MasCercanaAsync` (mismo tipo, ≤ 100 m, la más cercana, con `FOR UPDATE`), `ObtenerParaActualizarAsync`, `ReportesDesdeAsync`, `AnteriorDelSeudonimoAsync`, `GuardarAsync` con transiciones y `km_via` — `IntAn/RepositorioEspacialTests.cs` · (FR-008, FR-030, FR-032)
- [x] T027 [US2] `ConcurrenciaTests`: escenario 15 con 8 trabajadores; mismo evento dos veces en paralelo; punto en esquina de 4 celdas sin interbloqueo; trabajador y caducidad sobre la misma anomalía sin actualización perdida (estado e historial coherentes) — `IntAn/ConcurrenciaAdicionalTests.cs` · (FR-006, FR-007, FR-030, SC-002; T-BD02)

### Implementación

- [x] T028 [US2] Puertos `IAnomaliaRepository` e `ITransaccionAnomalias` (`An/Application/Puertos/`), `AnomaliasDbContext` y 4 configuraciones (`geography`, `timestamptz[]`, enums `smallint`) — `An/Infrastructure/` · (T-ARQ06)
- [x] T029 [US2] `TransaccionAnomalias` (bloqueos ordenados + `IContextoTransaccion.Unir`) — `An/Infrastructure/TransaccionAnomalias.cs` · (FR-007; R-004)
- [x] T030 [US2] `AnomaliaRepositoryPostgis` y `ReporteRepositoryPostgis` (puertos `IAnomaliaRepository` e `IReporteRepository`, ISP) (lecturas con `FOR UPDATE`; `km_via` del reporte calculado con `CorredorVia`) — `An/Infrastructure/` · (FR-008, FR-030, FR-032; T-ARQ06, T-ARQ08)
- [x] T031 [US2] `ProcesarReporteUseCase` (algoritmo de `enfoque` §6 pasos 0–6, con `RecibidoUtc` del contexto; revalida el estado de la anomalía bloqueada; `km_via` al nacer; métricas de duración y tope) y `AnomaliasService` — `An/Application/Procesamiento/` · (FR-006…FR-015, FR-030…FR-032)
- [x] T032 [US2] `AddAnomaliasModule` (todo menos tareas y administración) — `An/AnomaliasModule.cs`

**Checkpoint**: escenarios 5–10, 14, 15 y 19–21 del quickstart en verde. 005 puede integrarse.

---

## Phase 5: User Story 3 — Clasificar y caducar (P1)

- [x] T033 [P] [US3] Puerto `ICaducidadRepository` y `CaducidadRepository.CaducarAsync` (CTE de R-008 con `FOR UPDATE SKIP LOCKED`) — `An/Application/Puertos/`, `An/Infrastructure/CaducidadRepository.cs` · (FR-018; T-ARQ08)
- [x] T034 [US3] `CaducarAnomaliasUseCase` (publica `AnomaliaCambiada` por fila tras el commit; `RegistrarTareaCorrecta("caducidad")`) + prueba unitaria — `An/Application/Tareas/` · (FR-018, FR-024)
- [x] T035 [US3] `TareaCaducidad` (al arrancar y cada hora en `:05` con `ProgramaReloj` sobre `TimeProvider`) — `An/Infrastructure/Tareas/TareaCaducidad.cs` · (FR-018, FR-024; T-OPS05)
- [x] T036 [US3] `TareasCaducidadTests` (Testcontainers + `FakeTimeProvider`): escenario 17; `SKIP LOCKED` salta la fila que un trabajador tiene con `FOR UPDATE`; transición con `ocurrida_utc = vence_utc` — `IntAn/RetencionYBloqueosTests.cs` · (FR-018, FR-030, SC-007)

## Phase 6: User Story 4 — Privacidad y retención (P1)

- [x] T037 [P] [US4] Puerto `IPurgasRepository` y `PurgasRepository.PurgarSeudonimosAsync` (seudónimo `NULL` + `recibido_utc = date_trunc('hour', …)` en la misma sentencia) y `PurgarHistoricoAsync` (SQL de R-017: `Descartada` solo con `ultima_deteccion_utc` y `estado_desde_utc` > 2 años), lotes de 5 000 — `An/Application/Puertos/`, `An/Infrastructure/PurgasRepository.cs` · (FR-021, FR-022; D3, D7, T-ARQ08)
- [x] T038 [US4] `PurgarSeudonimosUseCase` y `PurgarHistoricoUseCase` + `TareaPurgaSeudonimos` (al arrancar y cada hora en `:10`) y `TareaPurgaHistorica` (al arrancar y a las 03:30 local), con `ProgramaReloj` — `An/Application/Tareas/`, `An/Infrastructure/Tareas/` · (FR-021, FR-022, FR-024; T-OPS05)
- [x] T039 [US4] `PurgasTests`: escenarios 18, 22 y 23 (recibido hace 72 h 1 s → purgado y `recibido_utc` en hora en punto; detectado hace días pero recibido hace 1 h → se conserva exacto; 2 años exactos; `Descartada` con un reporte de hace 1 año → se conserva) — `IntAn/RetencionYBloqueosTests.cs` · (FR-021, FR-022, SC-005)
- [x] T040 [US4] Auditoría del esquema: ninguna columna de cuenta, correo o token en `anomalias` — `IntAn/EsquemaAnomaliasTests.cs` · (FR-023)
- [x] T056 [P] [US4] `EsquemaRestriccionesTests`: los CHECK de `data-model.md` §1 rechazan `persistencia = 2`, `confidence = 1.01`, `precision_m = −1`, `desde = hacia` y `hacia = 6` — `IntAn/EsquemaAnomaliasTests.cs` · (FR-023; T-BD-CHK)
- [x] T058 [US4] `TareasProgramadasTests` (`FakeTimeProvider`): las 3 tareas corren al arrancar; luego caducidad a las `:05`, purga de seudónimos a las `:10` y purga histórica a las 03:30 Lima; reinicio a las 03:29 → corre al arrancar y otra vez a las 03:30 — `AnT/Application/TareasProgramadasTests.cs` · (FR-024; T-OPS05)

## Phase 7: User Story 7 — Lecturas para Mapa y Dashboard (P1)

- [x] T041 [P] [US7] Puertos `ILecturaAnomaliasRepository` e `IResumenDiaRepository`; `LecturaAnomaliasRepository` (activas por celdas, filtrado, cambios, con `KmVia`) y `ResumenDiaRepository` (R-011) — `An/Application/Puertos/`, `An/Infrastructure/` · (FR-029, FR-032; T-ARQ07)
- [x] T042 [US7] `AnomaliaLectura` (implementa `IAnomaliaLectura`) — `An/Application/Lecturas/AnomaliaLectura.cs` · (FR-029)
- [x] T043 [US7] `LecturasTests`: cada operación con datos sembrados; resumen con día local UTC−5 (reporte a las 23:30 Lima cuenta para ese día); `EventosRecibidos` = reportes procesados; `Renacimientos` cuenta `Expirada → Activa` y `Expirada → PendienteConfirmacion` y no `Reparada → Activa`; el mismo día recalculado tras la purga de 72 h da los mismos eventos; `CeldasDelCorredor` vacío antes de cargar — `IntAn/LecturasTests.cs` · (FR-029; D3, D14)

**Checkpoint**: 006 y 007 pueden integrarse.

---

## Phase 8: User Story 5 — Moderación (P2, recortable: HU-19 es el 5.º del orden de recorte)

- [x] T057 [P] [US5] `ComandoModeracion`, `AccionModeracion` y `ResultadoModeracion` (Application) + mapeo `AccionModeracion → AccionAdmin` — `An/Application/Administracion/` · (FR-025; D4, R-016)
- [x] T044 [P] [US5] Pruebas de `ModerarAnomaliaUseCase` (bloqueo de celda, lectura `FOR UPDATE` y revalidación, `RegistrarAsync(AnomaliaModerada)` en la misma transacción, transición con `actor_id`, `AnomaliaCambiada`, `NoPermitida` → 409; nunca `RegistrarAccionRiesgo`) y de `ListarPorRevisarUseCase` (con `severidadMostrada`), `ListarPendientesUseCase`, `ObtenerDetalleUseCase` (`venceUtc` nulo en estados finales; `kmVia` de la anomalía), `ListarTransicionesRecientesUseCase` — `IntAn/ModeracionApiTests.cs`, `IntAn/AtomicidadAuditoriaTests.cs`, `IntAn/ListasAdministracionTests.cs` · (FR-025…FR-027, FR-030, FR-031; D15, T-PO08, T-PO19)
- [x] T045 [US5] Los 5 casos de uso + puerto `ILecturaAdministracionRepository` y `LecturaAdministracionRepository` y `DetalleAnomaliaRepository` (puerto `IDetalleAnomaliaRepository`) — `An/Application/Administracion/`, `An/Application/Puertos/`, `An/Infrastructure/` · (FR-025…FR-027; T-ARQ07, T-ARQ08)
- [x] T046 [US5] `ModeracionController` y `DetalleAnomaliaController` (rutas con `{id:guid}` para no chocar con `/transiciones`; solo tipos de Application) + DTOs documentados — `An/Adapters/` · (FR-025; Constitución IX, D4)
- [x] T047 [US5] `ModeracionApiTests`: 403 a conductores; tabla completa de RN-07 por HTTP; listas y detalle con los campos del contrato; moderación y caducidad simultáneas sin actualización perdida — `IntAn/ModeracionApiTests.cs` · (FR-027, FR-030)

## Phase 9: User Story 6 — Exportación (P3, recortable)

- [x] T048 [P] [US6] `IEscritorExportacion`, `FilaExportacion`, `EscritorCsv` y `EscritorGeoJson` (*streaming*; columnas `id, tipo, severidad, estado, persistencia, km, latitud, longitud, reportes, primer_reporte, ultimo_reporte`) + pruebas: sin columna `seudonimo`; escapes CSV — `An/Application/Exportacion/`, `An/Infrastructure/Exportacion/` · (FR-028; T-PO11)
- [x] T049 [US6] Puerto `IExportacionRepository` y `ExportacionRepository` (`ContarAsync`, `LeerAsync`); `ExportarDatasetUseCase` (cuenta las filas → `RegistrarLecturaAsync(DatasetExportado)` con `filas` → escribe) y `ExportacionController` — `An/Application/`, `An/Infrastructure/ExportacionRepository.cs`, `An/Adapters/ExportacionController.cs` · (FR-028; D15, T-ARQ07)
- [x] T050 [US6] `ExportacionApiTests` (CSV y GeoJSON, filtros, columnas exactas, auditoría con `filas` igual a las filas escritas; si la auditoría falla no se escribe nada) — `IntAn/ExportacionApiTests.cs` · (FR-028)

## Phase 10: Polish

- [x] T051 [P] `README.md` del módulo (reglas → clases, endpoints, ejemplos, tareas programadas y sus horas) — `An/README.md` · (RNF-19)
- [x] T052 Prueba de contrato con `contracts/anomalias.openapi.yaml` en verde (incluye 429 en las 6 operaciones y `FilaExportacion`) · (T-SEG19)
- [ ] T053 Medir p95 del procesamiento con 300 dispositivos (junto con 005) ≤ 100 ms con el histograma `killauru.anomalias.proceso_ms` · (SC-001, RNF-23, FR-031)
- [ ] T054 Escenario EQ-12 completo (ataque + subida legítima de 5 h + lluvia de 600 eventos/h) — `IntAn/IntegridadDatoTests.cs` · (SC-003, SC-004)
- [x] T055 Marcar `checklists/trazabilidad.md`

## Dependencies & Execution Order

Phase 1 → Phase 2 → US1 → US2 → (US3, US4, US7 en paralelo) → US5 → US6 → Polish. Al terminar la Phase 2 puede empezar 005 con dobles; al terminar US2 puede integrarse de verdad; al terminar US7 pueden empezar 006 y 007. Si se recorta HU-19, el dominio de moderación (T013, T021, `.Moderacion.cs`) se queda porque `Descartada` y «Por revisar» los usa el procesamiento.

## Avance verificado — 2026-10-26

**56/58 tareas verificadas.** Implementación local terminada: dominio, procesamiento, mantenimiento, lecturas, moderación y exportación. Las pruebas previstas se agruparon en los archivos reales indicados arriba y en la matriz de [revisión de implementación](revision-implementacion.md). T025 combina dobles con `ProcesamientoTests` y las reglas de arquitectura; T026/T027 combinan repositorio, procesamiento y concurrencia. T044 verifica los casos de uso mediante HTTP y rollback real de PostgreSQL, además del dominio unitario.

- **T053 sigue abierta:** 300 dispositivos, ocho trabajadores, Release local: p95 **185,995 ms**, superior a 100 ms. La prueba registra la medición; que pase no certifica el umbral. Falta optimizar y validar junto con 005.
- **T054 sigue abierta:** los tres escenarios de `IntegridadDatoTests` pasan contra `IAnomaliasService`; falta EQ-12 por HTTP/cola/reintentos del spec 005.
- Dominio: 69 pruebas y cobertura 264/265 líneas (99,62 %). Arquitectura: 378 pruebas. Integración: regresión de 173 aprobadas y una explícita omitida; tres pruebas adicionales de lecturas/exportación aprobadas. Contrato final verificado por separado.
- El polígono demo corresponde a 006 T002. Los pendientes operativos de 001 y la revisión de módulos futuros de 002 conservan su alcance; esta entrega no los cierra.
