# Atributos de Calidad

> **Proyecto:** KillaUru · **Versión:** 1.2 · **Revisión:** 2026-10-14 · **Estado:** Vigente
> Este documento es el **único catálogo de metas numéricas**. Los demás documentos las referencian por ID (RNF-xx) y no repiten el valor.
> Los IDs existentes se conservaron, por lo que dentro de cada categoría no son consecutivos.
> **Relacionados:** `06-driver-arquitectonicos.md`, `08-escenarios-y-atributos-de-calidad.md`, `Nivel2-diagrama-de-contenedores.md`

## Rendimiento

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-01 | El 95 % de las consultas al mapa debe responder en **< 250 ms medido en la API** y en **< 300 ms de extremo a extremo desde Lima** (con caché en memoria y de borde). En el tramo demo, al menos el **80 %** de las consultas anónimas debe resolverse en la caché de borde. | Métrica p95 de la API; prueba sintética desde Lima; tasa de aciertos de caché |
| RNF-02 | El backend debe **recibir, persistir y confirmar** un lote de hasta 50 eventos en < 300 ms (p95). | Latencia p95 del endpoint de lotes |
| RNF-03 | El sistema debe soportar 300 dispositivos sincronizando al mismo tiempo sin degradarse (RNF-02 se mantiene y la tasa de error es < 1 %). | Prueba de carga con k6 durante ≥ 10 min: 300 clientes que suben cada uno 10 lotes de 50 eventos (una semana acumulada) y 300 que consultan el mapa. Los eventos sintéticos caen sobre la polilínea del corredor y respetan una velocidad plausible, para que no los descarten RN-13 ni RN-15b |
| RNF-04 | El modelo TFLite debe clasificar una ventana en < 50 ms (p95) en un celular Android de gama media (≤ 4 GB de RAM y procesador de gama media lanzado en los últimos 4 años). | Benchmark en ≥ 3 modelos de celular |
| RNF-35 | **Precisión del modelo.** Sobre un conjunto de prueba separado del de entrenamiento (≥ 200 ventanas por clase, grabado con vehículos livianos y celular en soporte, S-06, en al menos 3 celulares), el modelo debe lograr *precision* y *recall* ≥ 0,90 **por clase** (Bache, Hundimiento y Normal), y la severidad debe acertar con un margen de ± 1 nivel en ≥ 80 % de los casos. Se reporta con la matriz de confusión de cada `modeloVersion` (ADR-004). | Matriz de confusión del conjunto de prueba |
| RNF-23 | Un evento aceptado debe ser validado, clasificado y persistido en ≤ 100 ms (p95) en el worker, con retraso de cola ≤ 30 s (p95) durante el pico de 300 dispositivos. | Métrica interna de procesamiento y de tamaño de cola |

## Disponibilidad y continuidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-05 | El sistema debe estar disponible al menos el 99 % del tiempo, medido mensualmente (máximo ~7 h de caída al mes). El mantenimiento programado (despliegues, parches de la VM) **cuenta** como caída. | UptimeRobot (`/health`) |
| RNF-06 | **Ningún evento al que el servidor respondió HTTP 200 puede perderse**, ni siquiera ante un reinicio del proceso. Debe cumplirse: aceptados = procesados + descartados en procesamiento + pendientes (RN-11). Tras un reinicio, el procesamiento de los pendientes se reanuda en ≤ 2 min. | Conciliación app → tabla de recepción → BD; prueba de reinicio |
| RNF-07 | El sistema debe tener backup automático diario de la base de datos, **cifrado y guardado fuera de la VM** (Cloudflare R2), con retención de 30 días y una prueba de restauración mensual. Después de restaurar se aplica RN-10 (volver a purgar seudónimos y repetir las eliminaciones de cuenta). | Revisión del job de backup; simulacro mensual |
| RNF-24 | **RPO ≤ 24 h** (backup diario) y **RTO ≤ 2 h** ante pérdida de la base de datos y **≤ 4 h** ante pérdida total de la VM (por ejemplo, si Oracle recupera la instancia, RS-01), volviendo a crearla desde el repositorio, los secretos guardados fuera (RNF-34) y el último backup. | Simulacro de restauración |
| RNF-33 | La app debe operar **sin conexión durante al menos 5 h continuas** (detección, guardado, mapa descargado y alertas), conservar sin pérdida al menos **7 días** de eventos pendientes (la ventana de RN-11) y **reiniciar la sincronización en ≤ 60 s** al recuperar señal. | Prueba de campo en modo avión (EQ-01) |

