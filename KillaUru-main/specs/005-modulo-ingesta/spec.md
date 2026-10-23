# Feature Specification: Módulo Ingesta

**Feature Branch**: `005-modulo-ingesta`

**Created**: 2026-10-21

**Status**: Draft

**Input**: User description: "Módulo Ingesta de KillaUru: recibir lotes de hasta 50 eventos, decir qué pasó con cada uno, guardar los aceptados de forma durable e idempotente antes de responder 200, procesarlos en segundo plano con una cola acotada y trabajadores, recuperar lo pendiente tras un reinicio y purgar el seudónimo y la copia de recepción."

---

## 0. Ficha del módulo

| Campo | Valor |
|---|---|
| Qué es | La **puerta de entrada** de los datos de la app: recepción durable e idempotente de lotes (ADR-005), cola de trabajo y trabajadores que entregan cada evento a Anomalías |
| Qué **no** es | No decide si un evento es bueno (las reglas son de Anomalías, 004: Ingesta solo las **llama**); no crea anomalías; no abre transacciones ni bloqueos sobre tablas de Anomalías |
| Componente C3 | Ingesta (recepción durable · cola · trabajadores) |
| Esquema de BD | `ingesta`: `evento_recibido` |
| Depende de | `Shared`; Anomalías (004) por `IAnomaliasService` |
| Lo usan | Nadie por contrato. Aporta su comprobación de la cola a `/health/ready` (001) |
| Pantallas del prototipo | App → indicador de estado y contador de pendientes (lo que responde el lote); Panel → Resumen («Eventos ayer» lo cuenta Dashboard con los reportes procesados, no Ingesta; D13, D14) |
| Prioridad | *Must*: HU-04, HU-05 (recepción), HU-21 (purga en la copia de recepción) |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historias | HU-04, HU-05, HU-21 | `02-historias-de-usuario.md` |
| Actores | Quién sincroniza eventos (el administrador no) | `01-actores.md` §4 |
| Requisitos funcionales | RF-11, RF-27 (purga en recepción), RF-28 (cola en `/health/ready`) · contrato del lote | `03-requisitos-funcionales.md` |
| Atributos de calidad | RNF-02, RNF-03, RNF-06, RNF-11 (cuota de lotes), RNF-22, RNF-23 (retraso de cola), RNF-25 | `04-atributos-de-calidad.md` |
| Reglas de negocio | RN-09, RN-10, RN-11 (recepción vs procesamiento), RN-14 (`con_sesion`), RN-15a (conteo por hora) | `09-reglas-de-negocio.md` |
| Decisiones | ADR-005, ADR-014 (payload `jsonb`), ADR-015 (antifraude) | `07-decisiones-arquitectonicas.md` |
| Escenarios | EQ-02, EQ-06, EQ-09 | `08-escenarios-y-atributos-de-calidad.md` |
| Diseño | `RecibirLoteEventosUseCase`, algoritmo del procesador, `IngestaOptions`, `IngestaModule` | `enfoque-arquitectonico.md` §4.1, §5, §6, §7 |
| Diseño | Tabla `evento_recibido`, recepción con `unnest`, cola y trabajadores, `SyncController`, purgas | `diseño-interno-de-modulos.md` §3.2, §3.5, §3.8, §5, §6.4, §6.6 |
| Decisiones del autor (2026-10-21) | D2 (un lote = un seudónimo), D3 (`loteId` del servidor y purga de 72 h), D4 (tipos del lote en Application), D5 (evento sin `eventoId`), D8 (`con_sesion` con correo verificado), D11 (retraso de cola y carga) · correcciones T-BD06, T-BD-CHK, T-OPS05, T-ARQ08 | `specs/revision/decisiones.md`, `specs/revision/plan-de-cambios.md` |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Enviar un lote y saber qué pasó con cada evento (Priority: P1)

Como conductor (con o sin cuenta), quiero que la app envíe mis detecciones en lotes y reciba el resultado de **cada** evento, para que ninguno se quede en el limbo ni un evento malo arruine el lote (HU-05, RF-11, RN-11).

**Why this priority**: Sin recepción no entra ningún dato al sistema.

**Independent Test**: Enviar lotes con eventos buenos, malos y repetidos; comprobar la respuesta por evento y las filas en `evento_recibido`.

**Acceptance Scenarios**:

