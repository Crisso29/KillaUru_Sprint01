# Escenarios de atributos de calidad de KillaUru

> **Proyecto:** KillaUru · **Versión:** 1.0 · **Revisión:** 2026-10-12
> Traduce los atributos de `04-atributos-de-calidad.md` en escenarios verificables, con el formato de **seis partes** del método ATAM: **fuente, estímulo, artefacto, entorno, respuesta y medida de respuesta**. Se añaden *verificación*, *trazabilidad* y *estado* para el seguimiento del proyecto.
> Las metas numéricas viven en `04`; aquí se referencian por ID.

## EQ-01. Disponibilidad offline: operación sin cobertura celular

| Elemento | Descripción |
|---|---|
| **Atributo** | Disponibilidad offline (AC-01) |
| **Fuente** | Conductor en tránsito por la Vía Los Libertadores |
| **Estímulo** | Circula por un tramo sin señal (más del 50 % del corredor, hasta 4–5 h continuas). |
| **Artefacto** | App Android: Sensores, Detector, Buffer, Mapa y Proximidad |
| **Entorno** | App en ejecución, GPS activo, sin datos móviles ni WiFi; mapa base y anomalías descargados previamente. |
| **Respuesta** | La app sigue detectando con el modelo embebido, guarda los eventos en SQLite como `PENDING`, muestra el mapa y emite alertas con datos locales. Al recuperar señal **reinicia el backoff** y sincroniza sin intervención. |
| **Medida** | 100 % de los eventos con confidence ≥ 0.75 quedan almacenados; la sincronización se inicia en ≤ 60 s tras recuperar señal (RF-04, RF-05, RF-26). |
| **Verificación** | Prueba de campo en modo avión en Ayacucho–Vinchos durante un recorrido completo; comparar el conteo de SQLite antes y después de reactivar datos. |
| **Trazabilidad** | R-08, DA-01, DA-14, HU-03, ADR-003, ADR-011 |
| **Estado** | Definido |

## EQ-02. Confiabilidad del dato: entrega garantizada y sin duplicados

| Elemento | Descripción |
|---|---|
| **Atributo** | Confiabilidad del dato (AC-02) |
| **Fuente** | Sincronizador de la app Android |
| **Estímulo** | La sincronización falla a mitad de camino (corte de señal, timeout después de que el servidor ya persistió el lote, error HTTP). |
| **Artefacto** | Sincronizador (app) y endpoint `POST /api/v1/anomalies/batch` (Ingesta) |
| **Entorno** | Existen eventos `PENDING` en SQLite esperando confirmación. |
| **Respuesta** | La app reintenta con backoff, *jitter* y **los mismos `eventoId`**. El servidor devuelve los ya recibidos como `duplicados` y no los reinserta. |
| **Medida** | 0 % de eventos válidos perdidos y 0 duplicados en anomalías y reportes (RNF-06). El conteo enviado por la app coincide con el de la tabla de recepción. |
| **Verificación** | Interrupción simulada de red a mitad del envío, incluida la pérdida de la respuesta 200; reenviar y conciliar app ↔ PostgreSQL. |
| **Trazabilidad** | DA-02, RF-06, RF-07, RF-11, HU-04, ADR-003, ADR-005 |
| **Estado** | Definido — requiere ADR-005 (propuesto) |

## EQ-03. Rendimiento en móvil: clasificación del modelo ML

