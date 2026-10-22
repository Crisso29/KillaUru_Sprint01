# Specification Quality Checklist: Módulo Ingesta

**Purpose**: Validar que la spec está completa y clara antes de planificar
**Created**: 2026-10-21
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] CHK001 Enfocada en el valor (ningún evento perdido ni duplicado; respuesta por evento)
- [x] CHK002 Todas las secciones obligatorias completas
- [x] CHK003 El cómo (`unnest`, `Channel`, `ProgramaReloj`, `JsonElement`) está en plan y research

## Requirement Completeness

- [x] CHK004 No quedan marcas `[NEEDS CLARIFICATION]`
- [x] CHK005 Los códigos del contrato del lote tienen escenario o caso borde (200, 400, 401, 403, 413, 503); el 429 y el techo por IP los prueba 001
- [x] CHK006 Cada parámetro de `IngestaOptions` tiene valor y fuente
- [x] CHK007 Casos borde identificados (repetidos en el lote, rechazado que vuelve, evento sin `eventoId`, seudónimo inconsistente, cola casi llena, fila vieja `Recibido`, reinicio con miles de pendientes)
- [x] CHK008 Criterios de éxito medibles (RNF-02, RNF-03, RNF-06, RNF-23, RNF-25)

## Feature Readiness

- [x] CHK009 Cada FR tiene tarea (FR-001…FR-026)
- [x] CHK010 Las 5 historias cubren recepción, durabilidad, procesamiento, privacidad y salud
- [x] CHK011 Cada FR cita su fuente de docs o la decisión del autor (D2, D3, D4, D5, D8, D11) o la corrección técnica (T-BD06, T-BD-CHK, T-OPS05, T-ARQ08)

## Notes

- Los pendientes de `research.md` quedaron **confirmados por el autor el 2026-10-21** (D1, D2, D3, D4, D5, D8, D11 y `01-actores.md` §4). Lo que no se aplicó está en `specs/revision/no-aplicado-005.md`.