1. **Given** un lote de 1 a 50 eventos, **When** lo envío, **Then** recibo 200 con `loteId` (generado por el servidor), `aceptados[]`, `duplicados[]` y `rechazados[{id, indice, motivo}]`, y cada evento del lote aparece en exactamente una de las tres listas (RF-11; D3, D5).
2. **Given** un lote de 50 con 1 evento al que le falta la severidad, **When** lo envío, **Then** recibo 200 con 49 aceptados y 1 rechazado `EsquemaInvalido`; el lote no falla (`diseño-interno` §10).
3. **Given** eventos que no cumplen las reglas de **recepción** (confidence, precisión GPS, antigüedad respecto de la hora de recepción, tipo, distancia a la vía), **When** llegan, **Then** salen en `rechazados` con el motivo que devuelve `IAnomaliasService.EvaluarIntrinseca` y **no** se guardan (RN-11).
4. **Given** un lote ya enviado, **When** la app lo reenvía con los mismos `eventoId`, **Then** esos eventos salen en `duplicados` y no se reinsertan (EQ-02).
5. **Given** un cuerpo que no es JSON, sin arreglo `eventos`, un lote vacío o más de 50 eventos, **When** lo envío, **Then** recibo 400 `lote_invalido` y no se guarda nada (RF-11; D5; `research.md` R-002).
6. **Given** un lote enviado con un token válido de conductor registrado con el *claim* `verificado = true`, **When** se guarda, **Then** cada evento queda con `con_sesion = true`; sin token, o con un conductor registrado que aún no verificó su correo, con `con_sesion = false`. Ningún campo que mande la app cambia esta marca (RN-14; D8).
7. **Given** un token inválido o vencido, **When** envío el lote, **Then** recibo 401 y no se guarda nada; la app renueva y reenvía el mismo lote (contrato del lote).
8. **Given** un lote de 50 en el que el evento de la posición 7 (0-based) no trae `eventoId` UUID, **When** lo envío, **Then** recibo 200: los otros 49 se evalúan como siempre y ese evento sale en `rechazados` como `{ id: null, indice: 7, motivo: EsquemaInvalido }`; la app marca `REJECTED_LOCAL` el evento de esa posición (RF-11; D5).
9. **Given** un lote con la cabecera `X-KillaUru-Seudonimo: S`, **When** un evento trae otro seudónimo válido T, **Then** ese evento sale en `rechazados` con `SeudonimoInconsistente` y no se guarda; los eventos de S siguen su curso (RN-11; D2).
10. **Given** un lote **sin** la cabecera, **When** se recibe, **Then** el seudónimo del lote es el del **primer** evento con seudónimo válido y los eventos con otro seudónimo salen `SeudonimoInconsistente` (D2).
11. **Given** un cuerpo que trae `loteId`, **When** se recibe, **Then** ese valor se ignora y la respuesta devuelve el `loteId` (UUIDv7) que generó el servidor (D3).

---

### User Story 2 - Ningún evento aceptado se pierde (Priority: P1)

Como conductor, quiero que todo evento que el servidor me confirmó llegue a procesarse aunque el servidor se reinicie, para que mis datos nunca se pierdan (HU-04, RNF-06, EQ-09).

**Why this priority**: *Must*; la app borra lo que el servidor confirmó, así que una pérdida es definitiva.

**Independent Test**: Aceptar lotes, matar el proceso antes y durante el procesamiento, arrancar de nuevo y conciliar.

**Acceptance Scenarios**:

1. **Given** un lote aceptado, **When** se responde 200, **Then** sus eventos ya están confirmados (*commit*) en `evento_recibido` (ADR-005 punto 2).
2. **Given** eventos `Recibido` cuando el proceso se cae, **When** arranca otra vez, **Then** el barredor los encuentra y los procesa en ≤ 2 min (RNF-06).
3. **Given** un aviso de lote que no entró en la cola (llena o perdida), **When** pasa 1 min, **Then** el barredor lo recupera (EQ-09).
4. **Given** un evento que Anomalías ya procesó pero Ingesta no alcanzó a marcar, **When** se reprocesa, **Then** Anomalías responde `YaProcesado` y Ingesta lo marca `Procesado` sin duplicar nada (ADR-005 punto 5).
5. **Given** cualquier momento, **When** se concilia, **Then** aceptados = procesados + descartados + pendientes (RNF-06).

---

### User Story 3 - Procesar en segundo plano sin saturarse (Priority: P1)

