# Research: Módulo Ingesta

## R-001 · Leer cada evento por separado (`LectorEvento`)

- **Problema**: si el controlador deserializa el lote a `List<EventoDto>` con tipos (`int Severidad`), un solo `"severidad": "alta"` hace fallar **todo** el cuerpo y el lote termina en 400, contradiciendo RF-11 («un evento mal formado nunca rechaza el lote»).
- **Decision**: `LoteDto(JsonElement[] Eventos)` en Adapters (sin `loteId`, D3). El controlador arma un `LoteEntrante` de **Application** con los `JsonElement` crudos, `con_sesion` y el seudónimo de la cabecera. `LectorEvento.Leer(JsonElement, indice)` (Application) lee cada propiedad con `TryGet…` y devuelve el `eventoId` (o `null`) y un `EventoReportado` **crudo** (004, R-002): lo que falta o tiene otro tipo queda `null`, y luego `EvaluarIntrinseca` responde `EsquemaInvalido`.
- **Rationale**: el resultado por evento es el corazón del contrato del lote. Mantiene un solo mapeo (Anomalías decide qué es inválido). El mapeo a `EventoReportado` (contrato de Anomalías) vive en Application, no en Adapters ni en Domain (D4; `enfoque` §3).
- **Alternatives considered**: convertidor JSON personalizado (más código y más difícil de probar); `[FromBody]` tipado (rompe RF-11); lector en Adapters (el controlador tocaría un contrato de otro módulo, D4).

## R-002 · Cuándo es 400, cuándo 413 y qué pasa con un evento sin `eventoId`

- **Decision** (D5): 400 `lote_invalido` **solo** si el cuerpo no es JSON, no trae el arreglo `eventos`, el arreglo está vacío o trae más de 50 (RF-11 tal cual). Un evento sin `eventoId` UUID **no** hace fallar el lote: sale en `rechazados` como `{ id: null, indice, motivo: EsquemaInvalido }`, donde `indice` es su posición 0-based en el arreglo `eventos`. Todo elemento de `rechazados` lleva `indice`, siempre. 413 si el cuerpo pasa de 256 KB (`RequestSizeLimit`, lo corta Kestrel antes de leer).
- **Rationale**: «un evento malo nunca tumba el lote» (RF-11). Con `indice` la app sabe qué evento marcar `REJECTED_LOCAL` aunque no tenga id. Un lote de 50 eventos pesa ~20 KB: 256 KB solo se supera con un cliente abusivo.
- **Queda para 008-app-android**: cómo trata la app el 413 (no lo decide D5).

## R-003 · Repetidos dentro del mismo lote

- **Decision**: `LoteAceptado.De` (Application) se queda con la **primera** aparición de cada `eventoId`; las siguientes van a `duplicados`. El `INSERT` nunca recibe dos veces la misma llave.
- **Rationale**: «cada evento en exactamente una lista» (SC-004) y un `INSERT` limpio.

## R-004 · Quién puede enviar lotes

- **Decision**: anónimos y `ConductorRegistrado`. Un `Administrador` recibe 403 `sin_permiso`. `con_sesion` se decide según R-013.
- **Rationale**: `01-actores.md` §4 da «—» al administrador en «Sincronizar eventos (`POST /api/v1/anomalies/batch`)»; RN-14 habla de «sesión iniciada» del conductor; el administrador no detecta (RN-18) y aceptar sus lotes daría una vía para confirmar baches graves sin conducir.

## R-005 · Orden de entrega y reparto entre trabajadores

- **Decision**: el elemento de la cola es el **lote**; cada trabajador procesa un lote completo, evento por evento, en orden de `detectado_utc` (y `evento_id` como desempate).
- **Rationale**: RN-15b compara un evento con el **anterior** del mismo seudónimo. Un lote lleva un solo seudónimo: ya no es un supuesto sino una **regla** de la recepción (R-010, D2). Procesarlo en orden evita descartar por velocidad un evento que llegó «antes» que su predecesor. Entre lotes distintos no importa el orden (bloqueo por celda en Anomalías).

## R-006 · Contrapresión

