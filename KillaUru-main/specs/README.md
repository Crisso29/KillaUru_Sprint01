# Specs de KillaUru (SDD con Spec Kit) — Backend, módulo por módulo

> **Alcance de esta tanda:** backend (001–007), app Android (008), dataset y modelo (009) y panel (010). Fuente de verdad: `referencia/docs/`.
> **Revisión por roles y decisiones del autor (2026-10-21):** [`revision/`](revision/README.md) — [`decisiones.md`](revision/decisiones.md) (D1…D17) y [`plan-de-cambios.md`](revision/plan-de-cambios.md) (nombres y valores canónicos).
> **Fuente de verdad:** `referencia/docs/` · **Reglas del proyecto:** `.specify/memory/constitution.md` · **Interfaz de referencia:** `referencia/prototipo/killauru-diseno.html`

## 1. Una feature por módulo

El backend es un monolito con **6 módulos** (ADR-001). Cada módulo tiene **su propia feature**, más una feature base (001) con lo que todos comparten. El orden sigue las dependencias entre módulos (`componentes-arquitectonicos.md`, regla 4): primero lo que los demás necesitan.

```text
001 Plataforma (host + Shared + despliegue)        ← base de todo
 ├── 002 Auditoría                                  ← la usan Identidad y Anomalías
 ├── 003 Identidad y Accesos ──► Auditoría
 ├── 004 Anomalías ───────────► Auditoría           ← corazón del negocio
 ├── 005 Ingesta ─────────────► Anomalías
 ├── 006 Consulta de Mapa ────► Anomalías
 └── 007 Dashboard ───────────► Anomalías, Identidad

Escritas el 2026-10-22 (D17): 008 App Android · 009 Dataset y modelo · 010 Panel de administración
```

| # | Feature | Qué construye | Historias (HU) | MoSCoW | Recortable |
|---|---|---|---|---|---|
| 001 | [`001-plataforma-backend`](001-plataforma-backend/spec.md) | Solución .NET, host de la API, kernel `Shared`, seguridad común, documentación de la API, salud, observabilidad, despliegue, respaldo, pruebas de arquitectura, CI/CD y pruebas de carga | HU-16, HU-17 | Must / Should | HU-17 (último en el orden) |
| 002 | [`002-modulo-auditoria`](002-modulo-auditoria/spec.md) | Registro inmutable de acciones sensibles en la misma transacción y su consulta | HU-15 | Should, **no recortable** | No |
| 003 | [`003-modulo-identidad-accesos`](003-modulo-identidad-accesos/spec.md) | Cuentas, login, tokens, bloqueo, correo, consentimientos, derechos ARCO, usuarios y privilegios | HU-11, HU-14, HU-18, HU-18b, HU-22, HU-23, HU-24, HU-25, HU-27 | Must (HU-24 Should no recortable) | No |
| 004 | [`004-modulo-anomalias`](004-modulo-anomalias/spec.md) | Validación, confirmación, clasificación, ciclo de vida, purgas, moderación, exportación y lecturas | HU-05, HU-06, HU-19, HU-20, HU-21 | Must (HU-19 Should, HU-20 Could) | HU-20 (exportación, la 1.ª) y HU-19 (moderación, la 5.ª) |
| 005 | [`005-modulo-ingesta`](005-modulo-ingesta/spec.md) | Recepción durable e idempotente de lotes, cola, trabajadores y barredor | HU-04, HU-05, HU-21 | Must | No |
| 006 | [`006-modulo-consulta-mapa`](006-modulo-consulta-mapa/spec.md) | Mapa por rol, caché por celdas, ETag, `updatedSince` con bajas | HU-07, HU-08, HU-12 | Must | No |
| 007 | [`007-modulo-dashboard`](007-modulo-dashboard/spec.md) | Indicadores de negocio diarios sin vincular identidad | HU-13 | Should | Todo el módulo (HU-13, la 4.ª) |
| 008 | [`008-app-android`](008-app-android/spec.md) | App Android: captura y detección, base local (SQLite/Room), sincronización, mapa offline, alertas, cuenta; publicación en R2 de la polilínea, el mapa base y los manifiestos (D16) | HU-01, HU-02, HU-03, HU-09, HU-10, HU-26 y la parte app de las demás | Must (HU-10 Could) | HU-10 (la 2.ª) y RF-33 (la 3.ª) |
| 009 | [`009-dataset-y-modelo`](009-dataset-y-modelo/spec.md) | Grabación y etiquetado de recorridos, entrenamiento y versión del modelo TFLite con su manifiesto | HU-28 | Must | No |
| 010 | [`010-panel-admin`](010-panel-admin/spec.md) | Panel web «KillaUru Ops»: resumen, mapa con filtros (abre con «Activa + PendienteConfirmacion», D6), moderación, usuarios, auditoría, exportación | Parte panel de HU-11…HU-15, HU-19, HU-20 | Must / Should | Lo que recorten 004 y 007 |

