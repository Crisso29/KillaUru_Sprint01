# Historias de Usuario

> **Proyecto:** KillaUru · **Versión:** 1.2 · **Revisión:** 2026-10-13
> **Prioridad (MoSCoW):** **M** = Must · **S** = Should · **C** = Could. Las *Could* se recortan primero si el plazo de 6–8 semanas (R-10) se ajusta.
> **Relacionados:** `01-actores.md`, `03-requisitos-funcionales.md`

**Definición de Hecho (DoD) común:** código con pruebas, requisitos (RF/RNF) trazados, criterio de aceptación verificado y documentación C4 actualizada si cambia un componente.

## EP-01: Detección automática de anomalías

| ID | Historia de usuario | Prio. | Criterios de aceptación |
|---|---|:---:|---|
| HU-01 | Como conductor, quiero que la app detecte baches automáticamente mientras manejo, para no tener que hacer nada con el celular durante el viaje. | M | **Dado** que la app está en ejecución y la velocidad GPS supera 5 km/h sostenida, **cuando** el vehículo pasa por un bache, **entonces** se registra un evento sin ninguna interacción (0 toques). |
| HU-02 | Como conductor, quiero que la app distinga entre tipos de anomalía, para que la información sea útil para otros conductores. | M | **Dado** una ventana de 500 ms, **cuando** el modelo la clasifica, **entonces** devuelve tipo (Bache o Hundimiento, RN-01), severidad 1–5 (RN-02) y confidence 0.0–1.0 en menos de 50 ms. El catálogo debe estar respaldado por la matriz de confusión del modelo. |

## EP-02: Operación offline y sincronización

| ID | Historia de usuario | Prio. | Criterios de aceptación |
|---|---|:---:|---|
| HU-03 | Como conductor, quiero que la app siga detectando anomalías aunque no haya señal de celular, para no perder datos en los tramos remotos de la vía. | M | **Dado** que no hay conexión durante 4–5 h, **cuando** se detectan anomalías, **entonces** el 100 % queda en SQLite como `PENDING` y el mapa y las alertas siguen funcionando con los datos descargados. |
| HU-04 | Como conductor, quiero que ningún evento detectado se pierda aunque la conexión falle a mitad de la sincronización, para que mis datos siempre lleguen al servidor. | M | **Dado** una sincronización interrumpida, **cuando** la app reintenta, **entonces** reenvía los mismos `eventoId`, el servidor no genera duplicados y los eventos pasan a `SYNCED` solo tras un HTTP 200 que los incluya. **Y** al recuperar señal, la sincronización se reanuda en ≤ 60 s. |

## EP-03: Gestión de anomalías en el servidor

| ID | Historia de usuario | Prio. | Criterios de aceptación |
|---|---|:---:|---|
| HU-05 | Como sistema, necesito validar la calidad de cada evento recibido antes de mostrarlo en el mapa, para que solo aparezcan datos confiables. *(historia técnica)* | M | **Dado** un lote de hasta 50 eventos, **cuando** se procesa, **entonces** se rechazan los de confidence < 0.75, precisión GPS > 20 m o timestamp fuera de rango, y la respuesta indica el resultado **por evento**. Los de severidad alta se confirman con ≥ 2 dispositivos distintos. |
| HU-06 | Como administrador, quiero que el sistema distinga automáticamente baches permanentes de obstáculos temporales y retire del mapa lo que ya no existe, para priorizar las alertas correctamente. | M | **Dado** reportes de una misma anomalía, **cuando** aparece en ≥ 3 días locales distintos dentro de 15 días, **entonces** el servicio de fondo del servidor la clasifica como `PERSISTENTE`; si no, `EPISÓDICA`. **Y** una anomalía sin nuevos reportes durante su plazo de caducidad expira automáticamente. |

## EP-04: Mapa y visualización para el conductor

