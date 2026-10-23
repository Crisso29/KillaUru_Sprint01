# Trazabilidad Checklist: Módulo Auditoría

**Purpose**: Todo lo que docs pide a Auditoría → FR → tarea
**Created**: 2026-10-20
**Feature**: [spec.md](../spec.md)

- [ ] CHK001 HU-15 · log completo, inmutable, solo lectura → FR-008, FR-011 → T010, T020, T025
- [ ] CHK002 RF-15 · catálogo de acciones sensibles (alta, rol, desactivación, eliminación, login fallido, bloqueo, recuperación, cambio de contraseña, revisión mensual, moderación, exportación, accesos administrativos) → FR-005 → T003, T007
- [ ] CHK003 RF-15 · descartes de ingesta **no** se auditan → FR-007 → (revisión de código en 004 y 005)
- [ ] CHK004 RF-30 · eliminación auditada sin correo → FR-006 → T006
- [ ] CHK005 RNF-29 · reconstruir incidentes y notificar en 48 h; procedimiento escrito; alertas de riesgo → FR-013…FR-015 → T027, T028, T031
- [ ] CHK006 RNF-30 · revisión mensual registrada → acción `RevisionPrivilegios` → T003 (la registra 003)
- [ ] CHK007 RNF-31 · acción y registro en la misma transacción → FR-002, FR-003 → T009, T016
- [ ] CHK008 RN-10 · retención permanente; repetir eliminaciones tras restaurar → FR-009, FR-016 → T029
- [ ] CHK009 ADR-013 · excepción documentada a «cada módulo, su transacción»; id interno, nunca correo → FR-002, FR-006 → T009, T016
- [ ] CHK010 ADR-012 · esquema `auditoria` y `DbContext` propio → T001, T015
- [ ] CHK011 `diseño-interno` §3.4 · tabla, índices y `REVOKE` → T001
- [ ] CHK012 EQ-13 · simulacro semestral, en el calendario de verificaciones (T-QA14) → FR-018 → T031, T035
- [ ] CHK013 Prototipo · pantalla «Auditoría» (categorías, solo lectura, retención permanente, ids de cuentas eliminadas) → FR-011, catálogo → T020, T025
- [ ] CHK014 D15 · `RegistrarAsync` exige transacción (`InvalidOperationException`) → FR-001, FR-002 → T008, T009, T014, T016
- [ ] CHK015 D15 · `RegistrarLecturaAsync` con transacción propia para lecturas sensibles; la consulta de la auditoría se registra así → FR-017, FR-012 → T019, T023, T036, T037
- [ ] CHK016 D15 · métrica de riesgo solo tras el commit (`AlConfirmar`), Auditoría único emisor → FR-013 → T008, T009, T038
- [ ] CHK017 D9 · eliminaciones tras restaurar: riesgo aceptado y revisión manual → FR-016 → T029
- [ ] CHK018 T-SEG19 / T-SEG20 · 429 declarado; 401 sin token y 403 `sin_permiso` con otro rol → FR-011 → T020, T026
- [ ] CHK019 `ActorAuditoria.Sistema = new(null, null)` coherente en contrato, research y data-model → FR-004 → T004
