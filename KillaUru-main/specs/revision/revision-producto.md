# Revisión de producto: KillaUru, specs 001–007 frente a `referencia/docs`

> **Rol:** Product Owner / analista de negocio · **Fecha:** 2026-10-21 · **Tipo:** solo lectura (no se editó ningún archivo del proyecto)
> **Raíz:** `KillaUru-main/` · **Fuente de verdad:** `referencia/docs/01-analisis-del-sistema/` (01, 02, 03, 09, 10) y `02-arquitectura-de-software/enfoque-arquitectonico.md` §5
> **Revisado:** `specs/README.md`; `spec.md`, `tasks.md`, `research.md`, `data-model.md`, `contracts/*`, `checklists/trazabilidad.md` de 001…007; `referencia/prototipo/killauru-diseno.html` (app y panel «KillaUru Ops»).
> La copia de `specs/` del worktree `specs-005-007` es idéntica a la de `KillaUru-main` (`diff -rq` sin diferencias).

**Comprobación automática de IDs:** en las 7 trazabilidades, todos los `FR-###` y `T###` citados existen en su `spec.md` y `tasks.md`. Todo FR está citado por al menos una tarea, salvo 001 FR-005 y 002 FR-007/FR-009, que solo se comprueban en el simulacro (T054) o en revisión de código. Ninguna tarea cita un FR inexistente.

---

## 1. Matriz de trazabilidad

### 1.1 Historia → RF → feature → FR → tarea

Leyenda de estado: ✅ cubierto en backend · 🟡 el backend está cubierto, pero la parte de app o panel no tiene spec · ⛔ ninguna feature lo cubre.

| HU | MoSCoW | RF principal / apoyo (`03`) | Feature | FR | Tareas | Estado |
|---|:---:|---|---|---|---|:---:|
| HU-01 | M | RF-01, RF-35, RF-36 / RF-02, RF-03, RF-33, RF-34 | App (sin spec) | — | — | 🟡 solo app |
| HU-02 | M | RF-02, RF-33 / RF-03 | App (sin spec). Catálogo y severidad en el servidor: 004, 001 | 004 FR-001; 001 FR-023 | 004 T007, T010; 001 T010 | 🟡 |
| HU-26 | M | RF-34 / RF-35 | App (sin spec) | — | — | 🟡 solo app |
| HU-28 | M | — (RNF-35) | **Ninguna** | — | — | ⛔ (PO-01) |
| HU-03 | M | RF-03, RF-05 / RF-04, RF-26, RF-36 | App + 006 (parte servidor de RF-26) | 006 FR-005, FR-012 | 006 T010, T025 | 🟡 |
| HU-04 | M | RF-04, RF-06, RF-07 / RF-11 | App + 005 | 005 FR-005, FR-006, FR-016 | 005 T010…T018, T025…T027 | 🟡 |
| HU-05 | M | RF-11, RF-12 / RF-27 | 005 + 004 | 005 FR-001…FR-010; 004 FR-001…FR-013 | 005 T005, T007, T010, T012; 004 T006…T031 | ✅ |
| HU-06 | M | RF-13, RF-23 / RF-24 | 004 | FR-008, FR-009, FR-016…FR-019 | T011, T012, T033…T036 | ✅ |
| HU-07 | M | RF-08 / RF-14 | App + 006 | 006 FR-002…FR-004 | T019…T021 | 🟡 (sombreado y aviso: app) |
| HU-08 | M | RF-09, RF-14, RF-26 / RF-21 | App + 006 | 006 FR-002, FR-005, FR-012 | T011…T018, T025…T027 | 🟡 |
| HU-09 | M | RF-10 / RF-23, RF-26 | App (+ `venceUtc` de 006 y 004) | 006 FR-005; 004 FR-017 | 006 T010; 004 T012 | 🟡 solo app |
| HU-10 | C | RF-25 / RF-38 | App (sin spec) | — | — | 🟡 solo app |
| HU-11 | M | RF-16 / RF-15 | 003 (+002) | FR-007, FR-009, FR-010, FR-014, FR-015 | T028…T032 | ✅ API · 🟡 pantalla del panel |
| HU-12 | M | RF-17 / RF-14 | 006 | FR-010, FR-011 | T022…T024 | ✅ API · 🟡 panel |
| HU-13 | S | RF-18 | 007 (+004 FR-029, 003 FR-036) | 007 FR-001…FR-012 | 007 T006, T016…T019; 004 T041…T043; 003 T052 | ✅ API · 🟡 panel |
| HU-14 | M | RF-19 / RF-15 | 003 | FR-030…FR-034 | T036, T047…T050 | ✅ API · 🟡 panel |
| HU-15 | S | RF-15 | 002 | FR-001…FR-012 | T003, T007, T010, T020, T025 | ✅ API · 🟡 panel |
| HU-16 | M | — (RNF-17, RNF-18) | 001 | FR-001…FR-005 | T023, T029, T031, T054 | ✅ |
| HU-17 | S | RF-28 / RNF-20, RNF-21 | 001 (+005 FR-021) | FR-018…FR-022 | T045…T050 | ✅ |
| HU-18 | M | RF-21, RF-38 / RF-22, RF-29 | 003 + app | FR-001, FR-014, FR-015, FR-018, FR-021 | T023…T027, T033, T034 | 🟡 (RN-21 de la app) |
| HU-18b | M | RF-22 | 003 | FR-021, FR-022, FR-025 | T037…T042 | ✅ |
| HU-19 | S | RF-24 / RF-15 | 004 | FR-020, FR-025…FR-027 | T013, T044…T047 | ✅ API · 🟡 panel |
| HU-20 | C | RF-20 / RF-15 | 004 | FR-028 | T048…T050 | ✅ API · 🟡 panel |
| HU-21 | M | RF-27 / RF-30, RF-38, RNF-10, RNF-25 | 004 + 005 + app | 004 FR-021, FR-023; 005 FR-008, FR-017 | 004 T037…T040; 005 T028…T030 | 🟡 (seudónimo rotativo: app) |
| HU-22 | M | RF-29 / RF-37 | 003 + app | FR-002, FR-003 | T023, T024 | 🟡 (aviso al anónimo y «Solo ver el mapa»: app) |
| HU-27 | M | RF-37 | 003 + app | FR-002…FR-004 | T023, T024, T043 | 🟡 |
| HU-23 | M | RF-30 / RF-15 | 003 + 002 + app | 003 FR-027, FR-028; 002 FR-006 | 003 T043…T046; 002 T006 | 🟡 (borrar el historial local: app) |
| HU-24 | S | RF-31 | 003 | FR-026 | T043…T046 | ✅ |
| HU-25 | M | RF-32 / RF-15, RF-22 | 003 | FR-022, FR-023 | T037…T042 | ✅ |

