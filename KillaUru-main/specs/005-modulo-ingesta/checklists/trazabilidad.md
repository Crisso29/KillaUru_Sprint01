# Trazabilidad Checklist: Módulo Ingesta

**Purpose**: Todo lo que docs pide a Ingesta → FR → tarea
**Created**: 2026-10-21
**Feature**: [spec.md](../spec.md)

## Historias y requisitos funcionales

- [ ] CHK001 HU-04 · reintento con los mismos `eventoId` sin duplicados; `SYNCED` solo tras un 200 que los incluya → FR-005, FR-006, FR-016 → T010…T018, T025…T027
- [ ] CHK002 HU-05 (recepción) · resultado por evento; un evento malo no rechaza el lote → FR-002…FR-006, FR-023 → T005, T007, T010, T012
- [ ] CHK003 HU-21 (recepción) · seudónimo purgado a 72 h también en la copia de recepción → FR-008, FR-017 → T028…T030
- [ ] CHK004 RF-11 · `POST /api/v1/anomalies/batch`, 200 tras persistir, 400/401/503, `con_sesion` → FR-001…FR-010, FR-023 → T012, T016
- [ ] CHK005 RF-27 (parte Ingesta) · purga del seudónimo en `evento_recibido` → FR-017 → T029
- [ ] CHK006 RF-28 · cola en `/health/ready` → FR-021 → T031, T032
- [ ] CHK007 Contrato del lote (`03-requisitos-funcionales.md`) · 200/400/401/429/503 y `rechazados{id, indice, motivo}` → FR-006, FR-023 → T012, T034
- [ ] CHK008 `01-actores.md` §4 · el administrador no sincroniza (403) → FR-001 → T016, T012

## Reglas de negocio

- [ ] CHK009 RN-09 · 72 h desde la recepción, en ambas tablas (esta y la de 004) → FR-017 → T028, T030
- [ ] CHK010 RN-10 · recepción 30 días → FR-018 → T030
- [ ] CHK011 RN-11 · recepción sin BD antes del 200; rechazados sin guardar; `SeudonimoInconsistente`; conciliación → FR-004, FR-024 → T010, T012, T027, T038
- [ ] CHK012 RN-14 · `con_sesion` por el token (rol y `verificado`), nunca por la app → FR-007 → T012 (escenarios 10 y 20), T016
- [ ] CHK013 RN-15a · conteo por hora de **detección** entregado en el contexto → FR-013 → T006, T019, T020

## Calidad y decisiones

- [ ] CHK014 RNF-02 · lote < 300 ms p95 → SC-001 → T035
- [ ] CHK015 RNF-03 · 300 sincronizando + 300 mapa, error < 1 % (script de 001) → SC-002 → T035
- [ ] CHK016 RNF-06 / EQ-09 · ningún aceptado perdido; reanudación ≤ 2 min → FR-016 → T027
- [ ] CHK017 RNF-11 · cuota de 120 lotes/h del anónimo (política `lotes` de 001); techo por IP en el host → FR-001 → T016
- [ ] CHK018 RNF-22 · métricas de lotes, rechazos, cola y tareas → FR-010, FR-020, FR-022 → T010, T029, T031
- [ ] CHK019 RNF-23 · retraso de cola ≤ 30 s p95 (histograma `killauru.ingesta.retraso_cola_s`) → FR-026 → T019, T022, T035
- [ ] CHK020 RNF-25 · 72 h y 30 días; filas `Recibido` > 30 días como riesgo con alerta → FR-017, FR-018 → T030, T040
- [ ] CHK021 ADR-005 · reglas por evento, `INSERT … ON CONFLICT`, cola como aviso, Anomalías dueño de su transacción, idempotencia → T011, T017, T022, T027
- [ ] CHK022 ADR-014 · `payload jsonb` sin seudónimo → FR-008 → T005, T030
- [ ] CHK023 EQ-02 · ni pérdida ni duplicado entre app y servidor → T012, T027

## Decisiones del autor (confirmadas el 2026-10-21)

- [ ] CHK024 D2 · un lote = un seudónimo, `SeudonimoInconsistente` (8), regla en Application → FR-024 → T037, T038, T015, T012
- [ ] CHK025 D3 · `loteId` del servidor; purga de 72 h con `lote_id` y `date_trunc('hour', …)`; `ix_recibido_purga` → FR-017, FR-025 → T001, T010, T028, T030
- [ ] CHK026 D4 · Domain solo `EstadoRecepcion` y `HoraDeDeteccion`; lote y resultado en Application; controlador sin Domain → T008, T037, T009, T016
- [ ] CHK027 D5 · evento sin `eventoId` → `{id: null, indice, EsquemaInvalido}`; 400 solo ilegible/sin eventos/vacío/> 50 → FR-003, FR-006, FR-023 → T007, T010, T012
- [ ] CHK028 D8 · `con_sesion` = `ConductorRegistrado` y `verificado = true` → FR-007 → T016, T012
- [ ] CHK029 D11 · `RegistrarRetrasoCola` al marcar; scripts de carga de 001 → FR-026 → T019, T022, T035
- [ ] CHK030 T-OPS05 · `purga-seudonimos-recepcion` :10 y `purga-recepcion` 03:00, al arrancar, `ProgramaReloj` → FR-017, FR-018, FR-020 → T029, T039
- [ ] CHK031 T-ARQ08 · `PendientesRepositorio` partido (`BarridoPendientesPostgres`) → T021
- [ ] CHK032 T-BD-CHK · `CHECK (motivo BETWEEN 1 AND 8)` → T001
- [ ] CHK033 Índice `ix_recibido_lote` (D1) · 403 al administrador (`01-actores.md` §4) → T001, T016
