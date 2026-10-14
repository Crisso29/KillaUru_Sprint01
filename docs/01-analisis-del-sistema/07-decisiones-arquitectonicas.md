# Decisiones Arquitectónicas

> **Proyecto:** KillaUru · **Versión:** 1.2 · **Revisión:** 2026-10-14 · **Estado:** Vigente
> Un ADR (Architecture Decision Record) documenta una decisión importante: el contexto que la motivó, las alternativas evaluadas, la decisión tomada y sus consecuencias.
> **Estados:** **Aceptado** (vigente). La columna «Drivers» de cada ADR coincide con la columna «ADR» de `06-driver-arquitectonicos.md` (trazabilidad en ambos sentidos).

## Resumen de decisiones

| ID | Decisión | Estado | Drivers | Resultado |
|---|---|---|---|---|
| **ADR-001** | Monolito modular | Aceptado | DA-06, DA-09, DA-12 | Un proceso con 6 módulos: Identidad y Accesos, Ingesta, Anomalías, Consulta de Mapa, Dashboard y Auditoría |
| **ADR-002** | Clean Architecture por módulo | Aceptado | DA-06, DA-09 | Capas Domain, Application, Adapters e Infrastructure; puertos en Application; `Contracts/` entre módulos |
| **ADR-003** | Store-and-Forward con SQLite | Aceptado | DA-01, DA-02 | Buffer local PENDING/SYNCED/REJECTED_LOCAL con reintentos y *jitter* |
| **ADR-004** | Edge AI con LiteRT (TFLite) | Aceptado | DA-03, DA-04, DA-18 | Modelo MLP cuantizado; inicial en el APK y actualizable desde R2 con SHA-256; `modeloVersion` en cada evento |
| **ADR-005** | Ingesta durable e idempotente | Aceptado | DA-02, DA-05 | Tabla de recepción + Channel como aviso + HTTP 200 tras el *commit* |
| **ADR-006** | Caché en memoria (`IMemoryCache`) | Aceptado | DA-05, DA-08, DA-09 | Caché por celdas, propiedad de Consulta de Mapa, invalidada por eventos de dominio (Observer); Redis diferido |
| **ADR-007** | Privacidad por diseño | Aceptado | DA-07 | Seudónimo rotativo, purga a 72 h, historial local |
| **ADR-008** | Ciclo de vida de la anomalía | Aceptado | DA-13 | Estados y caducidad parametrizable |
| **ADR-009** | Identidad y accesos | Aceptado | DA-10, DA-17 | Módulo unificado; tokens de acceso de 5 min y de actualización revocables; RBAC y recorte por rol en el servidor |
| **ADR-010** | Despliegue y operación | Aceptado | DA-08, DA-11 | Cloudflare Tunnel y Pages, backups, rollback por tag |
| **ADR-011** | Mapa base vectorial offline | Aceptado | DA-14 | PMTiles del corredor en R2; MapLibre en app y panel |
| **ADR-012** | Convenciones de diseño | Aceptado | DA-09, DA-12 | Lenguaje ubicuo, una tecnología de datos, umbrales por configuración |
| **ADR-013** | Auditoría en la misma transacción | Aceptado | DA-06, DA-16 | «No hay acción sensible sin rastro»; excepción documentada a la transacción por módulo |
| **ADR-014** | Estrategia de persistencia (políglota) | Aceptado | DA-01, DA-06, DA-08, DA-14 | PostgreSQL + JSONB, SQLite en el dispositivo, objetos en R2 y clave-valor en memoria; disparadores para NoSQL dedicado |
| **ADR-015** | Integridad del dato contra eventos falsos | Aceptado | DA-15 | Solo sobre la vía, sesión para confirmar lo grave, plausibilidad del dispositivo sin descartar picos reales |

---

## ADR-001 — Monolito Modular

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-06, DA-09, DA-12

**Contexto y problema.** KillaUru lo desarrolla y opera una sola persona con un plazo de 6 a 8 semanas. Los microservicios exigirían gestionar varios servicios y una infraestructura compleja, lo que haría inviable el proyecto; además no son óptimos cuando un proyecto recién inicia.

| Opción | Pros | Contras |
|---|---|---|
| Microservicios | Escalado independiente por módulo | Complejidad operacional inviable para un solo desarrollador |
| Monolito tradicional | Simple de desarrollar | Difícil de mantener y escalar conforme crece |
| **Monolito modular** | Simple de desplegar; módulos separados; puede evolucionar hacia servicios | Requiere disciplina para mantener los límites |

**Decisión.** Monolito modular en ASP.NET Core (.NET 10 LTS): un solo proceso con *bounded contexts* internos (6 módulos: se unificaron Autenticación y Usuarios en Identidad y Accesos según ADR-009, Validación y Clasificación en **Anomalías**, y Exportación pasó a ser un caso de uso de Anomalías). El backend **no** incluye módulos de detección ni de sincronización: esas funciones viven en la app Android (ADR-003, ADR-004).

