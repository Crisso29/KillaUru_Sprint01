# Specification Quality Checklist: Módulo Anomalías

**Purpose**: Validar que la spec está completa y clara antes de planificar
**Created**: 2026-10-20
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] CHK001 Enfocada en el valor (un mapa confiable y fiel a la vía)
- [x] CHK002 Todas las secciones obligatorias completas
- [x] CHK003 El cómo (bloqueos, SQL, `ProgramaReloj`) está en plan y research

## Requirement Completeness

- [x] CHK004 No quedan marcas `[NEEDS CLARIFICATION]`
- [x] CHK005 Cada regla RN del servidor (01, 02, 05, 06, 07, 09–15) tiene FR y valor en la tabla de parámetros
- [x] CHK006 Los escenarios usan los valores límite de docs (0.75, 20 m, 50 m, 3 min, 10 s, 120, 168 h, 22:00 Lima)
- [x] CHK007 Casos borde identificados (bordes de celda, llegada tarde, seudónimo purgado, reapertura, corredor no cargado)
- [x] CHK008 Criterios de éxito medibles (RNF-23, EQ-02, EQ-09, EQ-12)

## Feature Readiness

- [x] CHK009 Cada FR tiene tarea
- [x] CHK010 Las 7 historias cubren calidad, confirmación, ciclo de vida, privacidad, moderación, exportación y lecturas

## Notes

- Los pendientes de `research.md` (R-001, R-005, R-006, R-007, R-011, R-012) quedaron **confirmados por el autor el 2026-10-21** (D1); la revisión del 2026-10-21 suma D2, D3, D4, D7, D13, D14, D15 y las correcciones técnicas (FR-030…FR-034, R-013…R-017). Lo no aplicado está en `specs/revision/no-aplicado-004.md`.
