# Trazabilidad Checklist: Dataset y modelo de detección

**Purpose**: Todo lo que docs pide a esta feature → FR → tarea
**Created**: 2026-10-22
**Feature**: [spec.md](../spec.md)

## Historias y requisitos funcionales

- [ ] CHK001 HU-28 · recorridos reales, vehículos livianos en soporte, ≥ 3 celulares → FR-001…FR-007 → T016…T022
- [ ] CHK002 HU-28 · etiquetas Bache, Hundimiento, Normal y severidad → FR-008…FR-012 → T024…T032
- [ ] CHK003 HU-28 · prueba separada que no se usa para entrenar, ≥ 200 ventanas por clase → FR-013…FR-015 → T033…T037
- [ ] CHK004 HU-28 · cumple RNF-35 y publica la matriz con la `modeloVersion` → FR-021, FR-022, FR-028 → T041, T047, T048, T060, T064
- [ ] CHK005 HU-28 · ~2 semanas, empieza en la semana 1, mayor riesgo → FR-034 → T069…T071
- [ ] CHK006 HU-01 · detección sin tocar el celular (también al grabar) → FR-002 → T016, T019
- [ ] CHK007 HU-02 / RF-02 · ventana de 500 ms → tipo, severidad 1–5, confidence → FR-012, FR-017, FR-019, FR-020 → T027, T031, T038, T044, T045
- [ ] CHK008 HU-26 / RF-34 / RN-18 · solo Liviano con soporte → FR-004, FR-006 → T008, T015, T022
- [ ] CHK009 RF-01 · 50 Hz, servicio en primer plano → FR-001 → T017, T018
- [ ] CHK010 RF-03 · hora del GPS y `modeloVersion` → FR-003, FR-027 → T018, T062, T063
- [ ] CHK011 RF-33 · manifiesto con SHA-256 y versión mínima de la app; *Should* recortable → FR-029, FR-030, FR-032 → T061, T064…T066, T068

## Reglas de negocio

- [ ] CHK012 RN-01 · catálogo = etiquetas del modelo; Normal nunca se envía; rompemuelles fuera → FR-009, FR-019, FR-028 → T025, T050, T060
- [ ] CHK013 RN-02 · severidad 1–5 con su significado → FR-009, FR-010 → T025, T028
- [ ] CHK014 RN-03 · solo ventanas con > 5 km/h sostenidos ≥ 10 s → FR-012 → T010, T027
- [ ] CHK015 RN-04 / RN-11 · confidence ≥ 0.75 en la decisión del sistema; fuera del manifiesto → FR-020, FR-021 → T040, T061

## Calidad, decisiones y escenarios

- [ ] CHK016 RNF-35 · *precision* y *recall* ≥ 0,90 por clase; severidad ± 1 ≥ 80 %; ≥ 200 por clase; ≥ 3 celulares → FR-014, FR-021, FR-028 → T033, T041, T060
- [ ] CHK017 RNF-04 · < 50 ms p95 en ≥ 3 celulares de gama media → FR-026, FR-028 → T053, T054, T057…T059
- [ ] CHK018 RNF-15 · prefiltro de energía y tasa de paso para 008 → FR-018 → T039, T044
- [ ] CHK019 RNF-34 · credenciales de R2 como secretos de GitHub → FR-030 → T066
- [ ] CHK020 ADR-004 · MLP cuantizado, < 5 MB, inicial en el APK, manifiesto + SHA-256 en R2, matriz por versión → FR-019, FR-022, FR-023, FR-029…FR-032 → T045, T055, T064…T068
- [ ] CHK021 ADR-004 punto 4 · catálogo nuevo = versión mayor → FR-027 → T062, T063
- [ ] CHK022 EQ-03 · benchmark en ≥ 3 celulares y matriz contra el dataset del tramo demo → FR-021, FR-026 → T047, T059
- [ ] CHK023 S-06 · liviano + soporte fijo → FR-006 → T022
- [ ] CHK024 R-06 / RNF-10 · sin datos personales en el dataset → FR-005 → T008, T013, T023, T027
- [ ] CHK025 `diseño-interno` §7.4 · sensores en lote y prefiltro → FR-001, FR-018 → T018, T044
- [ ] CHK026 D10 · GitHub Actions (CI de `ml/` y publicación manual) → FR-030, FR-033 → T004, T066
- [ ] CHK027 D16 · polilínea de 004 para el kilometraje → FR-017 → T012
- [ ] CHK028 Constitución VII y VIII · pruebas primero; `.py`/`.kt` < 200 líneas → FR-033 → T003, T004
