# Requisitos Funcionales

> **Proyecto:** KillaUru · **Versión:** 1.2 · **Revisión:** 2026-10-13
> **Regla de trazabilidad:** cada RF tiene **una historia principal** y puede **apoyar** a otras. La tabla inversa se deriva de las filas de RF (fuente de verdad) y debe coincidir con ellas. Cada HU debe tener al menos un RF (o RNF, en el caso de HU-16). Los IDs RF-01…RF-28 se conservan; RF-29…RF-33 se agregaron el 2026-10-13.
> **Relacionados:** `02-historias-de-usuario.md`, `Nivel3-diagrama-de-componentes.md`, `Nivel4-diagrama-de-codigo.md`

## RF-01 a RF-10 — App Android (Detección, Sincronización y Mapa)

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-01 | El sistema debe capturar acelerómetro y giroscopio a 50 Hz, en segundo plano mediante un servicio en primer plano, cuando la velocidad GPS supere 5 km/h de forma sostenida. | HU-01 | — | App · Sensores |
| RF-02 | El sistema debe clasificar cada ventana de 500 ms con el modelo TFLite embebido, retornando tipo, severidad (1–5) y confidence (0.0–1.0). | HU-02 | HU-01 | App · Detector |
| RF-03 | El sistema debe guardar en SQLite local todo evento con confidence ≥ 0.75 con: `eventoId` (UUID generado en el dispositivo), timestamp UTC, coordenadas con precisión GPS, seudónimo de instalación, tipo, severidad, confidence y `modeloVersion`, en estado `PENDING`. | HU-03 | HU-01, HU-02 | App · Buffer |
| RF-04 | El sistema debe detectar cuándo hay señal de internet y sincronizar automáticamente los eventos pendientes. Al recuperar conectividad debe **reiniciar** el backoff. | HU-04 | HU-03 | App · Sincronizador |
| RF-05 | El sistema debe funcionar completamente sin internet: detección, almacenamiento local, mapa descargado y alertas continúan sin interrupciones. | HU-03 | — | App |
| RF-06 | El sistema debe enviar eventos en lotes de hasta 50. Un evento pasa a `SYNCED` solo tras un HTTP 200 que lo incluya como aceptado, duplicado o rechazado. Ante HTTP 400 los marca `REJECTED_LOCAL` y no los reintenta. | HU-04 | — | App · Sincronizador |
| RF-07 | El sistema debe reintentar con backoff exponencial (30 s → 2 min → 10 min → 30 min → 2 h, tope 2 h), con *jitter* de ±25 %, respetando `Retry-After` ante HTTP 429/503. | HU-04 | — | App · Sincronizador |
| RF-08 | El sistema debe ofrecer al conductor no registrado solo el tramo demo Ayacucho–Vinchos (~65 km). El servidor aplica la misma restricción (RF-14). | HU-07 | — | App · Mapa |
| RF-09 | El sistema debe mostrar al conductor registrado el mapa del corredor de 333 km con las anomalías activas, tiempo relativo, severidad y estado (persistente/episódica). | HU-08 | — | App · Mapa |
| RF-10 | El sistema debe emitir alertas anticipadas (sonido + visual) al aproximarse a una anomalía activa **por delante en el sentido de la marcha**, a una distancia que depende de la velocidad (RN-16), usando solo GPS y la copia local de anomalías, sin internet. | HU-09 | — | App · Proximidad |

## RF-11 a RF-15 — Backend (Ingesta, Anomalías, Consulta de Mapa y Auditoría)

| ID | Requisito funcional | HU principal | Apoya a | Componente (C3) |
|---|---|---|---|---|
| RF-11 | El sistema debe recibir lotes mediante `POST /api/v1/anomalies/batch`, validar el esquema y **persistir el lote de forma durable antes de responder HTTP 200**. La respuesta indica por evento: aceptados, duplicados y rechazados (con motivo). Ante saturación responde 503 con `Retry-After`. | HU-05 | HU-04 | Ingesta |
| RF-12 | El sistema debe rechazar eventos con confidence < 0.75, precisión GPS > 20 m o timestamp fuera de rango (más de 7 días en el pasado o más de 5 min en el futuro), fuera del corredor o con velocidad imposible (RN-11, RN-13, RN-15b). Los eventos de un dispositivo que supera el tope por hora de detección no se descartan, pero esperan confirmación de otro dispositivo (RN-15a). Para severidad alta (4 o 5) debe exigir ≥ 2 dispositivos distintos en 100 m y 72 h antes de mostrar la anomalía, al menos uno con sesión iniciada (RN-06, RN-14). | HU-05 | — | Anomalías (validación) |
| RF-13 | El sistema debe agrupar reportes del mismo tipo a ≤ 100 m en una anomalía y clasificarla como PERSISTENTE (≥ 3 días locales distintos, zona America/Lima UTC−5, dentro de 15 días) o EPISÓDICA (RN-05, RN-12). | HU-06 | — | Anomalías (clasificación) |
| RF-14 | El sistema debe exponer `GET /api/v1/anomalies?bbox=` en GeoJSON, con filtros opcionales (`tipo`, `severidad`, `desde`), `updatedSince` y `ETag`. **Debe recortar el resultado según el rol**: Anónimo → polígono del tramo demo; Registrado → corredor completo con anomalías activas y confirmadas; Administrador → todas. | HU-08 | HU-07, HU-12 | Consulta de Mapa · Caché |
| RF-15 | El sistema debe registrar en un log append-only toda acción sensible: alta de usuario, cambio de rol, desactivación, eliminación de cuenta, login fallido y bloqueo, moderación de anomalías, exportaciones y accesos administrativos a datos. Los descartes de ingesta **no** se registran aquí (van a métricas, RF-18). | HU-15 | HU-11, HU-14, HU-19, HU-20, HU-23, HU-25 | Auditoría |

