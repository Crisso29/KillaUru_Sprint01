# Trazabilidad Checklist: Módulo Ingesta

**Purpose**: Todo lo que docs pide a Ingesta → FR → tarea
**Created**: 2026-10-21
**Feature**: [spec.md](../spec.md)

## Historias y requisitos funcionales

- [ ] CHK001 HU-04 · reintento con los mismos `eventoId` sin duplicados; `SYNCED` solo tras un 200 que los incluya → FR-005, FR-006, FR-016 → T010…T018, T025…T027
- [ ] CHK002 HU-05 (recepción) · resultado por evento; un evento malo no rechaza el lote → FR-002…FR-006 → T005, T007, T010, T012
- [ ] CHK003 HU-21 (recepción) · seudónimo purgado a 72 h también en la copia de recepción → FR-008, FR-017 → T028…T030
- [ ] CHK004 RF-11 · `POST /api/v1/anomalies/batch`, 200 tras persistir, 400/401/503, `con_sesion` → FR-001…FR-010 → T012, T016
- [ ] CHK005 RF-27 (parte Ingesta) · purga del seudónimo en `evento_recibido` → FR-017 → T029
- [ ] CHK006 RF-28 · cola en `/health/ready` → FR-021 → T031, T032
- [ ] CHK007 Contrato del lote (`03-requisitos-funcionales.md`) · 200/400/401/429/503 → T012, T034

## Reglas de negocio

- [ ] CHK008 RN-09 · 72 h desde la recepción, en ambas tablas (esta y la de 004) → T030
- [ ] CHK009 RN-10 · recepción 30 días → T030
- [ ] CHK010 RN-11 · recepción sin BD antes del 200; rechazados sin guardar; conciliación → T010, T012, T027
- [ ] CHK011 RN-14 · `con_sesion` por el token, nunca por la app → T012 (escenario 10)
- [ ] CHK012 RN-15a · conteo por hora de **detección** entregado en el contexto → T006, T019, T020

## Calidad y decisiones

- [ ] CHK013 RNF-02 · lote < 300 ms p95 → T035
- [ ] CHK014 RNF-03 · 300 dispositivos, error < 1 % → T035
- [ ] CHK015 RNF-06 / EQ-09 · ningún aceptado perdido; reanudación ≤ 2 min → T027
- [ ] CHK016 RNF-11 · cuota de 120 lotes/h del anónimo (política `lotes` de 001) → T016
- [ ] CHK017 RNF-22 · métricas de lotes, rechazos, cola y tareas → T010, T029, T031
- [ ] CHK018 RNF-23 · retraso de cola ≤ 30 s p95 → T035
- [ ] CHK019 RNF-25 · 72 h y 30 días → T030
- [ ] CHK020 ADR-005 · reglas por evento, `INSERT … ON CONFLICT`, cola como aviso, Anomalías dueño de su transacción, idempotencia → T011, T017, T022, T027
- [ ] CHK021 ADR-014 · `payload jsonb` sin seudónimo → T005, T030
- [ ] CHK022 EQ-02 · ni pérdida ni duplicado entre app y servidor → T012, T027

## Huecos de docs (confirmar)

- [ ] CHK023 Evento sin `eventoId` → 400; 413 por tamaño (R-002)
- [ ] CHK024 403 a lotes del administrador (R-004)
- [ ] CHK025 Índice `ix_recibido_lote` (Complexity Tracking)
