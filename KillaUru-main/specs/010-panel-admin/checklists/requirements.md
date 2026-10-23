# Specification Quality Checklist: Panel de administración «KillaUru Ops»

**Purpose**: Validar que la spec está completa y clara antes de planificar
**Created**: 2026-10-22
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] CHK001 Enfocada en el valor (operar el corredor y la plataforma sin reglas en el cliente)
- [x] CHK002 Todas las secciones obligatorias completas
- [x] CHK003 El cómo (librerías, CSP exacta, renovación, refresco) está en plan y research

## Requirement Completeness

- [x] CHK004 No quedan marcas `[NEEDS CLARIFICATION]`; las decisiones abiertas están en «Pendientes de confirmar» de `research.md`
- [x] CHK005 Cada pantalla tiene datos, operación del contrato, estados de carga/vacío/error, acciones, permisos y textos del prototipo (`spec.md`, `data-model.md` §2, §5, §7; `contracts/contratos-internos.md`)
- [x] CHK006 Cada dato del prototipo sin contrato está decidido: derivado (km del demo, «Qué pasó», franja, «N temas»), omitido (99,9 %, «llegó a N vehículos», «por ti») o hueco (H-01…H-06), sin endpoints nuevos
- [x] CHK007 Casos borde identificados (sesión agotada, pestañas, suspensión, reloj, sin red, R2 caído, 429, 403, 404, nulos, enlaces repetidos)
- [x] CHK008 Criterios de éxito medibles y con su herramienta (SC-001…SC-011, `quickstart.md` §4)

## Feature Readiness

- [x] CHK009 Cada FR tiene tarea (comprobado con `python -I`)
- [x] CHK010 Las 8 historias cubren las 7 pantallas de §5.1 y las páginas de enlaces de 003
- [x] CHK011 La constitución no se editó; la adaptación al frontend está propuesta en `research.md`

## Notes

- Se sigue el prototipo salvo donde un contrato o docs lo impide: periodo de 24 h en el Resumen (R-019), «3» del bloqueo (R-020), «A-1042» (R-029), «99,9 %» (R-018), «por ti» (R-017). La tipografía de texto es Source Sans 3, la del prototipo (R-021).
