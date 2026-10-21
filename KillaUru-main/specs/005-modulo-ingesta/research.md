# Research: Módulo Ingesta

## R-001 · Leer cada evento por separado (`EventoDtoLector`)

- **Problema**: si el controlador deserializa el lote a `List<EventoDto>` con tipos (`int Severidad`), un solo `"severidad": "alta"` hace fallar **todo** el cuerpo y el lote termina en 400, contradiciendo RF-11 («un evento mal formado nunca rechaza el lote»).
- **Decision**: `LoteDto(Guid? LoteId, JsonElement[] Eventos)`. `EventoDtoLector.Leer(JsonElement)` lee cada propiedad con `TryGet…` y devuelve un `EventoReportado` **crudo** (004, R-002): lo que falta o tiene otro tipo queda `null`, y luego `EvaluarIntrinseca` responde `EsquemaInvalido`.
- **Rationale**: el resultado por evento es el corazón del contrato del lote. Mantiene un solo mapeo (Anomalías decide qué es inválido).
- **Alternatives considered**: convertidor JSON personalizado (más código y más difícil de probar); `[FromBody]` tipado (rompe RF-11).

## R-002 · Cuándo es 400 y cuándo 413

- **Decision**: 400 `lote_invalido` si el cuerpo no es JSON, no trae el arreglo `eventos`, el arreglo está vacío, trae más de 50, o **algún evento no tiene `eventoId` UUID** (no se puede informar su resultado por id). 413 si el cuerpo pasa de 256 KB (`RequestSizeLimit`, lo corta Kestrel antes de leer).
- **Rationale**: la app ante un 400 parte el lote y marca `REJECTED_LOCAL` solo el evento que falla solo (contrato del lote), así que el evento sin id termina aislado sin bloquear a los demás. Un lote de 50 eventos pesa ~20 KB: 256 KB solo se supera con un cliente abusivo.
- **Pendiente para el autor**: agregar «o un evento sin `eventoId` válido» a la fila 400 del contrato del lote (`03-requisitos-funcionales.md`) y decidir si la app trata el 413 como un 400.

## R-003 · Repetidos dentro del mismo lote

- **Decision**: `LoteAceptado.De` se queda con la **primera** aparición de cada `eventoId`; las siguientes van a `duplicados`. El `INSERT` nunca recibe dos veces la misma llave.
- **Rationale**: «cada `eventoId` en exactamente una lista» (SC-004) y un `INSERT` limpio.

## R-004 · Quién puede enviar lotes

- **Decision**: anónimos y `ConductorRegistrado`. `con_sesion = (rol == ConductorRegistrado)`. Un `Administrador` recibe 403 `sin_permiso`.
- **Rationale**: RN-14 habla de «sesión iniciada» del conductor; el administrador no detecta (RN-18) y aceptar sus lotes daría una vía para confirmar baches graves sin conducir.
- **Pendiente para el autor**: confirmar el 403 al administrador (docs no lo dice).

## R-005 · Orden de entrega y reparto entre trabajadores

- **Decision**: el elemento de la cola es el **lote**; cada trabajador procesa un lote completo, evento por evento, en orden de `detectado_utc` (y `evento_id` como desempate).
- **Rationale**: RN-15b compara un evento con el **anterior** del mismo seudónimo; un lote viene de un solo celular, así que procesarlo en orden evita descartar por velocidad un evento que llegó «antes» que su predecesor. Entre lotes distintos no importa el orden (bloqueo por celda en Anomalías).

## R-006 · Contrapresión

- **Decision**: `cola.HayCapacidad()` se mira **antes** del `INSERT`; si no hay → 503 `saturado` con `Retry-After: 30` (`ReintentarEnSegundos`). Si entre la comprobación y el aviso la cola se llena, `TryEscribir` falla en silencio y el barredor recupera el lote en ≤ 2 min.
- **Rationale**: nunca se responde 503 por un lote **ya guardado** (la app lo reenviaría y saldría como duplicado, lo cual es correcto pero gasta cuota).

## R-007 · Conteo de la hora de detección (RN-15a)

- **Decision**: `EventosEnLaHora = SELECT count(*) FROM ingesta.evento_recibido WHERE seudonimo = @s AND detectado_utc >= @h AND detectado_utc < @h + 1 h` (índice `ix_recibido_tope`), con `@h` = inicio de la hora UTC de la detección. Cuenta todos los recibidos de esa hora, incluido el propio evento.
- **Rationale**: RN-15a mide por **hora de detección** y no por llegada; la hora UTC y la de Lima comparten los límites de hora (desfase entero de 5 h).
- **Límite aceptado**: si un evento se procesa más de 72 h después de recibido (no debería: `/health/ready` alerta a los 5 min), su seudónimo ya está purgado y el conteo es 0.

## R-008 · Qué borra la purga de 30 días

- **Decision**: solo filas `Procesado` o `Descartado`. Una fila `Recibido` con más de 30 días no se borra (sería perder un evento aceptado, RNF-06); la alarma de `/health/ready` ya estaba activa desde los 5 min.
- **Rationale**: RNF-06 pesa más que RNF-25 en ese caso extremo, que además nunca debería ocurrir.

## R-009 · Salud y métrica de la cola

- **Decision**: `ChequeoColaIngesta` consulta `min(recibido_utc) WHERE estado = 0` (índice `ix_recibido_pendiente`) con caché de 5 s, igual que `ChequeoBaseDatos` (001). La misma lectura alimenta el *gauge* de antigüedad de `IMetricas.ObservarCola`; el tamaño es `Channel.Reader.Count`.

## Pendientes de confirmar por el autor

1. R-002: evento sin `eventoId` → 400; 413 por tamaño.
2. R-004: 403 a lotes del administrador.
3. Complexity Tracking: índice `ix_recibido_lote` (no está en `diseño-interno` §3.2).