**Consecuencias**
- ✅ `docker compose up -d` levanta todo el sistema.
- ✅ Los módulos con interfaces claras pueden extraerse como servicios (candidatos naturales: Ingesta y Consulta de Mapa).
- ⚠️ Requiere disciplina: se hace cumplir con pruebas de arquitectura (RNF-28).
- ✅ **Validación y Clasificación se unen en el módulo Anomalías** (2026-10-13): comparten agregado, tabla, bloqueo por celda y momento de ejecución; separarlas era una frontera artificial. Dentro del módulo se conservan como pasos del flujo (validar → confirmar → clasificar).
- ✅ **Exportación deja de ser módulo** (2026-10-13): no tiene tablas ni reglas propias; su única regla («sin seudónimos») es sobre datos de Anomalías. Pasa a ser `ExportarDatasetUseCase` dentro de Anomalías. **Criterio de módulo:** reglas de negocio propias y una razón propia para cambiar o escalar.
- ⚠️ Las métricas operativas pasan a `Shared/Metricas` (puerto técnico, como la caché) para evitar el ciclo Anomalías ↔ Dashboard.

---

## ADR-002 — Clean Architecture por módulo

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-06, DA-09

**Contexto y problema.** Con un solo desarrollador, el código debe ser fácil de modificar y probar. Si la lógica de negocio se mezcla con controladores o base de datos, cualquier cambio tecnológico rompe el sistema.

| Opción | Pros | Contras |
|---|---|---|
| MVC tradicional | Simple y conocido | Lógica acoplada a frameworks y BD |
| **Clean Architecture** | Dominio independiente; casos de uso explícitos; fácil de probar | Más carpetas y disciplina inicial |
| Hexagonal (Ports & Adapters) | Prácticamente equivalente en la separación | Se prefiere el vocabulario de *casos de uso* de Clean para organizar el código |

**Decisión.** Cuatro capas por módulo: **Domain, Application, Adapters, Infrastructure**, con dependencias solo hacia adentro (Adapters solo usa Application; Infrastructure implementa puertos de Application y conoce Domain). Los **puertos viven en Application**. Los módulos se comunican solo mediante interfaces de su carpeta `Contracts/`. Las reglas se verifican con pruebas de arquitectura.

**Consecuencias**
- ✅ RN-11 y RN-12 se prueban sin base de datos ni sensores.
- ✅ Cambiar de motor de datos solo afecta a Infrastructure.
- ⚠️ La estructura requiere más archivos que un MVC simple.

---

## ADR-003 — Store-and-Forward con SQLite como buffer offline

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-01, DA-02

**Contexto y problema.** Más del 50 % del corredor PE-28A no tiene cobertura celular. Si la app depende de conexión permanente, el sistema no cumple su propósito en la mayor parte de la vía.

| Opción | Pros | Contras |
|---|---|---|
| Envío directo al servidor | Simple | No funciona sin internet |
| Archivo local en disco | Simple | Sin integridad ni control de estado por evento |
| **SQLite + Store-and-Forward** | Persistencia garantizada; estado por evento; nativo en Android | Requiere lógica de sincronización |

**Decisión.** SQLite como buffer local. Cada evento lleva un `eventoId` (UUID) y nace `PENDING`. Pasa a `SYNCED` solo tras un HTTP 200 que lo incluya. Un HTTP 400 lo marca `REJECTED_LOCAL`. Backoff 30 s → 2 min → 10 min → 30 min → 2 h, con *jitter* de ±25 %, respetando `Retry-After` y **reiniciando el backoff al recuperar conectividad**.

**Consecuencias**
- ✅ La app funciona sin internet durante todo el trayecto.
- ✅ Garantía *at-least-once*: ningún evento se pierde silenciosamente.
- ⚠️ El servidor **debe** deduplicar por `eventoId` (ADR-005).
- ✅ Retención local definida (2026-10-14): los estados locales y su purga a los 7 días están en la tabla «Estados locales del evento» de `03-requisitos-funcionales.md` (RF-06).

---

## ADR-004 — Edge AI con TFLite para clasificación local

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-03, DA-04, DA-18

**Contexto y problema.** El conductor no puede tocar el celular mientras maneja. La clasificación debe ser automática, en tiempo real y sin internet.

| Opción | Pros | Contras |
|---|---|---|
| Enviar señal cruda al servidor | Modelo más potente en la nube | Requiere internet permanente; latencia inaceptable |
| Reglas manuales con umbrales fijos | Sin dependencia de ML | Baja precisión; no distingue tipos |
| **TFLite embebido en la app** | Inferencia < 50 ms; sin internet; modelo < 5 MB | Hay que distribuir los modelos nuevos (resuelto con el punto 2 de la decisión) |

