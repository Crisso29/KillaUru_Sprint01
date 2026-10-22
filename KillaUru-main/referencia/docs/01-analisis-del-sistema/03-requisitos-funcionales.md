# Requisitos Funcionales

> **Proyecto:** KillaUru · **Versión:** 1.2 · **Revisión:** 2026-10-21
> **Regla de trazabilidad:** cada RF tiene **una historia principal** y puede **apoyar** a otras. La tabla inversa se deriva de las filas de RF (fuente de verdad) y debe coincidir con ellas. Cada HU debe tener al menos un RF (o RNF, en el caso de HU-16). Los IDs RF-01…RF-28 se conservan; RF-29…RF-33 se agregaron el 2026-10-13 y RF-34…RF-38 el 2026-10-14.
> **Relacionados:** `02-historias-de-usuario.md`, `Nivel3-diagrama-de-componentes.md`, `Nivel4-diagrama-de-codigo.md`

## RF-01 a RF-10 — App Android (Detección, Sincronización y Mapa)

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-01 | El sistema debe capturar acelerómetro y giroscopio a 50 Hz, en segundo plano mediante un servicio en primer plano, cuando la velocidad GPS supere 5 km/h de forma sostenida (RN-03). | HU-01 | — | App · Sensores |
| RF-02 | El sistema debe clasificar cada ventana de 500 ms con el modelo TFLite embebido, retornando tipo, severidad (1–5) y confidence (0.0–1.0). | HU-02 | HU-01, HU-28 | App · Detector |
| RF-03 | El sistema debe guardar en SQLite local todo evento con confidence ≥ 0.75 con: `eventoId` (UUID generado en el dispositivo), hora de detección en UTC **tomada del GPS** (no del reloj del celular, que puede estar mal configurado), coordenadas con precisión GPS, seudónimo de instalación, tipo, severidad, confidence y `modeloVersion`, en estado `PENDING`. | HU-03 | HU-01, HU-02 | App · Buffer |
| RF-04 | El sistema debe detectar cuándo hay señal de internet y sincronizar automáticamente los eventos pendientes. Al recuperar conectividad debe **reiniciar** el backoff. | HU-04 | HU-03 | App · Sincronizador |
| RF-05 | El sistema debe funcionar completamente sin internet: detección, almacenamiento local, mapa descargado y alertas continúan sin interrupciones. | HU-03 | — | App |
| RF-06 | El sistema debe enviar eventos en lotes de hasta 50, **primero los más antiguos**, siguiendo los estados locales de la tabla «Estados locales del evento» (al final de este documento). Un evento pasa a `SYNCED` solo tras un HTTP 200 que lo incluya como aceptado, duplicado o rechazado; si el 200 no lo menciona, vuelve a `PENDING`. Ante HTTP 400 (lote ilegible) **parte el lote** y reintenta por partes; solo un evento que falla **solo** se marca `REJECTED_LOCAL`. Un lote lleva eventos de **un solo seudónimo**: si el seudónimo rota, la app parte el lote (RN-11). Un evento con más de 168 h desde su detección no se envía (el servidor lo rechazaría, RN-11) y pasa a `EXPIRADO_LOCAL`. | HU-04 | — | App · Sincronizador |
| RF-07 | El sistema debe reintentar con backoff exponencial (30 s → 2 min → 10 min → 30 min → 2 h, tope 2 h), con *jitter* de ±25 %, respetando `Retry-After` ante HTTP 429/503. | HU-04 | — | App · Sincronizador |
| RF-08 | El sistema debe ofrecer al conductor no registrado solo el tramo demo Ayacucho–Vinchos (~65 km). El servidor aplica la misma restricción (RF-14). Fuera del tramo demo, la app sombrea el mapa en gris con un mensaje que explica que no hay información de baches sin cuenta, y lo avisa una vez por viaje al salir del tramo; la detección y el envío continúan (RN-08). | HU-07 | — | App · Mapa |
| RF-09 | El sistema debe mostrar al conductor registrado el mapa del corredor de 333 km con las anomalías activas, tiempo relativo, severidad y estado (persistente/episódica). | HU-08 | — | App · Mapa |
| RF-10 | El sistema debe emitir alertas anticipadas (sonido + visual) al aproximarse a una anomalía activa **por delante en el sentido de la marcha**, a una distancia que depende de la velocidad y de la categoría de vehículo elegida, **medida a lo largo de la vía** (RN-16, RN-18), con el comportamiento de sonido de RN-20, usando solo GPS, la polilínea del corredor y la copia local de anomalías, sin internet. Solo para el rol que recibe alertas (RN-08). | HU-09 | — | App · Proximidad |

