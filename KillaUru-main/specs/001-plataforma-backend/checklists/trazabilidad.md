# Trazabilidad Checklist: Plataforma base del backend

**Purpose**: Comprobar que todo lo que `referencia/docs` pide a esta feature está en un FR y en una tarea
**Created**: 2026-10-20
**Feature**: [spec.md](../spec.md)

Marcar `[x]` cuando la tarea esté hecha **y** su prueba pase. Actualizado el 2026-10-23 con la implementación (168 pruebas en verde: 62 del kernel, 56 de arquitectura y 50 de integración con PostgreSQL + PostGIS reales).

## Historias y requisitos funcionales

- [ ] CHK001 HU-16 · despliegue con un comando y vuelta atrás → FR-001, FR-005 → T029, T031, T054 · **pendiente: T054**
- [ ] CHK002 HU-17 · monitoreo en tiempo real → FR-018, FR-020…FR-022 → T045…T050 · **pendiente: T050**
- [x] CHK003 RF-28 · `/health/live` y `/health/ready` sin datos internos → FR-018, FR-019 → T045, T046

## Atributos de calidad

- [ ] CHK004 RNF-05 disponibilidad 99 % → SC-007 → T050 · **pendiente: T050**
- [ ] CHK005 RNF-07 respaldo diario cifrado, 30 días, simulacro mensual → FR-027 → T051, T054 · **pendiente: T054**
- [x] CHK006 RNF-08 / RNF-09 TLS, HSTS y cabeceras → FR-014 → T034, T039
- [x] CHK007 RNF-11 límite de tasa por rol con IP real → FR-010, FR-011 → T033, T037, T038
- [ ] CHK008 RNF-16 costo ≤ US$ 10 con alerta a US$ 8 → SC-008 → T057 · **pendiente: T057**
- [x] CHK009 RNF-17 un comando → FR-001 → T029
- [ ] CHK010 RNF-18 migraciones explícitas, `pg_dump` previo, dos fases, rollback < 5 min → FR-002, FR-003, FR-005 → T023, T031, T054 · **pendiente: T054**
- [x] CHK011 RNF-19 README por módulo → FR-017 → T006, T055
- [ ] CHK012 RNF-20 alerta de caída ≤ 10 min y de degradación → US4 → T050 · **pendiente: T050**
- [x] CHK013 RNF-21 OpenTelemetry + Serilog + `/health` → FR-020, FR-021 → T047, T048
- [ ] CHK014 RNF-22 paneles y alertas (disco, tareas, plan gratis) → FR-022 → T049, T050 · **pendiente: T050**
- [ ] CHK015 RNF-24 RPO/RTO y recreación de la VM → FR-028 → T053, T054 · **pendiente: T054**
- [x] CHK016 RNF-28 pruebas de arquitectura → FR-029 → T007…T009
- [x] CHK017 RNF-32 telemetría dentro del plan gratuito → FR-020 → T047…T049
- [x] CHK018 RNF-34 secretos fuera del código y de la VM → FR-007 → T030, T053

## Decisiones (ADR)

- [x] CHK019 ADR-001 monolito de 6 módulos → T003
- [x] CHK020 ADR-002 capas y `Contracts/` → T003, T004, T008, T009
- [x] CHK021 ADR-006 `private, no-store` por defecto → FR-013 → T039
- [x] CHK022 ADR-009 middleware de sesión y cookie del panel → FR-008, FR-009 → T035
- [x] CHK023 ADR-010 túnel, sin puertos, `CF-Connecting-IP`, OTLP → FR-011, FR-021 → T029, T037, T048
- [x] CHK024 ADR-012 esquema por módulo, `TimeProvider` → FR-004, FR-026 → T023, T026
- [x] CHK025 ADR-013 mecanismo de transacción compartida → FR-025 → T020

## Restricciones y riesgos

- [x] CHK026 R-02 .NET 10 LTS → T001
- [x] CHK027 R-03 PostgreSQL + PostGIS → T023, T029
- [x] CHK028 R-11 entorno de pruebas en la misma VM → FR-006 → T052
- [x] CHK029 RS-02 imagen PostGIS arm64 verificada → T028