## RF-16 a RF-20 — Panel Administrativo e Identidad

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-16 | El sistema debe permitir al administrador iniciar sesión con correo y contraseña, emitiendo un token de acceso (JWT) de **5 minutos** y un token de actualización que mantiene la sesión como máximo **8 horas**, y aplicando bloqueo temporal progresivo tras 3 intentos fallidos (ADR-009). | HU-11 | — | Identidad y Accesos |
| RF-17 | El sistema debe mostrar al administrador el mapa completo con todas las anomalías (incluidas las no confirmadas) y filtros por tipo, severidad, fecha y estado. | HU-12 | — | Panel Admin · Consulta de Mapa |
| RF-18 | El sistema debe mostrar al administrador un dashboard con: eventos por día, conductores registrados activos (cuentas que renovaron su sesión en los últimos 7 días), dispositivos que aportaron datos (seudónimos distintos con eventos en las últimas 24 h, guardado solo como número diario antes de la purga), cobertura del corredor, tasa de confirmación y tasa de reaparición de anomalías expiradas (RN-07). **Ningún indicador une cuentas con eventos** (RNF-10). Los indicadores **técnicos** (latencia p95, errores, descartes por motivo, uso de CPU y memoria) se consultan en la herramienta de observabilidad (ADR-010), no se recalculan en el panel. | HU-13 | — | Dashboard |
| RF-19 | El sistema debe permitir al administrador crear, editar el rol y desactivar usuarios. | HU-14 | — | Identidad y Accesos |
| RF-20 | El sistema debe permitir al administrador exportar el dataset en CSV y GeoJSON con filtros, **sin seudónimos de dispositivo**. | HU-20 | — | Anomalías (caso de uso de exportación) |

## RF-21 a RF-28 — Requisitos nuevos y de Identidad

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-21 | El sistema debe permitir al conductor registrarse e iniciar sesión con correo y contraseña, emitiendo un token de acceso (JWT) de **5 minutos** con token de actualización de **30 días** que se renueva con el uso y asignando el rol `ConductorRegistrado`. | HU-18 | HU-08 | Identidad y Accesos |
| RF-22 | El sistema debe enviar un correo de verificación con un enlace de confirmación (vigente 24 h) al registrar una cuenta, mediante un proveedor de correo transaccional integrado solo desde Identidad y Accesos. | HU-18b | HU-18, HU-25 | Identidad y Accesos (adaptador de correo) |
| RF-23 | El sistema debe gestionar el **ciclo de vida** de las anomalías con los estados `Activa`, `PendienteConfirmacion`, `Expirada`, `Reparada` y `Descartada` (RN-07), con caducidad parametrizable (valores iniciales: episódica sin reportes durante 72 h; persistente sin reportes durante 7 días; pendiente sin segundo dispositivo durante 72 h). El mapa y las alertas usan solo `Activa`. | HU-06 | HU-09 | Anomalías (ciclo de vida) |
| RF-24 | El sistema debe permitir al administrador marcar una anomalía como `Reparada`, `Descartada` (falso positivo) o reabrirla (RN-07). | HU-19 | HU-06 | Anomalías (moderación) |
| RF-25 | El sistema debe mostrar al conductor registrado su historial de recorridos de 6 meses, calculado y almacenado **solo en el dispositivo**. | HU-10 | — | App · Historial |
| RF-26 | El sistema debe permitir descargar el mapa base vectorial del corredor y las anomalías activas para uso offline, con actualización incremental mediante `updatedSince`. | HU-08 | HU-03, HU-09 | App · Mapa · Consulta de Mapa |
| RF-27 | El sistema debe identificar la instalación con un UUID aleatorio (no derivado de hardware) y enviarlo como **seudónimo rotativo** (rotación mensual). El servidor debe purgar el seudónimo de los reportes a las 72 h. | HU-21 | HU-05 | App · Ingesta · Anomalías |
| RF-28 | El sistema debe exponer `GET /health` con comprobación de proceso (*liveness*) y de base de datos (*readiness*) para el monitoreo. | HU-17 | — | API REST |