## RF-11 a RF-15 — Backend (Ingesta, Anomalías, Consulta de Mapa y Auditoría)

| ID | Requisito funcional | HU principal | Apoya a | Componente (C3) |
|---|---|---|---|---|
| RF-11 | El sistema debe recibir lotes mediante `POST /api/v1/anomalies/batch`, aplicar las reglas de recepción de RN-11 a **cada evento por separado** y **persistir de forma durable los aceptados antes de responder HTTP 200**, guardando con cada uno la marca `con_sesion` según el token del lote (RN-14). El identificador del lote (`loteId`) lo genera el servidor. La respuesta indica por evento: aceptados (recibidos para procesar), duplicados y rechazados (con su posición en el lote y su motivo; ver «Contrato de respuesta del lote»). Responde 400 **solo** si el lote no se puede leer, no trae el arreglo de eventos, está vacío o trae más de 50 eventos; un evento mal formado, incluso sin `eventoId`, nunca rechaza el lote entero. Responde 401 si el token es inválido o venció. Ante saturación responde 503 con `Retry-After`. | HU-05 | HU-04 | Ingesta |
| RF-12 | El sistema debe rechazar en la recepción los eventos con esquema inválido, confidence < 0.75, precisión GPS > 20 m, timestamp fuera de rango (más de 168 h en el pasado o más de 5 min en el futuro), tipo desconocido, fuera del corredor o con un seudónimo distinto al del lote (`SeudonimoInconsistente`); y descartar en el procesamiento los de velocidad imposible (RN-11, RN-13, RN-15b). Los eventos de un dispositivo que supera el tope por hora de detección no se descartan, pero esperan confirmación de otro dispositivo (RN-15a). Para severidad alta (4 o 5) debe exigir ≥ 2 dispositivos distintos en 100 m y 72 h, con detecciones separadas al menos 3 min (celulares de un mismo vehículo cuentan como uno), antes de mostrar la anomalía, al menos uno con sesión iniciada de una cuenta con correo verificado (RN-06, RN-14). | HU-05 | — | Anomalías (validación) |
| RF-13 | El sistema debe agrupar reportes del mismo tipo a ≤ 100 m en una anomalía, mostrar como su severidad la mediana de los reportes de los últimos 15 días redondeada hacia arriba, y clasificarla como `Persistente` (≥ 3 días locales distintos, zona America/Lima UTC−5, dentro de 15 días) o `Episodica` (RN-05, RN-12). | HU-06 | — | Anomalías (clasificación) |
| RF-14 | El sistema debe exponer `GET /api/v1/anomalies?bbox=` en GeoJSON, con filtros opcionales (`tipo`, `severidad`, `desde`, `hasta`; y `estado` solo para el administrador), `updatedSince` y `ETag`. **Debe recortar el resultado según el rol** tal como dice RN-08: Anónimo → solo `Activa` dentro del tramo demo; Registrado → solo `Activa` en el corredor completo; Administrador → todos los estados (sin el filtro `estado` recibe los 5). Cada anomalía lleva su kilómetro sobre la vía (`kmVia`, calculado una sola vez al nacer, porque su posición no cambia) y su fecha de vencimiento (`venceUtc`), que es nula en los estados finales (`Expirada`, `Reparada`, `Descartada`), porque no vencen. Con `updatedSince`, la respuesta incluye también las anomalías que **dejaron de ser visibles** para ese rol desde esa fecha, como `{id, eliminada: true}`, sin revelar su estado interno, para que la copia offline las borre. Solo las respuestas a anónimos pueden guardarse en la caché de borde; las respuestas con token se marcan como privadas (ADR-006). | HU-08 | HU-07, HU-12 | Consulta de Mapa · Caché |
| RF-15 | El sistema debe registrar en un log append-only toda acción sensible: alta de usuario, cambio de rol, desactivación, eliminación de cuenta, login fallido y bloqueo (RN-19), recuperación y cambio de contraseña, revisión mensual de privilegios (RNF-30), moderación de anomalías, exportaciones y accesos administrativos a datos personales o de seguridad (lista y detalle de usuarios, consulta de la auditoría y exportación; la consulta del mapa y del dashboard no se registra). Los descartes de ingesta **no** se registran aquí (van a métricas, RNF-22). | HU-15 | HU-11, HU-14, HU-19, HU-20, HU-23, HU-25 | Auditoría |