### 1.2 RF → feature (lo marcado con ⚠ no coincide con la tabla del README)

| RF | Feature | FR | Tareas | Nota |
|---|---|---|---|---|
| RF-01, RF-02, RF-03, RF-05, RF-09, RF-10 | App | — | — | Sin spec |
| RF-04, RF-06, RF-07 | App (el contrato del lote lo cumple 005) | 005 FR-003, FR-006, FR-009 | 005 T012, T034 | Sin spec de la app |
| RF-08 | App + **006** (recorte en el servidor) | 006 FR-003, FR-004 | T019…T021 | ⚠ El README lo pone solo en la app (PO-18) |
| RF-11 | 005 | FR-001…FR-010 | T012, T016 | ⚠ FR-003 contradice el RF (PO-03) |
| RF-12 | 004 (y la recepción en 005) | 004 FR-001…FR-013 | T006…T031 | |
| RF-13 | 004 | FR-008, FR-009, FR-016 | T012, T020, T026 | |
| RF-14 | 006 | FR-001…FR-013 | T005…T025 | |
| RF-15 | 002 (y los módulos que auditan) | 002 FR-001…FR-012 | T003, T007, T009, T016 | |
| RF-16 | 003 | FR-007, FR-009, FR-014, FR-015 | T021, T028…T032 | |
| RF-17 | 006 | FR-010, FR-011 | T022…T024 | |
| RF-18 | 007 (+004 y 003 aportan datos) | 007 FR-001…FR-012 | 007 T006…T019 | ⚠ Dos fórmulas se apartan de docs (PO-14, PO-15) |
| RF-19 | 003 | FR-030…FR-034 | T047…T050 | |
| RF-20 | 004 | FR-028 | T048…T050 | |
| RF-21 | 003 | FR-001, FR-007, FR-015 | T023…T034 | |
| RF-22 | 003 | FR-021, FR-025 | T037…T041 | |
| RF-23 | 004 | FR-017, FR-018 | T012, T033…T035 | |
| RF-24 | 004 | FR-025…FR-027 | T013, T044…T047 | |
| RF-25 | App | — | — | Sin spec |
| RF-26 | 006 (parte servidor) + app | 006 FR-005, FR-012 | T010, T025 | ⚠ Nadie publica el mapa base, la polilínea ni el manifiesto (PO-17) |
| RF-27 | 004 y 005 (parte servidor) + app | 004 FR-021; 005 FR-008, FR-017; 001 FR-010 | 004 T037…T039; 005 T028…T030 | La rotación mensual es de la app y no tiene spec |
| RF-28 | 001 (+005) | 001 FR-018, FR-019; 005 FR-021 | 001 T045, T046; 005 T031 | |
| RF-29 | 003 + app | FR-002, FR-003 | T023, T024 | Las partes de la app no tienen spec |
| RF-30 | 003 + 002 + app | FR-027, FR-028 | T043…T046 | |
| RF-31 | 003 | FR-026 | T043…T046 | |
| RF-32 | 003 | FR-022, FR-023 | T037…T039 | |
| RF-33, RF-34, RF-35, RF-36 | App | — | — | Sin spec |
| RF-37 | 003 (parte servidor) + app | FR-002…FR-004 | T023, T024, T043 | |
| RF-38 | 003 (cierre de sesión en el servidor) + app | FR-018 | T033, T034 | RN-21 a, b y d son de la app y no tienen spec |