| Elemento | Descripción |
|---|---|
| **Atributo** | Rendimiento en móvil (AC-03) |
| **Fuente** | Sensores del celular (acelerómetro y giroscopio) |
| **Estímulo** | Se completa una ventana de 500 ms a 50 Hz con el vehículo a más de 5 km/h de forma sostenida. |
| **Artefacto** | Detector (TFLite) de la app |
| **Entorno** | Celular Android de gama media, app en primer o segundo plano, modelo ya cargado en memoria. |
| **Respuesta** | El modelo clasifica localmente (tipo, severidad 1–5, confidence), sin enviar la señal cruda. |
| **Medida** | Latencia < 50 ms (p95) (RNF-04); precisión ≥ 90 % **por clase** en campo; consumo < 8 % de batería por hora (RNF-15). |
| **Verificación** | Benchmark en ≥ 3 modelos de celular; matriz de confusión contra el dataset etiquetado del tramo demo. |
| **Trazabilidad** | DA-03, DA-04, RF-02, HU-02, ADR-004 |
| **Estado** | Definido |

## EQ-04. Seguridad y privacidad: protección de la identidad y de los accesos

| Elemento | Descripción |
|---|---|
| **Atributo** | Seguridad y privacidad (AC-04) |
| **Fuente** | Usuario sin sesión válida o actor malicioso |
| **Estímulo** | Intenta acceder al panel sin JWT, con token expirado o por fuerza bruta; o intenta reconstruir la identidad de un conductor a partir de los datos almacenados. |
| **Artefacto** | API REST, Identidad y Accesos, base de datos |
| **Entorno** | `api.killauru.pe` y `panel.killauru.pe` bajo HTTPS; seudónimos rotativos purgados a 72 h. |
| **Respuesta** | El sistema rechaza el acceso sin exponer información, bloquea temporalmente tras 3 intentos fallidos y ningún dato almacenado permite reconstruir la identidad. |
| **Medida** | 100 % de intentos no autorizados rechazados; 0 reidentificación posible en la auditoría del esquema (sin vínculo cuenta ↔ eventos y sin seudónimos con más de 72 h); A+ en SSL Labs; cuenta desactivada sin acceso en ≤ 5 min (RNF-08…11, RNF-25, RNF-26). |
| **Verificación** | Pruebas de acceso al panel (sin token, expirado, fuerza bruta); auditoría del esquema; consulta de verificación de purga; escaneo SSL Labs; verificar que `/metrics` no es público. |
| **Trazabilidad** | R-06, DA-07, HU-11, HU-21, ADR-007, ADR-009 |
| **Estado** | Definido |

## EQ-05. Mantenibilidad: cambios aislados por módulo

| Elemento | Descripción |
|---|---|
| **Atributo** | Mantenibilidad (AC-05) |
| **Fuente** | Equipo de desarrollo (una persona: Crisólogo Aguilar Flores) |
| **Estímulo** | Se cambia una regla de negocio (p. ej. el umbral de confidence) o se sustituye un componente interno. |
| **Artefacto** | Módulos del monolito (Domain, Application, Adapters, Infrastructure) |
| **Entorno** | Backend como monolito modular, con pruebas de arquitectura en el pipeline. |
| **Respuesta** | El cambio se concentra en un solo módulo; los umbrales se modifican **por configuración**, sin tocar código; el sistema completo se despliega con un único comando. |
| **Medida** | El cambio de umbral toca 0 líneas de código; el cambio de componente toca solo 1 módulo; las pruebas del módulo y de regresión pasan; las pruebas de arquitectura pasan (RNF-28); despliegue con `docker compose up -d` y rollback en < 5 min (RNF-17, RNF-18). |
| **Verificación** | Revisión de dependencias entre módulos; ejecución de la suite completa; medición del tiempo de rollback en staging. |
| **Trazabilidad** | R-09, DA-06, DA-09, HU-16, ADR-001, ADR-002, ADR-012 |
| **Estado** | Definido |

## EQ-06. Escalabilidad: sincronización concurrente de la flota

