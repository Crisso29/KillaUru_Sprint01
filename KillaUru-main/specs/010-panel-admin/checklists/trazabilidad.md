# Trazabilidad Checklist: Panel de administración «KillaUru Ops»

**Purpose**: Todo lo que docs y las decisiones del autor piden al panel → FR → tarea
**Created**: 2026-10-22
**Feature**: [spec.md](../spec.md)

## Historias y requisitos funcionales

- [ ] CHK001 HU-11 · login del panel, sesión ≤ 8 h sin volver a escribir la contraseña, bloqueo RN-19 → FR-005…FR-010 → T013…T034
- [ ] CHK002 HU-12 · mapa completo con no confirmadas y filtros tipo, severidad, fecha, estado → FR-021…FR-026 → T035…T050
- [ ] CHK003 HU-13 · 6 indicadores de negocio y acceso a los paneles técnicos → FR-015…FR-020 → T069…T077
- [ ] CHK004 HU-14 · crear, editar rol, desactivar; acceso perdido en ≤ 5 min y auditado → FR-031…FR-033 → T051…T058
- [ ] CHK005 HU-15 · auditoría completa de solo lectura (UTC, actor, acción, contexto) → FR-037…FR-039 → T078…T081
- [ ] CHK006 HU-19 · reparada, descartada, reabrir, mantener descartada, «Por revisar» → FR-027…FR-030 → T063…T068
- [ ] CHK007 HU-20 · CSV y GeoJSON con filtros, sin seudónimos, auditada → FR-035, FR-036 → T082…T085
- [ ] CHK008 HU-25 · «Olvidé mi contraseña» con respuesta idéntica; restablecer por enlace → FR-013, FR-014 → T030, T031, T059…T062
- [ ] CHK009 HU-18b · página del enlace de verificación → FR-014 → T059…T062
- [ ] CHK010 RF-15 · consulta de la auditoría (y las lecturas que la API audita, D15) → FR-037…FR-039, FR-046 → T078…T081, T055
- [ ] CHK011 RF-16 · acceso de 5 min, sesión de 8 h, bloqueo → FR-005, FR-007, FR-010 → T015, T023, T028…T033
- [ ] CHK012 RF-17 · mapa del administrador; filtro de estado abre con `Activa` + `PendienteConfirmacion` → FR-022 → T035, T036, T040, T050
- [ ] CHK013 RF-18 · indicadores en el panel; técnicos en Grafana → FR-015…FR-017 → T069, T073…T076
- [ ] CHK014 RF-19 · usuarios y revisión mensual → FR-031…FR-034 → T051…T058
- [ ] CHK015 RF-20 · exportación sin seudónimos → FR-035 → T082…T085
- [ ] CHK016 RF-24 · acciones de RN-07 y «Por revisar» con `MantenerDescartada` → FR-027, FR-028 → T063…T067
- [ ] CHK017 RF-32 · recuperación con respuesta idéntica → FR-013 → T030, T031
- [ ] CHK018 RF-38 (parte panel) · cerrar sesión → FR-009 → T019, T023, T032

## Reglas de negocio

- [ ] CHK019 RN-07 · tabla de acciones del administrador (solo ayuda, decide la API) → FR-027, FR-002 → T063, T064
- [ ] CHK020 RN-08 · filtro inicial del administrador y tramo demo km 268–333 → FR-022, FR-025 → T035, T037
- [ ] CHK021 RN-19 · mensaje de bloqueo del panel (423) → FR-005 → T028, T029

## Calidad, decisiones y escenarios

- [ ] CHK022 RNF-08 · HSTS en `panel` → FR-011 → T020, T027, T091 (TLS y SSL Labs de la zona: H-06)
- [ ] CHK023 RNF-09 · HSTS, CSP, X-Frame-Options, X-Content-Type-Options en el panel → FR-011 → T020, T027, T091, T093
- [ ] CHK024 RNF-10 · nada en el navegador que vincule cuentas; sin analítica → FR-006, FR-050 → T003, T034
- [ ] CHK025 RNF-13 · Chrome, Firefox, Edge → FR-004 → T012, T033
- [ ] CHK026 RNF-14 · tiempo relativo legible → FR-045 → T014, T047
- [ ] CHK027 RNF-26 · aviso «pierde el acceso en máximo 5 minutos»; renovación que respeta la revocación → FR-007, FR-033 → T016, T053
- [ ] CHK028 RNF-29 · reconstruir quién hizo qué desde la auditoría → FR-037 → T081
- [ ] CHK029 RNF-30 · revisión mensual de privilegios visible con su vencimiento → FR-034, FR-018 → T054, T057, T071
- [ ] CHK030 ADR-009 · cookie `HttpOnly; Secure; SameSite=Strict`, renovación silenciosa, anti-bot → FR-005…FR-007 → T016, T023, T031
- [ ] CHK031 ADR-010 · Cloudflare Pages y enlace a Grafana → FR-001, FR-017, FR-049 → T076, T091
- [ ] CHK032 ADR-011 · MapLibre GL JS, mismo PMTiles y estilo que la app, por rangos desde R2, atribución OSM → FR-021 → T045, T089
- [ ] CHK033 EQ-04 · acceso al panel sin token, expirado o por fuerza bruta → FR-005, FR-007, FR-008 → T016, T032, T033
- [ ] CHK034 `componentes-arquitectonicos.md` §5.1 · 7 pantallas sin lógica de negocio → FR-002 y fases 3–10 → T025, T053, T064

## Decisiones del autor y hallazgos

- [ ] CHK035 D6 · el panel abre con «Activa + PendienteConfirmacion» → FR-022 → T035, T050 (SC-001)
- [ ] CHK036 D13 · «Eventos válidos (ayer)»; `kmVia` en lista, franja y detalle → FR-015, FR-019, FR-024 → T069, T070, T040
- [ ] CHK037 D14 · «eventos válidos» en indicador y gráfico → FR-015, FR-016 → T069, T073
- [ ] CHK038 D15 · lecturas auditadas agrupadas y sin refresco automático → FR-031, FR-039, FR-046 → T022, T055, T079
- [ ] CHK039 D16 · rutas de R2 que fija 008 → FR-021 → T007, T045 (H-04)
- [ ] CHK040 PO-09 · km en el mapa del panel → FR-024 → T040
- [ ] CHK041 PO-12 · filas exportadas en «Recientes» → FR-036 → T083
- [ ] CHK042 PO-19 · «Qué pasó» derivado; «por revisar» no es transición → FR-020 → T072
- [ ] CHK043 PO-20 · filtro de auditoría por correo → FR-038 → T078, T079
- [ ] CHK044 PO-21 · disponibilidad solo como enlace a UptimeRobot → FR-018 → T073
- [ ] CHK045 SEG-07 · CSP y X-Frame-Options del panel en `_headers` → FR-011 → T020, T027
- [ ] CHK046 SEG-21 · CSRF: JSON + `SameSite`; `Origin` en el servidor → FR-012 → T016 (H-01)

## Constitución (adaptada, propuesta en `research.md`)

- [ ] CHK047 VII · pruebas antes del código; cobertura de `modelo/` ≥ 90 %; CI en cada PR → FR-049 → T009, T090
- [ ] CHK048 VIII · archivos < 200 líneas y reloj inyectado → FR-047 → T003, T005
- [ ] CHK049 IX · tipos generados desde los YAML y datos simulados validados → FR-048 → T006, T011, T090
- [ ] CHK050 X · costo cero (Pages, Actions, Turnstile, librerías libres) → FR-049, FR-050 → T091