**Resumen de huecos**
- **Sin ninguna feature:** HU-28 (dataset y entrenamiento del modelo, *Must*, la tarea con más riesgo del plan). También la publicación en R2 del mapa base, la polilínea y su manifiesto (RF-26).
- **Solo app, sin spec todavía:** HU-01, HU-02, HU-03, HU-09, HU-10, HU-26 y las partes de la app de HU-04, HU-07, HU-08, HU-18, HU-21, HU-22, HU-23 y HU-27. Son los RF-01…RF-10, RF-25 y RF-33…RF-36, más las partes de la app de RF-26, RF-27, RF-29, RF-30, RF-37 y RF-38.
- **Panel sin spec:** las pantallas de HU-11, HU-12, HU-13, HU-14, HU-15, HU-19 y HU-20. Sus API sí están especificadas.
- **Errores en el README:** ver PO-18.

### 1.3 Reglas de negocio y parámetros (09 ↔ spec / Options)

| RN | Valor en docs (`09`) | Dónde vive en las specs | Valor en la spec | ¿Igual? |
|---|---|---|---|:---:|
| RN-01 | Bache, Hundimiento (Normal nunca se envía) | 004 `TipoAnomalia`; contratos 004, 005 y 006 | `[Bache, Hundimiento]` | ✅ |
| RN-02 | Severidad 1–5; alta = 4–5 | 001 FR-023; 004 `SeveridadAltaDesde` | 1–5; 4 | ✅ |
| RN-03 | > 5 km/h durante ≥ 10 s | App (enfoque §5, configuración del APK) | — | ✅ anotado como de la app |
| RN-04 | confidence ≥ 0.75 | App; el servidor lo repite con RN-11 | 0.75 | ✅ |
| RN-05 | 100 m · 15 días | 004 `RadioAgrupacionMetros`, `VentanaSeveridadDias` | 100 · 15 | ✅ |
| RN-06 | 4 · 2 · 72 h · 3 min | 004 | 4 · 2 · 72 · 3 | ✅ |
| RN-07 | 72 h · 7 días · 72 h · 3 vehículos en 7 días · revisión cada hora | 004 | 72 · 7 · 72 · 3/7 · cada 60 min | ✅ (se agrega `MantenerDescartada` y el vencimiento al reabrir, ver pendientes) |
| RN-08 | Anónimo: demo · Registrado: corredor · Admin: **todos los estados** | 006 | Admin por defecto: `Activa` + `PendienteConfirmacion` | ⚠ PO-07 |
| RN-09 | Rotación 30 días (app) · purga a las 72 h desde la recepción | 004 y 005 `PurgaSeudonimoHoras` | 72 en las dos | ✅ (el parámetro está duplicado, PO-29) |
| RN-10 | Recepción 30 días · histórico 2 años · auditoría permanente | 005 `RetencionRecepcionDias`; 004 y 007 730; 002 FR-009 | 30 · 730 · permanente | ✅ |
| RN-11 | 0.75 · > 20 m · 168 h / 5 min · 50 m · 150 km/h; 7 motivos | 004 (`MotivoDescarte`); contrato de 005 (6 motivos de recepción) | Iguales | ✅ |
| RN-12 | 3 días locales · 15 días · UTC−5 | 004; 007 `DesfaseHorarioHoras` | 3 · 15 · −5 | ✅ |
| RN-13 | 50 m | 004 `DistanciaMaximaViaMetros` | 50 | ✅ |
| RN-14 | ≥ 1 reporte con sesión | 004 `ReportesConSesionRequeridos`; 005 FR-007 | 1 | ✅ |
| RN-15 | 120 por hora · 150 km/h · 10 s | 004 | 120 (`> 120`) · 150 · 10 | ✅ |
| RN-16 | 8 s / 100 m · 12 s / 150 m · 500 m | App | — | ✅ anotado como de la app |
| RN-17 | 7 días · 24 h | 007 `DiasConductorActivo`; 003 FR-036 | 7; la ventana de 24 h se toma como el día local | ✅ |
| RN-18 | Solo Liviano detecta | App | — | ✅ anotado como de la app |
| RN-19 | 3 intentos en 15 min → 5, 15 y 60 min; mismo mensaje al conductor | 003 `IdentidadOptions` | 3 · 15 · [5, 15, 60]; app → 401 `credenciales_invalidas` | ✅ (el prototipo no lo cumple, PO-05) |
| RN-20 | Sonido de la alerta | App | — | ✅ anotado como de la app |
| RN-21 | 30 días · 30 min; (c) el servidor informa la baja | 003 `SesionConductorDias` = 30; FR-017; el resto en la app | 30 | ✅ (debe coincidir con `DuracionPermisoLocalDias` de la app, PO-29) |
| RN-22 | < 15 % sin cargar | App | — | ✅ anotado como de la app |