## RF-16 a RF-20 — Panel Administrativo e Identidad

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-16 | El sistema debe permitir al administrador iniciar sesión con correo y contraseña, emitiendo un token de acceso (JWT) de **5 minutos** y un token de actualización que mantiene la sesión como máximo **8 horas**, y aplicando el bloqueo progresivo por intentos fallidos de RN-19 (ADR-009). | HU-11 | — | Identidad y Accesos |
| RF-17 | El sistema debe mostrar al administrador el mapa completo con todas las anomalías (incluidas las no confirmadas) y filtros por tipo, severidad, fecha y estado. El filtro de estado abre con `Activa` y `PendienteConfirmacion` marcados (RN-08). | HU-12 | — | Panel Admin · Consulta de Mapa |
| RF-18 | El sistema debe mostrar al administrador un dashboard con: eventos válidos por día (reportes procesados, es decir, aceptados en la recepción y no descartados al procesarse), conductores registrados activos (cuentas que renovaron su sesión en los últimos 7 días), dispositivos que aportaron datos (seudónimos distintos con eventos en las últimas 24 h, guardado solo como número diario antes de la purga), cobertura del corredor (porcentaje de los tramos de 1 km del corredor con al menos un evento válido en los últimos 7 días), tasa de confirmación (porcentaje de anomalías que salieron de `PendienteConfirmacion` hacia `Activa` antes de expirar) y tasa de reaparición (anomalías expiradas que vuelven a nacer, RN-07). Estos indicadores **de negocio** se ven en el panel. **Ningún indicador une cuentas con eventos** (RNF-10). Los indicadores **técnicos** (latencia p95, errores, descartes por motivo, uso de CPU y memoria) se consultan en la herramienta de observabilidad (ADR-010), no se recalculan en el panel. | HU-13 | — | Dashboard |
| RF-19 | El sistema debe permitir al administrador crear, editar el rol y desactivar usuarios, y registrar la **revisión mensual de privilegios** (RNF-30): listar las cuentas `Administrador`, confirmar o retirar cada una y dejar la revisión en la auditoría. | HU-14 | — | Identidad y Accesos |
| RF-20 | El sistema debe permitir al administrador exportar el dataset en CSV y GeoJSON con filtros, **sin seudónimos de dispositivo**. | HU-20 | — | Anomalías (caso de uso de exportación) |

