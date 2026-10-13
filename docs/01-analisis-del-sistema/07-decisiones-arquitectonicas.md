# Decisiones Arquitectónicas

> **Proyecto:** KillaUru · **Versión:** 1.1 · **Revisión:** 2026-10-12 · **Estado:** Vigente
> Un ADR (Architecture Decision Record) documenta una decisión importante: el contexto que la motivó, las alternativas evaluadas, la decisión tomada y sus consecuencias.
> **Estados:** **Aceptado** (ya reflejado en C2–C4)

## Resumen de decisiones

| ID | Decisión | Estado | Drivers | Resultado |
|---|---|---|---|---|
| **ADR-001** | Monolito modular | Aceptado | DA-06, DA-09, DA-12 | Un proceso con 7 módulos: Identidad y Accesos, Ingesta, Anomalías, Consulta de Mapa, Dashboard, Exportación y Auditoría |
| **ADR-002** | Clean Architecture por módulo | Aceptado | DA-06, DA-09 | Capas Domain, Application, Adapters e Infrastructure; puertos en Application; `Contracts/` entre módulos |
| **ADR-003** | Store-and-Forward con SQLite | Aceptado | DA-01, DA-02 | Buffer local PENDING/SYNCED/REJECTED_LOCAL con reintentos y *jitter* |
| **ADR-004** | Edge AI con TFLite embebido | Aceptado | DA-03, DA-04 | Modelo MLP cuantizado en el APK; `modeloVersion` en cada evento |
| **ADR-005** | Ingesta durable e idempotente | Aceptado | DA-02, DA-05 | Tabla de recepción + Channel como aviso + HTTP 200 tras el *commit* |
| **ADR-006** | Caché en memoria (`IMemoryCache`) | Aceptado | DA-06, DA-08 | Caché por celdas de rejilla con invalidación explícita; Redis diferido |
| **ADR-007** | Privacidad por diseño | Aceptado | DA-07 | Seudónimo rotativo, purga a 72 h, historial local |
| **ADR-008** | Ciclo de vida de la anomalía | Aceptado | DA-13 | Estados y caducidad parametrizable |
| **ADR-009** | Identidad y accesos | Aceptado | DA-10 | Módulo unificado (fusión de Autenticación y Usuarios), JWT diferenciado por rol, alta de conductor en v1.0 |
| **ADR-010** | Despliegue y operación | Aceptado | DA-08, DA-11 | Cloudflare Tunnel y Pages, backups, rollback por tag |
| **ADR-011** | Mapa base vectorial offline | Aceptado | DA-14 | PMTiles del corredor en R2; MapLibre en app y panel |
| **ADR-012** | Convenciones de diseño | Aceptado | DA-12 | Lenguaje ubicuo, una tecnología de datos, umbrales por configuración |

---

## ADR-001 — Monolito Modular

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-06, DA-09, DA-12

**Contexto y problema.** KillaUru lo desarrolla y opera una sola persona con un plazo de 6 a 8 semanas. Los microservicios exigirían gestionar varios servicios y una infraestructura compleja, lo que haría inviable el proyecto; además no son óptimos cuando un proyecto recién inicia.

| Opción | Pros | Contras |
|---|---|---|
| Microservicios | Escalado independiente por módulo | Complejidad operacional inviable para un solo desarrollador |
| Monolito tradicional | Simple de desarrollar | Difícil de mantener y escalar conforme crece |
| **Monolito modular** | Simple de desplegar; módulos separados; puede evolucionar hacia servicios | Requiere disciplina para mantener los límites |

**Decisión.** Monolito modular en ASP.NET Core (.NET 10 LTS): un solo proceso con *bounded contexts* internos (7 módulos: se unificaron Autenticación y Usuarios en Identidad y Accesos según ADR-009, y Validación y Clasificación en **Anomalías**). El backend **no** incluye módulos de detección ni de sincronización: esas funciones viven en la app Android (ADR-003, ADR-004).

**Consecuencias**
- ✅ `docker compose up -d` levanta todo el sistema.
- ✅ Los módulos con interfaces claras pueden extraerse como servicios (candidatos naturales: Ingesta y Consulta de Mapa).
- ⚠️ Requiere disciplina: se hace cumplir con pruebas de arquitectura (RNF-28).
- ✅ **Validación y Clasificación se unen en el módulo Anomalías** (2026-10-13): comparten agregado, tabla, bloqueo por celda y momento de ejecución; separarlas era una frontera artificial. Dentro del módulo se conservan como pasos del flujo (validar → confirmar → clasificar).
- ⚠️ Las métricas operativas pasan a `Shared/Metricas` (puerto técnico, como la caché) para evitar el ciclo Anomalías ↔ Dashboard.

