# Trazabilidad Checklist: App Android KillaUru

**Purpose**: Todo lo que docs pide a la app → FR → tarea
**Created**: 2026-10-22
**Feature**: [spec.md](../spec.md)

## Historias

- [ ] CHK001 HU-01 · detección automática con 0 toques, RN-03, Liviano, «Detectando» → FR-009, FR-010, FR-016 → T036, T037, T051…T053
- [ ] CHK002 HU-02 · tipo, severidad y confidence en < 50 ms → FR-011, FR-012, FR-015 → T039, T044, T050, T122
- [ ] CHK003 HU-26 · categoría en el primer uso, soporte y cargador, solo en el celular → FR-004 → T027, T029…T031
- [ ] CHK004 HU-03 · 5 h sin señal, 100 % `PENDING`, mapa y alertas → FR-019, FR-025, FR-059 → T065, T123
- [ ] CHK005 HU-04 · mismos `eventoId`, `SYNCED` solo tras 200, reanudar ≤ 60 s → FR-028…FR-038 → T054…T072
- [ ] CHK006 HU-07 · demo, zona sombreada, aviso una vez por viaje → FR-045, FR-049 → T081, T083, T085, T096
- [ ] CHK007 HU-08 · 335.073 km, severidad, tiempo relativo, sin señal → FR-041…FR-044, FR-047, FR-050 → T082, T086, T089, T094, T096
- [ ] CHK008 HU-09 · 8 s / 12 s ± 1 s por la vía, una alerta, sin internet, pantalla bloqueada → FR-054…FR-059 → T097…T104
- [ ] CHK009 HU-10 · 6 meses solo en el celular (*Could*) → FR-069 → T115…T117
- [ ] CHK010 HU-18 / HU-18b · registro, sesión, verificación, conservar corredor offline, cerrar sesión → FR-060…FR-064, FR-066 → T105…T114
- [ ] CHK011 HU-21 · seudónimo rotativo aleatorio → FR-039, FR-040 → T033…T035
- [ ] CHK012 HU-22 · aceptación de la política (también anónimo), «Solo ver el mapa» → FR-001, FR-002, FR-005, FR-060 → T025…T032, T105
- [ ] CHK013 HU-23 · eliminar cuenta con contraseña y borrar historial local → FR-067 → T107, T110, T112
- [ ] CHK014 HU-24 · ver mis datos → FR-068 → T109, T110, T114
- [ ] CHK015 HU-25 · recuperar con el mismo mensaje → FR-065 → T109, T110, T114
- [ ] CHK016 HU-27 · términos antes de usar; versión nueva → FR-001, FR-003 → T025, T026, T031

## Requisitos funcionales

- [ ] CHK017 RF-01 · 50 Hz en servicio en primer plano con RN-03 → FR-009, FR-010 → T051, T052
- [ ] CHK018 RF-02 · ventana de 500 ms con el modelo embebido → FR-011 → T044, T050
- [ ] CHK019 RF-03 · ≥ 0.75, `eventoId`, hora del GPS, `PENDING` → FR-012, FR-013, FR-026 → T009, T041, T048
- [ ] CHK020 RF-04 · sincronizar al volver la red y reiniciar el *backoff* → FR-028 → T062, T071
- [ ] CHK021 RF-05 · todo sin internet → FR-059, FR-041 → T099, T123
- [ ] CHK022 RF-06 · lotes ≤ 50, más antiguos, estados locales, 400 → partir, un seudónimo, 168 h → FR-020…FR-023, FR-029…FR-032 → T054, T056, T057, T059, T060
- [ ] CHK023 RF-07 · *backoff* 30 s → 2 h, ±25 %, `Retry-After` → FR-035, FR-036 → T055, T062
- [ ] CHK024 RF-08 · sombreado y aviso del anónimo → FR-049 → T083, T096
- [ ] CHK025 RF-09 · mapa del registrado con persistencia → FR-050 → T089, T096
- [ ] CHK026 RF-10 · alertas por la vía, solo el rol que las recibe → FR-054…FR-057 → T097…T102
- [ ] CHK027 RF-11 (parte app) · envío del lote según el contrato → FR-030, FR-031 → T061, T070
- [ ] CHK028 RF-14 (parte app) · `updatedSince` con bajas → FR-044 → T082, T085
- [ ] CHK029 RF-21, RF-22 (parte app) · registro, login, verificación → FR-060…FR-064 → T107…T114
- [ ] CHK030 RF-25 · historial local → FR-069 → T115…T117
- [ ] CHK031 RF-26 · mapa base, polilínea, manifiesto, reanudable, Wi-Fi, «Actualizado hace…», `venceUtc`, pantalla sin mapa → FR-041…FR-048, FR-053 → T082, T086…T096
- [ ] CHK032 RF-27 · UUID aleatorio, rotación mensual, cabecera sin token → FR-039, FR-040 → T033…T035
- [ ] CHK033 RF-29 · aviso al anónimo, «Solo ver el mapa» → FR-001, FR-002, FR-005, FR-037 → T025…T032, T059
- [ ] CHK034 RF-30, RF-31, RF-32 (parte app) → FR-065, FR-067, FR-068 → T107…T114
- [ ] CHK035 RF-33 · modelo nuevo con manifiesto (*Should*) → FR-070, FR-071 → T118…T120
- [ ] CHK036 RF-34 · categoría → FR-004 → T027, T029, T031
- [ ] CHK037 RF-35 · permisos explicados → FR-006 → T004, T027, T031
- [ ] CHK038 RF-36 · indicador con 7 estados y pendientes → FR-016 → T043, T048, T052
- [ ] CHK039 RF-37 · términos versionados → FR-001…FR-003 → T025, T026, T029
- [ ] CHK040 RF-38 · cerrar sesión y permiso local → FR-063, FR-066 → T106, T107, T112
- [ ] CHK041 Contrato del lote y «Estados locales del evento» (`03`) → FR-020…FR-022, FR-029…FR-036 → T054…T072

