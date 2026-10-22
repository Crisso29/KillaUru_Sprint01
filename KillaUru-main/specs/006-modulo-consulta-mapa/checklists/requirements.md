# Specification Quality Checklist: Módulo Consulta de Mapa

**Purpose**: Validar que la spec está completa y clara antes de planificar
**Created**: 2026-10-21
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] CHK001 Enfocada en el valor (mapa rápido y frontera anónimo/registrado)
- [x] CHK002 Todas las secciones obligatorias completas
- [x] CHK003 El cómo (HybridCache, `PipeWriter`, SHA-256) está en plan y research

## Requirement Completeness

- [x] CHK004 No quedan marcas `[NEEDS CLARIFICATION]`
- [x] CHK005 Los 3 roles × 3 tipos de petición tienen comportamiento definido (`data-model.md` §1)
- [x] CHK006 Las cabeceras de caché por rol están en requisitos, escenarios y contrato
- [x] CHK007 Casos borde identificados (`bbox` inválido o ausente, reinicio, estampida, corredor sin cargar, token vencido)
- [x] CHK008 Criterios de éxito medibles (RNF-01, RNF-27, EQ-11, EQ-14)

## Feature Readiness

- [x] CHK009 Cada FR tiene tarea
- [x] CHK010 Las 4 historias cubren anónimo, conductor, offline y administrador

## Notes

- Las 5 decisiones pendientes fueron **confirmadas por el autor el 2026-10-21** (D1, D6, D17; lista al final de `research.md`). Se agregaron `kmVia` (D13), bajas anónimas solo del demo (T-SEG16), `venceUtc` nulo (T-PO08), la regla 401/403 (T-SEG20) y la métrica de caché (T-OPS15). La carga la mide 001 (D11).
