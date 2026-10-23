# Specification Quality Checklist: Módulo Identidad y Accesos

**Purpose**: Validar que la spec está completa y clara antes de planificar
**Created**: 2026-10-20
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] CHK001 Enfocada en el valor (acceso seguro, derechos del titular)
- [x] CHK002 Todas las secciones obligatorias completas
- [x] CHK003 El cómo (PBKDF2, JWT, HMAC) está en plan y research, no en las historias

## Requirement Completeness

- [x] CHK004 No quedan marcas `[NEEDS CLARIFICATION]`
- [x] CHK005 Cada FR verificable y con fuente
- [x] CHK006 Todos los valores numéricos están en la tabla de parámetros con su fuente
- [x] CHK007 Casos borde identificados (mayúsculas, doble pestaña, proveedor de correo caído, invitación vencida)
- [x] CHK008 Alcance delimitado (rectificación y oposición fuera de v1.0)

## Feature Readiness

- [x] CHK009 Cada FR tiene tarea
- [x] CHK010 Las 8 historias cubren registro, login, sesión, correo, derechos, administración, consultas y retención/restauración
- [x] CHK011 Las decisiones D3, D4, D8, D9, D13 y D15 del 2026-10-21 están aplicadas con su FR (FR-037…FR-043) y su tarea (T058…T065)

## Notes

- Las decisiones pendientes fueron **confirmadas por el autor el 2026-10-21** (`research.md`, sección final). Lo no aplicado está en `specs/revision/no-aplicado-003.md`.