Como sistema, necesito entregar cada evento aceptado a Anomalías con el contexto que solo Ingesta conoce, en paralelo y con un límite, para absorber el pico de 300 dispositivos sin perder datos (RNF-03, RNF-23, DA-05).

**Why this priority**: Une la recepción con el corazón del negocio (004).

**Independent Test**: Con dobles de `IAnomaliasService`, comprobar el contexto armado, el orden de entrega y el marcado; con la cola llena, comprobar el 503.

**Acceptance Scenarios**:

1. **Given** un lote aceptado, **When** un trabajador lo toma, **Then** entrega sus eventos `Recibido` a `ProcesarReporteAsync` **en orden de hora de detección**, cada uno con `ContextoReporte(ConSesion, EventosEnLaHora, RecibidoUtc)` (004, R-001).
2. **Given** un evento del seudónimo S detectado a las 14:37, **When** se arma su contexto, **Then** `EventosEnLaHora` = eventos recibidos de S con detección entre 14:00 y 15:00 (RN-15a).
3. **Given** el resultado `Procesado`, `YaProcesado` o `Descartado(motivo)`, **When** vuelve, **Then** la fila pasa a `Procesado` o `Descartado` con su motivo, en una transacción propia de Ingesta (ADR-005 punto 4).
4. **Given** que `ProcesarReporteAsync` lanza un error, **When** ocurre, **Then** la fila sigue `Recibido` y el barredor la reintenta; el trabajador sigue con el siguiente evento.
5. **Given** la cola llena, **When** llega un lote, **Then** recibo 503 `saturado` con `Retry-After` y no se guarda nada (DA-05).
6. **Given** el pico de RNF-03, **When** se mide, **Then** el retraso de la cola es ≤ 30 s p95, medido con el histograma `killauru.ingesta.retraso_cola_s` (procesado − recibido) que se registra al marcar cada fila (RNF-23; D11).

---

### User Story 4 - Privacidad y retención de la copia de recepción (Priority: P1)

Como conductor, quiero que mi seudónimo también se borre de la copia de recepción a las 72 h y que esa copia dure como máximo 30 días, para que nada me vincule con mis eventos (HU-21, RN-09, RN-10, RNF-25).

**Why this priority**: *Must* y obligación legal (R-06).

**Independent Test**: Sembrar filas con distintas horas de recepción y estados; correr las purgas con `FakeTimeProvider`.

**Acceptance Scenarios**:

1. **Given** filas recibidas hace más de 72 h, **When** corre la purga horaria (minuto :10 y al arrancar), **Then** en la **misma sentencia** su `seudonimo` y su `lote_id` quedan en `NULL` y su `recibido_utc` se redondea hacia abajo a la hora; las más nuevas no cambian (RN-09; D3, T-OPS05).
2. **Given** el `payload` guardado, **When** se revisa, **Then** nunca contiene el seudónimo (vive solo en su columna) (`diseño-interno` §3.1).
3. **Given** filas `Procesado` o `Descartado` recibidas hace más de 30 días, **When** corre la purga diaria (03:00 hora local y al arrancar), **Then** se borran por lotes de 5 000 (RN-10; T-OPS05).
4. **Given** el esquema `ingesta`, **When** se audita, **Then** no hay cuentas, correos ni tokens: solo `con_sesion` sí/no (RNF-10).

---

### User Story 5 - Ver la salud de la cola (Priority: P2)

Como operador, quiero saber si la cola se atascó, para enterarme antes de que los conductores lo noten (RF-28, RNF-22).

**Why this priority**: Alimenta `/health/ready` (UptimeRobot) y los paneles de Grafana.

**Independent Test**: Dejar una fila `Recibido` con 6 min de antigüedad y consultar `/health/ready`.

**Acceptance Scenarios**:

1. **Given** una fila `Recibido` con más de 5 min, **When** se consulta `/health/ready`, **Then** responde 503 (RF-28).
2. **Given** la API en marcha, **When** se miran las métricas, **Then** hay lotes por resultado, rechazos por motivo, tamaño y antigüedad de la cola, retraso de cola y la última ejecución correcta de cada purga (RNF-22, RNF-23).

### Edge Cases