- **Decision**: `cola.HayCapacidad()` se mira **antes** del `INSERT`; si no hay → 503 `saturado` con `Retry-After: 30` (`ReintentarEnSegundos`). Si entre la comprobación y el aviso la cola se llena, `TryEscribir` falla en silencio y el barredor recupera el lote en ≤ 2 min.
- **Rationale**: nunca se responde 503 por un lote **ya guardado** (la app lo reenviaría y saldría como duplicado, lo cual es correcto pero gasta cuota).

## R-007 · Conteo de la hora de detección (RN-15a)

- **Decision**: `EventosEnLaHora = SELECT count(*) FROM ingesta.evento_recibido WHERE seudonimo = @s AND detectado_utc >= @h AND detectado_utc < @h + 1 h` (índice `ix_recibido_tope`), con `@h` = inicio de la hora UTC de la detección. Cuenta todos los recibidos de esa hora, incluido el propio evento.
- **Rationale**: RN-15a mide por **hora de detección** y no por llegada; la hora UTC y la de Lima comparten los límites de hora (desfase entero de 5 h). Con R-010 todos los eventos de un lote cuentan para el mismo seudónimo, así que un script no puede repartir sus eventos entre seudónimos inventados dentro de un lote.
- **Límite aceptado**: si un evento se procesa más de 72 h después de recibido (no debería: `/health/ready` alerta a los 5 min), su seudónimo ya está purgado y el conteo es 0.

## R-008 · Qué borra la purga de 30 días

- **Decision**: solo filas `Procesado` o `Descartado`. Una fila `Recibido` con más de 30 días no se borra (sería perder un evento aceptado, RNF-06); la alarma de `/health/ready` ya estaba activa desde los 5 min.
- **Rationale**: RNF-06 pesa más que RNF-25 en ese caso extremo, que además nunca debería ocurrir.
- **Riesgo anotado (SEG-05)**: choca con RNF-25 («como máximo 30 días»). Se mitiga con una **alerta** de Grafana cuando la antigüedad del pendiente más viejo (*gauge* de `ObservarCola`) pasa de 72 h —después de eso la fila ya no tiene seudónimo para RN-15a— y con el reproceso manual del runbook de 001. Si alguna vez una fila `Recibido` llega a 30 días, se registra como incidente.

## R-009 · Salud y métrica de la cola

- **Decision**: `ChequeoColaIngesta` consulta `min(recibido_utc) WHERE estado = 0` (índice `ix_recibido_pendiente`) con caché de 5 s, igual que `ChequeoBaseDatos` (001). La misma lectura alimenta el *gauge* de antigüedad de `IMetricas.ObservarCola`; el tamaño es `Channel.Reader.Count`.

## R-010 · Un lote = un seudónimo (D2)

- **Decision**: `SeudonimoDelLote` (Application de Ingesta, `Recepcion/`) fija el seudónimo del lote: el de la cabecera `X-KillaUru-Seudonimo` si viene con un UUID válido; si no, el del **primer** evento (por `indice`) con seudónimo válido. Orden por evento en `RecibirLoteEventosUseCase`: (1) sin `eventoId` UUID → `EsquemaInvalido` con `id: null`; (2) seudónimo válido distinto del del lote → `SeudonimoInconsistente` (código 8 de `MotivoDescarte`, 004 Contracts); (3) `EvaluarIntrinseca`. Un seudónimo vacío o inválido no fija el seudónimo del lote y lo rechaza la regla (3) con `EsquemaInvalido`. Prueba: `SeudonimoDelLoteTests`.
- **Rationale**: cierra SEG-01 (un script ya no reparte un lote entre seudónimos inventados para no pasar nunca del tope de RN-15a). Es una regla de **recepción** que solo Ingesta puede aplicar (conoce el lote y la cabecera); vive en Application porque el dominio de Ingesta se limita a `EstadoRecepcion` y `HoraDeDeteccion` (D4).
- **Fuera de Ingesta**: el techo por IP (`LimiteTasa:TechoIpLotesPorHora = 600`) es del *host* (001). La app parte el lote cuando rota su seudónimo (008).

## R-011 · `loteId` del servidor y purga de 72 h (D3)