## Seguridad y privacidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-08 | El 100 % del tráfico debe ir cifrado con TLS (1.3 habilitado, mínimo 1.2 para compatibilidad con Android antiguos). Calificación objetivo en SSL Labs: A+, con HSTS en **todos** los subdominios (`api`, `panel`, `cdn`), configurado en Cloudflare. | Escaneo SSL Labs |
| RNF-09 | El panel administrativo debe aplicar headers de seguridad HTTP: HSTS, CSP, X-Frame-Options y X-Content-Type-Options. | Escáner de headers |
| RNF-10 | Ningún dato almacenado debe vincular a una persona o cuenta con sus eventos o trayectorias. Los eventos usan un seudónimo rotativo. | Auditoría del esquema de BD |
| RNF-11 | El sistema debe aplicar rate limiting diferenciado: **60 req/h** de consultas para el anónimo (clave compuesta seudónimo + IP, para tolerar redes CGNAT; el seudónimo viaja en una cabecera, RF-27) más una cuota aparte de **120 lotes/h** para sincronizar (así una subida acumulada en temporada de lluvia no bloquea el mapa), **1 000 req/h** para el conductor registrado y **3 000 req/h** para el administrador (ambos con la cuenta como clave). La IP real del cliente se toma de la cabecera `CF-Connecting-IP`, aceptada solo si la petición llega por el túnel de Cloudflare. Al superarlo responde 429 con `Retry-After`. | Pruebas de API |
| RNF-25 | El seudónimo de dispositivo debe purgarse a las 72 h de recibido, de los reportes **y** de la copia de recepción (RN-09), y los eventos de recepción deben retenerse como máximo 30 días. | Consulta de verificación sobre la BD |
| RNF-26 | Una cuenta desactivada o eliminada debe perder el acceso **a la API** en ≤ 5 min. Se logra con tokens de acceso de 5 min y la revocación inmediata de sus tokens de actualización (ADR-009). El acceso **sin conexión** termina en cuanto el celular se conecta, o a los 30 días como máximo (RN-21). | Prueba funcional |
| RNF-27 | El servidor debe **hacer cumplir el alcance por rol**: el 100 % de las respuestas a Anónimos queda recortado al tramo demo. | Prueba de API con y sin token |
| RNF-29 | Ante un incidente de seguridad con datos personales, el sistema y su operación deben permitir **notificar a la Autoridad Nacional de Protección de Datos Personales y a los titulares afectados en ≤ 48 h** desde que se conoce, y **documentar** el incidente (hechos, efectos y medidas). La auditoría (RF-15) debe bastar para reconstruir quién accedió a qué y cuándo. Debe existir un **procedimiento escrito** de incidentes: plantilla de notificación, canal de la Autoridad, pasos para contener y una persona de reemplazo si el responsable no está disponible. Las acciones de riesgo (exportación, cambio de rol, ≥ 5 bloqueos de cuenta en 1 h) generan una alerta (RNF-20). | Simulacro de incidente (EQ-13) con cronómetro |
| RNF-30 | Los privilegios administrativos deben **revisarse al menos una vez al mes**: lista de cuentas con rol `Administrador`, confirmación o retiro de cada una, y registro de la revisión en la auditoría con fecha. | Registro mensual en la auditoría |
| RNF-34 | **Secretos fuera del código y fuera de la VM.** Existe un inventario de secretos (clave de firma JWT, contraseña de la BD, clave del proveedor de correo, credenciales de R2, token de Grafana, token del túnel de Cloudflare y clave de cifrado del backup). Viven en variables de entorno que no se suben al repositorio, tienen una copia cifrada fuera de la VM y un procedimiento de rotación. Sin la clave de cifrado guardada fuera, el backup no sirve. | Revisión del inventario; simulacro de recreación de la VM (RNF-24) |
| RNF-31 | **Ninguna acción sensible puede quedar sin rastro:** la acción y su registro de auditoría se confirman en la misma transacción; si uno falla, se revierten ambos (ADR-013). | Prueba que fuerza un fallo de la auditoría y verifica que la acción se revirtió |

## Usabilidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-12 | La app Android debe estar completamente en español. Las acciones principales no deben requerir más de 3 toques. | Revisión de interfaz |
| RNF-13 | El panel web debe funcionar correctamente en Chrome, Firefox y Edge (últimas 2 versiones). | Pruebas manuales |
| RNF-14 | Las anomalías deben mostrarse con tiempo relativo legible: «hace 2 horas», «ayer a las 15:30». | Revisión de interfaz |

## Eficiencia de recursos

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-15 | La app debe consumir **≤ 5 % de batería por hora con la pantalla apagada** (detección, GPS y alertas en segundo plano; el consumo de la pantalla encendida depende del brillo y del celular y no se cuenta), medido en 3 celulares. En turnos largos se asume el celular conectado al cargador del vehículo (S-07), y con batería baja se aplica el ahorro de RN-22. La app debe consumir además < 1 MB de datos por hora (sin contar la descarga única del mapa base) y < 100 MB de almacenamiento tras 6 meses de uso, incluido el historial (sin contar el mapa base, que puede ocupar hasta 80 MB aparte). | Perfilado en campo con la pantalla apagada |
| RNF-16 | El costo operativo mensual de infraestructura debe ser **≤ US$ 10**, con alertas de presupuesto en Oracle Cloud y Cloudflare al llegar a US$ 8 (confirmado el 2026-10-13; costo estimado de v1.0: ~US$ 4/mes, casi todo el dominio `.pe` prorrateado). Cuando exista ingreso recurrente, además debe ser ≤ 40 % de este. | Facturación mensual |

## Mantenibilidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-17 | El sistema debe poder desplegarse completo con `docker compose up -d`. | Despliegue en entorno limpio |
| RNF-18 | El sistema debe poder volver a la versión anterior en < 5 minutos ante una falla crítica post-despliegue. Para que sea posible: las migraciones de BD se ejecutan como **paso explícito** (no al arrancar la app), se hace un respaldo (`pg_dump`) justo antes de migrar, y se aplican **en dos fases**: primero se agrega lo nuevo y solo en un despliegue posterior se borra lo viejo. Así la versión anterior siempre funciona con el esquema nuevo. | Simulacro de rollback |
| RNF-19 | Cada módulo del backend debe tener su propio README con propósito, endpoints y ejemplos. | Revisión de documentación |
| RNF-28 | Las reglas de dependencia entre capas y módulos (ADR-002), incluido que ningún módulo use el esquema o el `DbContext` de otro (ADR-012), deben verificarse con pruebas de arquitectura en cada build. | Pipeline de CI (NetArchTest) |

## Observabilidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-20 | Una caída debe generar alerta por correo en ≤ 10 min (UptimeRobot consulta cada 5 min y confirma antes de avisar), y una degradación (p95 de la API > 250 ms o errores 5xx > 1 % durante 5 min) debe generar alerta en Grafana. | Simulacro de caída y de lentitud |
| RNF-21 | El backend debe instrumentarse con **OpenTelemetry** (métricas y trazas) y emitir logs estructurados en JSON (**Serilog**) con correlación por petición, enviados a **Grafana Cloud (plan gratuito)**; y exponer `/health` (liveness y readiness). | Revisión de paneles, logs y endpoint |
| RNF-22 | Los paneles de Grafana deben mostrar latencia p95, errores, descartes por motivo, eventos retenidos por RN-15a, tamaño y antigüedad de la cola de ingesta, uso de CPU, memoria y **disco**, y la última ejecución correcta de cada tarea programada (purga, caducidad, backup). Hay alerta si el disco supera el 80 %, si una tarea lleva más de 26 h sin ejecutarse bien, si el uso del plan gratuito de Grafana llega al 80 % y por las acciones de riesgo de RNF-29. Los logs de Docker rotan para no llenar el disco. | Revisión de los paneles |
| RNF-32 | La observabilidad debe operar **dentro del plan gratuito** de Grafana Cloud: cuenta **sin tarjeta registrada** (no puede generar cobros), muestreo de trazas ≤ 10 %, logs de nivel `Information` o superior sin registrar `/health`, y métricas sin etiquetas de alta cardinalidad (nunca seudónimo, IP ni `eventoId` como etiqueta). | Revisión mensual del uso frente a los límites del plan |

## Resumen de prioridades

| ID | Atributo | ¿Por qué es importante en KillaUru? | Prioridad | RNF clave | Escenarios asociados |
|---|---|---|---|---|---|
| AC-01 | Disponibilidad offline | Más del 50 % del corredor no tiene señal celular. Si el sistema no funciona sin internet, no sirve para la vía. | 🔴 Crítica | RNF-33, RNF-15 | EQ-01 |
| AC-02 | Confiabilidad del dato | Un bache mal registrado o perdido silenciosamente daña la confianza. Cada evento aceptado debe persistirse de forma durable ante cortes o reinicios. | 🔴 Crítica | RNF-06, RNF-07, RNF-24 | EQ-02, EQ-09, EQ-10, EQ-12 |
| AC-03 | Rendimiento en móvil | El modelo ML debe clasificar en < 50 ms sin agotar la batería. Un sistema lento o que consume batería será desinstalado. | 🔴 Crítica | RNF-04, RNF-15, RNF-35 | EQ-03 |
| AC-04 | Seguridad y privacidad | La Ley 29733 obliga a proteger la identidad del conductor. Además, el servidor debe restringir el acceso a datos según el rol y no aceptar datos falsos (RN-13…RN-15). | 🔴 Crítica | RNF-08…11, RNF-25…27, RNF-29…31, RNF-34 | EQ-04, EQ-11, EQ-12, EQ-13 |
| AC-05 | Mantenibilidad | El sistema lo desarrolla y mantiene una sola persona. Si el código es difícil de modificar, el proyecto muere. | 🟠 Alta | RNF-17…19, RNF-28 | EQ-05 |
| AC-06 | Escalabilidad y rendimiento del servidor | El sistema debe absorber ráfagas de sincronización de la flota y responder el mapa con rapidez, sin reescribir la arquitectura. | 🟠 Alta | RNF-01, RNF-02, RNF-03, RNF-23 | EQ-06, EQ-14 |
| AC-07 | Observabilidad | Si algo falla en producción, debe detectarse mediante monitoreo proactivo antes de que los usuarios lo reporten. | 🟠 Alta | RNF-05, RNF-20…22, RNF-32 | EQ-07 |
| AC-08 | Usabilidad | El conductor no puede distraerse mientras maneja. La detección debe ser pasiva y automática. | 🟡 Media | RNF-12, RNF-13, RNF-14 | EQ-08 |
| AC-09 | Costo operativo | El MVP es autofinanciado: si la infraestructura empieza a cobrar, el proyecto no se sostiene. Cada decisión debe caber en los planes gratuitos o en el tope mensual. | 🟠 Alta | RNF-16, RNF-32 | EQ-15 |