**MoSCoW y orden de recorte:** los specs respetan las prioridades. 001: HU-16 *Must*, HU-17 *Should* «recortable al final». 002: HU-15 *Should* **no recortable**. 003: todo *Must*, salvo HU-24 *Should* no recortable. 004: HU-19 *Should*, HU-20 *Could*, «lo primero que se recorta», fase P3 «recortable». 007: HU-13 *Should*, 4.º en el orden de recorte, marcado en `tasks.md`. El README copia bien la secuencia HU-20 → HU-10 → RF-33 → HU-13 → HU-19 → HU-17 y la lista de historias que no se recortan. El único detalle está en las tareas: ver PO-22.

---

## 2. Hallazgos

Severidad: **Alta** = contradice docs o deja una historia *Must* sin cubrir · **Media** = falla un contrato o un dato que pide el prototipo o las docs · **Baja** = forma, nombres o documentación.

| ID | Sev. | Archivo:línea | Qué dice docs (o el prototipo) | Qué dice la spec | Corrección propuesta | ¿Decide el autor? |
|---|:---:|---|---|---|---|:---:|
| PO-01 | Alta | `specs/README.md:3` · `02-historias-de-usuario.md:100` | HU-28 es *Must*: grabar y etiquetar recorridos y entrenar el modelo. Es «la tarea con más riesgo de retraso» y empieza en la semana 1 | Ninguna feature la cubre; el README limita la tanda al backend | Abrir una feature «dataset y modelo» (HU-28, RNF-35, RN-01/02, manifiesto de RF-33), al menos con plan y tareas, porque debe arrancar ya | Sí: dónde y cuándo se especifica |
| PO-02 | Alta | `specs/README.md:3,47` | 22 HU *Must*. De ellas, HU-01, 02, 03, 09 y 26 son solo de la app, y 8 más tienen una parte en la app | No hay spec de la app ni del panel; solo se anuncia «después» | Planificar las features de la app y del panel (p. ej. 008 y 009), con su propia tabla RF→feature y la lista de lo que cada una consume de 001…007 | Sí: orden y alcance |
| PO-03 | Alta | `005/spec.md:147` (FR-003) · `005/research.md:12-14` | RF-11 y el contrato del lote (`03:26,119`): 400 **solo** si el lote no se lee, está vacío o trae más de 50; «un evento mal formado nunca rechaza el lote entero» | 400 `lote_invalido` también si **un** evento no trae `eventoId` UUID; 413 si el cuerpo pasa de 256 KB | Gana docs mientras el autor no decida. O se cambia `03` (fila 400 + 413), o el evento sin id se descarta sin reportarlo. La app ya parte el lote ante un 400, así que el cambio es compatible | **Sí** |
| PO-04 | Media | `003/spec.md:249` · `003/data-model.md` (`ContrasenaMinima = 8`) · `identidad.openapi.yaml` (`minLength: 8`) · `prototipo:1083` | El prototipo, en Crear cuenta, dice «Mínimo 10 caracteres». Docs no fijan el valor | Mínimo 8 y máximo 128 (decisión SDD, NIST) | Elegir un valor y alinear la spec, el contrato y el texto del prototipo | **Sí** |
| PO-05 | Media | `prototipo:1099-1105` · `003/research.md:35` | RN-19 (`09:204`): «al conductor se le muestra el mismo mensaje exista o no la cuenta» | La app siempre recibe 401 `credenciales_invalidas` (correcto). El prototipo de la app muestra «Demasiados intentos. Espera 5 minutos» y «Disponible en 4:52» | Corregir el prototipo de la app: mismo mensaje que con credenciales inválidas. El aviso de bloqueo solo en el panel (423) | No (gana docs) |
| PO-06 | Media | `003/spec.md:216` (FR-027) · `identidad.openapi.yaml:215` · `prototipo:1130-1137` | HU-23: «Dado que confirmo la eliminación». El prototipo confirma con una casilla («Entiendo que no se puede deshacer»), sin contraseña | El contrato exige `contrasena` (obligatoria) | Alinear: agregar el campo de contraseña al prototipo o quitarlo del contrato | **Sí** |
| PO-07 | Media | `006/spec.md:127,162` · `006/research.md:23-25` · `prototipo:1243` | RN-08 (`09:170`): el administrador recibe «todos los estados, con filtros». En el prototipo, el filtro Estado empieza con los 5 estados marcados | Sin `estado`, el administrador recibe solo `Activa` y `PendienteConfirmacion` | Gana docs: por defecto todos los estados (o cambiar RN-08 y el prototipo) | **Sí** |
| PO-08 | Media | `mapa.openapi.yaml:106` · `anomalias.openapi.yaml:162` · `004/data-model.md:111-117` | Las anomalías `Expirada`, `Reparada` y `Descartada` no tienen vencimiento (el prototipo muestra «—») | En `AnomaliaFeature`, `venceUtc` es obligatorio y no admite nulo, pero el administrador puede pedir esos estados | `venceUtc: [string, 'null']`, o declarar que solo vale para `Activa` y `PendienteConfirmacion` | No |
| PO-09 | Media | `mapa.openapi.yaml:96-113` · `prototipo:1330,1345,1373,1013-1021` | El panel muestra «Bache · km 287,4» en la lista del mapa, la franja «Anomalías activas a lo largo del corredor» (barras cada 10 km) y «Dentro / fuera del tramo demo». La app muestra el hito «km 288,4» | El GeoJSON del mapa no trae `kmVia`. Solo lo traen el detalle y las listas de 004 | Agregar `kmVia` a `AnomaliaFeature` (al menos para el administrador) o documentar que el cliente lo calcula con la polilínea | Sí (menor) |
| PO-10 | Media | `prototipo:1021` · `mapa.openapi.yaml:106` | El detalle del bache en la app dice «Reportado 5 días distintos · permanente» | El mapa entrega `persistencia`, pero no `diasDistintos15d` | Agregar el campo al mapa o cambiar el texto del prototipo a «permanente» solamente | Sí |
| PO-11 | Media | `004/research.md:56` · `prototipo:1428` | Columnas del prototipo: `id, tipo, severidad, estado, persistencia, km, latitud, longitud, reportes, primer_reporte, ultimo_reporte` | Columnas de 004: `…, km_via, ultima_deteccion_utc, reportes, dias_distintos_15d` (sin `primer_reporte`) | Fijar una sola lista de columnas en la spec y en el prototipo | Sí |
| PO-12 | Media | `004/research.md:56` y `anomalias.openapi.yaml:111` (se audita **antes** de escribir) vs `004/spec.md:144`, `004/tasks.md:114` (T050), `002/data-model.md:72` y `prototipo` «Recientes · Filas» | El prototipo y 002 esperan `filas` en el contexto del registro | Se audita antes de escribir; en *streaming* no se sabe el número de filas. Además, el objetivo «id de la exportación» no tiene entidad | Contar las filas antes de auditar (`COUNT` con los mismos filtros) o quitar `filas` del contexto, de T050 y del prototipo | **Sí** |
| PO-13 | Media | `004/spec.md:205` (FR-019) · `004/data-model.md:101` | RN-07 (`09:133-134`): `Reparada` + nuevo reporte → vuelve a nacer. La condición «detectado después del vencimiento» (`09:147`) es **solo para `Expirada`** | La misma condición se aplica a `Reparada`. Un bache reparado ayer que alguien reporta hoy, antes de su `vence_utc` anterior, se absorbe sin renacer | Para `Reparada`, usar «detectado después de la reparación» (`estado_desde_utc`) | Sí (docs no lo precisan) |
| PO-14 | Media | `004/research.md:65` · `007/research.md:25` | Glosario (`10:46`) y RN-07 (`09:158`): la reaparición es una anomalía que «vuelve a `Activa`» después de expirar | `Renacimientos` cuenta `Expirada → Activa` **y** `Expirada → PendienteConfirmacion`; la tasa sale inflada | Contar solo `Expirada → Activa`, o cambiar la definición en docs | **Sí** |
| PO-15 | Media | `004/research.md:62-66` · `prototipo:1321` | RF-18 (`03:38`): cobertura = tramos con «al menos un evento **aceptado**». El prototipo: «Eventos aceptados en la recepción» | Eventos y cobertura salen de `reporte_anomalia` (ya procesados). No cuentan los descartados al procesar (`VelocidadImposible`) | Aceptar la diferencia y cambiar la etiqueta («eventos procesados»), o pedir el conteo a Ingesta (nuevo contrato o dependencia) | **Sí** |
| PO-16 | Media | `006/research.md:44-45` | RF-26 (`03:51`): las anomalías se actualizan de forma incremental con `updatedSince`, sin distinguir rol | El anónimo pide sin `bbox` ni `updatedSince` y refresca la copia completa, para aprovechar la caché del borde | Anotar la excepción en RF-26 o en la futura spec de la app | **Sí** |
| PO-17 | Media | `004/spec.md:271` · `006/spec.md:186` | RF-26: mapa base, polilínea y manifiesto (versión, tamaño, SHA-256) para la descarga offline | Las dos specs lo dejan «fuera del backend»; ninguna feature lo produce ni lo publica | Asignarlo a 001 (despliegue o datos) o a la feature de la app, con su tarea | Sí |
| PO-18 | Baja | `specs/README.md:34-47` | RF-08 tiene una parte en el servidor (recorte del demo). RF-04, RF-06 y RF-07 dependen del contrato de 005 | La tabla RF→feature no lista RF-08 en 006 y dice «RF-01…RF-10 son de la app». No menciona las partes de la app de RF-26, 27, 29, 30, 37 y 38 | Agregar «RF-08 → 006 (parte servidor)» y una columna «parte app pendiente» | No |
| PO-19 | Baja | `anomalias.openapi.yaml:179-210` · `prototipo:1331,1390` | Moderación: «Por revisar» muestra la severidad. «Últimos cambios» muestra qué pasó («Confirmada por 2.º vehículo», «Llegó a 4 vehículos: por revisar») | `ElementoPorRevisar` no trae `severidadMostrada`. `TransicionReciente` no trae motivo, y «pasó a Por revisar» no es una transición, así que no aparece | Agregar `severidadMostrada` y un `motivo` corto; decidir si «por revisar» se lista como evento | Sí (menor) |
| PO-20 | Baja | `auditoria.openapi.yaml:30` · `prototipo:1415` | El filtro de la auditoría dice «Actor (correo o id)» | `actorId` solo acepta UUID | Que el panel resuelva el correo con `GET /api/v1/usuarios?busqueda=` antes de filtrar, o aceptar el correo | No |
| PO-21 | Baja | `prototipo:1342` | El Resumen muestra «Servicio operativo · 99,9 % · último chequeo hace 3 min» | Ningún contrato lo entrega (es dato de UptimeRobot) | Dejarlo como enlace a la página de estado, sin números, o declarar la integración | No |
| PO-22 | Baja | `004/tasks.md:41,47-48,103` · `001/tasks.md:115` · `007/tasks.md:14` | Orden de recorte: HU-13 → HU-19 → HU-17 | El dominio de moderación (HU-19: T013, `AccionAdmin` en T021, `.Moderacion.cs` en T022) está en la fase Foundational. La fase de HU-17 no dice «recortable». Si se recorta HU-13, igual quedan 004 T041…T043 (resumen del día, `km_via`) y 003 T052 | Marcar «recortable» en esas fases y anotar el recorte en cadena (004 y 003) cuando se quita HU-13 | No |
| PO-23 | Baja | `001/research.md:44` | — | Cita «FR-005» para que cada módulo sume su comprobación de salud; ese requisito es FR-019 | Corregir la referencia | No |
| PO-24 | Baja | `004/plan.md:33` | enfoque §5: `AnomaliasOptions` tiene 25 parámetros | «30 parámetros» | Corregir el número | No |
| PO-25 | Baja | `004/data-model.md` (sin sección) · `specs/README.md:56` | El README pide que `data-model.md` incluya la clase `Options` | 004 la tiene solo en `spec.md:229-245`; las demás features sí la incluyen en `data-model.md` | Agregar la sección `AnomaliasOptions` a `data-model.md` | No |
| PO-26 | Baja | `001/spec.md:209` · `005/data-model.md:84-97` | — | 001: `Salud:AntiguedadMaximaColaMinutos` en `IngestaOptions`. 005: `Ingesta:AntiguedadMaximaColaMinutos` | Usar un solo nombre (`Ingesta:…`) | No |
| PO-27 | Baja | `005/data-model.md:69-72` | Glosario (`10:23`): **Lote** = `LoteRecibido`, el grupo de hasta 50 eventos | `LoteEntrante` es el grupo; `LoteRecibido` es solo el `LoteId` que va a la cola | Renombrar (p. ej. `AvisoLote`) o ajustar el glosario | No |
| PO-28 | Baja | `anomalias.openapi.yaml:145` · `006/data-model.md:46` · `02-historias-de-usuario.md:114` | Glosario (`10:38-39`): `EstadoPersistencia.Persistente/Episodica`. HU-06 escribe `PERSISTENTE` / `EPISÓDICA` | Los contratos llaman al esquema `Persistencia` (los valores coinciden) | Llamar `EstadoPersistencia` al esquema; en docs, unificar HU-06 con el glosario | No |
| PO-29 | Baja | `004` y `005` (`PurgaSeudonimoHoras`) · `004` y `007` (`DesfaseHorarioHoras`) · `003` `SesionConductorDias` ↔ app `DuracionPermisoLocalDias` | Cada umbral de 09 tiene un solo valor | Hay parámetros duplicados entre módulos (hoy con valores iguales) | Agregar una prueba de configuración que compruebe que los duplicados son iguales, o leerlos de una sola sección | No |
| PO-30 | Media | `003/research.md:59-61` | HU-25 y RF-32: no revelar qué correos están registrados. RN-19 lo pide en el login | El registro responde 409 `correo_en_uso`, que confirma qué correos existen | Decidir: aceptar el riesgo (con el límite de tasa) o responder siempre «revisa tu correo» | **Sí** |
| PO-31 | Baja | `005/research.md:21-25` | `01-actores.md` §4: Administrador «—» en «Sincronizar eventos» | El 403 al administrador aparece como «pendiente: docs no lo dice» | Cerrar el pendiente citando 01 §4: docs ya lo responden | No |
| PO-32 | Baja | `007/research.md:35-39` · `prototipo:1321` | El prototipo dice «Eventos hoy» | La spec muestra el último día cerrado (ayer) | Cambiar la etiqueta a «Eventos ayer» o calcular el día en vivo | **Sí** |

