# Specification Quality Checklist: Dataset y modelo de detección

**Purpose**: Validar que la spec está completa y clara antes de planificar
**Created**: 2026-10-22
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] CHK001 Enfocada en el valor (detección que funciona en la vía real y se puede demostrar con RNF-35 y RNF-04)
- [x] CHK002 Todas las secciones obligatorias completas (ficha, fuentes, 7 historias, casos borde, 34 FR, parámetros, entidades, 8 SC, fuera de alcance, supuestos)
- [x] CHK003 El cómo (TensorFlow, `TFLiteConverter`, rclone, CameraX) está en plan y research; la spec fija contratos y umbrales

## Requirement Completeness

- [x] CHK004 No quedan marcas `[NEEDS CLARIFICATION]`; las 3 decisiones de negocio abiertas están en `research.md` «Pendientes de confirmar por el autor»
- [x] CHK005 Protocolo de campo con seguridad del conductor (FR-002), grabador y su justificación (R-001), frecuencia y formato (FR-001, FR-003), metadatos (FR-004) y privacidad (FR-005)
- [x] CHK006 Etiquetado con criterios de tipo y severidad (FR-008…FR-012), acuerdo medido (FR-011)
- [x] CHK007 Partición por recorrido y celular sin fuga, con tamaño mínimo (FR-013…FR-015)
- [x] CHK008 Pipeline reproducible, cuantización, equivalencia, *benchmark*, versionado (FR-016…FR-027)
- [x] CHK009 Puerta de publicación con RNF-35 y RNF-04; manifiesto; workflow manual; modelo inicial en el APK (FR-028…FR-032)
- [x] CHK010 Cada parámetro tiene valor y fuente (docs o Decisión SDD con su R-xxx)
- [x] CHK011 Casos borde (soporte inestable, muestreo irregular, sin GPS, borde de ventana, rompemuelles, pocos hundimientos, video con personas)
- [x] CHK012 Criterios de éxito medibles (SC-001…SC-008)

## Feature Readiness

- [x] CHK013 Cada FR tiene al menos una tarea (FR-001…FR-034; comprobado con script)
- [x] CHK014 Riesgo principal y plan B documentados (R-020, R-021; docs no traía plan B)
- [x] CHK015 Coherente con 008 (ruta `modelo/manifiesto.json`, firma del modelo, ventana y normalización en el manifiesto, fusión de ventanas en 008 FR-014)
- [x] CHK016 Límite de 100 líneas aplicado a `.py` y `.kt`; la enmienda de la constitución se propone, no se aplica

## Notes

- Ninguna decisión contradice `referencia/docs/`; los agregados que convendría llevar a docs están en `research.md` «Para llevar a docs» (D1).