**Decisión.**
1. Modelo MLP cuantizado convertido a TFLite. El APK incluye un **modelo inicial**, que clasifica ventanas de 500 ms y registra el evento si confidence ≥ 0.75. Cada evento incluye `modeloVersion`.
2. **Actualización del modelo sin reinstalar la app** (2026-10-13; prioridad *Should* desde 2026-10-14, recortable: si no entra en v1.0, se usa el modelo del APK y este canal se agrega después sin cambiar la arquitectura): la app consulta, solo con Wi-Fi, un **manifiesto** en `cdn.killauru.pe` (Cloudflare R2) con la versión vigente, la URL del archivo y su **huella SHA-256**. Si hay una versión nueva, la descarga, verifica la huella y recién entonces la activa; si la verificación falla, la descarta y sigue con la anterior.
3. **Calibrado para un solo tipo de vehículo** (2026-10-14): el modelo de v1.0 se entrena con vehículos livianos y el celular en soporte, y solo esa categoría detecta (RN-18, S-06). Para vehículos pesados o motos se entrenará otro modelo y se distribuirá por el mismo canal del punto 2.
4. Un modelo nuevo debe respetar el catálogo de RN-01 y venir con su matriz de confusión. Si cambia el catálogo, cambia la versión mayor y el servidor la acepta solo si conoce sus tipos.
5. **Con Play Store (futuro):** las actualizaciones de la app llegan por Google Play automáticamente; el modelo puede seguir descargándose desde R2 (es un archivo de datos, no código ejecutable) o migrarse a la entrega de recursos de Google Play cambiando solo el componente Actualizador de modelo.

**Consecuencias**
- ✅ Clasificación < 50 ms sin internet; el modelo tiene validación de campo en Ayacucho–Vinchos.
- ⚠️ El catálogo de tipos (RN-01: Bache, Hundimiento y la clase interna Normal) debe coincidir con las etiquetas del modelo y estar respaldado por su matriz de confusión por clase.
- ✅ Un modelo mejorado llega a los celulares sin publicar un APK nuevo.
- ⚠️ Conviven varias versiones del modelo en campo; el servidor las acepta gracias a `modeloVersion` y al `JSONB` de recepción (ADR-014).
- ⚠️ El servidor debe poder recibir eventos de varias versiones de modelo.

---

## ADR-005 — Ingesta durable e idempotente

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-02, DA-05

**Contexto y problema.** Responder HTTP 200 al *encolar* en un Channel en memoria hace que un reinicio pierda eventos que la app ya marcó `SYNCED` (viola RF-06 y RNF-06). Además, el lote se encola evento por evento (un lote parcial genera duplicados al reintentar) y no hay deduplicación.

| Opción | Pros | Contras |
|---|---|---|
| 200 al encolar en memoria (estado actual) | Muy rápido | Pierde eventos en reinicio |
| Procesar todo el lote antes de responder | Sin pérdida | Latencia alta con 300 dispositivos |
| **Persistir el lote en una tabla de recepción y luego procesar de forma asíncrona** | Durable, rápido, conserva el absorbedor de picos, da trazabilidad | Una tabla adicional |
| Broker externo (RabbitMQ, etc.) | Robusto | Complejidad y costo inviables (R-09, DA-08) |

**Decisión**
1. El controlador aplica a **cada evento** las reglas de recepción de RN-11 (esquema, confidence, GPS, timestamp, tipo y distancia a la polilínea cargada en memoria), sin consultar la BD. Los que fallan salen en `rechazados[]` y no se guardan; un evento mal formado nunca hace fallar el lote (el 400 queda solo para un lote ilegible, vacío o de más de 50).
2. Inserta los eventos aceptados en la tabla `evento_recibido` con un único `INSERT … ON CONFLICT DO NOTHING` sobre `evento_id`, y **solo entonces responde 200** con el resultado por evento.
3. El Channel queda como **aviso de trabajo** (el elemento es el lote completo, no el evento); al arrancar, el `BackgroundService` reprocesa las filas pendientes.
4. El módulo Anomalías (validación, confirmación y clasificación) procesa de forma asíncrona, **serializando por celda geográfica** (bloqueo consultivo de PostgreSQL por celda) para evitar carreras al confirmar y al crear anomalías. **Anomalías es dueño de su transacción y de su bloqueo**; Ingesta solo lo invoca y marca `PROCESADO` después de una respuesta correcta.
5. **Idempotencia de extremo a extremo:** `evento_recibido.evento_id` (Ingesta) y `reporte_anomalia.evento_id` (Anomalías) son únicos. Si el proceso se cae entre ambos pasos, el reintento no duplica nada (*at-least-once* + receptor idempotente).

**Consecuencias**
- ✅ RNF-06 pasa a ser verificable: aceptados = procesados + descartados + pendientes.
- ✅ Los descartes se cuentan como métricas, no como filas de auditoría.
- ⚠️ La tabla se purga a los 30 días (RNF-25).
- ⚠️ Cambia el flujo de C3 y C4 (HTTP 200 tras el *commit*).

---

## ADR-006 — Caché en memoria (`IMemoryCache`)

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-05, DA-08, DA-09

**Contexto y problema.** La consulta del mapa debe responder en < 250 ms. Algunos documentos antiguos mencionaban Redis.

| Opción | Pros | Contras |
|---|---|---|
| **`IMemoryCache` dentro de la API** | Costo cero; sin servicio extra | No compartida entre instancias |
| Redis | Compartida entre instancias | Un contenedor más que operar (R-09) |
| Sin caché | Simple | Riesgo para RNF-01 |