## Reglas de negocio

- [ ] CHK042 RN-01, RN-04 · catálogo y 0.75 → FR-012, FR-071 → T039, T118
- [ ] CHK043 RN-03 · > 5 km/h ≥ 10 s → FR-010 → T036
- [ ] CHK044 RN-07 · solo `Activa`; `venceUtc` → FR-048 → T084, T098
- [ ] CHK045 RN-08 · alcance y alertas por rol, demo → FR-045, FR-049, FR-054 → T083, T085, T099
- [ ] CHK046 RN-09 · rotación mensual → FR-039 → T033
- [ ] CHK047 RN-11 · un lote = un seudónimo; no enviar > 168 h → FR-022, FR-029 → T059, T060
- [ ] CHK048 RN-16 · distancia por categoría, por la vía, por delante, una por pasada, agrupada → FR-055…FR-057 → T097, T098, T100
- [ ] CHK049 RN-18 · solo Liviano detecta; alertas con cualquier categoría → FR-004, FR-010, FR-059 → T037, T099
- [ ] CHK050 RN-19 · mensaje genérico en la app → FR-061 → T107, T110
- [ ] CHK051 RN-20 · sonido que baja la música, pantalla bloqueada, vibración → FR-058 → T101, T103
- [ ] CHK052 RN-21 · permiso local, no cortar en viaje, baja de cuenta, cierre de sesión → FR-051, FR-063, FR-066 → T106…T108
- [ ] CHK053 RN-22 · pausa detección y envío, mantiene alertas → FR-017, FR-037, FR-059 → T038, T059, T099

## Calidad, decisiones y escenarios

- [ ] CHK054 RNF-04 / EQ-03 · < 50 ms p95 en 3 celulares → FR-011 → T122, T121
- [ ] CHK055 RNF-12 / EQ-08 · español, ≤ 3 toques, 0 para detectar → FR-008 → T053, T126
- [ ] CHK056 RNF-14 · tiempo relativo → FR-047 → T011, T089
- [ ] CHK057 RNF-15 · batería, datos, almacenamiento → FR-025 → T065, T124
- [ ] CHK058 RNF-26 · fin del acceso offline → FR-063 → T106, T108
- [ ] CHK059 RNF-33 / EQ-01 · 5 h, 7 días, ≤ 60 s → FR-025, FR-028 → T062, T065, T123
- [ ] CHK060 EQ-02 (lado app) · 0 pérdidas / 0 duplicados → FR-031, FR-036 → T063, T125
- [ ] CHK061 EQ-11 / 006 R-007 · anónimo sin `bbox` ni `updatedSince` → FR-045 → T085
- [ ] CHK062 ADR-003 · Store-and-Forward → FR-019…FR-024 → T013, T014, T054, T066
- [ ] CHK063 ADR-004 · LiteRT, `modeloVersion`, actualización con SHA-256 → FR-011, FR-015, FR-070 → T044, T118
- [ ] CHK064 ADR-007 · seudónimo aleatorio, historial local → FR-039, FR-069 → T033, T115
- [ ] CHK065 ADR-009 · tokens de 5 min con renovación y gracia de 60 s; permiso local → FR-062, FR-063 → T108, T113
- [ ] CHK066 ADR-011 · PMTiles en R2, MapLibre, polilínea, atribución → FR-041…FR-043, FR-072 → T076, T094, T096
- [ ] CHK067 D2 · partir el lote al rotar → FR-029 → T059, T060
- [ ] CHK068 D3 · sin `loteId` en el cuerpo → FR-030 → T061
- [ ] CHK069 D5 · `id: null` + `indice` → FR-031 → T056
- [ ] CHK070 D13 · contraseña 10, mensaje genérico, eliminar con contraseña, `kmVia`, «permanente» → FR-050, FR-060, FR-061, FR-067 → T089, T105, T107
- [ ] CHK071 D16 · datos del corredor en R2 desde `backend/db/datos/` → FR-072…FR-074 → T073…T079, T127
- [ ] CHK072 BD-A1…BD-A9, BD-15, BD-19 · Room versionado, `ultimaDeteccionUtc`, `enDemo`, reloj, purgas, códigos, transacción del lote, UUIDv7, km común → FR-013, FR-019…FR-026, FR-051, FR-074 → T008, T009, T013…T015, T060, T064…T066, T079