**Glosario (punto 4).** Coinciden los estados (`Activa`, `PendienteConfirmacion`, `Expirada`, `Reparada`, `Descartada`), `Episodica`/`Persistente`, los 7 `MotivoDescarte` de RN-11 (005 expone 6 en la recepción, lo cual es correcto), los roles `Anonimo`, `ConductorRegistrado` y `Administrador`, `corredor_via`, `PoligonoDemo`, `evento_recibido`, `con_sesion` y `SeudonimoInstalacion`. Las únicas diferencias son de nombre: PO-27 y PO-28. Auditoría agrega el actor «Sistema», que no es un rol: es aceptable, pero conviene anotarlo en el glosario. El prototipo muestra «Pendiente de confirmar» como etiqueta de pantalla para `PendienteConfirmacion`; está bien mientras se use solo como texto.

**Prototipo frente a contratos (punto 5).** Están cubiertos: el login del panel (423 con `bloqueadoHastaUtc`, `sesionVenceUtc`), los 6 indicadores, el gráfico de eventos y el enlace a los paneles técnicos (007), «Requiere atención» (004 pendientes y por revisar, 003 revisión mensual), el detalle de una anomalía (004), las acciones de moderación con la tabla RN-07 + `MantenerDescartada`, Usuarios (filtros, invitar, cambiar rol, reenviar, desactivar con motivo, activar, `ultimaSesionUtc`, revisión mensual), Auditoría (categorías, solo lectura, correo del actor vía `?ids=`) y Exportaciones (las recientes salen de la auditoría). En la app: Términos (`/legal/versiones`), Crear cuenta, Revisa tu correo (`/me/reenviar-verificacion`), Login, Olvidé mi contraseña, Mi perfil (`/me`), Cerrar sesión, Eliminar cuenta, Mapa y «Actualizado hace…» (`generadoUtc`).
**Lo que el prototipo muestra y ningún contrato entrega:** PO-09 (km), PO-10 (días distintos en la app), PO-11 (columnas), PO-12 (filas exportadas), PO-19 (severidad en «Por revisar» y motivo del cambio), PO-20 (actor por correo), PO-21 (disponibilidad), PO-32 («hoy»). Además, el tamaño del mapa (38 MB) y la versión del modelo (v1.0.3) vienen de manifiestos en R2 que nadie produce (PO-17).
**Lo que los contratos entregan y el prototipo no muestra:** `InvitacionPendiente` como estado de usuario (el prototipo solo filtra Activa/Desactivada), la paginación de usuarios y auditoría, `porRevisar`/`vehiculosRevision` en el detalle, `eventosPeriodoAnterior` por día (sí aparece como línea punteada) y el 403 `solo_administradores` / `antibot_fallido` del login.