## RF-21 a RF-28 — Requisitos nuevos y de Identidad

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-21 | El sistema debe permitir al conductor registrarse e iniciar sesión con correo y contraseña, emitiendo un token de acceso (JWT) de **5 minutos** con token de actualización de **30 días** que se renueva con el uso, asignando el rol `ConductorRegistrado` y aplicando el bloqueo de RN-19. La contraseña tiene **mínimo 10 caracteres**. Para frenar las cuentas desechables, se aceptan como máximo **5 registros por hora desde una misma IP** (`LimiteTasa:RegistrosPorHoraPorIp = 5`, calibrable); al superarlo responde 429 con `Retry-After`. | HU-18 | HU-08 | Identidad y Accesos |
| RF-22 | El sistema debe enviar un correo de verificación con un enlace de confirmación (vigente 24 h) al registrar una cuenta, mediante un proveedor de correo transaccional integrado solo desde Identidad y Accesos. | HU-18b | HU-18, HU-25 | Identidad y Accesos (adaptador de correo) |
| RF-23 | El sistema debe gestionar el **ciclo de vida** de las anomalías con los estados `Activa`, `PendienteConfirmacion`, `Expirada`, `Reparada` y `Descartada` (RN-07), con caducidad parametrizable (valores iniciales: episódica sin reportes durante 72 h; persistente sin reportes durante 7 días; pendiente sin segundo dispositivo durante 72 h), medida con la hora de detección y revisada **cada hora**. El mapa y las alertas usan solo `Activa`. | HU-06 | HU-09 | Anomalías (ciclo de vida) |
| RF-24 | El sistema debe permitir al administrador marcar una anomalía como `Reparada`, `Descartada` (falso positivo) o reabrirla, solo desde los estados que permite la tabla de acciones del administrador de RN-07. El panel muestra una lista **«Por revisar»** con las anomalías `Descartada` que siguen recibiendo reportes de varios vehículos (RN-07), para reabrirlas o mantenerlas descartadas (`MantenerDescartada`). | HU-19 | HU-06 | Anomalías (moderación) |
| RF-25 | El sistema debe mostrar al conductor registrado su historial de recorridos de 6 meses, calculado y almacenado **solo en el dispositivo**; se borra al cerrar sesión (RN-21). | HU-10 | — | App · Historial |
| RF-26 | El sistema debe permitir descargar para uso offline el mapa base vectorial del corredor, la **polilínea del corredor** (para medir distancias a lo largo de la vía, RN-16) y las anomalías activas. El mapa base se publica con un manifiesto (versión, tamaño y SHA-256) igual que el modelo (RF-33), con descarga **reanudable** y solo con Wi-Fi salvo que el usuario acepte usar datos. Las anomalías se actualizan de forma incremental con `updatedSince`, borrando las que llegan como `eliminada` (RF-14). La app muestra «Actualizado hace …» y, si la copia supera la caducidad de una anomalía (RN-07) sin poder actualizarse, deja de alertar de esa anomalía; para eso cada anomalía llega con su fecha de vencimiento (`venceUtc`), calculada por el servidor. Si aún no hay mapa descargado, muestra una pantalla que lo explica. El anónimo puede pedir siempre la copia completa del tramo demo, sin `updatedSince`, para aprovechar la caché de borde. *Nota:* cómo se publican en R2 el mapa base, la polilínea y sus manifiestos se define en la spec de la app; mientras tanto, los archivos de datos del corredor y del tramo demo del backend son la fuente única que la app reutilizará. | HU-08 | HU-03, HU-09 | App · Mapa · Consulta de Mapa |
| RF-27 | El sistema debe identificar la instalación con un UUID aleatorio (no derivado de hardware) y enviarlo como **seudónimo rotativo** (rotación mensual), dentro de cada evento y, en las peticiones sin token, también en una cabecera para el límite de tasa (RNF-11). Todos los eventos de un lote llevan el mismo seudónimo (RN-11). El servidor debe purgar el seudónimo a las 72 h de recibirlo, de los reportes **y** de la copia de recepción (RN-09). | HU-21 | HU-05 | App · Ingesta · Anomalías |
| RF-28 | El sistema debe exponer `GET /health/live` (el proceso responde; lo usa Docker para reiniciar) y `GET /health/ready` (la base de datos responde, el evento pendiente más antiguo de la cola tiene menos de 5 min, la polilínea del corredor está cargada y las migraciones están al día; lo usa UptimeRobot). Ninguno de los dos devuelve datos internos. | HU-17 | — | API REST |

## RF-29 a RF-31 — Derechos del titular de datos (Ley 29733)

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-29 | El sistema debe exigir la aceptación explícita de la política de privacidad para crear una cuenta, guardando la fecha y la versión aceptada, y exigir también al conductor no registrado la **aceptación explícita** del aviso de privacidad antes de iniciar la detección (misma pantalla que RF-37; la versión aceptada se guarda en el celular). La política declara sus finalidades (mostrar el mapa y las alertas, mejorar el modelo de detección y elaborar **analítica agregada por tramo** para clientes (empresas y autoridades), nunca datos por persona ni por dispositivo) y los proveedores que reciben datos y su país (R-06). La app ofrece en Ajustes la opción **«Solo ver el mapa, no enviar datos»**, que apaga la detección y el envío sin cambiar la categoría de vehículo (RN-18). | HU-22 | HU-18 | App · Sesión · Identidad y Accesos |
| RF-30 | El sistema debe permitir al conductor registrado eliminar su cuenta: borrar correo y credenciales, revocar sus sesiones de *refresh*, borrar su historial local y auditar la acción usando solo el identificador interno (sin correo). | HU-23 | HU-21 | App · Sesión · Identidad y Accesos · Auditoría |
| RF-31 | El sistema debe exponer al conductor registrado todos sus datos personales almacenados (`GET /api/v1/me`): correo, rol, fecha de alta y versión de la política aceptada. | HU-24 | — | Identidad y Accesos |
| RF-32 | El sistema debe permitir recuperar la contraseña con un enlace de un solo uso enviado al correo verificado, que vence en 30 min; responder igual exista o no la cuenta; limitar las solicitudes por correo e IP; cerrar todas las sesiones al cambiarla y auditar la acción. | HU-25 | — | Identidad y Accesos (adaptador de correo) |
| RF-33 | El sistema debe permitir actualizar el modelo TFLite sin reinstalar la app: con Wi-Fi, la app consulta un manifiesto versionado, descarga el modelo nuevo, verifica su huella SHA-256 antes de activarlo y conserva el anterior si la verificación falla. El manifiesto indica la **versión mínima de la app** que puede usar ese modelo; un modelo nuevo se activa solo cuando la detección está detenida, nunca a mitad de un viaje. **Prioridad *Should*:** es lo primero que se recorta dentro de HU-02 si falta tiempo; la v1.0 puede salir con el modelo incluido en el APK. | HU-02 | HU-01 | App · Actualizador de modelo |

