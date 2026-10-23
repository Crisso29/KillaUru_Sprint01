# Trazabilidad Checklist: Módulo Identidad y Accesos

**Purpose**: Todo lo que docs pide a Identidad → FR → tarea
**Created**: 2026-10-20
**Feature**: [spec.md](../spec.md)

## Historias

- [ ] CHK001 HU-11 · login admin, sesión ≤ 8 h, bloqueo tras 3 intentos en 15 min auditado → FR-007, FR-009, FR-010, FR-015 → T028…T032
- [ ] CHK002 HU-14 · crear, editar rol, desactivar; acceso perdido en ≤ 5 min y auditado → FR-030…FR-033 → T047…T050, T036
- [ ] CHK003 HU-18 · registro, rol `ConductorRegistrado`, sesión renovable, verificación por correo → FR-001, FR-014, FR-015, FR-021 → T023…T027
- [ ] CHK004 HU-18b · enlace vigente 24 h; solo correo verificado recupera → FR-021, FR-022 → T037…T042
- [ ] CHK005 HU-22 · no se crea la cuenta sin aceptar; fecha y versión guardadas; finalidades (texto de la política) → FR-002, FR-003 → T023, T024
- [ ] CHK006 HU-23 · eliminar cuenta, revocar sesiones, auditar sin correo → FR-027, FR-028 → T043…T046
- [ ] CHK007 HU-24 · ver todos los datos personales → FR-026 → T043…T046
- [ ] CHK008 HU-25 · enlace de un solo uso 30 min, respuesta idéntica, cerrar sesiones, auditar → FR-022, FR-023 → T037…T042
- [ ] CHK009 HU-27 · términos aceptados y versionados; re-aceptar si cambian → FR-002…FR-004 → T023, T024, T043

## Requisitos funcionales

- [ ] CHK010 RF-16 · JWT 5 min + *refresh* máx. 8 h + bloqueo RN-19 → FR-014, FR-015, FR-009
- [ ] CHK011 RF-19 · CRUD de usuarios y revisión mensual → FR-030…FR-034
- [ ] CHK012 RF-21 · JWT 5 min + *refresh* 30 días que se renueva con el uso → FR-015
- [ ] CHK013 RF-22 · correo de verificación 24 h, solo desde Identidad → FR-021, FR-025
- [ ] CHK014 RF-29 · consentimiento explícito con fecha y versión; finalidades y proveedores en la política → FR-002, FR-003
- [ ] CHK015 RF-30 · eliminar cuenta, revocar *refresh*, auditar sin correo → FR-027
- [ ] CHK016 RF-31 · `GET /api/v1/me` → FR-026
- [ ] CHK017 RF-32 · recuperación con límites por correo e IP → FR-022
- [ ] CHK018 RF-37 · términos versionados para ambos conductores → FR-002…FR-004
- [ ] CHK019 RF-38 (parte servidor) · cerrar sesión revoca el *refresh* → FR-018

## Reglas, calidad y decisiones

- [ ] CHK020 RN-17 · conductores activos = sesiones renovadas en 7 días → FR-036 → T052
- [ ] CHK021 RN-19 · 3 en 15 min → 5, 15, 60 min; mismo mensaje para conductores → FR-008, FR-009, FR-011 → T005, T028
- [ ] CHK022 RN-21 c · el servidor informa cuenta desactivada → FR-017 → T033, T036
- [ ] CHK023 RNF-10 · sin vínculo cuenta ↔ eventos → FR-029 → T056
- [ ] CHK024 RNF-26 · acceso perdido ≤ 5 min → FR-019 → T036
- [ ] CHK025 RNF-30 · revisión mensual registrada → FR-034 → T047…T050
- [ ] CHK026 RNF-31 · auditoría en la misma transacción → T016
- [ ] CHK027 ADR-009 · módulo unificado, tokens, rotación con 60 s de gracia, cookie del panel, bloqueo, anti-bot → FR-012, FR-014…FR-020
- [ ] CHK028 `diseño-interno` §6.7 · PasswordHasher, JWT `sub`+`rol`, refresh de 32 B con SHA-256, familias → T017…T019
- [ ] CHK029 Regla 11 · correo solo desde Identidad con adaptador → FR-025 → T020, T041

## Derivados del prototipo (confirmar)

- [ ] CHK030 Invitar usuario (24 h), activar cuenta, reenviar verificación, estado «Invitación pendiente» → FR-031, FR-032
- [ ] CHK031 Bloqueo visible en el panel («Cuenta bloqueada 5 min…») → FR-011
- [ ] CHK032 «Sesión hasta 17:12» → `sesionVenceUtc` en la respuesta del panel

## Decisiones del autor y correcciones (2026-10-21)

- [ ] CHK033 D3 · horas de sesión truncadas, `usuario.creado_utc` al día, purga a 30 días, índices del `ON DELETE CASCADE` (BD-05) → FR-039, FR-040 → T001, T006, T058…T061
- [ ] CHK034 D4 · resultados en Application, un archivo cada uno; controladores sin `Domain/`; `UsuariosController` partido → FR-030…FR-035 → T009, T049
- [ ] CHK035 D8 · *claim* `verificado`; 5 registros/h por IP; 409 riesgo aceptado → FR-014, FR-037 → T019, T023, T026, T027
- [ ] CHK036 D9 · restauración: revocar todas las sesiones y rotar `KU_JWT_CLAVE_FIRMA` → FR-041 → T062
- [ ] CHK037 D13 · contraseña mínima 10 (`minLength: 10`); eliminar cuenta pide contraseña → FR-005, FR-027 → T002, T023, T043
- [ ] CHK038 D15 · `RegistrarAsync` en la transacción; `RegistrarLecturaAsync` en lista, `?ids=` y estado de la revisión; sin métrica de riesgo → FR-030, FR-035, FR-038 → T016, T047, T050, T063
- [ ] CHK039 T-OPS05 · `purga-identidad` por reloj (al arrancar y 03:45) con `ProgramaReloj` → FR-040 → T060
- [ ] CHK040 T-QA14 · revisión mensual de privilegios en el calendario de `simulacros.md` → FR-034 → T064
- [ ] CHK041 T-QA18 · registro 5/6 → FR-037 → T027
- [ ] CHK042 T-SEG09 · `ClockSkew = TimeSpan.Zero` → FR-014 → T019
- [ ] CHK043 T-SEG14 · enlace del primer administrador solo en la consola → FR-043 → T051
- [ ] CHK044 T-SEG19 / T-SEG20 · 429 en todas las operaciones; 401 sin token, 403 por rol → FR-042 → T065, T054
- [ ] CHK045 SEG-13 · alerta al crear o reactivar un administrador → **no aplicado** (`specs/revision/no-aplicado-003.md`)