## Pedidos del autor (no vienen de docs)

- [x] CHK030 Archivos de código < 200 líneas → FR-029 → T007
- [x] CHK031 API documentada y alineada con contratos → FR-015, FR-016 → T041…T044

## Decisiones del autor y correcciones técnicas (revisión 2026-10-21)

- [x] CHK032 D2 techo por IP de anónimos (600 lotes/h, 300 consultas/h) → FR-030 → T024, T065, T066
- [x] CHK033 D8 tope de 5 registros/h por IP (política `registro`) → FR-031 → T024, T038, T065
- [x] CHK034 D4 tabla de capas en las pruebas de arquitectura → FR-035 → T008, T059
- [ ] CHK035 D9 restauración: revocar sesiones, rotar clave JWT, purgas al arrancar, riesgo aceptado → FR-033 → T053, T054 · **pendiente: T054**
- [x] CHK036 D10 CI (cobertura ≥ 90 % con Microsoft.Testing.Platform; `main` protegida) y CD (GHCR arm64, Cloudflare Access, `desplegar.sh`) → FR-037, FR-038, FR-005, FR-007 → T005, T064, T069…T072 · `.github/workflows/ci.yml` + `.github/workflows/cd.yml`
- [x] CHK037 D11 pruebas de sistema (carga combinada, picos, estrés, resistencia, caos) y entorno de pruebas con límites; VM A1 4 OCPU / 24 GB → FR-039, FR-006 → T052, T073…T079 · `tests/carga/*.k6.js` + `comun.js` (tasa_429) + `caos.k6.js` (T079)
- [x] CHK038 D11 métrica de retraso de cola (`RegistrarRetrasoCola`) → FR-022 → T019, T049
- [x] CHK039 D12 Grafana Alloy, rotación de logs y métrica del backup `.prom` → FR-040, FR-041, FR-027 → T029, T051, T068
- [x] CHK040 D15 `AlConfirmar` y liberación del contexto; Auditoría único emisor de acciones de riesgo → FR-025, FR-022 → T020, T060
- [x] CHK041 T-BD01 `V0000__arranque.sql` como superusuario, `ON_ERROR_STOP`, roles en el backup, Testcontainers con los mismos roles, orden del primer despliegue → FR-032, FR-027 → T021…T023, T031, T051
- [ ] CHK042 T-OPS05 tareas por reloj (`ProgramaReloj`) y calendario → FR-042 → T061, T062, T050 · **pendiente: T050**
- [x] CHK043 T-OPS08/09 `restart`, *healthcheck* y rotación de logs → FR-041 → T029
- [ ] CHK044 T-OPS17 alertas de p95 por ruta → FR-046 → T050 · **pendiente: T050**
- [x] CHK045 T-OPS19 `/health/ready` con BD, cola, corredor y migraciones → FR-018 → T045, T046
- [x] CHK046 T-QA14 calendario de verificaciones periódicas → FR-043 → T081
- [x] CHK047 T-QA16 `BannedApiAnalyzers` → FR-036 → T001, T058
- [x] CHK048 T-QA17 límite de líneas para `.cs`, `.sql`, `.sh`, `.js` → FR-029 → T007
- [x] CHK049 T-QA18 valores límite de cuotas y registro → FR-010, FR-030, FR-031 → T033, T065
- [ ] CHK050 T-QA20 prueba de punta a punta → FR-044 → T080 · **pendiente: T080**
- [x] CHK051 T-SEG03 logs y trazas sin datos personales → FR-034 → T047, T067
- [x] CHK052 T-SEG09 `ClockSkew = 0` (RNF-26) → FR-008 → T032, T035
- [x] CHK053 T-SEG19/T-SEG20 429 en todas las operaciones y regla 401/403 → FR-016, FR-045 → T032, T036, T042, T044
- [x] CHK054 T-BD-CHK sin FK entre esquemas → FR-004 → T063