- El mismo `eventoId` dos veces **dentro** de un lote: el primero se evalúa; el segundo sale en `duplicados` (R-003).
- Un `eventoId` que en un envío anterior salió **rechazado** y vuelve: se evalúa otra vez (los rechazados nunca se guardan); normalmente sale rechazado de nuevo.
- Un evento con el seudónimo vacío o inválido: `EsquemaInvalido` (no hay confirmación ni antifraude sin seudónimo); no fija el seudónimo del lote.
- Un evento sin `eventoId` **y** con otro seudónimo: `{ id: null, indice, EsquemaInvalido }` (la falta de id se evalúa primero; R-010).
- Un lote sin cabecera y sin ningún seudónimo válido: no hay seudónimo del lote; todos salen `EsquemaInvalido` por las reglas de recepción.
- La app rota su seudónimo con eventos pendientes: parte el lote para que cada lote lleve un solo seudónimo (nota para 008; D2).
- Un lote que llega mientras la cola está a punto de llenarse: se guarda y el aviso puede no entrar; el barredor lo recupera.
- Un lote de un administrador (no detecta): 403 `sin_permiso` (R-004; `01-actores.md` §4).
- Cuerpo de más de 256 KB: 413 antes de leerlo (R-002).
- Un evento que se queda `Recibido` más de 30 días: no se borra; `/health/ready` ya estaba en 503 y la alerta de pendiente > 72 h ya avisó. Choca con RNF-25 (máx. 30 días): riesgo anotado (R-008, SEG-05).
- Una fila `Recibido` con más de 72 h pierde su `lote_id` en la purga: el barredor la recupera como «lote sin id» (R-011).
- Reinicio con miles de pendientes: el barredor encola por lotes de 100 `lote_id` y respeta la capacidad de la cola.

## Requirements *(mandatory)*

### Functional Requirements

**A. Recepción (RF-11, RN-11, RN-14; ADR-005 puntos 1–2)**

- **FR-001**: El sistema MUST exponer `POST /api/v1/anomalies/batch` para anónimos y conductores registrados, con la cuota de lotes de RNF-11 (política `lotes` de 001); el techo por IP (`LimiteTasa:TechoIpLotesPorHora`) lo aplica el *host* (001), no Ingesta. El administrador recibe 403 `sin_permiso`. *(RF-11, RNF-11, `01-actores.md` §4; D2)*
- **FR-002**: El sistema MUST leer cada evento del lote **por separado**, de modo que un valor de tipo o formato inválido en un evento solo produzca el rechazo de ese evento (`EsquemaInvalido`). *(RF-11; R-001)*
- **FR-003**: El sistema MUST responder 400 `lote_invalido` solo si el cuerpo no se puede leer, no trae el arreglo `eventos`, el arreglo está vacío o trae más de `MaxEventosPorLote` eventos. *(RF-11; D5; R-002)*
- **FR-004**: El sistema MUST evaluar cada evento con `IAnomaliasService.EvaluarIntrinseca(evento, recibidoUtc)`, donde `recibidoUtc` es una sola hora de recepción para todo el lote. *(RN-11)*
- **FR-005**: El sistema MUST guardar los aceptados con **un solo** `INSERT … ON CONFLICT (evento_id) DO NOTHING` y responder 200 **después** del *commit*. *(ADR-005, RNF-02)*
- **FR-006**: El sistema MUST clasificar cada evento del lote en exactamente una lista: `aceptados` (insertados ahora), `duplicados` (ya existían o repetidos en el lote) o `rechazados`, cuyo elemento es `{ id: uuid | null, indice, motivo }` con `indice` = posición 0-based en el arreglo `eventos`, siempre presente. *(RF-11; D5)*
- **FR-007**: El sistema MUST marcar `con_sesion = true` solo si el token es de rol `ConductorRegistrado` **y** trae el *claim* `verificado = true`; en otro caso (anónimo o correo sin verificar), `false`. Nunca lee una marca del cuerpo. *(RN-14; D8)*
- **FR-008**: El sistema MUST guardar el `payload` del evento **sin** el seudónimo, que va solo en su columna. *(RN-09, ADR-014)*
- **FR-009**: El sistema MUST responder 503 `saturado` con `Retry-After` si la cola no tiene capacidad, **antes** de guardar. *(DA-05)*
- **FR-010**: El sistema MUST registrar como métrica cada lote (aceptados, duplicados, rechazados) y cada rechazo por motivo (incluido `SeudonimoInconsistente`), nunca en la auditoría. *(RF-15, RNF-22)*
- **FR-023**: Un evento sin `eventoId` UUID MUST NOT hacer fallar el lote: sale en `rechazados` con `id: null`, su `indice` y `EsquemaInvalido`, y no se guarda. *(RF-11; D5)*
- **FR-024**: El sistema MUST aplicar «un lote = un seudónimo» en Application de Ingesta: el seudónimo del lote es el de la cabecera `X-KillaUru-Seudonimo` si viene con un UUID válido; si no, el del primer evento con seudónimo válido. Todo evento con otro seudónimo válido sale en `rechazados` con `SeudonimoInconsistente` (código 8 de `MotivoDescarte`) y no se guarda. *(RN-11, ADR-015; D2; R-010)*
- **FR-025**: El servidor MUST generar el `loteId` (UUIDv7) de cada lote y devolverlo en la respuesta; el cuerpo no lo trae y, si lo trae, se ignora. *(RNF-10; D3; R-011)*