**Recorte si falta tiempo** (`02-historias-de-usuario.md`): HU-20 (004) → HU-10 (008) → RF-33 (008) → HU-13 (007) → HU-19 (004) → HU-17 (001). **No se recortan:** HU-15, HU-22, HU-23, HU-24, HU-25, HU-27. La columna «Recortable» de la tabla marca dónde vive cada una.

## 2. Qué requisito vive en qué feature

| RF | Feature (servidor) | Parte app / panel pendiente |
|---|---|---|
| RF-01…RF-03, RF-05, RF-09, RF-10, RF-25, RF-33…RF-36 | — | 008 (solo app) |
| RF-04, RF-06, RF-07 | — (consumen el contrato del lote de 005) | 008 |
| RF-08 | 006 (parte servidor: recorte al tramo demo) | 008 (sombreado y aviso) |
| RF-11 | 005 | 008 (envío de lotes) |
| RF-12 | 004 (y la recepción en 005) | — |
| RF-13 | 004 | — |
| RF-14 | 006 | 008 (mapa y copia offline), 010 (mapa) |
| RF-15 | 002 (y cada módulo que audita) | 010 (consulta) |
| RF-16 | 003 | 010 |
| RF-17 | 006 | 010 |
| RF-18 | 007 | 010 (Resumen) |
| RF-19 | 003 | 010 |
| RF-20 | 004 | 010 |
| RF-21, RF-22 | 003 | 008 |
| RF-23, RF-24 | 004 | 010 (moderación) |
| RF-26 | 006 (parte servidor) | 008 (mapa base y polilínea en R2, D16) |
| RF-27 | 004 y 005 (parte servidor) | 008 |
| RF-28 | 001 | — |
| RF-29, RF-30 | 003 | 008 |
| RF-31, RF-32 | 003 | 008, 010 |
| RF-37 | 003 (parte servidor) | 008 |
| RF-38 | 003 (cierre de sesión en el servidor) | 008, 010 |

HU-28 (dataset y modelo propio, *Must*) no tiene RF de servidor: es la feature 009.

## 3. Estructura fija de cada feature (para leerlas todas igual)

| Archivo | Qué contiene, siempre en este orden |
|---|---|
| `spec.md` | 0) Ficha del módulo · 1) Fuentes en docs · historias con escenarios · casos borde · requisitos FR agrupados · reglas y parámetros · entidades · criterios de éxito · fuera de alcance · supuestos |
| `plan.md` | Resumen · contexto técnico · puertas de la constitución · **árbol de archivos exacto** del módulo (cada archivo ≤ 100 líneas) · dependencias · complejidad |
| `research.md` | Decisiones `R-xxx`: decisión, por qué, alternativas, fuente |
| `data-model.md` | Tablas SQL, entidades, value objects, estados y transiciones, validaciones, clase `Options` |
| `contracts/` | `<modulo>.openapi.yaml` (API REST) y `contratos-internos.md` (interfaces de `Contracts/` que usan otros módulos) |
| `quickstart.md` | Cómo comprobar la feature de punta a punta |
| `tasks.md` | Tareas `T###` por fase e historia, con ruta de archivo y requisito que cumplen; pruebas antes del código |
| `checklists/requirements.md` | Calidad de la spec |
| `checklists/trazabilidad.md` | Cada HU/RF/RN/RNF/ADR de docs ↔ FR ↔ tarea |

**Convenciones de IDs dentro de una feature:** `FR-###` requisitos, `SC-###` criterios de éxito, `R-###` decisiones, `T###` tareas, `CHK###` checklist. Los IDs de docs (`HU-`, `RF-`, `RN-`, `RNF-`, `ADR-`, `EQ-`) se citan tal cual.

## 4. Cómo seguir

1. Revisar `001`. Luego `/speckit-analyze` y, sin hallazgos críticos, `/speckit-implement`.
2. Repetir en orden con `002` … `007`. `.specify/feature.json` indica la feature activa.
3. Después: escribir 008, 009 y 010, cada una con su revisión por roles (D17).
4. Si una spec choca con `referencia/docs/`, gana docs (constitución, principio I). Las decisiones del autor y lo que no se aplicó están en [`revision/`](revision/README.md).