---

## 3. Lista unificada de «Pendientes de confirmar por el autor» (001…007)

La feature 001 no tiene pendientes. Total: 24 entradas en los `research.md`; 23 temas distintos, porque 004 R-012 y 006 R-001 son el mismo. Columna **¿Hueco real de docs?**: **Sí** = docs no lo dicen o se contradicen y hay que corregir `referencia/docs`; **No** = es una decisión técnica o docs ya lo responden; **Prototipo** = viene del prototipo y docs no lo recogen.

### A. Cómo se pasan los datos entre módulos (diseño interno)

| # | Pendiente, en palabras sencillas | Origen | ¿Hueco real de docs? |
|---|---|---|:---:|
| 1 | Anomalías necesita saber **a qué hora llegó** cada evento, no solo cuándo se detectó. Sin ese dato, un procesamiento atrasado descartaría eventos buenos y adelantaría la purga del seudónimo. Hay que agregar `RecibidoUtc` al contrato `ContextoReporte` | 004 R-001 | **Sí** (componentes §3.1, Nivel3 §2.2) |
| 2 | El mapa necesita la lista de **celdas que toca la carretera**; si no, revisaría ~150 000 celdas vacías del cerro. Hay que agregar la operación `CeldasDelCorredor` a la lectura de Anomalías | 004 R-012 = 006 R-001 | **Sí** |
| 3 | Guardar **quién** hizo cada cambio de estado (para el historial del panel) y el **km sobre la vía** de cada reporte (para calcular la cobertura) | 004 R-006, R-011 | **Sí** (lo piden el prototipo y RF-18; las tablas de docs no lo tienen) |
| 4 | Docs dicen que el «tamaño de celda» es configurable en el módulo Mapa; en realidad debe ser fijo y compartido, o se invalida la celda equivocada. Hay que quitarlo de enfoque §5 | 006 R-002 | **Sí** (error de docs) |
| 5 | Un índice extra en la tabla de recepción para que el barredor encuentre rápido los lotes pendientes | 005 (Complexity Tracking) | No (técnico) |