## RF-34 — Tipo de vehículo

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-34 | El sistema debe pedir al conductor, en el primer uso, la categoría de su vehículo (Liviano, Pesado o Moto) y la indicación de fijar el celular en un soporte y, en turnos largos, conectarlo al cargador del vehículo (S-07); guardar la categoría **solo en el dispositivo**, permitir cambiarla en Ajustes y activar la captura de sensores y el envío de eventos **solo para Liviano** (RN-18). Con Pesado o Moto, el mapa y las alertas siguen disponibles según el rol. | HU-26 | HU-01 | App · Sensores |

## RF-35 a RF-38 — Permisos, estado de la detección, términos de uso y cierre de sesión

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-35 | El sistema debe pedir los permisos de Android en el primer uso, explicando para qué sirve cada uno: ubicación precisa (y «todo el tiempo», para detectar con la pantalla apagada), notificaciones (Android 13 o superior) y exclusión del ahorro de batería. Si el usuario niega uno, la app sigue funcionando con lo que pueda (por ejemplo, sin ubicación no detecta, pero muestra el mapa) y explica qué se pierde. | HU-01 | HU-26 | App · Sesión |
| RF-36 | El sistema debe mostrar un **indicador permanente del estado de la detección** en la notificación del servicio y en la pantalla principal: «Detectando», «En pausa: velocidad baja», «En pausa: batería baja» (RN-22), «Sin GPS», «Falta un permiso», «Detección apagada (vehículo Pesado o Moto)», «Envío desactivado por el usuario», junto con el número de eventos pendientes de enviar. | HU-01 | HU-03 | App · Sensores |
| RF-37 | El sistema debe mostrar y exigir la aceptación de los **términos de uso** (alcance y límites de la app y responsabilidad del conductor) en el primer uso, para conductores con y sin cuenta, antes de iniciar la detección o mostrar el mapa; guardar la fecha y la versión aceptada (localmente para el anónimo y en Identidad y Accesos para el registrado) y volver a pedirla cuando cambie la versión. | HU-27 | HU-22 | App · Sesión · Identidad y Accesos |
| RF-38 | El sistema debe permitir al conductor **cerrar sesión** y gestionar el permiso local de acceso sin conexión según RN-21: vigencia igual a la sesión (30 días), sin cortes durante un viaje, borrado al detectar una cuenta desactivada o eliminada y, al cerrar sesión, recorte de la copia local al tramo demo, borrado del historial con aviso previo y envío de los eventos pendientes como anónimos. | HU-18 | HU-10, HU-21 | App · Sesión |

## Trazabilidad inversa: historia → requisitos

| Historia | RF principales | RF de apoyo |
|---|---|---|
| HU-01 Detección automática | RF-01, RF-35, RF-36 | RF-02, RF-03, RF-33, RF-34 |
| HU-02 Tipos de anomalía | RF-02, RF-33 | RF-03 |
| HU-26 Tipo de vehículo | RF-34 | RF-35 |
| HU-28 Dataset y entrenamiento del modelo | — (RNF-35) | RF-02 |
| HU-03 Operación sin señal | RF-03, RF-05 | RF-04, RF-26, RF-36 |
| HU-04 Sin pérdida de eventos | RF-04, RF-06, RF-07 | RF-11 |
| HU-05 Validación de calidad | RF-11, RF-12 | RF-27 |
| HU-06 Persistente vs episódica | RF-13, RF-23 | RF-24 |
| HU-07 Mapa tramo demo | RF-08 | RF-14 |
| HU-08 Mapa completo | RF-09, RF-14, RF-26 | RF-21 |
| HU-09 Alertas anticipadas | RF-10 | RF-23, RF-26 |
| HU-10 Historial | RF-25 | RF-38 |
| HU-11 Login administrador | RF-16 | RF-15 |
| HU-12 Mapa administrador | RF-17 | RF-14 |
| HU-13 Dashboard | RF-18 | — |
| HU-14 Gestión de usuarios | RF-19 | RF-15 |
| HU-15 Log de auditoría | RF-15 | — |
| HU-16 Despliegue y rollback | — (RNF-17, RNF-18) | — |
| HU-17 Monitoreo | RF-28 | RNF-20, RNF-21 |
| HU-18 Registro de conductor | RF-21, RF-38 | RF-22, RF-29 |
| HU-18b Verificación de correo | RF-22 | — |
| HU-19 Moderación de anomalías | RF-24 | RF-15 |
| HU-20 Exportar dataset | RF-20 | RF-15 |
| HU-21 Privacidad del conductor | RF-27 | RF-30, RF-38, RNF-10, RNF-25 |
| HU-22 Consentimiento | RF-29 | RF-37 |
| HU-27 Términos de uso | RF-37 | — |
| HU-23 Eliminar mi cuenta | RF-30 | RF-15 |
| HU-24 Ver mis datos | RF-31 | — |
| HU-25 Recuperar contraseña | RF-32 | RF-15, RF-22 |