**Decisión.** `IMemoryCache` **dentro del módulo Consulta de Mapa** (no en `Shared`), usada a través de **HybridCache** de .NET, cuyo nivel 1 es justamente `IMemoryCache`: protege contra la estampida de peticiones simultáneas, invalida por etiqueta y admite Redis como nivel 2 sin cambiar el código si algún día hay más de una instancia (S-03). Diseño detallado en `diseño-interno-de-modulos.md`, sección 4. La invalidación usa el **patrón Observer con eventos de dominio en proceso**: después del *commit*, Anomalías publica `AnomaliaCambiada(anomaliaId, celdas)` y **no sabe quién escucha**; Consulta de Mapa se suscribe y borra sus celdas. Futuros interesados (notificaciones v1.5, empresas v2.0) se suscriben sin modificar Anomalías (principio abierto/cerrado). El publicador es una lista de suscriptores en memoria: sin broker y sin costo. Las claves se calculan por **celdas de rejilla y alcance del rol** (el `bbox` se ajusta a celdas; «anónimo», «registrado» y «administrador» nunca comparten una entrada) para que se reutilicen sin mezclar datos. **Solo las respuestas a anónimos** llevan `Cache-Control: public` corto y `ETag` para que Cloudflare las cachee; las respuestas a peticiones con token llevan `Cache-Control: private, no-store`, y la regla de caché de Cloudflare excluye las peticiones con cabecera `Authorization`. Así la caché nunca le entrega a un anónimo el corredor completo que pidió un registrado (R-07, RNF-27).

**Consecuencias**
- ✅ Sin componentes adicionales.
- ⚠️ Si se despliega más de una instancia (supuesto S-03), debe revisarse y migrar a Redis.
- ⚠️ Redis queda eliminado de toda la documentación vigente.

---

## ADR-007 — Privacidad por diseño

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-07

**Contexto y problema.** Un hash estable del dispositivo junto con ubicación y hora es **seudonimización, no anonimización**: con pocos días se infieren domicilio y trabajo. Un SHA-256 sin sal de un identificador de baja entropía es reversible por fuerza bruta. Además HU-10 (historial) y las cuentas de conductor podrían vincular identidad con trayectorias.

| Opción | Pros | Contras |
|---|---|---|
| Hash SHA-256 estable del `installationId` (estado actual) | Simple | Seudónimo permanente; trazabilidad inferible |
| **UUID aleatorio, seudónimo rotativo, purga a 72 h, historial local** | Minimiza datos y vínculos | Más lógica en la app |
| Sin identificador de dispositivo | Máxima privacidad | Imposible confirmar con ≥ 2 dispositivos |

**Decisión**
1. La instalación genera un **UUID aleatorio** (no derivado de hardware).
2. Al servidor se envía un **seudónimo rotativo** (rotación mensual).
3. El servidor **purga el seudónimo a las 72 h de recibir el evento**, tanto de los reportes como de la copia de recepción `evento_recibido` (solo se necesita para la confirmación, RN-06, y la plausibilidad, RN-15; RN-12 usa días distintos). Tras restaurar un backup se repite la purga (RN-10).
4. El **historial de recorridos es solo local**: la cuenta nunca se une a los eventos.
5. La exportación no incluye seudónimos.
6. Se documenta una evaluación breve de impacto en privacidad, incluido el flujo transfronterizo hacia todos los proveedores que reciben datos (Cloudflare, Oracle en Santiago de Chile, el proveedor de correo y Grafana Cloud, R-06), y se informa en la política de privacidad junto con sus finalidades (RF-29). El conductor sin cuenta también acepta el aviso de forma explícita.

**Consecuencias**
- ✅ Cumple R-06 sin perder la confirmación por varios dispositivos.
- ⚠️ La persistencia (RN-12) debe calcularse sin depender del seudónimo.
- ⚠️ Un mismo dispositivo puede contar como dos tras rotar el seudónimo, dentro de la ventana de 72 h (impacto menor).

---

## ADR-008 — Ciclo de vida de la anomalía

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-13

**Contexto y problema.** Hoy solo existe `Episodica | Persistente`. Un bache reparado o una inundación seguiría en el mapa para siempre y generaría falsas alarmas (HU-09). Un evento de severidad alta sin confirmar queda `Confirmada = false` indefinidamente.

| Opción | Pros | Contras |
|---|---|---|
| Sin caducidad (estado actual) | Simple | El mapa se llena de anomalías obsoletas |
| **Estados + caducidad parametrizable + moderación** | Mapa fiel a la realidad | Un job y un campo `UltimaDeteccionUtc` |

**Decisión.** Estados `Activa`, `PendienteConfirmacion`, `Expirada`, `Reparada` y `Descartada` (diagrama en RN-07). Valores iniciales: episódica expira a las 72 h sin reportes; persistente expira a los 7 días sin reportes; pendiente expira a las 72 h sin segundo dispositivo; el administrador puede marcar `Reparada` o `Descartada` (RF-24). Un `BackgroundService` aplica la caducidad **cada hora**, con la hora de detección del último reporte (RN-07); una anomalía `Expirada` o `Reparada` que recibe un reporte vuelve a nacer y pasa otra vez por la confirmación (RN-06). El mapa y las alertas usan solo `Activa`. Los parámetros se calibran con el piloto.

**Consecuencias**
- ✅ Menos falsas alarmas.
- ⚠️ Cambia el agregado `Anomalia` en C4 (campo `Estado` y `UltimaDeteccionUtc`).

---

## ADR-009 — Identidad y accesos

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-10, DA-17