| ID | Historia de usuario | Prio. | Criterios de aceptación |
|---|---|:---:|---|
| HU-07 | Como conductor no registrado, quiero ver el mapa de anomalías del tramo Ayacucho–Vinchos, para conocer el estado de la vía antes de decidir registrarme. | M | **Dado** un usuario sin cuenta, **cuando** consulta el mapa, **entonces** solo recibe anomalías dentro del tramo demo, aunque pida un `bbox` mayor (recorte en el servidor). |
| HU-08 | Como conductor registrado, quiero ver el mapa completo del corredor de 333 km con las anomalías actualizadas, para planificar mi viaje con información real. | M | **Dado** una sesión válida, **cuando** abre el mapa, **entonces** ve las 333 km con anomalías activas, severidad y tiempo relativo («hace 2 horas»). **Y** sin señal, ve los datos y el mapa base descargados previamente. |
| HU-09 | Como conductor registrado, quiero recibir alertas anticipadas de anomalías próximas durante mi viaje, para reducir la velocidad a tiempo. | S | **Dado** que me acerco a una anomalía activa por delante en mi sentido de marcha, **cuando** faltan unos 8 s para llegar a ella según mi velocidad (mínimo 100 m, RN-16), **entonces** recibo una sola alerta sonora y visual sin necesidad de internet. |
| HU-10 | Como conductor registrado, quiero ver mi historial de recorridos, para saber qué tramos he cubierto y cuántas anomalías detecté. | C | **Dado** 6 meses de uso, **cuando** abro el historial, **entonces** veo tramos cubiertos y anomalías detectadas, almacenados **solo en mi dispositivo**. |

## EP-05: Panel administrativo

| ID | Historia de usuario | Prio. | Criterios de aceptación |
|---|---|:---:|---|
| HU-11 | Como administrador, quiero iniciar sesión en el panel web con mis credenciales, para acceder a la gestión del sistema de forma segura. | M | **Dado** credenciales válidas, **entonces** obtengo un JWT de 8 h. **Dado** 3 intentos fallidos, **entonces** la cuenta se bloquea temporalmente y el evento se audita. |
| HU-12 | Como administrador, quiero ver el mapa completo del corredor con todas las anomalías detectadas y filtrarlas, para monitorear el estado general de la vía. | M | **Dado** una sesión de administrador, **cuando** filtro por tipo, severidad, fecha o estado, **entonces** el mapa muestra también las anomalías sin confirmar. |
| HU-13 | Como administrador, quiero ver el dashboard con estadísticas globales del sistema, para tomar decisiones sobre el funcionamiento de la plataforma. | S | **Entonces** veo eventos por día, usuarios activos, cobertura, latencia p95, descartes por motivo y tasa de confirmación. |
| HU-14 | Como administrador, quiero gestionar los usuarios del sistema (crear, editar rol, desactivar), para mantener el control de acceso en el módulo Identidad y Accesos. | M | **Dado** una cuenta desactivada, **entonces** pierde el acceso en ≤ 5 min y la acción queda auditada. |
| HU-15 | Como administrador, quiero ver el log de auditoría completo del sistema, para tener trazabilidad de todas las acciones sensibles. | S | **Entonces** puedo consultar registros inmutables (timestamp UTC, actor, acción, contexto) solo en modo lectura. |
| HU-19 | Como administrador, quiero marcar una anomalía como reparada o descartarla, para que el mapa refleje la realidad de la vía. | S | **Cuando** modero una anomalía, **entonces** deja de mostrarse a los conductores y la acción se audita. |
| HU-20 | Como administrador, quiero exportar el dataset de anomalías en CSV y GeoJSON, para analizarlo fuera de la plataforma. | C | **Cuando** exporto con filtros, **entonces** el archivo **no contiene** seudónimos de dispositivo y la exportación se audita. |

## EP-06: Infraestructura y despliegue

| ID | Historia de usuario | Prio. | Criterios de aceptación |
|---|---|:---:|---|
| HU-16 | Como equipo de desarrollo, quiero que el sistema se despliegue con un solo comando y pueda volver a la versión anterior, para que cualquier miembro pueda levantar o recuperar el entorno sin pasos manuales. | M | `docker compose up -d` levanta el backend completo. El rollback a la versión anterior toma menos de 5 minutos. |
| HU-17 | Como equipo de desarrollo, quiero monitorear la disponibilidad del sistema en tiempo real, para detectar caídas antes de que los usuarios las reporten. | S | Una caída de `/health` genera alerta por correo en ≤ 5 min y se refleja en `status.killauru.pe`. |

