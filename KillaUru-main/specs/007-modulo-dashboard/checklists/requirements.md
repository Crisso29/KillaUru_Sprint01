# Specification Quality Checklist: Módulo Dashboard

**Purpose**: Validar que la spec está completa y clara antes de planificar
**Created**: 2026-10-21
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] CHK001 Enfocada en el valor (decidir sobre la plataforma sin vincular identidades)
- [x] CHK002 Todas las secciones obligatorias completas
- [x] CHK003 El cómo (`Task.Delay` sobre `TimeProvider`, `ON CONFLICT`) está en plan y research

## Requirement Completeness

- [x] CHK004 No quedan marcas `[NEEDS CLARIFICATION]`
- [x] CHK005 Los 6 indicadores de RF-18 tienen fórmula, unidad y comparación
- [x] CHK006 La hora del cálculo está justificada contra la purga de RN-09
- [x] CHK007 Casos borde identificados (día sin actividad, recuperación tarde, contrato que falla, sin historia, horario de verano)
- [x] CHK008 Criterios de éxito medibles

## Feature Readiness

- [x] CHK009 Cada FR tiene tarea
- [x] CHK010 Las 3 historias cubren cálculo, consulta y retención

## Notes

- **3 decisiones requieren confirmación del autor** (lista al final de `research.md`); una choca con el prototipo («Eventos hoy», R-005).