**B. Cola y procesamiento (ADR-005 puntos 3–5)**

- **FR-011**: Tras guardar, el sistema MUST avisar del lote en una cola en memoria acotada a `CapacidadCola` lotes; si el aviso no entra, el lote queda para el barredor. *(ADR-005 punto 3)*
- **FR-012**: `Trabajadores` consumidores MUST procesar lotes en paralelo; dentro de un lote, los eventos `Recibido` se entregan en orden de `detectado_utc`. *(`diseño-interno` §5; R-005)*
- **FR-013**: Para cada evento, el sistema MUST armar `ContextoReporte(ConSesion, EventosEnLaHora, RecibidoUtc)` con sus propios datos y llamar a `IAnomaliasService.ProcesarReporteAsync`. *(RN-15a; 004 R-001)*
- **FR-014**: Solo después de una respuesta correcta, el sistema MUST marcar la fila `Procesado` (también ante `YaProcesado`) o `Descartado` con su motivo, en una transacción propia sobre `evento_recibido`. *(ADR-005 punto 4)*
- **FR-015**: Ingesta MUST NOT abrir transacciones ni tomar bloqueos sobre tablas de Anomalías. *(ADR-005 punto 4)*
- **FR-016**: Un barredor MUST correr al arrancar y cada `IntervaloBarredorSegundos`, y encolar los lotes con filas `Recibido` de más de `AntiguedadMinimaBarredorSegundos` (al arrancar, todas), incluidas las filas sin `lote_id`. *(EQ-09, RNF-06; R-011)*
- **FR-026**: Al marcar una fila `Procesado` o `Descartado`, el sistema MUST registrar `IMetricas.RegistrarRetrasoCola(procesado − recibido_utc)` (histograma `killauru.ingesta.retraso_cola_s`). *(RNF-23; D11)*

**C. Privacidad y retención (RN-09, RN-10, RNF-25)**

- **FR-017**: Una tarea `purga-seudonimos-recepcion` MUST correr al arrancar y cada hora en el minuto :10 (`ProgramaReloj`), y en **una sola sentencia**, por lotes de 5 000, poner `seudonimo = NULL, lote_id = NULL, recibido_utc = date_trunc('hour', recibido_utc)` en las filas con seudónimo recibidas hace más de `PurgaSeudonimoHoras` (índice `ix_recibido_purga`). *(RN-09, RF-27; D3, T-OPS05, T-BD06)*
- **FR-018**: Una tarea `purga-recepcion` MUST correr al arrancar y cada día a las 03:00 hora local (`ProgramaReloj`), y borrar, por lotes de 5 000, las filas `Procesado` o `Descartado` recibidas hace más de `RetencionRecepcionDias`. *(RN-10, RNF-25; T-OPS05; R-008)*
- **FR-019**: El esquema `ingesta` MUST NOT guardar cuentas, correos ni tokens. *(RNF-10)*
- **FR-020**: Cada tarea MUST registrar su última ejecución correcta como métrica con su nombre (`purga-seudonimos-recepcion`, `purga-recepcion`). *(RNF-22; T-OPS05)*

**D. Salud y métricas (RF-28, RNF-22)**

- **FR-021**: El módulo MUST aportar a `/health/ready` una comprobación que falle si existe una fila `Recibido` con más de `AntiguedadMaximaColaMinutos`. *(RF-28; 001 FR-019)*
- **FR-022**: El módulo MUST publicar el tamaño de la cola y la antigüedad del pendiente más viejo con `IMetricas.ObservarCola`. *(RNF-22)*