## EP-07: Cuenta y privacidad del conductor

| ID | Historia de usuario | Prio. | Criterios de aceptación |
|---|---|:---:|---|
| HU-18 | Como conductor, quiero registrarme e iniciar sesión con mi correo y contraseña, para acceder al corredor completo y a las alertas. | M | **Cuando** me registro, **entonces** obtengo el rol `ConductorRegistrado` y una sesión renovable. En v1.0 la verificación de correo es opcional (HU-18b). |
| HU-18b | Como conductor, quiero verificar mi correo electrónico, para proteger mi cuenta. | C | **Cuando** abro el enlace recibido, **entonces** mi cuenta queda verificada. Depende de un proveedor de correo transaccional (roadmap). |
| HU-21 | Como conductor, quiero que mi identidad no pueda reconstruirse a partir de los datos que envío, para proteger mi privacidad (Ley 29733). | M | **Entonces** los eventos usan un seudónimo rotativo que el servidor purga a las 72 h; ningún dato almacenado vincula mi cuenta con mis eventos o trayectorias. |
| HU-22 | Como conductor, quiero conocer y aceptar la política de privacidad antes de crear mi cuenta, para saber qué datos se guardan y para qué (consentimiento, Ley 29733). | M | **Dado** que me estoy registrando, **cuando** no marco la aceptación, **entonces** no puedo crear la cuenta. **Y** el sistema guarda la fecha y la versión de la política aceptada. **Y** al usar la app sin cuenta, se muestra un aviso de privacidad antes de iniciar la detección. |
| HU-23 | Como conductor registrado, quiero eliminar mi cuenta desde la app, para que KillaUru deje de tener mis datos personales (derecho de cancelación, Ley 29733). | M | **Dado** que confirmo la eliminación, **entonces** se borran mi correo y mis credenciales, se revocan mis sesiones, mi historial local se borra del dispositivo y la acción queda auditada **sin guardar el correo**. Los eventos de anomalía ya enviados no se tocan porque no están vinculados a la cuenta (HU-21). |
| HU-24 | Como conductor registrado, quiero ver qué datos personales tiene el sistema sobre mí, para ejercer mi derecho de acceso (Ley 29733). | S | **Cuando** abro «Mi perfil», **entonces** veo todos los datos personales guardados: correo, rol, fecha de alta y versión de la política aceptada. |

## Resumen por prioridad y orden de recorte

| Prioridad | Historias |
|---|---|
| **Must** (núcleo de v1.0) | HU-01, HU-02, HU-03, HU-04, HU-05, HU-06, HU-07, HU-08, HU-11, HU-12, HU-14, HU-16, HU-18, HU-21, HU-22, HU-23 |
| **Should** | HU-09, HU-13, HU-15, HU-17, HU-19, HU-24 |
| **Could** (recortar primero) | HU-10, HU-18b, HU-20 |

**Secuencia completa de recorte si el plazo se aprieta:** 
`HU-20` → `HU-18b` → `HU-10` (bloque *Could*) → `HU-13` → `HU-24` → `HU-19` → `HU-15` → `HU-17` → `HU-09` (bloque *Should*). HU-22 y HU-23 son obligación legal y **no se recortan**.

## Mapa de épicas

| Épica | Historias |
|---|---|
| EP-01 Detección automática | HU-01, HU-02 |
| EP-02 Offline y sincronización | HU-03, HU-04 |
| EP-03 Gestión de anomalías en el servidor | HU-05, HU-06 |
| EP-04 Mapa y visualización | HU-07, HU-08, HU-09, HU-10 |
| EP-05 Panel administrativo | HU-11, HU-12, HU-13, HU-14, HU-15, HU-19, HU-20 |
| EP-06 Infraestructura | HU-16, HU-17 |
| EP-07 Cuenta y privacidad del conductor | HU-18, HU-18b, HU-21, HU-22, HU-23, HU-24 |

> **Nota:** las épicas de producto (`EP-xx`) no deben confundirse con la épica de Jira `[EPIC-04] Documentación: Modelo C4`.