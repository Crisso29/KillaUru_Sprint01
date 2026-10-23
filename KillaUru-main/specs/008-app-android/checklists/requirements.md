# Specification Quality Checklist: App Android KillaUru

**Purpose**: Validar que la spec está completa y clara antes de planificar
**Created**: 2026-10-22
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] CHK001 Enfocada en el valor (detectar sin tocar, no perder datos, mapa y alertas sin señal, control de la cuenta)
- [x] CHK002 Todas las secciones obligatorias completas, en el orden de `specs/README.md` §3
- [x] CHK003 El cómo (Room, WorkManager, LiteRT, MapLibre, Keystore) está en plan, research y data-model

## Requirement Completeness

- [x] CHK004 No quedan marcas `[NEEDS CLARIFICATION]`; lo que docs no decide es «Decisión SDD» en `research.md`
- [x] CHK005 Cada código del contrato del lote tiene comportamiento (200, 400, 401, 403, 413, 429, 503, 5xx/*timeout*)
- [x] CHK006 Cada parámetro de `ParametrosApp` tiene valor y fuente
- [x] CHK007 SQLite/Room definido: tablas, tipos, índices, validaciones, estados, transiciones, purgas, migraciones y presupuesto
- [x] CHK008 Casos borde (sin red en el primer uso, reloj mal configurado, reinicio sin ancla, rotación del seudónimo, 413, 403, cierre de sesión sin red, huella incorrecta, administrador en la app)
- [x] CHK009 Criterios de éxito medibles (RNF-04, RNF-12, RNF-15, RNF-33, EQ-01, EQ-02, RN-16)

## Feature Readiness

- [x] CHK010 Cada FR tiene tarea (FR-001…FR-078; verificado con script)
- [x] CHK011 Las 10 historias cubren primer uso, detección, sincronización, mapa, alertas, cuenta, seudónimo, datos en R2, modelo e historial
- [x] CHK012 Cada FR cita su fuente de docs, una decisión del autor (D2, D3, D5, D8, D13, D16) o una decisión SDD con su R-xxx
- [x] CHK013 Lo recortable está marcado (US10 = HU-10, 2.º; US9 = RF-33, 3.º)

## Notes

- Pendientes del autor: R-001, R-024, R-025, R-026, R-028 (`research.md`).
- Propuesta de enmienda a la constitución (II, III, VII, VIII, IX y restricciones) en `research.md`; no se editó la constitución.