**Contexto y problema.** El conductor registrado existe en v1.0, pero no hay mecanismo de alta ni de sesión. Además, en C3 Autenticación y Usuarios dependen entre sí (Usuarios valida sesión con Autenticación, y Autenticación lee la tabla `usuarios` de Usuarios), lo que viola la regla de no tener ciclos entre módulos.

| Opción | Pros | Contras |
|---|---|---|
| Mantener dos módulos con un ciclo | Sin cambios | Viola la regla 7 de C3 |
| Dos módulos, sin ciclo (contrato `IUsuarioCredenciales`) | Conserva la separación | Frontera artificial: comparten tabla y ritmo de cambio |
| **Un módulo «Identidad y Accesos»** | Sin ciclos; menos interfaces | Cambia la lista de módulos de C3 |

**Decisión**
1. Fusionar en **Identidad y Accesos** (cuentas, credenciales, roles).
2. La validación de sesión pasa a ser **middleware transversal** de la API REST.
3. Registro e inicio de sesión de conductor en v1.0 con correo y contraseña. **El correo transaccional entra en v1.0** (2026-10-13) para verificar la dirección (HU-18b) y **recuperar la contraseña** (HU-25, RF-32); sin recuperación, una contraseña olvidada es una cuenta perdida. Se integra solo desde Identidad con un adaptador (`IEnviadorCorreo`), para cambiar de proveedor sin tocar el resto.
4. **Tokens de acceso de 5 min para todos los roles + tokens de actualización revocables** guardados en el esquema `identidad`:

   | Rol | Token de acceso (JWT) | Sesión (token de actualización) |
   |---|---|---|
   | Administrador | 5 min | Máximo 8 h absolutas (RF-16) |
   | Conductor Registrado | 5 min | 30 días, se extiende con el uso (RF-21) |

   Cada renovación verifica que la cuenta siga activa. Al desactivar o eliminar una cuenta se **revocan** sus tokens de actualización, así pierde el acceso en ≤ 5 min (RNF-26) sin consultar la BD en cada petición. Los tokens de actualización **rotan** en cada uso. Si se reutiliza uno viejo **después de 60 s**, se revoca toda la sesión; dentro de esos 60 s se devuelve el mismo par nuevo, porque en la sierra es normal que la respuesta de la renovación se pierda y la app reintente con el token anterior. El acceso offline al corredor usa el derecho de acceso local, no el token; su vigencia, su revocación y el cierre de sesión siguen RN-21.
   - *Descartado:* JWT de 8 h sin revocación (incumple RNF-26) y consultar la BD en cada petición (más carga sin necesidad).
5. El panel usa cookie `HttpOnly; Secure; SameSite=Strict`.
6. **Bloqueo temporal progresivo** por intentos fallidos para administradores y conductores, con los valores de RN-19, más protección anti-bot en el login.
7. La Consulta de Mapa recorta el `bbox` por rol (RF-14).

**Consecuencias**
- ✅ Habilita HU-08, HU-09 y HU-10.
- ⚠️ Cambia la lista de módulos y las relaciones de C3.

---

## ADR-010 — Despliegue y operación

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-08, DA-11

**Contexto y problema.** RNF-07 (backup), RNF-18 (rollback) y «solo Cloudflare alcanza el origen» no tienen mecanismo definido; el hosting del panel sigue pendiente.

| Opción | Pros | Contras |
|---|---|---|
| Exponer el servidor y filtrar IPs de Cloudflare | Simple | El origen sigue expuesto |
| **Cloudflare Tunnel hacia el origen + panel en Cloudflare Pages** | Sin puertos abiertos; panel estático gratuito con cabeceras CSP/HSTS | Dependencia de Cloudflare |

**Decisión**
- **Cloudflare Tunnel** para `api.killauru.pe` y **Cloudflare Pages** para `panel.killauru.pe` (cabeceras por archivo `_headers`).
- **Backup** diario cifrado a almacenamiento de objetos fuera de la VM (R2), retención de 30 días, y simulacro de restauración mensual (RPO ≤ 24 h, RTO ≤ 2 h; ≤ 4 h si se pierde la VM entera, RNF-24). La clave de cifrado y los demás secretos tienen copia fuera de la VM (RNF-34).
- **Rollback**: imágenes con *tag* inmutable; volver al tag anterior con `docker compose up -d`. Las migraciones se ejecutan como paso explícito, precedidas de un `pg_dump`, y en dos fases (agregar primero, borrar en un despliegue posterior), para que la versión anterior funcione con el esquema nuevo (RNF-18).
- **IP real del cliente:** detrás del túnel todas las peticiones llegan desde `cloudflared`, así que el límite de tasa usa la cabecera `CF-Connecting-IP` (RNF-11).
- **Salud:** `/health/live` para el reinicio automático de Docker y `/health/ready` (BD y antigüedad de la cola) para UptimeRobot (RF-28).
- **Observabilidad** (2026-10-13), con costo cero:

  | Pieza | Herramienta | Uso |
  |---|---|---|
  | Instrumentación | **OpenTelemetry** (incluido en .NET 10) | Latencia, errores y trazas sin código de métricas hecho a mano |
  | Logs | **Serilog** en JSON con ID de correlación | Cada log dice qué petición lo generó |
  | Paneles y alertas | **Grafana Cloud, plan gratuito** | p95, errores, descartes por motivo, cola; alerta por correo |
  | Disponibilidad | **UptimeRobot** + `/health/ready` | Caída en ≤ 10 min (RNF-20) y página de estado pública gratuita de UptimeRobot |

  **Control de costo (RNF-32):** cuenta sin tarjeta; muestreo de trazas ≤ 10 %; sin logs de `/health`; sin etiquetas de alta cardinalidad. No se expone `/metrics`: la API **envía** la telemetría a Grafana (OTLP), sin abrir puertos.
  - *Descartado:* métricas hechas a mano en tablas propias (reinventa lo que OpenTelemetry ya hace) y Prometheus + Grafana autoalojados (consumen memoria del único servidor y hay que operarlos).