| Elemento | Descripción |
|---|---|
| **Atributo** | Escalabilidad (AC-06) |
| **Fuente** | Flota de dispositivos Android |
| **Estímulo** | 300 dispositivos intentan sincronizar lotes de hasta 50 eventos en una ventana corta (p. ej. al recuperar señal juntos). |
| **Artefacto** | Ingesta (tabla de recepción, Channel y `BackgroundService`) |
| **Entorno** | Un único servidor Oracle Cloud ARM, una sola instancia. |
| **Respuesta** | El servidor persiste y confirma cada lote, aplica contrapresión (503 con `Retry-After`) si se satura, y procesa de forma asíncrona. Los reintentos de las apps llevan *jitter*. |
| **Medida** | Confirmación del lote < 300 ms p95 (RNF-02); error < 1 % (RNF-03); evento procesado en ≤ 100 ms y retraso de cola ≤ 30 s p95 (RNF-23). |
| **Verificación** | Prueba de carga con 300 clientes concurrentes; medir latencia p95, tasa de error, tamaño de cola y CPU/memoria. |
| **Trazabilidad** | DA-05, RNF-02, RNF-03, RNF-23, HU-04, ADR-005 |
| **Estado** | Definido |

## EQ-07. Observabilidad: detección temprana de fallas

| Elemento | Descripción |
|---|---|
| **Atributo** | Observabilidad (AC-07) |
| **Fuente** | Componente de infraestructura (backend, base de datos, proxy) |
| **Estímulo** | Un subdominio deja de responder o `/health` retorna un código distinto de 200. |
| **Artefacto** | UptimeRobot, `/health` de la API, Dashboard |
| **Entorno** | Monitoreo cada 5 minutos; logs estructurados y métricas por minuto. |
| **Respuesta** | El sistema notifica al equipo antes de que los usuarios lo reporten y el estado queda visible en `status.killauru.pe`. |
| **Medida** | Alerta por correo en ≤ 5 min (RNF-20); disponibilidad mensual registrada con historial de 30 días (RNF-05); el dashboard muestra p95, descartes por motivo y tasa de confirmación (RNF-22). |
| **Verificación** | Simular la caída del endpoint `/health` y medir el tiempo hasta la alerta; revisar logs con correlación. |
| **Trazabilidad** | DA-11, HU-17, RF-28, ADR-010 |
| **Estado** | Definido |

## EQ-08. Usabilidad: operación pasiva sin distracción

| Elemento | Descripción |
|---|---|
| **Atributo** | Usabilidad (AC-08) |
| **Fuente** | Conductor en tránsito |
| **Estímulo** | Circula por un tramo mientras la app está en ejecución. |
| **Artefacto** | App Android (Detector, Alertas, Mapa) |
| **Entorno** | App en primer o segundo plano; el conductor no interactúa con la pantalla durante el trayecto. |
| **Respuesta** | La detección es automática y pasiva. Cuando interactúa (ver el mapa), las acciones principales son accesibles en pocos toques. |
| **Medida** | 0 interacciones manuales para la detección; acciones principales en ≤ 3 toques; interfaz en español; tiempos relativos legibles (RNF-12, RNF-14). |
| **Verificación** | Prueba con un conductor real completando un recorrido sin tocar el celular; conteo de toques por acción principal. |
| **Trazabilidad** | DA-03, HU-01, ADR-004 |
| **Estado** | Definido |

## EQ-09. Reinicio del servidor sin pérdida de eventos aceptados *(nuevo)*

| Elemento | Descripción |
|---|---|
| **Atributo** | Confiabilidad del dato (AC-02) |
| **Fuente** | Falla del proceso o despliegue de una nueva versión |
| **Estímulo** | El proceso de la API se reinicia o se cae mientras hay eventos aceptados (HTTP 200) sin procesar. |
| **Artefacto** | Tabla de recepción y `BackgroundService` de Ingesta |
| **Entorno** | Carga normal con lotes en curso. |
| **Respuesta** | Al arrancar, el servicio reprocesa los eventos pendientes de la tabla de recepción. |
| **Medida** | 0 eventos aceptados perdidos; tras recuperarse se cumple recibidos = procesados + descartados + pendientes; el procesamiento se reanuda en ≤ 2 min (RNF-06). |
| **Verificación** | Prueba de caos: matar el proceso a mitad de un lote y conciliar los conteos. |
| **Trazabilidad** | DA-02, RNF-06, ADR-005 |
| **Estado** | Definido — requiere ADR-005 (propuesto) |