---

## ADR-002 — Clean Architecture por módulo

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-06, DA-09

**Contexto y problema.** Con un solo desarrollador, el código debe ser fácil de modificar y probar. Si la lógica de negocio se mezcla con controladores o base de datos, cualquier cambio tecnológico rompe el sistema.

| Opción | Pros | Contras |
|---|---|---|
| MVC tradicional | Simple y conocido | Lógica acoplada a frameworks y BD |
| **Clean Architecture** | Dominio independiente; casos de uso explícitos; fácil de probar | Más carpetas y disciplina inicial |
| Hexagonal (Ports & Adapters) | Prácticamente equivalente en la separación | Se prefiere el vocabulario de *casos de uso* de Clean para organizar el código |

**Decisión.** Cuatro capas por módulo: **Domain, Application, Adapters, Infrastructure**, con dependencias solo hacia adentro. Los **puertos viven en Application**. Los módulos se comunican solo mediante interfaces de su carpeta `Contracts/`. Las reglas se verifican con pruebas de arquitectura.

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
- ⚠️ Hay que definir la retención local de eventos `SYNCED` (RNF-15).

---

## ADR-004 — Edge AI con TFLite para clasificación local

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-03, DA-04

**Contexto y problema.** El conductor no puede tocar el celular mientras maneja. La clasificación debe ser automática, en tiempo real y sin internet.

| Opción | Pros | Contras |
|---|---|---|
| Enviar señal cruda al servidor | Modelo más potente en la nube | Requiere internet permanente; latencia inaceptable |
| Reglas manuales con umbrales fijos | Sin dependencia de ML | Baja precisión; no distingue tipos |
| **TFLite embebido en la app** | Inferencia < 50 ms; sin internet; modelo < 5 MB | Actualizar el modelo requiere nueva versión de la app |

**Decisión.** Modelo MLP cuantizado convertido a TFLite, embebido en el APK. Clasifica ventanas de 500 ms y registra el evento si confidence ≥ 0.75. Cada evento incluye `modeloVersion`.

**Consecuencias**
- ✅ Clasificación < 50 ms sin internet; el modelo tiene validación de campo en Ayacucho–Vinchos.
- ⚠️ El catálogo de tipos (RN-01: Bache, Hundimiento y la clase interna Normal) debe coincidir con las etiquetas del modelo y estar respaldado por su matriz de confusión por clase.
- ⚠️ Con distribución por APK, el modelo queda congelado en campo. **Decisión abierta:** descarga versionada del `.tflite` desde el servidor.
- ⚠️ El servidor debe poder recibir eventos de varias versiones de modelo.

---

## ADR-005 — Ingesta durable e idempotente

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-02, DA-05

**Contexto y problema.** Responder HTTP 200 al *encolar* en un Channel en memoria hace que un reinicio pierda eventos que la app ya marcó `SYNCED` (viola RF-06 y RNF-06). Además, el lote se encola evento por evento (un lote parcial genera duplicados al reintentar) y no hay deduplicación.

| Opción | Pros | Contras |
|---|---|---|
| 200 al encolar en memoria (estado actual) | Muy rápido | Pierde eventos en reinicio |
| Procesar todo el lote antes de responder | Sin pérdida | Latencia alta con 300 dispositivos |
| **Persistir el lote en una tabla de recepción y luego procesar de forma asíncrona** | Durable, rápido, conserva el absorbedor de picos, da trazabilidad | Una tabla adicional |
| Broker externo (RabbitMQ, etc.) | Robusto | Complejidad y costo inviables (R-09, DA-08) |

**Decisión**
1. El controlador valida esquema y reglas intrínsecas puras de `ReglasCalidad` (sin BD).
2. Inserta el lote en la tabla `evento_recibido` con un único `INSERT … ON CONFLICT DO NOTHING` sobre `evento_id`, y **solo entonces responde 200** con el resultado por evento.
3. El Channel queda como **aviso de trabajo** (el elemento es el lote completo, no el evento); al arrancar, el `BackgroundService` reprocesa las filas pendientes.
4. El módulo Anomalías (validación, confirmación y clasificación) procesa de forma asíncrona, **serializando por celda geográfica** (bloqueo consultivo de PostgreSQL por celda) para evitar carreras al confirmar y al crear anomalías.

