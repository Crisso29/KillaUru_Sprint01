# Glosario (lenguaje ubicuo)

> **Proyecto:** KillaUru · **Versión:** 1.2 · **Revisión:** 2026-10-21 · **Estado:** Vigente
> Cada término tiene **una sola definición** y se usa igual en documentos, código, API y panel (ADR-012). Los términos de negocio van en español; los técnicos de capas, en inglés.
> **Relacionados:** `09-reglas-de-negocio.md`, `07-decisiones-arquitectonicas.md`, `componentes-arquitectonicos.md`

## 1. Dominio de la vía

| Término | Código | Definición | No confundir con |
|---|---|---|---|
| **Corredor** | `corredor_via` | Vía PE-28A (Los Libertadores), 333 km, guardada como polilínea. Solo se aceptan eventos a ≤ 50 m de ella (RN-13). | Tramo demo |
| **Tramo demo** | `PoligonoDemo` | Polígono Ayacucho–Vinchos (~65 km), única zona visible para el anónimo (RN-08): la polilínea de la PE-28A entre el km 268 (Vinchos) y el km 333 (Ayacucho) con un margen de 200 m a cada lado, simplificada a ≤ 60 vértices. | Corredor |
| **Kilómetro sobre la vía** | `kmVia` | Kilómetro de la PE-28A donde está una anomalía, medido a lo largo de la polilínea del corredor. Se calcula una sola vez, al nacer la anomalía, porque su posición no cambia (RN-05); lo muestran el mapa, el detalle y las listas del panel. | Coordenadas |
| **Celda** | `Celda` | Cuadro de rejilla geográfica que se usa para la caché del mapa y para el bloqueo de concurrencia (≥ 5 × 100 m). | Bbox |
| **Bbox** | `bbox` | Rectángulo que pide el cliente al consultar el mapa; el servidor lo recorta por rol y lo ajusta a celdas. | Celda |

## 2. Del sensor a la anomalía

| Término | Código | Definición | No confundir con |
|---|---|---|---|
| **Categoría de vehículo** | `CategoriaVehiculo` | Liviano, Pesado o Moto; la elige el conductor y se guarda solo en el celular. En v1.0 solo **Liviano** (auto, camioneta, station wagon o colectivo, con el celular en soporte) detecta y envía eventos (RN-18). | Rol |
| **Ventana** | — | 500 ms de lecturas del acelerómetro y el giroscopio a 50 Hz que clasifica el modelo. | Evento |
| **Evento** | `EventoReportado` (contrato) | Detección guardada en el celular y enviada al servidor: `eventoId`, hora UTC, ubicación, precisión, tipo, severidad, confidence, `modeloVersion`, seudónimo. | Reporte, Anomalía |
| **Lote** | `LoteEntrante` | Grupo de hasta 50 eventos de **un solo seudónimo** enviado en un solo `POST` (RN-11). Su identificador (`loteId`) lo genera el servidor; el aviso que va a la cola de Ingesta (`LoteRecibido`) lleva solo ese `loteId`. | — |
| **Recepción / Procesamiento** | — | Las dos etapas de la ingesta (RN-11). En la **recepción**, antes de responder 200, se aplican las reglas que no usan la BD y el evento sale como aceptado o rechazado. En el **procesamiento**, después, Anomalías aplica las reglas que necesitan la BD; lo que descarta solo se cuenta como métrica. | — |
| **Evento recibido** | `ingesta.evento_recibido` | Copia durable del evento aceptado tal como llegó (`JSONB`), antes de procesarlo. El seudónimo y el `lote_id` se le borran a las 72 h, y su hora de recepción queda redondeada a la hora (RN-09); la fila se borra a los 30 días. | Reporte |
| **Reporte** | `ReporteAnomalia` | Evento que pasó la validación y quedó asociado a una anomalía. Es único por `eventoId`. | Evento |
| **Anomalía** | `Anomalia` | Punto de la vía con un problema (Bache o Hundimiento), formado por uno o más reportes del mismo tipo a ≤ 100 m (RN-05). Es lo que se muestra en el mapa, con la mediana de las severidades de sus reportes de los últimos 15 días, redondeada hacia arriba. | Reporte |
| **Posición de la anomalía** | — | Posición del reporte que creó la anomalía; no cambia. Los 100 m de RN-05 se miden desde ella. | Centroide |
| **Tipo de anomalía** | `TipoAnomalia` | Bache o Hundimiento (RN-01). *Normal* es una clase interna del modelo que nunca se envía. | — |
| **Severidad** | `Severidad` | Intensidad del golpe, de 1 a 5. **Alta** = 4 o 5 (RN-02). | Confidence |
| **Confidence** | `Confidence` | Seguridad del modelo en su clasificación, de 0.0 a 1.0. Mínimo 0.75 (RN-04, RN-11). | Severidad |
| **Descarte** | `MotivoDescarte` | Evento rechazado por una regla de calidad o antifraude, en la recepción (sale en `rechazados[]` de la respuesta) o en el procesamiento (solo métrica) (RN-11); se cuenta como métrica por motivo, no se audita. Son 8 motivos, numerados de 1 (`EsquemaInvalido`) a 8 (`SeudonimoInconsistente`). | Anomalía `Descartada` |
| **Seudónimo inconsistente** | `MotivoDescarte.SeudonimoInconsistente` (8) | Motivo de rechazo en la recepción: el evento trae un seudónimo distinto al del lote (el de la cabecera `X-KillaUru-Seudonimo` o, si no viene, el del primer evento con seudónimo válido). Un lote = un seudónimo (RN-11). | Seudónimo |