## EQ-10. Restauración desde backup *(nuevo)*

| Elemento | Descripción |
|---|---|
| **Atributo** | Disponibilidad y continuidad (AC-02, AC-07) |
| **Fuente** | Falla de disco, corrupción de datos o eliminación accidental |
| **Estímulo** | Se pierde o corrompe la base de datos. |
| **Artefacto** | Job de backup y almacenamiento de objetos |
| **Entorno** | Existe backup diario cifrado con retención de 30 días. |
| **Respuesta** | El equipo restaura el último backup válido y el sistema vuelve a operar. |
| **Medida** | **RTO ≤ 2 h** y **RPO ≤ 24 h** (RNF-24); backup restaurable verificado mensualmente (RNF-07). |
| **Verificación** | Simulacro mensual de restauración en un entorno de prueba, con cronómetro. |
| **Trazabilidad** | RNF-07, RNF-24, HU-16, ADR-010 |
| **Estado** | Definido |

## EQ-11. Control de acceso al corredor completo *(nuevo)*

| Elemento | Descripción |
|---|---|
| **Atributo** | Seguridad (AC-04) |
| **Fuente** | Cliente anónimo o script que llama la API directamente |
| **Estímulo** | Solicita `GET /api/v1/anomalies?bbox=` fuera del tramo demo, o excede el límite de tasa. |
| **Artefacto** | Consulta de Mapa y middleware de límite de tasa de la API REST |
| **Entorno** | Sin JWT (Anónimo) o con JWT de Conductor Registrado. |
| **Respuesta** | La respuesta se recorta al polígono del tramo demo para el Anónimo; al exceder 60 req/h se responde 429. El conductor registrado recibe el corredor completo. |
| **Medida** | 100 % de las respuestas a Anónimos sin anomalías fuera del tramo demo (RNF-27); límite de 60 y 1 000 req/h aplicado (RNF-11). |
| **Verificación** | Pruebas de API con y sin token, con `bbox` mayor al tramo demo y ráfaga de peticiones. |
| **Trazabilidad** | R-07, DA-10, RF-08, RF-14, HU-07, ADR-009 |
| **Estado** | Definido |

## Resumen de escenarios

| Código | Atributo | Prioridad | Qué se busca comprobar |
|---|---|---|---|
| EQ-01 | Disponibilidad offline | 🔴 Crítica | Detección, mapa y alertas funcionan sin cobertura |
| EQ-02 | Confiabilidad del dato | 🔴 Crítica | Ningún evento se pierde ni se duplica entre la app y el servidor |
| EQ-03 | Rendimiento en móvil | 🔴 Crítica | El modelo clasifica en tiempo y precisión, sin agotar la batería |
| EQ-04 | Seguridad y privacidad | 🔴 Crítica | Identidad y panel protegidos |
| EQ-05 | Mantenibilidad | 🟠 Alta | Los cambios se aíslan en un módulo o en configuración |
| EQ-06 | Escalabilidad | 🟠 Alta | El sistema absorbe el pico de 300 dispositivos |
| EQ-07 | Observabilidad | 🟠 Alta | Una falla se detecta antes que los usuarios |
| EQ-08 | Usabilidad | 🟡 Media | El conductor no necesita tocar el celular |
| EQ-09 | Confiabilidad (reinicio) | 🔴 Crítica | Un reinicio del servidor no pierde eventos aceptados |
| EQ-10 | Continuidad (restauración) | 🟠 Alta | Se restaura la BD dentro de RPO y RTO |
| EQ-11 | Seguridad (alcance por rol) | 🟠 Alta | El servidor impide acceder al corredor completo sin cuenta |