## Contrato de respuesta del lote (RF-06 / RF-11)

| Código | Cuándo | Acción de la app |
|---|---|---|
| **200** | Lote procesado en la recepción. El cuerpo incluye `loteId` (lo **genera el servidor**; la app no lo envía), `aceptados[]` (recibidos para procesar), `duplicados[]` y `rechazados[{id, indice, motivo}]` con los motivos de recepción de RN-11. `indice` es la posición del evento en el arreglo `eventos` del lote (desde 0) y siempre viene; `id` es `null` si el evento no trae un `eventoId` UUID válido (motivo `EsquemaInvalido`). | Marcar `SYNCED` **todos** los ids devueltos (reintentar un rechazado no lo corrige) y contar los rechazos por motivo en la app; un rechazado con `id: null` se ubica por su `indice` y se marca `REJECTED_LOCAL`; los ids que el 200 no menciona vuelven a `PENDING` |
| **400** | **Solo** si el lote no se puede leer, no trae el arreglo `eventos`, está vacío o trae más de 50 eventos. Un evento mal formado (incluso sin `eventoId`) nunca produce 400: sale en `rechazados` | Partir el lote y reintentar por partes; marcar `REJECTED_LOCAL` solo el evento que falla solo |
| **401** | Token de acceso inválido o vencido | Renovar el token y reenviar el **mismo** lote; si no hay sesión válida, reenviarlo sin token (como anónimo). No marca nada como rechazado |
| **429 / 503** | Límite de tasa (por seudónimo + IP, o techo por IP, RNF-11) o cola saturada, con `Retry-After` | Reintentar respetando `Retry-After` más *jitter* |
| **5xx / timeout** | Falla del servidor o de red | Backoff exponencial (RF-07) |

> **Marca de sesión (RN-14):** el servidor pone `con_sesion = sí` a los eventos de un lote enviado con un token válido de `ConductorRegistrado` con el correo verificado. No confía en ninguna marca que mande la app.
>
> **Un lote = un seudónimo (RN-11):** los eventos con un seudónimo distinto al del lote salen en `rechazados` con `SeudonimoInconsistente`; la app parte el lote cuando rota su seudónimo.

## Estados locales del evento (RF-03 / RF-06)

| Estado | Significado | Pasa a |
|---|---|---|
| `PENDING` | Guardado, falta enviarlo | `ENVIANDO` al incluirlo en un lote; `EXPIRADO_LOCAL` si supera 168 h desde su detección |
| `ENVIANDO` | Va en un lote cuya respuesta aún no llega | `SYNCED`, `REJECTED_LOCAL` o de vuelta a `PENDING` (si Android detiene el envío, hay error de red o el 200 no lo menciona) |
| `SYNCED` | El servidor respondió por él (aceptado, duplicado o rechazado) | Se borra a los 7 días |
| `REJECTED_LOCAL` | Falló solo en un 400, o el 200 lo rechazó con `id: null` (ubicado por su `indice`) | Se borra a los 7 días |
| `EXPIRADO_LOCAL` | Pasó la ventana de 168 h sin poder enviarse | Se cuenta y se borra a los 7 días |

Al abrir la app, todo evento que haya quedado en `ENVIANDO` vuelve a `PENDING`. Reenviar un evento es seguro porque el servidor es idempotente por `eventoId`.