### Reglas y parámetros (`IngestaOptions`, sección `Ingesta`)

| Regla | Parámetro | Valor | Fuente |
|---|---|---|---|
| RF-11 | `MaxEventosPorLote` | 50 | `enfoque` §5 |
| DA-05 | `CapacidadCola` | 2 000 lotes | `diseño-interno` §5 |
| RNF-23 | `Trabajadores` | 0 = núcleos de la VM | `diseño-interno` §5 |
| DA-05 | `ReintentarEnSegundos` (`Retry-After` del 503) | 30 | SDD (R-006) |
| EQ-09 | `IntervaloBarredorSegundos` / `AntiguedadMinimaBarredorSegundos` | 60 / 60 | `enfoque` §6 |
| RF-28 | `AntiguedadMaximaColaMinutos` | 5 | 001 (tabla de parámetros) |
| RN-09 | `PurgaSeudonimoHoras` | 72 | `09-reglas-de-negocio.md` (mismo valor que en Anomalías) |
| RN-10 | `RetencionRecepcionDias` | 30 | `enfoque` §5 |
| — | `TamanoMaximoCuerpoKb` | 256 | `diseño-interno` §6.6 |

Horario de las tareas (T-OPS05, calendario común de 001): `purga-seudonimos-recepcion` cada hora en el minuto :10; `purga-recepcion` a las 03:00 hora local; las dos también al arrancar.

### Key Entities

- **Lote entrante** (Application): hasta 50 eventos crudos, `con_sesion` y el seudónimo de la cabecera si vino; el `loteId` lo genera el servidor (D3, D4).
- **Seudónimo del lote** (Application): el de la cabecera o el del primer evento con seudónimo válido (D2).
- **Evento recibido** (`evento_recibido`): evento aceptado, único por `eventoId`, con lote (hasta 72 h), hora de recepción (redondeada a la hora a las 72 h), hora de detección, seudónimo (hasta 72 h), `con_sesion`, estado (`Recibido`, `Procesado`, `Descartado`), motivo y `payload`.
- **Resultado del lote** (Application): `Aceptado(aceptados, duplicados, rechazados{id?, indice, motivo})` · `Invalido` · `Saturado(reintentarEn)`.
- **Aviso de lote** (`LoteRecibido`): solo el `loteId` (o «sin id» para las filas purgadas); la cola nunca lleva eventos.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Recibir, guardar y responder un lote de 50 eventos toma < 300 ms p95 (RNF-02).
- **SC-002**: En la prueba combinada de 001 (`tests/carga/carga-combinada.k6.js`: 300 sincronizando + 300 consultando el mapa, 10 min): tasa de error < 1 % y retraso de cola ≤ 30 s p95 según `killauru.ingesta.retraso_cola_s` (RNF-03, RNF-23; D11).
- **SC-003**: 0 eventos perdidos y 0 duplicados en la prueba de reinicio; los pendientes se reanudan en ≤ 2 min (RNF-06, EQ-09).
- **SC-004**: 100 % de los eventos de un lote legible aparecen en una sola lista de la respuesta (por `eventoId` o, en `rechazados`, por `indice`).
- **SC-005**: 0 filas de `evento_recibido` con más de 72 h que tengan seudónimo, `lote_id` o `recibido_utc` sin redondear; 0 filas finales con más de 30 días (RNF-25; D3).

## Fuera de alcance

- Las reglas de calidad, confirmación y antifraude: Anomalías (004).
- La resolución del rol y del *claim* `verificado`, el 401, el límite de tasa y el techo por IP: middleware de 001 (D2, D8).
- Los scripts de carga, picos, estrés, resistencia y caos: los tiene 001 en `backend/tests/carga/`; Ingesta solo se mide con ellos (D11).
- La cola, el reintento y los estados locales en la app (RF-04, RF-06, RF-07): feature de la app.

## Assumptions

- La app genera el `eventoId` (UUID) al guardar el evento y lo reenvía igual en cada reintento (RF-03).
- La app no envía eventos con más de 168 h ni de categorías que no detectan (RF-06, RN-18); si llegaran, la recepción los rechaza igual.
- La app parte el lote cuando rota su seudónimo, de modo que cada lote lleva un solo seudónimo (D2; se detalla en 008-app-android).