**Consecuencias**
- ✅ RNF-06 pasa a ser verificable: recibidos = procesados + descartados + pendientes.
- ✅ Los descartes se cuentan como métricas, no como filas de auditoría.
- ⚠️ La tabla se purga a los 30 días (RNF-25).
- ⚠️ Cambia el flujo de C3 y C4 (HTTP 200 tras el *commit*).

---

## ADR-006 — Caché en memoria (`IMemoryCache`)

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-06, DA-08

**Contexto y problema.** La consulta del mapa debe responder en < 250 ms. Algunos documentos antiguos mencionaban Redis.

| Opción | Pros | Contras |
|---|---|---|
| **`IMemoryCache` dentro de la API** | Costo cero; sin servicio extra | No compartida entre instancias |
| Redis | Compartida entre instancias | Un contenedor más que operar (R-09) |
| Sin caché | Simple | Riesgo para RNF-01 |

**Decisión.** `IMemoryCache`, con invalidación explícita por el puerto `ICacheMapa` al persistir una anomalía. Las claves se calculan por **celdas de rejilla** (el `bbox` se ajusta a celdas) para que se reutilicen. Las respuestas públicas llevan `Cache-Control` corto y `ETag` para que Cloudflare las cachee.

**Consecuencias**
- ✅ Sin componentes adicionales.
- ⚠️ Si se despliega más de una instancia (supuesto S-03), debe revisarse y migrar a Redis.
- ⚠️ Redis queda eliminado de toda la documentación vigente.

---

## ADR-007 — Privacidad por diseño

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-07

**Contexto y problema.** Un hash estable del dispositivo junto con ubicación y hora es **seudonimización, no anonimización**: con pocos días se infieren domicilio y trabajo. Un SHA-256 sin sal de un identificador de baja entropía es reversible por fuerza bruta. Además HU-10 (historial) y las cuentas de conductor podrían vincular identidad con trayectorias.

| Opción | Pros | Contras |
|---|---|---|
| Hash SHA-256 estable del `installationId` (estado actual) | Simple | Seudónimo permanente; trazabilidad inferible |
| **UUID aleatorio, seudónimo rotativo, purga a 72 h, historial local** | Minimiza datos y vínculos | Más lógica en la app |
| Sin identificador de dispositivo | Máxima privacidad | Imposible confirmar con ≥ 2 dispositivos |

**Decisión**
1. La instalación genera un **UUID aleatorio** (no derivado de hardware).
2. Al servidor se envía un **seudónimo rotativo** (rotación mensual).
3. El servidor **purga el seudónimo de los reportes a las 72 h** (solo se necesita para la confirmación; RN-12 usa días distintos).
4. El **historial de recorridos es solo local**: la cuenta nunca se une a los eventos.
5. La exportación no incluye seudónimos.
6. Se documenta una evaluación breve de impacto en privacidad, incluido el flujo transfronterizo (Cloudflare y región de nube).

**Consecuencias**
- ✅ Cumple R-06 sin perder la confirmación por varios dispositivos.
- ⚠️ La persistencia (RN-12) debe calcularse sin depender del seudónimo.
- ⚠️ Un mismo dispositivo puede contar como dos tras rotar el seudónimo, dentro de la ventana de 72 h (impacto menor).

---

## ADR-008 — Ciclo de vida de la anomalía

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-13

**Contexto y problema.** Hoy solo existe `Episodica | Persistente`. Un bache reparado o una inundación seguiría en el mapa para siempre y generaría falsas alarmas (HU-09). Un evento de severidad alta sin confirmar queda `Confirmada = false` indefinidamente.

| Opción | Pros | Contras |
|---|---|---|
| Sin caducidad (estado actual) | Simple | El mapa se llena de anomalías obsoletas |
| **Estados + caducidad parametrizable + moderación** | Mapa fiel a la realidad | Un job y un campo `UltimoReporteUtc` |

