# Documentación de implementación

Empieza por la pregunta que necesitas responder. Las guías explican cómo trabajar;
el seguimiento muestra qué falta; el historial conserva lo revisado en una fecha.

## Qué documento abrir

| Necesito saber… | Documento |
|---|---|
| ¿En qué estado está el proyecto y qué lo bloquea? | [Estado de implementación](seguimiento/estado-implementacion.md) |
| ¿Dónde están las unitarias, integración, E2E y demás pruebas? | [Guía de pruebas](guias/pruebas.md) |
| ¿Qué criterios de calidad, estructura y seguridad seguimos? | [Calidad y seguridad](guias/calidad-y-seguridad.md) |
| ¿En qué orden cerramos specs y qué falta para desplegar? | [Cierre y despliegue](guias/cierre-y-despliegue.md) |
| ¿Qué cambió al ordenar los tests y qué se verificó? | [Revisión de organización de tests — 21 de noviembre](historial/revision-organizacion-tests.md) |
| ¿Qué se corrigió en la documentación anterior? | [Revisión documental — 15 de noviembre](historial/revision-documental.md) |
| ¿Qué se registró sobre integración de ramas y CI? | [Registro de integración y CI — revisión del 15 de noviembre](historial/operacion-ci.md) |

## Estructura

```text
docs/
├── README.md                       ← empieza aquí
├── guias/                          ← cómo trabajar y validar
│   ├── pruebas.md
│   ├── calidad-y-seguridad.md
│   └── cierre-y-despliegue.md
├── seguimiento/                    ← situación actual
│   └── estado-implementacion.md
└── historial/                      ← registros fechados
    ├── revision-documental.md
    ├── revision-organizacion-tests.md
    ├── operacion-ci.md
    └── revision-arquitectura-2026-10-21/
```

Para revisar una funcionalidad concreta, abre su [spec](../specs/README.md):
`pruebas.md` enlaza los tests, `tasks.md` registra las tareas y `cierre.md` reúne
evidencias y pendientes. El estado global resume esos documentos, no los reemplaza.

## Revisiones de arquitectura

La [revisión de arquitectura del 21 de octubre](historial/revision-arquitectura-2026-10-21/README.md)
conserva sus decisiones e informes. Se trasladó al historial para mantener `specs/`
organizado por módulos. No es el backlog actual.

| Documento de esa carpeta | Para qué sirve |
|---|---|
| `decisiones.md` | Consultar qué se acordó y por qué |
| `plan-de-cambios.md` | Entender qué cambios se planificaron en aquella revisión |
| `revision-*.md` | Consultar los hallazgos originales de arquitectura, QA, seguridad y otras áreas |
| `no-aplicado-*.md` | Entender por qué una propuesta se descartó, reemplazó o dejó condicionada |

**“No aplicado” no significa automáticamente “pendiente hoy”.** Un hallazgo antiguo
debe contrastarse con la decisión aceptada, el requisito vigente y el cierre actual
de la spec. Las decisiones no quedan anuladas por ser antiguas: se consultan junto
con la [referencia normativa](../referencia/docs/) y la [constitución](../.specify/memory/constitution.md).

`docs/historial` conserva revisiones de diseño e implementación;
`docs/seguimiento` indica la situación actual. Las futuras revisiones transversales
se guardan en el historial con fecha y alcance, fuera de `specs/`.

## Dónde documentar un cambio nuevo

- Instrucciones que seguirán utilizándose: actualizar una guía existente.
- Tareas, bloqueos o resultados actuales: actualizar la spec y el seguimiento global.
- Informe de una ejecución o revisión concreta: guardarlo en historial con fecha,
  alcance, entorno y resultados. Un informe antiguo conserva sus cifras originales.
- Cambios de dominio o arquitectura: actualizar primero la documentación normativa.

Los procedimientos operativos detallados permanecen junto a sus herramientas:
[runbooks del backend](../backend/deploy/runbooks/) y
[simulacros transversales](../deploy/runbooks/). No duplicarlos aquí.