- **Decision**: el `loteId` lo genera el servidor (UUIDv7) en `RecibirLoteEventosUseCase`; el cuerpo ya no lo trae (si llega, se ignora). `evento_recibido.lote_id` es `NULL`able. La purga horaria hace, **en la misma sentencia** que borra el seudónimo, `lote_id = NULL` y `recibido_utc = date_trunc('hour', recibido_utc)`, con el índice parcial `ix_recibido_purga` (T-BD06).
- **Rationale**: tras 72 h ni el `lote_id` ni la hora exacta de recepción pueden agrupar los eventos de un celular ni cruzarse con las horas de sesión de Identidad (BD-04, SEG-02).
- **Límite aceptado**: una fila `Recibido` que llegue a 72 h pierde su `lote_id`. El barredor la encola como aviso «sin id» (`LoteRecibido(null)`) y el trabajador carga las filas `Recibido` con `lote_id IS NULL` (mismo índice `ix_recibido_lote`).

## R-012 · Tipos del lote en Application (D4)

- **Decision**: `LoteEntrante`, `LoteAceptado`, `ResultadoLote` (con `Rechazo`), `LoteRecibido` y `EventoPendiente` viven en `Application/`. `Domain/` solo tiene `EstadoRecepcion` y `HoraDeDeteccion`, y no usa tipos de Anomalías. El controlador y los DTO solo usan Application.
- **Rationale**: `enfoque` §3 (Adapters solo usa Application) y la regla de la prueba de arquitectura de 001 (Adapters ↛ Domain; Domain ↛ `Contracts` de otro módulo).

## R-013 · `con_sesion` con correo verificado (D8)

- **Decision**: `con_sesion = (rol == ConductorRegistrado && verificado == true)`, con `verificado` = *claim* del token de acceso. Tras verificar, el *claim* cambia en la siguiente renovación (≤ 5 min).
- **Rationale**: una cuenta desechable sin verificar no basta para confirmar baches graves (RN-14, SEG-04).

## R-014 · Tareas por reloj (T-OPS05)

- **Decision**: `TareaPurgaSeudonimosRecepcion` corre al arrancar y cada hora en el minuto :10; `TareaPurgaRecepcion` al arrancar y a las 03:00 hora local. Las dos calculan el próximo instante con `Shared/Tareas/ProgramaReloj` (sobre `TimeProvider`) y registran `RegistrarTareaCorrecta` con los nombres `purga-seudonimos-recepcion` y `purga-recepcion`.
- **Rationale**: un `PeriodicTimer` de 24 h nunca dispara si se despliega más seguido (OPS-05); nombres propios para que el *gauge* de una tarea no tape al de Anomalías (OPS-07).

## R-015 · Retraso de cola y pruebas de carga (D11)

- **Decision**: al marcar una fila, `ProcesarLoteUseCase` llama a `IMetricas.RegistrarRetrasoCola(ahora − recibido_utc)` → histograma `killauru.ingesta.retraso_cola_s`. Ingesta no tiene script de carga propio: SC-001 y SC-002 se miden con los scripts de 001 (`backend/tests/carga/carga-combinada.k6.js`, `picos.k6.js`, `estres.k6.js`, `resistencia.k6.js`, `caos.md` + `caos.sh`), fuera de la VM y contra el entorno de pruebas (R-11).
- **Rationale**: un p95 no sale de un *gauge* (QA-06); un solo dueño de los scripts y del entorno evita perfiles partidos (QA-03).

## Confirmados por el autor el 2026-10-21

1. R-002: evento sin `eventoId` → rechazado con `id: null`, `indice` y `EsquemaInvalido`; 400 solo para cuerpo ilegible, sin `eventos`, vacío o > 50 (**D5**). El 413 por tamaño se mantiene.
2. R-004: 403 a lotes del administrador (docs ya lo dice: `01-actores.md` §4).
3. Complexity Tracking: índice `ix_recibido_lote` (se pasa a docs con **D1**).
4. R-005 / R-010: un lote = un seudónimo, `SeudonimoInconsistente` (**D2**).
5. R-011: `loteId` del servidor y purga de 72 h con `lote_id` y hora redondeada (**D3**).
6. R-012: tipos del lote en Application (**D4**).
7. R-013: `con_sesion` solo con correo verificado (**D8**).
8. R-015: métrica de retraso de cola y scripts de carga de 001 (**D11**).