**Decisión.** Estados `Activa`, `PendienteConfirmacion`, `Expirada`, `Reparada` y `Descartada` (diagrama en RN-07). Valores iniciales: episódica expira a las 72 h sin reportes; pendiente expira a las 72 h sin segundo dispositivo; el administrador puede marcar `Reparada` o `Descartada` (RF-24). Un `BackgroundService` diario aplica la caducidad. El mapa y las alertas usan solo `Activa`. Los parámetros se calibran con el piloto.

**Consecuencias**
- ✅ Menos falsas alarmas.
- ⚠️ Cambia el agregado `Anomalia` en C4 (campo `Estado` y `UltimoReporteUtc`).

---

## ADR-009 — Identidad y accesos

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-10

**Contexto y problema.** El conductor registrado existe en v1.0, pero no hay mecanismo de alta ni de sesión. Además, en C3 Autenticación y Usuarios dependen entre sí (Usuarios valida sesión con Autenticación, y Autenticación lee la tabla `usuarios` de Usuarios), lo que viola la regla de no tener ciclos entre módulos.

| Opción | Pros | Contras |
|---|---|---|
| Mantener dos módulos con un ciclo | Sin cambios | Viola la regla 7 de C3 |
| Dos módulos, sin ciclo (contrato `IUsuarioCredenciales`) | Conserva la separación | Frontera artificial: comparten tabla y ritmo de cambio |
| **Un módulo «Identidad y Accesos»** | Sin ciclos; menos interfaces | Cambia la lista de módulos de C3 |

**Decisión**
1. Fusionar en **Identidad y Accesos** (cuentas, credenciales, roles).
2. La validación de sesión pasa a ser **middleware transversal** de la API REST.
3. Registro e inicio de sesión de conductor en v1.0 con correo y contraseña; verificación por correo diferida (HU-18b).
4. JWT de administrador de 8 h (RF-16); JWT de conductor de corta duración con *refresh*. La desactivación de cuentas se aplica en ≤ 5 min (RNF-26).
5. El panel usa cookie `HttpOnly; Secure; SameSite=Strict`.
6. **Bloqueo temporal progresivo** tras 3 intentos fallidos, más protección anti-bot en el login.
7. La Consulta de Mapa recorta el `bbox` por rol (RF-14).

**Consecuencias**
- ✅ Habilita HU-08, HU-09 y HU-10.
- ⚠️ Cambia la lista de módulos y las relaciones de C3.

---

## ADR-010 — Despliegue y operación

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-08, DA-11

**Contexto y problema.** RNF-07 (backup), RNF-18 (rollback) y «solo Cloudflare alcanza el origen» no tienen mecanismo definido; el hosting del panel sigue pendiente.

| Opción | Pros | Contras |
|---|---|---|
| Exponer el servidor y filtrar IPs de Cloudflare | Simple | El origen sigue expuesto |
| **Cloudflare Tunnel hacia el origen + panel en Cloudflare Pages** | Sin puertos abiertos; panel estático gratuito con cabeceras CSP/HSTS | Dependencia de Cloudflare |

**Decisión**
- **Cloudflare Tunnel** para `api.killauru.pe` y **Cloudflare Pages** para `panel.killauru.pe` (cabeceras por archivo `_headers`).
- **Backup** diario cifrado a almacenamiento de objetos, retención de 30 días, y simulacro de restauración mensual (RPO ≤ 24 h, RTO ≤ 2 h).
- **Rollback**: imágenes con *tag* inmutable; volver al tag anterior con `docker compose up -d`; migraciones compatibles hacia atrás.
- **Observabilidad**: `/health` (liveness y readiness), logs estructurados, métricas agregadas por minuto; `/metrics` no público.
- Vista de despliegue documentada en C4.

**Consecuencias**
- ✅ Origen oculto; panel sin servidor propio.
- ⚠️ Hay que confirmar los riesgos de la infraestructura gratuita (`05-restricciones.md`, RS-01…RS-05).

---

## ADR-011 — Mapa base vectorial offline

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-14

**Contexto y problema.** EQ-01 exige mapa sin señal y RNF-15 limita datos y almacenamiento. Las teselas raster en línea rompen ambos, y los servidores de teselas públicos de OpenStreetMap no permiten descarga masiva.

| Opción | Pros | Contras |
|---|---|---|
| Teselas raster en línea | Simple | Rompe offline y RNF-15 |
| Descargar teselas del servidor público de OSM | Sin infraestructura | Prohibido su uso masivo |
| **Mapa vectorial recortado al corredor, alojado como archivo estático** | Offline, liviano (decenas de MB), costo cero | Preparar y publicar el recorte |

