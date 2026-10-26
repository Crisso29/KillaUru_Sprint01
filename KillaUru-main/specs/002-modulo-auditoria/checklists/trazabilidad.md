# Trazabilidad Checklist: Módulo Auditoría

**Purpose**: Todo lo que docs pide a Auditoría → FR → tarea
**Created**: 2026-10-20
**Feature**: [spec.md](../spec.md)

**Revisión 2026-10-23:** los checks marcados tienen implementación/pruebas locales o documentación verificable.
Los no marcados dependen de otros módulos o de operación externa, según la tabla final.

- [x] CHK001 HU-15 · log completo, inmutable, solo lectura → FR-008, FR-011 → T010, T020, T025
- [x] CHK002 RF-15 · catálogo de acciones sensibles (alta, rol, desactivación, eliminación, login fallido, bloqueo, recuperación, cambio de contraseña, revisión mensual, moderación, exportación, accesos administrativos) → FR-005 → T003, T007
- [ ] CHK003 RF-15 · descartes de ingesta **no** se auditan → FR-007 → (revisión de código en 004 y 005)
- [x] CHK004 RF-30 · eliminación auditada sin correo → FR-006 → T006
- [ ] CHK005 RNF-29 · reconstruir incidentes y notificar en 48 h; procedimiento escrito; alertas de riesgo → FR-013…FR-015 → T027, T028, T031
- [ ] CHK006 RNF-30 · revisión mensual registrada → acción `RevisionPrivilegios` → T003 (la registra 003)
- [x] CHK007 RNF-31 · acción y registro en la misma transacción → FR-002, FR-003 → T009, T016
- [x] CHK008 RN-10 · retención permanente; repetir eliminaciones tras restaurar → FR-009, FR-016 → T029
- [x] CHK009 ADR-013 · excepción documentada a «cada módulo, su transacción»; id interno, nunca correo → FR-002, FR-006 → T009, T016
- [x] CHK010 ADR-012 · esquema `auditoria` y `DbContext` propio → T001, T015
- [x] CHK011 `diseño-interno` §3.4 · tabla, índices y `REVOKE` → T001
- [ ] CHK012 EQ-13 · simulacro semestral, en el calendario de verificaciones (T-QA14) → FR-018 → T031, T035
- [x] CHK013 Prototipo · pantalla «Auditoría» (categorías, solo lectura, retención permanente, ids de cuentas eliminadas) → FR-011, catálogo → T020, T025
- [x] CHK014 D15 · `RegistrarAsync` exige transacción (`InvalidOperationException`) → FR-001, FR-002 → T008, T009, T014, T016
- [x] CHK015 D15 · `RegistrarLecturaAsync` con transacción propia para lecturas sensibles; la consulta de la auditoría se registra así → FR-017, FR-012 → T019, T023, T036, T037
- [ ] CHK016 D15 · métrica de riesgo solo tras el commit (`AlConfirmar`), Auditoría único emisor → FR-013 → T008, T009, T038
- [x] CHK017 D9 · eliminaciones tras restaurar: riesgo aceptado y revisión manual → FR-016 → T029
- [x] CHK018 T-SEG19 / T-SEG20 · 429 declarado; 401 sin token y 403 `sin_permiso` con otro rol → FR-011 → T020, T026
- [x] CHK019 `ActorAuditoria.Sistema = new(null, null)` coherente en contrato, research y data-model → FR-004 → T004

## Evidencia y límites

| Alcance | Evidencia |
|---|---|
| Catálogo y privacidad | RegistroAuditoriaTests, CatalogoAccionesTests; validación recursiva y copia inmutable |
| Transacciones y riesgo | RegistrarAuditoriaUseCaseTests, RegistrarLecturaUseCaseTests, TransaccionCompartidaTests |
| Inmutabilidad y esquema | V0002, InmutabilidadTests y pruebas de migraciones con roles reales |
| Consulta administrativa | AuditoriaApiTests y ConsultaAuditoriaTests: filtros, páginas, acceso, autoauditoría y cuotas |
| Contrato | ContratoOpenApiTests y ContratoAuditoriaTests: YAML frente al documento generado |
| Rendimiento | RendimientoConsultaTests: 50 000 registros, 30 mediciones HTTP; reporte en TestResults |
| Calidad | Cobertura por dominio; pruebas de dependencias, un tipo por archivo, métodos ≤ 20 y archivos < 200 líneas |
| Operación preparada | Incidentes, restauración, semillas y reglas de Grafana; CRISOLOGO / CRISSO |

| Check pendiente | Motivo concreto |
|---|---|
| CHK003 | Consumidor 004 verificado: no audita descartes; falta revisar 005 cuando exista su implementación |
| CHK005 | Reglas preparadas y procedimiento escrito; falta entrega real de alertas y simulacro (T027/T031) |
| CHK006 | Acción RevisionPrivilegios disponible; 003 debe invocarla en la revisión mensual |
| CHK012 | Calendario semestral actualizado (T035); falta ejecutar EQ-13 (T031) |
| CHK016 | Métrica después de commit y consumidores 003/004 verificados; falta revisar Mapa/Dashboard (T038) |

No se marca un requisito operativo completo solo por existir su archivo de configuración.