## 3. Estados y clasificación

| Término | Código | Definición |
|---|---|---|
| **Persistente** | `EstadoPersistencia.Persistente` | Anomalía con reportes en ≥ 3 días locales distintos dentro de 15 días (RN-12). |
| **Episódica** | `EstadoPersistencia.Episodica` | Cualquier anomalía que no es persistente. |
| **Confirmación** | `PoliticaConfirmacion` | Exigencia de ≥ 2 dispositivos distintos, con detecciones separadas ≥ 3 min para que sean vehículos distintos, para mostrar una anomalía de severidad alta (con al menos un reporte con sesión, RN-14) o una anomalía creada por un dispositivo que superó su tope (sin exigir sesión) (RN-06, RN-14, RN-15a). También se aplica cuando una anomalía `Expirada` o `Reparada` vuelve a nacer. |
| `Activa` | `EstadoAnomalia.Activa` | Visible en el mapa y genera alertas. |
| `PendienteConfirmacion` | `EstadoAnomalia.PendienteConfirmacion` | Espera un segundo dispositivo; solo la ve el administrador. |
| `Expirada` | `EstadoAnomalia.Expirada` | Sin reportes durante su plazo (episódica 72 h, persistente 7 días, pendiente 72 h), medido con la hora de detección; con un reporte detectado después de su vencimiento vuelve a nacer y pasa otra vez por la confirmación (RN-06). |
| `Reparada` | `EstadoAnomalia.Reparada` | Marcada por el administrador como arreglada; con un reporte detectado después de la reparación vuelve a nacer (RN-07). |
| `Descartada` | `EstadoAnomalia.Descartada` | Falso positivo confirmado por el administrador; un nuevo reporte no la reactiva, pero si siguen llegando reportes de varios vehículos pasa a la lista «Por revisar» del panel, donde el administrador la reabre o la mantiene descartada (`MantenerDescartada`) (RN-07). |
| **Reaparición** | — | Anomalía que **vuelve a nacer** después de expirar (hacia `Activa` o `PendienteConfirmacion`); su tasa sirve para calibrar la caducidad. |
| **Pasada** | — | Paso del vehículo junto a una anomalía; termina cuando se aleja más de 500 m. Se emite una sola alerta por anomalía en cada pasada (RN-16). |