**Decisión.**
1. Mapa base vectorial de OpenStreetMap recortado al corredor, en **un solo archivo PMTiles**, alojado como estático en **Cloudflare R2** (Cloudflare Pages no admite archivos de más de 25 MiB).
2. **Una sola librería de mapas en todo el sistema: MapLibre.** La app usa **MapLibre Native (Android)** y el panel usa **MapLibre GL JS**. Ambos leen **el mismo PMTiles** y comparten **el mismo estilo** (colores y símbolos por tipo y severidad).
3. La app descarga el archivo una vez, por Wi-Fi o con confirmación del usuario; el panel lo lee en línea por rangos HTTP, sin descargarlo entero.
4. Las anomalías son una capa aparte, actualizada con `updatedSince`. Se muestra la atribución de OpenStreetMap.

| Opción para el panel | Pros | Contras |
|---|---|---|
| Leaflet con teselas del servidor público de OSM | Muy simple | Su política prohíbe el uso en producción; mapa distinto al de la app; dos librerías que mantener |
| **MapLibre GL JS con el mismo PMTiles** | Un solo mapa base, un solo estilo, costo cero | Curva de aprendizaje algo mayor que Leaflet |

**Consecuencias**
- ✅ Mapa y alertas funcionan sin red; consumo de datos controlado.
- ✅ App y panel muestran exactamente el mismo mapa; un cambio en el recorte o en el estilo se publica una sola vez.
- ⚠️ Hay que regenerar el PMTiles cuando se quiera actualizar la cartografía de OSM (proceso manual, pocas veces al año).

---

## ADR-012 — Convenciones de diseño

**Estado:** Aceptado (revisión 2026-10-12) · **Drivers:** DA-12

**Contexto y problema.** Hay nombres distintos para un mismo concepto (Sincronización/Ingesta, `Anomalia`/`AnomaliaVial`, Categoría/Tipo) y dos tecnologías de datos en los documentos (Dapper vs EF Core).

**Decisión**
- **Lenguaje ubicuo** en un glosario: términos de negocio en español (Anomalia, Reporte, Severidad, TipoAnomalia); términos técnicos de capas en inglés (Domain, Application, Adapters, Infrastructure).
- **EF Core con NetTopologySuite** para agregados y escritura. Consultas de mapa en SQL parametrizado solo si el perfilado lo exige.
- Columna de ubicación `geography(Point, 4326)` con índice GiST.
- Todos los umbrales (RN-11, RN-12, caducidad) por **configuración tipada**, no constantes.
- `TimeProvider` para el reloj; día calendario local con **desfase fijo UTC−5** (Perú no usa horario de verano).

**Consecuencias**
- ✅ Un solo vocabulario y una sola ruta de datos.
- ⚠️ Hay que actualizar los documentos de diseño interno, principios y patrones.

---

## Decisiones abiertas

| # | Tema | Opciones | Responsable |
|---|---|---|---|
| ~~1~~ | ~~Granularidad de módulos~~ | **Cerrada (2026-10-13):** un módulo «Anomalías» con el flujo interno validar → confirmar → clasificar | Autor |
| ~~2~~ | ~~Definición de «severidad alta»~~ | **Cerrada (2026-10-13):** 4 o 5 (RN-02, RN-06) | Autor |
| ~~3~~ | ~~Catálogo de `TipoAnomalia`~~ | **Cerrada (2026-10-13):** Bache y Hundimiento (RN-01) | Autor |
| ~~4~~ | ~~Confirmación falsificable por anónimos~~ | **Cerrada (2026-10-13):** solo eventos sobre la vía (RN-13), sesión para confirmar severidad 4–5 (RN-14), velocidad plausible y, si un dispositivo supera el tope por hora de detección, confirmación obligatoria en lugar de descarte para no perder datos reales en temporada de lluvia (RN-15). La verificación de integridad de Play queda para cuando se publique en Play Store | Autor |
| 5 | Actualización del modelo TFLite | Nueva versión de la app vs descarga versionada | Autor |
| 6 | Cuarto subdominio monitoreado | Confirmar o reducir a 3 | Autor |
| 7 | Tope de costo RNF-16 | Confirmar el valor absoluto | Autor |