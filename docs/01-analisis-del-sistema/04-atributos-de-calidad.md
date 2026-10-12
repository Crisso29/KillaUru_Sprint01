# Atributos de Calidad

> **Proyecto:** KillaUru · **Versión:** 1.1 · **Revisión:** 2026-10-12 · **Estado:** Vigente
> Este documento es el **único catálogo de metas numéricas**. Los demás documentos las referencian por ID (RNF-xx) y no repiten el valor.
> Los IDs existentes se conservaron, por lo que dentro de cada categoría no son consecutivos.
> **Relacionados:** `06-driver-arquitectonicos.md`, `08-escenarios-y-atributos-de-calidad.md`, `Nivel2-diagrama-de-contenedores.md`

## Rendimiento

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-01 | El 95 % de las consultas al mapa debe responder en **< 250 ms medido en la API** y en **< 300 ms de extremo a extremo desde Lima** (con caché en memoria y de borde). | Métrica p95 de la API; prueba sintética desde Lima |
| RNF-02 | El backend debe **recibir, persistir y confirmar** un lote de hasta 50 eventos en < 300 ms (p95). | Latencia p95 del endpoint de lotes |
| RNF-03 | El sistema debe soportar 300 dispositivos sincronizando al mismo tiempo sin degradarse (RNF-02 se mantiene y la tasa de error es < 1 %). | Prueba de carga con 300 clientes |
| RNF-04 | El modelo TFLite debe clasificar una ventana en < 50 ms (p95) en un celular Android de gama media. | Benchmark en ≥ 3 modelos de celular |
| RNF-23 | Un evento aceptado debe ser validado, clasificado y persistido en ≤ 100 ms (p95) en el worker, con retraso de cola ≤ 30 s (p95) durante el pico de 300 dispositivos. | Métrica interna de procesamiento y de tamaño de cola |

## Disponibilidad y continuidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-05 | El sistema debe estar disponible al menos el 99 % del tiempo, medido mensualmente (máximo ~7 h de caída al mes). | UptimeRobot (`/health`) |
| RNF-06 | **Ningún evento al que el servidor respondió HTTP 200 puede perderse**, ni siquiera ante un reinicio del proceso. Debe cumplirse: recibidos = procesados + descartados + pendientes. | Conciliación app → tabla de recepción → BD; prueba de reinicio |
| RNF-07 | El sistema debe tener backup automático diario de la base de datos con retención de 30 días y una prueba de restauración mensual. | Revisión del job de backup; simulacro mensual |
| RNF-24 | **RPO ≤ 24 h** (backup diario) y **RTO ≤ 2 h** ante pérdida de la base de datos. | Simulacro de restauración |

## Seguridad y privacidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-08 | El 100 % del tráfico debe ir cifrado con TLS (1.3 habilitado, mínimo 1.2 para compatibilidad con Android antiguos). Calificación objetivo en SSL Labs: A+. | Escaneo SSL Labs |
| RNF-09 | El panel administrativo debe aplicar headers de seguridad HTTP: HSTS, CSP, X-Frame-Options y X-Content-Type-Options. | Escáner de headers |
| RNF-10 | Ningún dato almacenado debe vincular a una persona o cuenta con sus eventos o trayectorias. Los eventos usan un seudónimo rotativo. | Auditoría del esquema de BD |
| RNF-11 | El sistema debe aplicar rate limiting diferenciado: 60 req/h para el público y 1 000 req/h para el conductor registrado, con clave compuesta (seudónimo + IP) para tolerar redes CGNAT. | Pruebas de API |
| RNF-25 | El seudónimo de dispositivo debe purgarse de los reportes a las 72 h, y los eventos de recepción deben retenerse como máximo 30 días. | Consulta de verificación sobre la BD |
| RNF-26 | Una cuenta desactivada debe perder el acceso en ≤ 5 min, aunque su JWT siga vigente. | Prueba funcional |
| RNF-27 | El servidor debe **hacer cumplir el alcance por rol**: el 100 % de las respuestas a Anónimos queda recortado al tramo demo. | Prueba de API con y sin token |

## Usabilidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-12 | La app Android debe estar completamente en español. Las acciones principales no deben requerir más de 3 toques. | Revisión de interfaz |
| RNF-13 | El panel web debe funcionar correctamente en Chrome, Firefox y Edge (últimas 2 versiones). | Pruebas manuales |
| RNF-14 | Las anomalías deben mostrarse con tiempo relativo legible: «hace 2 horas», «ayer a las 15:30». | Revisión de interfaz |

## Eficiencia de recursos

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-15 | La app debe consumir < 8 % de batería por hora activa, < 1 MB de datos por hora (sin contar la descarga única del mapa base) y < 100 MB de almacenamiento tras 30 días (sin contar el mapa base, que puede ocupar hasta 80 MB aparte). | Perfilado en campo |
| RNF-16 | El costo operativo mensual de infraestructura debe ser **≤ US$ 10** (valor propuesto, a confirmar). Cuando exista ingreso recurrente, además debe ser ≤ 40 % de este. | Facturación mensual |

## Mantenibilidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-17 | El sistema debe poder desplegarse completo con `docker compose up -d`. | Despliegue en entorno limpio |
| RNF-18 | El sistema debe poder volver a la versión anterior en < 5 minutos ante una falla crítica post-despliegue. | Simulacro de rollback |
| RNF-19 | Cada módulo del backend debe tener su propio README con propósito, endpoints y ejemplos. | Revisión de documentación |
| RNF-28 | Las reglas de dependencia entre capas y módulos (ADR-002) deben verificarse con pruebas de arquitectura en cada build. | Pipeline de CI (NetArchTest) |

## Observabilidad

| ID | Requisito | Cómo se mide |
|---|---|---|
| RNF-20 | Una caída debe generar alerta por correo en ≤ 5 min. | Simulacro de caída |
| RNF-21 | El backend debe emitir logs estructurados con correlación por petición y exponer `/health` (liveness y readiness). | Revisión de logs y endpoint |
| RNF-22 | El dashboard debe mostrar latencia p95, descartes por motivo y tasa de confirmación. | Revisión del dashboard |

## Resumen de prioridades

| ID | Atributo | ¿Por qué es importante en KillaUru? | Prioridad | RNF clave | Escenarios asociados |
|---|---|---|---|---|---|
| AC-01 | Disponibilidad offline | Más del 50 % del corredor no tiene señal celular. Si el sistema no funciona sin internet, no sirve para la vía. | 🔴 Crítica | RNF-15 | EQ-01 |
| AC-02 | Confiabilidad del dato | Un bache mal registrado o perdido silenciosamente daña la confianza. Cada evento aceptado debe persistirse de forma durable ante cortes o reinicios. | 🔴 Crítica | RNF-06, RNF-07, RNF-24 | EQ-02, EQ-09, EQ-10 |
| AC-03 | Rendimiento en móvil | El modelo ML debe clasificar en < 50 ms sin agotar la batería. Un sistema lento o que consume batería será desinstalado. | 🔴 Crítica | RNF-04, RNF-15 | EQ-03 |
| AC-04 | Seguridad y privacidad | La Ley 29733 obliga a proteger la identidad del conductor. Además, el servidor debe restringir el acceso a datos según el rol. | 🔴 Crítica | RNF-08…11, RNF-25…27 | EQ-04, EQ-11 |
| AC-05 | Mantenibilidad | El sistema lo desarrolla y mantiene una sola persona. Si el código es difícil de modificar, el proyecto muere. | 🟠 Alta | RNF-17…19, RNF-28 | EQ-05 |
| AC-06 | Escalabilidad | El sistema debe poder absorber ráfagas de sincronización de la flota sin reescribir la arquitectura. | 🟠 Alta | RNF-02, RNF-03, RNF-23 | EQ-06 |
| AC-07 | Observabilidad | Si algo falla en producción, debe detectarse mediante monitoreo proactivo antes de que los usuarios lo reporten. | 🟠 Alta | RNF-05, RNF-20…22 | EQ-07 |
| AC-08 | Usabilidad | El conductor no puede distraerse mientras maneja. La detección debe ser pasiva y automática. | 🟡 Media | RNF-12, RNF-14 | EQ-08 |