## RF-29 a RF-31 — Derechos del titular de datos (Ley 29733)

| ID | Requisito funcional | HU principal | Apoya a | Componente |
|---|---|---|---|---|
| RF-29 | El sistema debe exigir la aceptación explícita de la política de privacidad para crear una cuenta, guardando la fecha y la versión aceptada, y mostrar un aviso de privacidad al conductor no registrado antes de iniciar la detección. | HU-22 | HU-18 | App · Sesión · Identidad y Accesos |
| RF-30 | El sistema debe permitir al conductor registrado eliminar su cuenta: borrar correo y credenciales, revocar sus sesiones de *refresh*, borrar su historial local y auditar la acción usando solo el identificador interno (sin correo). | HU-23 | HU-21 | App · Sesión · Identidad y Accesos · Auditoría |
| RF-31 | El sistema debe exponer al conductor registrado todos sus datos personales almacenados (`GET /api/v1/me`): correo, rol, fecha de alta y versión de la política aceptada. | HU-24 | — | Identidad y Accesos |
| RF-32 | El sistema debe permitir recuperar la contraseña con un enlace de un solo uso enviado al correo verificado, que vence en 30 min; responder igual exista o no la cuenta; limitar las solicitudes por correo e IP; cerrar todas las sesiones al cambiarla y auditar la acción. | HU-25 | — | Identidad y Accesos (adaptador de correo) |
| RF-33 | El sistema debe permitir actualizar el modelo TFLite sin reinstalar la app: con Wi-Fi, la app consulta un manifiesto versionado, descarga el modelo nuevo, verifica su huella SHA-256 antes de activarlo y conserva el anterior si la verificación falla. | HU-02 | HU-01 | App · Actualizador de modelo |

## Trazabilidad inversa: historia → requisitos

| Historia | RF principales | RF de apoyo |
|---|---|---|
| HU-01 Detección automática | RF-01 | RF-02, RF-03, RF-33 |
| HU-02 Tipos de anomalía | RF-02, RF-33 | RF-03 |
| HU-03 Operación sin señal | RF-03, RF-05 | RF-04, RF-26 |
| HU-04 Sin pérdida de eventos | RF-04, RF-06, RF-07 | RF-11 |
| HU-05 Validación de calidad | RF-11, RF-12 | RF-27 |
| HU-06 Persistente vs episódica | RF-13, RF-23 | RF-24 |
| HU-07 Mapa tramo demo | RF-08 | RF-14 |
| HU-08 Mapa completo | RF-09, RF-14, RF-26 | RF-21 |
| HU-09 Alertas anticipadas | RF-10 | RF-23, RF-26 |
| HU-10 Historial | RF-25 | — |
| HU-11 Login administrador | RF-16 | RF-15 |
| HU-12 Mapa administrador | RF-17 | RF-14 |
| HU-13 Dashboard | RF-18 | — |
| HU-14 Gestión de usuarios | RF-19 | RF-15 |
| HU-15 Log de auditoría | RF-15 | — |
| HU-16 Despliegue y rollback | — (RNF-17, RNF-18) | — |
| HU-17 Monitoreo | RF-28 | RNF-20, RNF-21 |
| HU-18 Registro de conductor | RF-21 | RF-22, RF-29 |
| HU-18b Verificación de correo | RF-22 | — |
| HU-19 Moderación de anomalías | RF-24 | RF-15 |
| HU-20 Exportar dataset | RF-20 | RF-15 |
| HU-21 Privacidad del conductor | RF-27 | RF-30, RNF-10, RNF-25 |
| HU-22 Consentimiento | RF-29 | — |
| HU-23 Eliminar mi cuenta | RF-30 | RF-15 |
| HU-24 Ver mis datos | RF-31 | — |
| HU-25 Recuperar contraseña | RF-32 | RF-15, RF-22 |

## Contrato de respuesta del lote (RF-06 / RF-11)

| Código | Cuándo | Acción de la app |
|---|---|---|
| **200** | Lote persistido. El cuerpo incluye `loteId`, `aceptados[]`, `duplicados[]` y `rechazados[{id, motivo}]`. | Marcar `SYNCED` **todos** los ids devueltos (reintentar un rechazado no lo corrige) |
| **400** | Esquema inválido | Marcar `REJECTED_LOCAL`; no reintentar |
| **429 / 503** | Límite de tasa o cola saturada, con `Retry-After` | Reintentar respetando `Retry-After` más *jitter* |
| **5xx / timeout** | Falla del servidor o de red | Backoff exponencial (RF-07) |