- Vista de despliegue documentada en C4.

**Consecuencias**
- ✅ Origen oculto; panel sin servidor propio.
- ⚠️ Hay que confirmar los riesgos de la infraestructura gratuita (`05-restricciones.md`, RS-01…RS-05).

---

## ADR-011 — Mapa base vectorial offline

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-14

**Contexto y problema.** EQ-01 exige mapa sin señal y RNF-15 limita datos y almacenamiento. Las teselas raster en línea rompen ambos, y los servidores de teselas públicos de OpenStreetMap no permiten descarga masiva.

| Opción | Pros | Contras |
|---|---|---|
| Teselas raster en línea | Simple | Rompe offline y RNF-15 |
| Descargar teselas del servidor público de OSM | Sin infraestructura | Prohibido su uso masivo |
| **Mapa vectorial recortado al corredor, alojado como archivo estático** | Offline, liviano (hasta 80 MB, RNF-15), costo cero | Preparar y publicar el recorte |

**Decisión.**
1. Mapa base vectorial de OpenStreetMap recortado al corredor, en **un solo archivo PMTiles**, alojado como estático en **Cloudflare R2** (Cloudflare Pages no admite archivos de más de 25 MiB).
2. **Una sola librería de mapas en todo el sistema: MapLibre.** La app usa **MapLibre Native (Android)** y el panel usa **MapLibre GL JS**. Ambos leen **el mismo PMTiles** y comparten **el mismo estilo** (colores y símbolos por tipo y severidad).
3. La app descarga el archivo una vez, por Wi-Fi o con confirmación del usuario, de forma reanudable y verificando su SHA-256 con un manifiesto igual al del modelo (RF-26). Junto con el mapa descarga la **polilínea del corredor**, que usa para medir la distancia de aviso a lo largo de la vía (RN-16); el panel lo lee en línea por rangos HTTP, sin descargarlo entero.
4. Las anomalías son una capa aparte, actualizada con `updatedSince`. Se muestra la atribución de OpenStreetMap.

| Opción para el panel | Pros | Contras |
|---|---|---|
| Leaflet con teselas del servidor público de OSM | Muy simple | Su política prohíbe el uso en producción; mapa distinto al de la app; dos librerías que mantener |
| **MapLibre GL JS con el mismo PMTiles** | Un solo mapa base, un solo estilo, costo cero | Curva de aprendizaje algo mayor que Leaflet |

**Consecuencias**
- ✅ Mapa y alertas funcionan sin red; consumo de datos controlado.
- ✅ App y panel muestran exactamente el mismo mapa; un cambio en el recorte o en el estilo se publica una sola vez.
- ⚠️ Hay que regenerar el PMTiles cuando se quiera actualizar la cartografía de OSM (proceso manual, pocas veces al año). Al publicar una versión nueva se actualiza el manifiesto y las apps la descargan en el siguiente Wi-Fi.

---

## ADR-012 — Convenciones de diseño

**Estado:** Aceptado (revisión 2026-10-13) · **Drivers:** DA-09, DA-12

**Contexto y problema.** Hay nombres distintos para un mismo concepto (Sincronización/Ingesta, `Anomalia`/`AnomaliaVial`, Categoría/Tipo) y dos tecnologías de datos en los documentos (Dapper vs EF Core).

**Decisión**
- **Lenguaje ubicuo** en un glosario (`10-glosario.md`): términos de negocio en español (Anomalia, Reporte, Severidad, TipoAnomalia); términos técnicos de capas en inglés (Domain, Application, Adapters, Infrastructure).
- **Un esquema de PostgreSQL y un `DbContext` por módulo**, sin llaves foráneas entre esquemas (detalle en `componentes-arquitectonicos.md`, sección 6).
- **EF Core con NetTopologySuite** para agregados y escritura. Consultas de mapa en SQL parametrizado solo si el perfilado lo exige.
- Columna de ubicación `geography(Point, 4326)` con índice GiST.
- Todos los umbrales (`09-reglas-de-negocio.md`) por **configuración tipada, una clase por módulo** (`IngestaOptions`, `AnomaliasOptions`, `IdentidadOptions`, `MapaOptions`), no constantes ni una clase global (principio de segregación de interfaces).
- `Shared` contiene solo el kernel mínimo; los contratos de un módulo (p. ej. `EventoReportado`) viven en su carpeta `Contracts/`.
- `TimeProvider` para el reloj; día calendario local con **desfase fijo UTC−5** (Perú no usa horario de verano).

