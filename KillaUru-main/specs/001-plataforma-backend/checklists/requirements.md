# Specification Quality Checklist: Plataforma base del backend

**Purpose**: Validar que la spec está completa y clara antes de implementar
**Created**: 2026-10-20
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] CHK001 Enfocada en el valor para el equipo y los clientes de la API
- [x] CHK002 Todas las secciones obligatorias completas
- [x] CHK003 Sin detalles de implementación innecesarios en la spec *(nota: al ser una feature de plataforma, nombra tecnologías fijadas por docs —R-02, R-03, ADR-010— porque son restricciones, no elecciones)*

## Requirement Completeness

- [x] CHK004 No quedan marcas `[NEEDS CLARIFICATION]`
- [x] CHK005 Cada FR es verificable y cita su fuente en docs
- [x] CHK006 Criterios de éxito medibles (tiempos, porcentajes, montos)
- [x] CHK007 Cada historia tiene escenarios Given/When/Then
- [x] CHK008 Casos borde identificados (IP falsa, sin seudónimo, telemetría caída, migraciones pendientes)
- [x] CHK009 Alcance delimitado (sección «Fuera de alcance»)
- [x] CHK010 Supuestos y dependencias explícitos

## Feature Readiness

- [x] CHK011 Cada FR tiene al menos una tarea en `tasks.md`
- [x] CHK012 Las historias cubren los flujos principales (arranque, seguridad, documentación, salud, recuperación, reglas)
- [x] CHK013 Los parámetros numéricos vienen de RNF y viven en configuración

## Notes

- Validado el 2026-10-20 en una sola iteración.
- Revalidado el 2026-10-21 tras aplicar D2, D4, D8, D9, D10, D11, D12, D15 y las correcciones T-BD01, T-OPS05/08/09/17/19, T-QA14/16/17/18/20, T-SEG03/09/20 (FR-030…FR-046, SC-009…SC-012, US7, US8, T058…T081).
- [x] CHK014 Lo que no viene de docs se marca «Decisión SDD» o con la decisión D# del autor
- [x] CHK015 Cada FR nuevo tiene tarea y cada tarea nueva cita un FR existente (verificado con script)
