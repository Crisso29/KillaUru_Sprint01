# Revisión por roles — 2026-10-21

Seis revisores, cada uno con un rol de empresa, compararon `.specify/memory/constitution.md` y `specs/001…007` con `referencia/docs/` y el prototipo. Fue solo lectura; cada hallazgo tiene archivo:línea y la cita de docs en su informe.

| Rol | Informe | Hallazgos | Altos |
|---|---|---:|---:|
| Arquitecto de software | [revision-arquitecto.md](revision-arquitecto.md) | 24 (ARQ) | 3 |
| DBA (PostgreSQL y SQLite) | [revision-dba.md](revision-dba.md) | 23 + 9 de la app (BD, BD-A) | 1 |
| QA lead (pruebas y rendimiento) | [revision-qa.md](revision-qa.md) | 25 (QA) + matriz RNF/EQ | 6 |
| Seguridad y privacidad | [revision-seguridad.md](revision-seguridad.md) | 26 (SEG) | 3 |
| DevOps / SRE | [revision-devops.md](revision-devops.md) | 27 (OPS) | 6 |
| Product owner | [revision-producto.md](revision-producto.md) | 32 (PO) + matriz HU/RF + pendientes | 3 |

## Lo que está bien (coinciden varios revisores)

- Grafo de módulos sin ciclos y `.csproj` según la regla 4; `<Modulo>Options` en su lugar.
- Un esquema por módulo sin FK entre esquemas; idempotencia por `evento_id`; bloqueo por celda; migraciones V0001→V0007 en orden.
- RN-01…RN-22 con los mismos valores que docs; MoSCoW y orden de recorte respetados.
- Ningún contrato expone seudónimos ni correos; auditoría en la misma transacción; purgas de 72 h; derechos ARCO.

## Decisiones del autor (se preguntan una por una en el chat)

| # | Tema | Hallazgos |
|---|---|---|
| D1 | Cuando una spec necesita algo que docs no dice: ¿se actualiza docs primero? | ARQ-05, PO pendientes |
| D2 | Seudónimo falsificable por evento y límite de tasa sin techo por IP | SEG-01, SEG-08, QA-15 |
| D3 | Datos que permiten unir una cuenta con sus eventos por la hora | SEG-02, BD-04, SEG-12, BD-05 |
| D4 | Controladores que usan clases del dominio (docs se contradice) | ARQ-01, ARQ-02, ARQ-03 |
| D5 | Lote con un evento sin `eventoId`: ¿400 al lote o rechazo del evento? | PO-03, 005 R-002 |
| D6 | Qué estados ve el administrador por defecto en el mapa | PO-07, 006 R-004 |
| D7 | `Descartada` «para siempre» frente al borrado a 2 años | BD-03 |
| D8 | Registro sin verificar correo puede confirmar baches graves; enumeración de cuentas | SEG-04, SEG-06 |
| D9 | Restaurar un backup revive sesiones y cambios de rol | SEG-10, SEG-11 |
| D10 | CI/CD: plataforma y reglas para aceptar un cambio | QA-01, OPS-01 |
| D11 | Dónde y cómo corren carga, estrés y resistencia | QA-03…QA-06, QA-09, OPS-21 |
| D12 | Métricas del servidor (disco, CPU, memoria) y del backup | OPS-02, OPS-03 |
| D13 | Choques del prototipo con docs/specs (contraseña 8 o 10, bloqueo visible, «Eventos hoy», km en el mapa) | PO (prototipo) |
| D14 | Reglas finas: reaparición, eventos aceptados vs procesados, renacer de una `Reparada` | PO-13, PO-14, PO-15 |
| D15 | Auditoría que abre su propia transacción; auditoría de accesos del host | ARQ-09, ARQ-11 |
| D16 | Quién publica polilínea, mapa base y manifiestos en R2 | QA-25, ARQ-21, PO-17 |
| D17 | Cuándo se hacen las specs de la app (SQLite/Room), el panel y HU-28 | ARQ-23, PO-01, BD-A1…A9 |

## Correcciones técnicas sin decisión (se aplican en bloque)

Primera migración con superusuario (V0000) y roles en el backup (BD-01, OPS-10…12); carrera caducidad–trabajador (BD-02); tareas diarias por hora de reloj, no `PeriodicTimer` de 24 h (OPS-05); índices y CHECK faltantes (BD-06, BD bajos); purgas de Identidad (BD-05); métricas sin emisor y emisor duplicado (OPS-14, OPS-16, SEG-18); `venceUtc` nulo en estados finales (PO-08); nombres de docs (`AnomaliaRepositoryPostgis`, ARQ-06); archivos > 199 líneas y puertos faltantes (ARQ-07, ARQ-08); límite de líneas también para SQL/scripts (QA-17); `BannedApiAnalyzers` para `DateTime.UtcNow` (QA-16); 429 declarados (SEG-19); tolerancia de reloj del token (SEG-09); logs sin datos personales (SEG-03); valores límite faltantes (QA-18); prueba de punta a punta (QA-20); `restart:` y rotación de logs (OPS-08, OPS-09).