**Consecuencias**
- ✅ Un solo vocabulario y una sola ruta de datos.
- ⚠️ Hay que actualizar los documentos de diseño interno, principios y patrones.

---

## ADR-013 — Auditoría en la misma transacción que la acción

**Estado:** Aceptado (2026-10-13) · **Drivers:** DA-06, DA-16 · **Requisitos:** RF-15, RNF-29, RNF-30, RNF-31

**Contexto y problema.** Las acciones sensibles (cambio de rol, desactivación, eliminación de cuenta, moderación, exportación) escriben en la tabla de su módulo y además en `auditoria`. Si la acción se confirma y el registro falla, queda una acción **sin rastro**. El D.S. 016-2024-JUS exige gestionar y verificar accesos, y documentar y notificar incidentes en 48 h; sin un rastro confiable eso no es posible.

| Opción | Pros | Contras |
|---|---|---|
| Auditar después de la acción, sin garantía | Simple; módulos totalmente independientes | Puede quedar una acción sensible sin rastro |
| **Acción y auditoría en la misma transacción** | «No hay acción sensible sin rastro»; costo cero porque comparten la misma BD | Excepción a «cada módulo, su transacción» (ver consecuencias) |
| Bandeja de salida (*Outbox*) por módulo | Robusto y extraíble a servicios | Una tabla y un proceso más por módulo; desproporcionado para una persona |

**Decisión**
1. `IAuditoriaService.RegistrarAsync` se ejecuta **dentro de la transacción del módulo que actúa** (unidad de trabajo compartida sobre la misma conexión de PostgreSQL). Si la auditoría falla, la acción se revierte.
2. Es una **excepción documentada** a la regla de que cada módulo maneja su propia transacción (ADR-005): se admite porque Auditoría **solo inserta** filas, no bloquea ni modifica datos de otros módulos.
3. Los registros guardan el **identificador interno** del actor, nunca el correo de un titular que pidió eliminar su cuenta (RF-30).
4. Se audita también la **revisión mensual de privilegios** (RNF-30).

**Consecuencias**
- ✅ Trazabilidad garantizada para responder un incidente en ≤ 48 h (EQ-13).
- ⚠️ Si Auditoría se extrae alguna vez a otro servidor, debe pasarse a *Outbox*.

---

## ADR-014 — Estrategia de persistencia (políglota)

**Estado:** Aceptado (2026-10-13) · **Drivers:** DA-01, DA-06, DA-08, DA-14

**Contexto y problema.** En la industria es común combinar almacenes SQL y NoSQL (*persistencia políglota*). El criterio correcto no es «tener uno de cada tipo», sino guardar **cada dato en el almacén que mejor encaja con su forma, su acceso y su ciclo de vida**. Cada almacén adicional es un servicio más que pagar, respaldar, asegurar y monitorear (R-09, DA-08).

| Opción | Pros | Contras |
|---|---|---|
| Un solo almacén para todo | Mínima operación | Archivos grandes y caché en la BD relacional: costo y latencia innecesarios |
| Añadir una BD NoSQL dedicada desde v1.0 (p. ej. MongoDB o Redis) | Cumple «SQL + NoSQL» al pie de la letra | Ningún requisito la exige hoy; un contenedor, un respaldo y un punto de falla más |
| **Persistencia políglota guiada por la forma del dato, sin servicios nuevos** | Cada dato en su almacén natural; costo cero | Hay que documentar y vigilar los disparadores de cambio |

**Decisión.** Mapa de persistencia de v1.0:

| Dato | Forma y acceso | Almacén | Modelo |
|---|---|---|---|
| Anomalías, reportes, usuarios, sesiones, auditoría, métricas | Relaciones, transacciones ACID, consultas espaciales | **PostgreSQL + PostGIS** (un esquema por módulo, ADR-012) | SQL relacional |
| Evento tal como llegó (`ingesta.evento_recibido.payload`) | Documento cuya forma puede variar según `modeloVersion` | Columna **`JSONB`** en PostgreSQL | Documental (estilo NoSQL) dentro de SQL |
| Buffer de eventos y anomalías locales en el celular | Cola offline y consultas por cercanía | **SQLite** en el dispositivo | SQL embebido |
| Mapa base (PMTiles), backups cifrados, archivos `.tflite` versionados | Archivos grandes leídos por nombre | **Cloudflare R2** | NoSQL: almacenamiento de objetos (clave → archivo) |
| Caché del mapa por celdas | Clave → GeoJSON temporal | **`IMemoryCache`** en Consulta de Mapa | NoSQL: clave-valor en memoria |

**Disparadores para añadir un almacén NoSQL dedicado** (se evalúan con métricas reales, no por adelantado):

| Si ocurre… | Entonces | Tipo |
|---|---|---|
| Hay más de una instancia del backend (S-03) | Redis/Valkey para caché compartida y límite de tasa | Clave-valor |
| Las métricas superan ~10 millones de filas o se necesitan consultas de series largas | Base de series de tiempo (p. ej. TimescaleDB como extensión, o un servicio gestionado) | Series de tiempo |
| La analítica de tramos para empresas (v1.5+) exige consultas pesadas que afectan a la BD transaccional | Réplica de lectura o almacén analítico separado | Columnar |