## 4. Personas, identidad y privacidad

| Término | Código | Definición | No confundir con |
|---|---|---|---|
| **Anónimo** | rol `Anonimo` | Conductor que usa la app sin cuenta. | Seudónimo |
| **Conductor Registrado** | rol `ConductorRegistrado` | Conductor con cuenta (correo y contraseña). | — |
| **Administrador** | rol `Administrador` | Personal interno que gestiona el sistema desde el panel. | — |
| **Seudónimo** | `SeudonimoInstalacion` | UUID aleatorio de la instalación que rota cada mes; el servidor lo borra a las 72 h (RN-09). No identifica a la persona. | Cuenta |
| **Con sesión** | `con_sesion` | Marca sí/no de un reporte enviado con sesión iniciada; **no** guarda qué cuenta (RN-14). | — |
| **Derecho de acceso local** | *entitlement* | Permiso guardado en el celular que habilita el corredor completo y las alertas sin conexión. Dura lo mismo que la sesión (30 días), no se corta durante un viaje y se borra al cerrar sesión o al saberse que la cuenta fue desactivada (RN-21). | Token |
| **Token de acceso** | JWT | Credencial de 5 min que acompaña cada petición. | Token de actualización |
| **Token de actualización** | *refresh token* | Credencial revocable y de un solo uso que renueva el token de acceso; define la duración de la sesión (8 h admin, 30 días conductor). | Token de acceso |
| **Conductor registrado activo** | — | Cuenta que renovó su sesión en los últimos 7 días (RN-17). | Dispositivo que aportó datos |
| **Dispositivo que aportó datos** | — | Seudónimo distinto con eventos en las últimas 24 h; solo se guarda el número diario (RN-17). | Conductor activo |
| **Derechos ARCO** | — | Acceso, Rectificación, Cancelación y Oposición del titular de datos (Ley 29733); en v1.0: ver mis datos (RF-31) y eliminar mi cuenta (RF-30). | — |
| **Términos de uso** | — | Texto que todo conductor acepta antes de usar la app (RF-37): la app es una ayuda y no una garantía, puede no mostrar todos los baches o dar falsas alarmas, y el conductor sigue siendo responsable de su manejo. | Política de privacidad |

## 5. Arquitectura

| Término | Definición |
|---|---|
| **Módulo** | *Bounded context* del monolito con reglas propias y una razón propia para cambiar o escalar. Son 6: Identidad y Accesos, Ingesta, Anomalías, Consulta de Mapa, Dashboard y Auditoría. |
| **Contrato** (`Contracts/`) | Interfaces, DTOs y eventos públicos de un módulo; lo único que otros módulos pueden usar. |
| **Kernel compartido** (`Shared/`) | Tipos básicos (`Coordenada`, `Severidad`, `Confidence`, `SeudonimoInstalacion`) y puertos técnicos (`IPublicadorEventos`, `IMetricas`). |
| **Evento de dominio** | Hecho que un módulo publica después del *commit* (p. ej. `AnomaliaCambiada`) para que otros reaccionen sin acoplarse (patrón Observer). |
| **Puerto** | Interfaz definida en Application que Infrastructure implementa. |
| **Domain · Application · Adapters · Infrastructure** | Las 4 capas de cada módulo; las dependencias apuntan hacia Domain. |
| **Idempotencia** | Procesar dos veces el mismo `eventoId` produce el mismo resultado que procesarlo una vez. |
| **Store-and-Forward** | Guardar en el celular y enviar cuando haya señal (ADR-003). |
| **Edge AI** | Clasificar en el dispositivo con LiteRT, sin enviar la señal cruda (ADR-004). |
| **Manifiesto del modelo** | Archivo en `cdn.killauru.pe` que indica la versión vigente del modelo, su URL y su SHA-256 (ADR-004). |