### B. Reglas de negocio

| # | Pendiente | Origen | ¿Hueco real? |
|---|---|---|:---:|
| 6 | Agregar la acción **«Mantener descartada»** a la tabla de acciones del administrador. El texto de RN-07 ya la menciona y el prototipo tiene el botón | 004 R-005 | **Sí** (falta una fila en RN-07) |
| 7 | Cuando el administrador **reabre** una anomalía vieja, ¿desde cuándo cuenta su vencimiento? Se propone desde la reapertura; si no, la tarea horaria la expiraría al instante | 004 R-007 | **Sí** |
| 8 | ¿Qué estados ve el administrador en el mapa si no filtra? La spec dice Activa + Pendiente; RN-08 y el prototipo dicen todos (ver PO-07) | 006 R-004 | **Sí** (la spec choca con docs) |
| 9 | Límites exactos del **tramo demo**: km 268 (Vinchos) a 333 (Ayacucho), con 200 m de margen a cada lado | 006 R-010 | Parcial: el km sale del prototipo y es coherente con «~65 km»; el margen no está en docs |
| 10 | Cómo se compara cada indicador del Resumen (promedio de los 7 días previos; tasas como suma de los 7 días) | 007 R-003 | **Sí** (RF-18 define los indicadores, no la comparación) |
| 11 | Cuántos días atrás puede recalcular el Dashboard si la tarea falló (2, por la purga de 72 h). Un día recuperado tarde cuenta «conductores activos» del momento del cálculo | 007 R-002 | No (técnico) |
| 12 | El prototipo dice «Eventos **hoy**», pero el cálculo es diario y cerrado (ayer) | 007 R-005 | Prototipo |