**Consecuencias**
- ✅ El sistema ya combina SQL relacional, SQL embebido, objetos y clave-valor sin añadir servicios ni costo.
- ✅ `JSONB` permite aceptar eventos de varias versiones del modelo sin migrar el esquema (ADR-004).
- ⚠️ Los datos en R2 y en `IMemoryCache` no participan de las transacciones de PostgreSQL: la caché se invalida por eventos (ADR-006) y los backups se verifican con un simulacro mensual (RNF-07).

---

## ADR-015 — Integridad del dato contra eventos falsos

**Estado:** Aceptado (2026-10-13) · **Drivers:** DA-15 · **Reglas:** RN-13, RN-14, RN-15 · **Escenario:** EQ-12

**Contexto y problema.** La API de ingesta es pública y acepta eventos anónimos. El APK se puede analizar y su protocolo, imitar; como se distribuye fuera de Play Store (S-02), no existe la verificación de integridad de Google Play. Un script podría inventar baches en cualquier parte o confirmar baches graves con seudónimos falsos, y destruir la confianza en el mapa. Además, la defensa **no puede castigar al conductor real**: tras horas sin señal sube muchos eventos de golpe, y en temporada de lluvia detecta cientos por hora.

| Opción | Pros | Contras |
|---|---|---|
| No hacer nada (solo límite de tasa) | Simple | Cualquiera publica o confirma baches falsos |
| Exigir cuenta para enviar eventos | Atribuye cada evento | Rompe la detección anónima y pasiva; menos datos; choca con la privacidad (ADR-007) |
| Verificación de integridad de Google Play | Fuerte: certifica app y dispositivo | Solo funciona con Play Store; no disponible en el MVP |
| Límite fijo de eventos por hora que **descarta** el exceso | Simple | Pierde datos reales en lluvia y tras horas offline |
| **Defensas por contenido, sin descartar datos reales** | Detiene los ataques baratos, conserva el anonimato y no castiga picos legítimos | Más lógica en el dominio; un atacante con varias cuentas reales aún podría confirmar |

**Decisión.** El servidor juzga cada evento por **su contenido** (ubicación y hora de detección), **nunca por el momento en que llega**:
1. **Solo sobre la vía (RN-13):** se descarta todo evento a más de 50 m del eje de la PE-28A (polilínea `anomalias.corredor_via`).
2. **Sesión para confirmar lo grave (RN-14):** una anomalía de severidad 4–5 pasa a `Activa` solo si al menos uno de los dispositivos que la confirman envió su reporte con sesión. El reporte guarda solo `con_sesion` (sí/no), nunca la cuenta.
3. **Plausibilidad del dispositivo (RN-15):** (a) si un seudónimo supera 120 eventos por hora **de detección**, sus eventos se guardan pero exigen confirmación de otro dispositivo, en lugar de descartarse; (b) se descartan los saltos de posición que implican más de 150 km/h.
4. Los descartes y retenciones se cuentan por motivo en la telemetría, con alerta en Grafana (ADR-010).

**Consecuencias**
- ✅ Un script con un solo dispositivo no puede publicar baches fuera de la vía, ni confirmar graves, ni inundar el mapa.
- ✅ Ningún evento real se pierde por picos legítimos (lluvia, subida tras horas offline).
- ⚠️ Riesgo residual: varios atacantes con cuentas reales coordinadas. Se mitiga con la moderación del administrador (`Descartada`, RN-07) y la revisión de las métricas de descartes.
- 🔜 **Disparador:** al publicar en Play Store, añadir la verificación de integridad de Google Play como señal adicional en Ingesta.

---

## Decisiones abiertas

| # | Tema | Opciones | Responsable |
|---|---|---|---|
| ~~1~~ | ~~Granularidad de módulos~~ | **Cerrada (2026-10-13):** un módulo «Anomalías» con el flujo interno validar → confirmar → clasificar | Autor |
| ~~2~~ | ~~Definición de «severidad alta»~~ | **Cerrada (2026-10-13):** 4 o 5 (RN-02, RN-06) | Autor |
| ~~3~~ | ~~Catálogo de `TipoAnomalia`~~ | **Cerrada (2026-10-13):** Bache y Hundimiento (RN-01) | Autor |
| ~~4~~ | ~~Confirmación falsificable por anónimos~~ | **Cerrada (2026-10-13), ver ADR-015:** solo eventos sobre la vía (RN-13), sesión para confirmar severidad 4–5 (RN-14), velocidad plausible y, si un dispositivo supera el tope por hora de detección, confirmación obligatoria en lugar de descarte para no perder datos reales en temporada de lluvia (RN-15). La verificación de integridad de Play queda para cuando se publique en Play Store | Autor |
| ~~5~~ | ~~Actualización del modelo TFLite~~ | **Cerrada (2026-10-13):** descarga versionada desde R2 con manifiesto y SHA-256, solo con Wi-Fi (ADR-004) | Autor |
| ~~6~~ | ~~Cuarto subdominio~~ | **Cerrada (2026-10-13):** `cdn.killauru.pe` publica R2 (mapa base y modelos) | Autor |
| ~~7~~ | ~~Tope de costo RNF-16~~ | **Cerrada (2026-10-13):** ≤ US$ 10/mes; región Oracle Santiago (Chile) | Autor |