# Trazabilidad Checklist: Módulo Dashboard

**Purpose**: Todo lo que docs pide a Dashboard → FR → tarea
**Created**: 2026-10-21
**Feature**: [spec.md](../spec.md)

## Historias y requisitos funcionales

- [ ] CHK001 HU-13 · eventos por día, conductores activos, dispositivos, cobertura, tasa de confirmación y de reaparición; acceso a los paneles técnicos → FR-006…FR-011 → T006, T016…T019
- [ ] CHK002 RF-18 · eventos por día → `EventosRecibidos` → T006, T015
- [ ] CHK003 RF-18 · conductores registrados activos (sesión renovada en 7 días) → `ContarConductoresActivosAsync(7)` → T009, T015
- [ ] CHK004 RF-18 · dispositivos (seudónimos distintos, guardado solo como número antes de la purga) → FR-001, FR-004 → T009, T015
- [ ] CHK005 RF-18 · cobertura (tramos de 1 km con evento en 7 días) → T006
- [ ] CHK006 RF-18 · tasa de confirmación (`Pendiente → Activa` antes de expirar) → T006
- [ ] CHK007 RF-18 · tasa de reaparición (vuelven a nacer desde `Expirada`, RN-07) → T006
- [ ] CHK008 RF-18 · ningún indicador une cuentas con eventos; técnicos en Grafana → FR-004, FR-011 → T022

## Reglas de negocio

- [ ] CHK009 RN-17 · conductores por sesiones de Identidad; dispositivos como número diario antes de la purga → T009, T022
- [ ] CHK010 RN-09 · el cálculo ocurre antes de que se purguen los seudónimos del día → T004, T005 (R-002)
- [ ] CHK011 RN-12 · día local UTC−5 → T004

## Calidad y decisiones

- [ ] CHK012 RNF-10 · sin vínculo cuenta ↔ eventos → T022
- [ ] CHK013 RNF-22 · última ejecución correcta de la tarea → T009
- [ ] CHK014 ADR-007 · privacidad por diseño (solo números) → T022
- [ ] CHK015 ADR-010 · indicadores técnicos fuera del panel → FR-011 (enlace)
- [ ] CHK016 `componentes-arquitectonicos.md` §6 · retención 2 años → T020

## Diseño (`diseño-interno` §3.4, §3.8)

- [ ] CHK017 Tabla `indicador_diario (dia, indicador, valor)` → T001
- [ ] CHK018 Tarea a las 00:30 hora local con los dos contratos → T012, T013

## Huecos de docs y prototipo (confirmar)

- [ ] CHK019 `DiasRecuperables = 2` (R-002)
- [ ] CHK020 Fórmulas de comparación (R-003)
- [ ] CHK021 «Eventos hoy» del prototipo vs. último día cerrado (R-005)