### C. Contrato con la app

| # | Pendiente | Origen | ¿Hueco real? |
|---|---|---|:---:|
| 13 | Un evento **sin `eventoId`** hace fallar todo el lote con 400, y un cuerpo de más de 256 KB recibe 413. Docs dicen que un evento malo nunca tumba el lote (ver PO-03) | 005 R-002 | **Sí** (choca con RF-11; hay que decidir y actualizar `03`) |
| 14 | El administrador **no** puede enviar lotes (403) | 005 R-004 | No: docs ya lo dicen (`01-actores.md` §4, Administrador «—») |
| 15 | La app **sin cuenta** baja siempre el mapa demo completo, sin `bbox` ni `updatedSince`, para aprovechar la caché de Cloudflare (ver PO-16) | 006 R-007 | **Sí** (RF-26 dice incremental para todos) |

### D. Cuentas y seguridad

| # | Pendiente | Origen | ¿Hueco real? |
|---|---|---|:---:|
| 16 | Para devolver «el mismo par de tokens» si el celular reintenta en 60 s, sin guardar el token, cada token se **deriva** del anterior con una clave secreta nueva | 003 R-003 | **Sí** (ADR-009 y diseño-interno §6.7 se contradicen) |
| 17 | Solo el **panel** muestra «cuenta bloqueada hasta las 10:47» (423); la app nunca lo dice | 003 R-005 | Parcial: RN-19 solo habla del conductor; para el administrador manda el prototipo |
| 18 | Los enlaces de correo llevan el token después de `#` (no queda en los logs) y abren páginas de `panel.killauru.pe/cuenta/…`, también para conductores | 003 R-006 | No (diseño; docs no lo definen) |
| 19 | El anti-bot del login del panel es **Cloudflare Turnstile**; la app no lo usa | 003 R-008 | No (ADR-009 pide «anti-bot» sin nombrar herramienta) |
| 20 | «Crear usuario» = **invitar**: el usuario recibe un correo de 24 h para fijar su contraseña y queda en «Invitación pendiente» | 003 R-009 | **Sí** / Prototipo (RF-19 dice «crear») |
| 21 | Registrarse con un correo ya usado responde «correo en uso» (409), lo que revela qué correos existen (ver PO-30) | 003 R-010 | **Sí** (docs no lo dicen; choca con el espíritu de HU-25) |
| 22 | Límites de recuperación (3 por hora por correo, 10 por hora por IP) y contraseña de 8 a 128 caracteres. El prototipo dice mínimo 10 (ver PO-04) | 003 lista final, punto 7 | **Sí** (RF-32 pide límites sin valores; docs no fijan la política de contraseñas) |

### E. Auditoría y recuperación ante desastres

| # | Pendiente | Origen | ¿Hueco real? |
|---|---|---|:---:|
| 23 | Si se pierde la base de datos, también se pierde la lista de cuentas eliminadas después del último respaldo. Se propone guardar cada eliminación como log en Grafana Cloud (solo el id) para repetirla tras restaurar | 002 R-009 | **Sí** (RN-10 asume que la auditoría sobrevive) |

**Huecos reales de docs que hay que corregir en `referencia/docs`:** los temas 1, 2, 3, 4, 6, 7, 8, 10, 13, 15, 16, 20, 21, 22 y 23 (15 temas). Los temas 9 y 17 son parciales. Los temas 5, 11, 18 y 19 son decisiones técnicas. El tema 14 ya está resuelto por docs. El tema 12 sale del prototipo.
