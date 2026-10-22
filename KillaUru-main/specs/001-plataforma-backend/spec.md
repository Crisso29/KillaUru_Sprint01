# Feature Specification: Plataforma base del backend

**Feature Branch**: `001-plataforma-backend`

**Created**: 2026-10-20

**Status**: Draft

**Input**: User description: "Base común del backend de KillaUru: levantar todo con un comando, seguridad común a todas las peticiones, documentación viva de la API, salud y monitoreo, respaldo y vuelta atrás, y reglas de arquitectura (incluido el límite de 200 líneas por archivo) verificadas en cada compilación."

**Revisión 2026-10-21**: aplica las decisiones del autor D2, D4, D8, D9, D10, D11, D12 y D15 y las correcciones técnicas T-BD01, T-OPS05/08/09/17/19, T-QA14/16/17/18/20 y T-SEG03/09/20 de `specs/revision/plan-de-cambios.md`.

---

## 0. Ficha de la feature

| Campo | Valor |
|---|---|
| Qué es | Todo lo que **no es de un módulo de negocio** pero todos necesitan: la solución .NET, el host de la API, el kernel `Shared`, la seguridad común, la documentación de la API, la salud, la observabilidad, el despliegue (CI/CD) y las pruebas de sistema (carga, picos, estrés, resistencia y caos) |
| Qué **no** es | No emite tokens (eso es 003), no guarda auditoría (002), no tiene reglas de negocio de anomalías (004) |
| Contenedores C4 | API KillaUru, Base de datos, Tarea de backup, agente Grafana Alloy (D12) (`Nivel2-diagrama-de-contenedores.md`) |
| Componentes C3 | API REST (host), Eventos de dominio, Métricas, Shared/Kernel (`Nivel3-diagrama-de-componentes.md`) |
| Depende de | Nada |
| La usan | Todas las features 002–007 |
| Prioridad | HU-16 *Must* · HU-17 *Should* (recortable al final) |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historias | HU-16, HU-17 | `02-historias-de-usuario.md` |
| Requisitos funcionales | RF-28 | `03-requisitos-funcionales.md` |
| Atributos de calidad | RNF-03, RNF-05, RNF-06, RNF-07, RNF-08, RNF-09, RNF-11, RNF-16…RNF-24, RNF-26, RNF-28, RNF-29, RNF-32, RNF-34 | `04-atributos-de-calidad.md` |
| Restricciones | R-02, R-03, R-04, R-05, R-09, R-11 · RS-01, RS-02, RS-03 | `05-restricciones.md` |
| Reglas de negocio | RN-09, RN-10 (restauración) | `09-reglas-de-negocio.md` |
| Decisiones | ADR-001, ADR-002, ADR-006 (cabeceras de caché), ADR-009 (puntos 2 y 5), ADR-010, ADR-012, ADR-013 (mecanismo), ADR-014, ADR-015 (techo por IP) | `07-decisiones-arquitectonicas.md` |
| Escenarios | EQ-05, EQ-06, EQ-07, EQ-09, EQ-10, EQ-14 | `08-escenarios-y-atributos-de-calidad.md` |
| Diseño | Estructura de la solución, capas, kernel, roles de BD, migraciones, tareas programadas | `diseño-interno-de-modulos.md` §2, §3.7, §3.8, §6.1 · `estilo-arquitectonico.md` §4, §6 · `enfoque-arquitectonico.md` §3 |
| Decisiones del autor | D2, D4, D8, D9, D10, D11, D12, D15 (2026-10-21) | `specs/revision/decisiones.md` y `plan-de-cambios.md` |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Levantar el backend completo con un solo comando (Priority: P1)

Como equipo de desarrollo (una sola persona), quiero levantar la API, la base de datos, el túnel, la tarea de respaldo y el agente de métricas con un solo comando, para que cualquiera pueda poner el sistema en marcha sin pasos manuales (HU-16).

**Why this priority**: Sin esto no se puede probar ninguna otra feature ni desplegar para la sustentación (R-10).

**Independent Test**: En una máquina limpia con el repositorio y los secretos, un comando deja la API respondiendo `GET /health/live` con 200.

**Acceptance Scenarios**:

1. **Given** un entorno limpio con las variables del inventario de secretos, **When** el equipo ejecuta el comando de arranque, **Then** quedan en marcha la API, PostgreSQL + PostGIS, el túnel de Cloudflare, la tarea de respaldo y Grafana Alloy, y `/health/live` responde 200 (RNF-17).
2. **Given** una base de datos vacía, **When** se ejecuta una sola vez `migrar.sh --arranque` (superusuario `KU_PG_SUPERUSUARIO`) y luego `migrar.sh`, **Then** `V0000__arranque.sql` crea los roles `killauru_migrador` y `killauru_app`, las extensiones `postgis` y `citext`, los esquemas `identidad`, `ingesta`, `anomalias`, `auditoria` y `dashboard`, la tabla `migraciones.aplicada` y los permisos, y `V0001…V0007` se aplican con `killauru_migrador`; la API **no** migra al arrancar (RNF-18, T-BD01).
3. **Given** el servidor en marcha, **When** se intenta llegar al origen sin pasar por Cloudflare, **Then** no hay ningún puerto público abierto (ADR-010).
4. **Given** el primer despliegue en una VM nueva, **When** se sigue el orden arranque → migraciones → `docker compose up -d`, **Then** `/health/ready` responde 200 sin reiniciar nada (T-BD01, T-OPS19).

---

### User Story 2 - Consultar la documentación viva de la API (Priority: P1)

Como desarrollador de la app y del panel, quiero una documentación navegable de todos los endpoints, generada desde el propio código y alineada con los contratos de las specs, para integrarme sin leer el código del servidor (pedido del autor; constitución IX).

**Why this priority**: Es el puente entre el backend y sus dos clientes (app y panel).

**Independent Test**: Con la API en desarrollo, abrir `/docs` y ver cada endpoint con resumen, rol, ejemplos y códigos de respuesta.

**Acceptance Scenarios**:

1. **Given** la API en desarrollo o en el entorno de pruebas, **When** abro `/docs`, **Then** veo todos los endpoints de `/api/v1` agrupados por módulo, cada uno con resumen, descripción, rol que puede usarlo, cuerpo de ejemplo y todos sus códigos de respuesta.
2. **Given** la API en producción, **When** se pide `/docs` o `/openapi/v1.json`, **Then** responde 404 (R-07: no se publica la superficie de ataque).
3. **Given** un endpoint que no coincide con su contrato en `specs/*/contracts/`, **When** se ejecuta la suite, **Then** la prueba de contrato falla.

---

### User Story 3 - Seguridad común a todas las peticiones (Priority: P1)

Como responsable del sistema, quiero que toda petición pase por la misma validación de sesión, roles, límite de tasa, cabeceras y manejo de errores, para que ningún módulo dependa de la app o del panel para protegerse (ADR-009, regla 8 de `estilo-arquitectonico.md`).

**Why this priority**: Todas las features necesitan saber quién llama (rol) y limitar el abuso con la IP real.

**Independent Test**: Llamar a un endpoint de prueba sin token, con token vencido, con cada rol y en ráfaga; comprobar 401, el rol resuelto y 429.

**Acceptance Scenarios**:

1. **Given** una petición sin token, **When** llega a la API, **Then** se trata como rol `Anonimo`. **Given** un token inválido o vencido, **Then** responde 401 con `codigo = token_invalido`, sin datos internos.
2. **Given** un anónimo que superó 60 consultas en la última hora (clave seudónimo + IP real), **When** hace otra consulta, **Then** responde 429 con `Retry-After`; su cuota de 120 lotes por hora es independiente (RNF-11).
3. **Given** un conductor registrado (1 000 req/h) o un administrador (3 000 req/h) que superó su cuota, **When** pide algo más, **Then** responde 429 con `Retry-After`; la clave es su cuenta (RNF-11).
4. **Given** una petición que llega por el túnel, **When** se calcula la IP del cliente, **Then** se usa `CF-Connecting-IP`; si no llega por el túnel, esa cabecera se ignora (RNF-11, ADR-010).
5. **Given** cualquier error, **When** la API responde, **Then** el cuerpo es un `ProblemDetails` con `codigo` estable y `correlacionId`, sin trazas internas.
6. **Given** cualquier respuesta, **When** ningún módulo dijo lo contrario, **Then** lleva `Cache-Control: private, no-store` (ADR-006).
7. **Given** una misma IP real que envía peticiones anónimas rotando `X-KillaUru-Seudonimo`, **When** supera 600 lotes o 300 consultas en la última hora, **Then** responde 429 con `Retry-After` aunque cada seudónimo esté dentro de su cuota (D2).
8. **Given** una IP que ya hizo 5 registros de cuenta en la última hora, **When** intenta el 6.º, **Then** responde 429 con `Retry-After` (D8).
9. **Given** un endpoint que exige sesión, **When** llega sin token, **Then** responde 401; **When** llega con token de un rol sin permiso, **Then** responde 403 `sin_permiso` (T-SEG20).
10. **Given** un token de acceso vencido hace 1 s, **When** llega a la API, **Then** responde 401: no hay tolerancia de reloj (RNF-26, T-SEG09).
11. **Given** cualquier petición, incluido un lote con token, **When** se escriben logs y trazas, **Then** no aparecen seudónimo, IP, tokens, correos ni el par `sub` + `loteId` (T-SEG03).

---

### User Story 4 - Saber si el sistema está sano antes que los usuarios (Priority: P2)

Como equipo, quiero que una caída o una degradación me avise por correo y quede visible en una página de estado pública, para actuar antes de que los conductores lo noten (HU-17).

**Why this priority**: HU-17 es *Should* y se recorta al final, pero `/health` es base del despliegue (el *healthcheck* de Docker usa `/health/live`).

**Independent Test**: Detener la base de datos y comprobar que `/health/ready` responde 503 y que llega la alerta en ≤ 10 min.

**Acceptance Scenarios**:

1. **Given** la API viva, **When** se consulta `/health/live`, **Then** responde 200 sin cuerpo con datos internos (RF-28).
2. **Given** la base de datos caída, el evento pendiente más antiguo de la cola de ingesta con más de 5 min, el corredor sin cargar en Anomalías o migraciones pendientes, **When** se consulta `/health/ready`, **Then** responde 503 sin datos internos (RF-28, T-OPS19).
3. **Given** `/health/ready` falla en dos chequeos seguidos de UptimeRobot (cada 5 min), **When** se confirma la caída, **Then** llega un correo en ≤ 10 min y la página de estado pública de UptimeRobot lo muestra (RNF-20).
4. **Given** p95 > 250 ms en el mapa o > 300 ms en el lote (y la meta SDD del resto de rutas) o 5xx > 1 % durante 5 min, disco > 80 %, una tarea programada (incluido el backup) sin éxito en 26 h, o el uso del plan gratuito de Grafana ≥ 80 %, **Then** Grafana envía una alerta (RNF-20, RNF-22, T-OPS17, D12).
5. **Given** la VM en marcha, **When** se abre Grafana Cloud, **Then** se ven disco, CPU y memoria de la VM y los logs de Docker, enviados por Grafana Alloy sin abrir puertos (D12, ADR-010).
6. **Given** el contenedor de la API terminado con `kill -9`, **When** Docker lo detecta, **Then** lo vuelve a levantar solo (`restart: unless-stopped`) y el procesamiento se reanuda en ≤ 2 min (EQ-09, T-OPS08).

---

### User Story 5 - Volver atrás y recuperarse de una pérdida (Priority: P2)

Como equipo, quiero respaldo diario cifrado fuera del servidor, vuelta a la versión anterior en menos de 5 minutos y un procedimiento para recrear el servidor, para no perder la plataforma por un mal despliegue o por la pérdida de la VM (HU-16, RNF-07, RNF-18, RNF-24, RNF-34).

**Why this priority**: Protege AC-02, pero puede completarse cuando ya existan módulos con datos.

**Independent Test**: Simulacro en el entorno de pruebas (R-11): desplegar, volver al *tag* anterior y cronometrar; restaurar el último respaldo y medir RPO y RTO.

**Acceptance Scenarios**:

1. **Given** una versión nueva con una falla crítica, **When** se vuelve al *tag* anterior, **Then** el servicio funciona en < 5 min con el esquema nuevo, gracias a las migraciones en dos fases (RNF-18).
2. **Given** el respaldo diario, **When** se revisa el *bucket* privado de R2, **Then** hay un respaldo cifrado por día durante 30 días (roles con `pg_dumpall --roles-only` + datos con `pg_dump`), y la clave para descifrarlo está fuera de la VM (RNF-07, RNF-34, T-BD01).
3. **Given** una base de datos perdida, **When** se restaura en el entorno de pruebas, **Then** RPO ≤ 24 h y RTO ≤ 2 h, y antes de reabrir se revocan todas las `sesion_refresh`, se rota `KU_JWT_CLAVE_FIRMA`, se arranca la API (las tareas de purga corren al arrancar) y se comprueba 0 seudónimos de más de 72 h (RNF-24, RN-10, D9).
4. **Given** la VM perdida por completo (RS-01), **When** se sigue el procedimiento de recreación, **Then** el servicio vuelve en ≤ 4 h con el repositorio, los secretos guardados fuera y el último respaldo (RNF-24).
5. **Given** una restauración, **When** se revisa lo perdido, **Then** el runbook pide revisar a mano las solicitudes de eliminación del último día; las eliminaciones, desactivaciones y cambios de rol posteriores al backup se pierden como **riesgo aceptado**, acotado por el RPO de 24 h (D9).

---

### User Story 6 - Reglas de arquitectura y de tamaño que no se pueden romper (Priority: P1)

Como autor, quiero que la compilación o las pruebas fallen si un módulo usa el interior de otro, si hay ciclos, si una capa usa otra que no le corresponde, si el dominio depende de frameworks o del reloj real, o si un archivo de código pasa de 199 líneas, para que el sistema siga ordenado mientras crece (RNF-28; constitución II y VIII).

**Why this priority**: Impedir el desorden desde la primera línea es más barato que corregirlo.

**Independent Test**: Introducir a propósito una referencia prohibida o un archivo de 101 líneas y ver que la suite falla.

**Acceptance Scenarios**:

1. **Given** un módulo que usa una clase `internal` de otro, **When** se compila, **Then** no compila.
2. **Given** un `Domain` que usa EF Core o ASP.NET, un ciclo entre módulos o un `DbContext` ajeno, **When** se ejecutan las pruebas de arquitectura, **Then** fallan.
3. **Given** un archivo `.cs`, `.sql` de consultas, `.sh` o `.js` de más de 199 líneas (las migraciones `V*.sql`, los generados y los OpenAPI no cuentan), **When** se ejecutan las pruebas de arquitectura, **Then** fallan e indican el archivo y su número de líneas (T-QA17).
4. **Given** un controlador o DTO de `Adapters` que usa un tipo de `Domain`, un `Domain` que usa `Contracts/` de otro módulo o un `Application` que usa `Adapters` o `Infrastructure`, **When** se ejecutan las pruebas de arquitectura, **Then** fallan (D4).
5. **Given** cualquier código que llama `DateTime.Now/UtcNow` o `DateTimeOffset.Now/UtcNow`, **When** se compila, **Then** el analizador de API prohibidas rompe el build (T-QA16).

---

### User Story 7 - Cada cambio pasa por CI y se despliega solo (Priority: P1)

Como autor, quiero que cada *pull request* compile y pase todas las pruebas antes de unirse a `main`, y que cada unión a `main` se despliegue sola en la VM con respaldo previo, para no depender de pasos manuales ni publicar algo roto (RNF-17, RNF-18, RNF-28, EQ-05; D10).

**Why this priority**: RNF-28 se verifica con el «Pipeline de CI»; sin él, las reglas de US6 solo corren cuando alguien se acuerda.

**Independent Test**: Abrir un PR con una prueba rota → no se puede unir. Unir un PR verde → la imagen `ghcr.io/<cuenta>/killauru-api:<sha>` existe y la VM responde `/health/ready` 200 con ese *tag*.

**Acceptance Scenarios**:

1. **Given** un PR, **When** corre `.github/workflows/ci.yml`, **Then** ejecuta build (advertencias = errores), pruebas unitarias, de arquitectura (incluido el límite de líneas), de integración (Testcontainers), de contrato OpenAPI y la cobertura con coverlet ≥ 90 % de los proyectos `Domain`; si algo falla, `main` (protegida) no acepta la unión (D10).
2. **Given** una unión a `main`, **When** corre `.github/workflows/cd.yml`, **Then** construye la imagen arm64 en un runner `ubuntu-24.04-arm`, la publica en GHCR con etiqueta = commit y entra por SSH a través de Cloudflare Access a la VM para ejecutar `desplegar.sh <sha>`: respaldo → migraciones → `docker compose up -d` → espera `/health/ready` (D10).
3. **Given** una versión mala ya desplegada, **When** se ejecuta `desplegar.sh <sha-anterior>`, **Then** el servicio vuelve en < 5 min (RNF-18).
4. **Given** el repositorio, **When** se revisan los secretos del despliegue, **Then** `KU_CF_ACCESS_CLIENT_ID`, `KU_CF_ACCESS_CLIENT_SECRET` y `KU_DEPLOY_SSH_KEY` están solo como secretos de GitHub del entorno `produccion` y figuran en el inventario de RNF-34 (D10).

---

### User Story 8 - Saber cuánto aguanta el sistema antes de cada versión (Priority: P2)

Como autor, quiero pruebas de carga combinada, picos, estrés, resistencia y caos que corran fuera de la VM contra el entorno de pruebas, para comprobar RNF-03, RNF-06 y RNF-23 sin quitar recursos a producción (EQ-06, EQ-09, EQ-14; D11).

**Why this priority**: Es la única forma de medir los RNF de rendimiento; puede completarse cuando existan 005 y 006, que la referencian.

**Independent Test**: Correr `carga-combinada.k6.js` desde la PC del autor (o con el workflow manual `carga.yml`) contra «killauru-pruebas» y anotar el resultado en `simulacros.md`.

**Acceptance Scenarios**:

1. **Given** el entorno de pruebas con límites de CPU/RAM y límites de tasa relajados, **When** corre la carga combinada (300 sincronizando + 300 consultando el mapa, 10 min), **Then** p95 del lote < 300 ms, p95 del mapa < 250 ms, retraso de cola ≤ 30 s (p95), error < 1 % y ningún 429 (RNF-03, RNF-23, EQ-14).
2. **Given** una ráfaga de 600 clientes, **When** corre `picos.k6.js`, **Then** no hay 5xx ni eventos aceptados perdidos (EQ-06).
3. **Given** la escalera de `estres.k6.js` (de 100 en 100), **When** p95 pasa de 2× la meta o los errores de 1 %, **Then** la prueba se detiene y se registra el punto de quiebre; solo aquí se aceptan los 503 con `Retry-After`.
4. **Given** `resistencia.k6.js` (4 h al 50 %), **When** termina, **Then** memoria y p95 quedan estables y los logs rotan.
5. **Given** carga activa, **When** `caos.sh` hace `kill -9` del contenedor de la API, **Then** la conciliación da 0 eventos aceptados perdidos y 0 duplicados, y el procesamiento se reanuda en ≤ 2 min (RNF-06, EQ-09).

### Edge Cases

- `CF-Connecting-IP` llega en una petición que no vino por el túnel → se ignora; se usa la IP de la conexión.
- Anónimo sin cabecera `X-KillaUru-Seudonimo` → el límite de tasa usa solo la IP.
- Muchos conductores detrás de una misma IP (CGNAT) → el techo por IP solo cuenta peticiones **anónimas**; las autenticadas usan la cuota de su cuenta (D2).
- Grafana Cloud no responde → la API sigue funcionando; la telemetría se descarta sin bloquear peticiones.
- La API arranca con migraciones pendientes → no migra; `/health/ready` responde 503 hasta que el paso de migración termine.
- `/health/ready` en ráfaga → no se escribe en los logs (RNF-32) y la consulta a la BD se reutiliza durante 5 s.
- Un token con un rol desconocido → 401 (no se degrada a anónimo).
- La API se reinicia antes de la hora de una tarea diaria → la tarea corre igual **al arrancar** y luego a su hora fija (T-OPS05).
- `/health/ready` en 503 → Docker **no** reinicia la API (Compose no reinicia por `unhealthy`); solo la levanta si el proceso termina (T-OPS08).
- Un script de migración con error a mitad → `ON_ERROR_STOP` y `--single-transaction` lo revierten entero y `migraciones.aplicada` no lo registra (T-BD01).
- Un PR que solo cambia documentación → el CI corre igual; `main` no acepta uniones sin CI en verde (D10).

## Requirements *(mandatory)*

### Functional Requirements

**A. Solución y despliegue**

- **FR-001**: El sistema MUST levantarse completo (API, PostgreSQL + PostGIS, túnel, tarea de respaldo y Grafana Alloy) con un solo comando. *(HU-16, RNF-17; Alloy: D12)*
- **FR-002**: El sistema MUST ejecutar las migraciones solo como paso explícito, precedido de un respaldo, con el rol `killauru_migrador` (DDL), mientras la API usa `killauru_app` (solo DML); el arranque de la BD sigue FR-032. *(RNF-18, `diseño-interno-de-modulos.md` §3.7)*
- **FR-003**: Las migraciones MUST aplicarse en dos fases (agregar primero, borrar en un despliegue posterior) para que la versión anterior funcione con el esquema nuevo. *(RNF-18)*
- **FR-004**: El sistema MUST crear un esquema por módulo (`identidad`, `ingesta`, `anomalias`, `auditoria`, `dashboard`), sin llaves foráneas entre esquemas (comprobado por prueba). *(ADR-012; T-BD-CHK)*
- **FR-005**: El sistema MUST volver a la versión anterior en < 5 min desplegando la etiqueta anterior (`desplegar.sh <sha-anterior>`). *(RNF-18; D10)*
- **FR-006**: El sistema MUST ofrecer un entorno de pruebas aislado (otra BD en un volumen aparte, otros puertos, datos sintéticos) que se levanta solo para simulacros y pruebas de carga en la misma VM, con límites por contenedor (`cpus: 2`, `mem_limit: 6g` para API + BD de pruebas) y `ApiOptions` de pruebas con límites de tasa altos; la VM es una A1 de 4 OCPU / 24 GB (Always Free), con límites por contenedor también en producción. *(R-11; D11)*
- **FR-007**: El sistema MUST leer los secretos del inventario de RNF-34 desde variables de entorno fuera del repositorio (incluidos `KU_PG_SUPERUSUARIO` y, como secretos de GitHub, `KU_CF_ACCESS_CLIENT_ID`, `KU_CF_ACCESS_CLIENT_SECRET` y `KU_DEPLOY_SSH_KEY`), y documentar su copia cifrada fuera de la VM y su rotación. *(RNF-34; D10, T-BD01)*

**B. Seguridad común**

- **FR-008**: El sistema MUST validar el token de acceso en un middleware común, sin tolerancia de reloj (`ClockSkew = TimeSpan.Zero`), resolver el rol (`Anonimo` sin token; `ConductorRegistrado` o `Administrador` con token) y responder 401 ante un token inválido, vencido o con rol desconocido. *(ADR-009 punto 2; RNF-26, T-SEG09)*
- **FR-009**: El sistema MUST aceptar el token de acceso en la cabecera `Authorization: Bearer` (app) o en la cookie `ku_acceso` `HttpOnly; Secure; SameSite=Strict` (panel). *(ADR-009 punto 5)*
- **FR-010**: El sistema MUST aplicar el límite de tasa de RNF-11: 60 consultas/h y 120 lotes/h para el anónimo (clave seudónimo + IP real), 1 000 req/h para el conductor registrado y 3 000 req/h para el administrador (clave: cuenta), respondiendo 429 con `Retry-After`; además, los techos de FR-030 y FR-031.
- **FR-011**: El sistema MUST tomar la IP real de `CF-Connecting-IP` solo si la conexión viene del contenedor `cloudflared`. *(RNF-11, ADR-010)*
- **FR-012**: El sistema MUST responder todos los errores como `ProblemDetails` (RFC 9457) con `codigo` estable y `correlacionId`, sin trazas ni datos internos. *(Constitución IX)*
- **FR-013**: El sistema MUST marcar por defecto toda respuesta con `Cache-Control: private, no-store`; solo un endpoint anónimo puede declararse público. *(ADR-006)*
- **FR-014**: El sistema MUST enviar HSTS y `X-Content-Type-Options: nosniff`, permitir CORS solo desde `https://panel.killauru.pe` con credenciales, y servir solo HTTPS a través de Cloudflare. *(RNF-08, RNF-09)*
- **FR-030**: El sistema MUST aplicar, solo a las peticiones **anónimas** y con clave = IP real, un techo por IP de `LimiteTasa:TechoIpLotesPorHora = 600` lotes/h y `LimiteTasa:TechoIpConsultasPorHora = 300` consultas/h, que se suma a la cuota por seudónimo + IP y responde 429 con `Retry-After`. *(RNF-11, ADR-015; D2)*
- **FR-031**: El sistema MUST ofrecer la política de límite de tasa `registro`, con clave = IP real y `LimiteTasa:RegistrosPorHoraPorIp = 5`, que aplica el endpoint de registro de 003; el 6.º registro en la hora responde 429 con `Retry-After`. *(RNF-11; D8)*
- **FR-034**: Los logs y las trazas MUST NOT contener seudónimo, IP, tokens (de acceso, de actualización o de enlace), cookies, correos, cuerpos de petición ni el par `sub` + `loteId`; un filtro de Serilog y de los atributos de OpenTelemetry los quita, y una prueba lo verifica. *(RN-09, RNF-32, R-06; T-SEG03)*
- **FR-045**: Todos los endpoints MUST seguir una regla única: sin token en un endpoint que exige sesión → 401; con token y rol insuficiente → 403 `sin_permiso`; un parámetro reservado (`estado`) usado por otro rol, con o sin token → 403 `sin_permiso`. La regla vive en `contracts/convenciones-api.md`. *(ADR-009, RN-08; T-SEG20)*

**C. Documentación de la API**

- **FR-015**: El sistema MUST generar el documento OpenAPI desde el código y mostrarlo en una página navegable (`/docs`) en desarrollo y en el entorno de pruebas, y responder 404 a ambos en producción. *(Constitución IX; R-07)*
- **FR-016**: Cada endpoint MUST declarar resumen, descripción, rol permitido, ejemplos y todos sus códigos de respuesta (429 en todas las operaciones de `/api/v1`), y coincidir con el contrato de su spec (prueba de contrato). *(Constitución IX; T-SEG19)*
- **FR-017**: Cada módulo MUST tener un `README.md` con propósito, endpoints y ejemplos. *(RNF-19)*

**D. Salud y observabilidad**

- **FR-018**: El sistema MUST exponer `GET /health/live` (el proceso responde) y `GET /health/ready`, que responde 200 solo si: la BD responde (001), la cola de ingesta no tiene eventos pendientes de más de 5 min (005), el corredor está cargado (004) y no hay migraciones pendientes (001, versión esperada frente a `migraciones.aplicada`); sin datos internos. *(RF-28, ADR-010; T-OPS19)*
- **FR-019**: Cada módulo MUST poder sumar su propia comprobación a `/health/ready` sin que el host conozca su interior. *(ADR-002)*
- **FR-020**: El sistema MUST emitir métricas, trazas (muestreo ≤ 10 %) y logs JSON de nivel `Information` o superior con id de correlación, sin registrar `/health` y sin etiquetas de alta cardinalidad (seudónimo, IP, `eventoId`). *(RNF-21, RNF-32)*
- **FR-021**: El sistema MUST **enviar** la telemetría a Grafana Cloud por OTLP, sin exponer `/metrics` ni abrir puertos. *(ADR-010)*
- **FR-022**: El sistema MUST ofrecer a los módulos un puerto de métricas con: lotes, rechazos y descartes por motivo, retenidos por tope (RN-15a), duración del procesamiento, retraso de la cola (`RegistrarRetrasoCola`, histograma `killauru.ingesta.retraso_cola_s`, lo emite Ingesta), tamaño y antigüedad de la cola, última ejecución correcta de cada tarea y acciones de riesgo (emisor único: Auditoría, después del commit). *(RNF-22, RNF-23, RNF-29; D11, D15)*
- **FR-040**: El compose de producción MUST incluir un contenedor `alloy` (imagen oficial `grafana/alloy`, fija por versión) que **solo envía** a Grafana Cloud: disco, CPU y memoria de la VM, logs de Docker y la métrica del backup leída de un archivo `.prom` de un volumen compartido; alertas: disco > 80 % y tarea > 26 h; dentro del plan gratuito sin tarjeta. *(RNF-22, RNF-32, ADR-010; D12)*
- **FR-041**: Cada servicio del compose MUST tener `restart: unless-stopped`, *healthcheck* (la API con `/health/live`) y rotación de logs (`json-file`, `max-size: 10m`, `max-file: 3`). *(RNF-22, EQ-09; T-OPS08, T-OPS09, D12)*
- **FR-046**: Las alertas de p95 MUST ser por ruta: mapa > 250 ms y lote > 300 ms (docs); el resto de rutas, 250 ms como **meta SDD**, salvo las de sesión de 003 con su propia meta SDD de 500 ms; 5xx > 1 % se mantiene global. *(RNF-01, RNF-02, RNF-20; T-OPS17)*

**E. Kernel compartido**

- **FR-023**: El sistema MUST ofrecer los value objects `Coordenada`, `Severidad` (1–5, mediana redondeada hacia arriba), `Confidence` (0.0–1.0), `SeudonimoInstalacion` (UUID) y `Celda` (rejilla de 0,005°, con `Alrededor` en orden ascendente y `Limites()`). *(`diseño-interno-de-modulos.md` §6.1, RN-02, RN-05)*
- **FR-024**: El sistema MUST ofrecer un publicador de eventos de dominio en memoria (Observer) al que los módulos se suscriben por tipo de evento; el publicador lo llama el módulo **después** de su commit. *(ADR-006)*
- **FR-025**: El sistema MUST ofrecer un contexto de transacción por petición o por trabajo (`IContextoTransaccion`) que permita a Auditoría unirse a la transacción del módulo que actúa, ejecute las acciones registradas con `AlConfirmar(Func<Task>)` **solo después** del commit, y libere la transacción tras el commit o el rollback. *(ADR-013; D15, ARQ-10; ver `research.md` R-005)*
- **FR-026**: El sistema MUST registrar `TimeProvider` como único reloj. *(ADR-012)*
- **FR-042**: El sistema MUST ofrecer `Shared/Tareas/ProgramaReloj` (próximo instante con `TimeProvider`) para que todas las tareas programadas corran **al arrancar** y luego por reloj: horarias en un minuto fijo (caducidad `:05`, purgas de seudónimo `:10`) y diarias a una hora local fija (00:30 indicadores, 02:00 backup, 03:00 purga de recepción, 03:30 purga histórica, 03:45 purga de Identidad); nada usa `PeriodicTimer` de 24 h. *(`diseño-interno` §3.8, RNF-22; T-OPS05)*

**F. Respaldo y recuperación**

- **FR-027**: El sistema MUST hacer un respaldo diario a las 02:00 (hora local), cifrado, a un *bucket* privado de R2 con retención de 30 días, y otro antes de cada migración; cada respaldo guarda los roles (`pg_dumpall --roles-only`) y los datos (`pg_dump`), y al terminar bien escribe `killauru_tarea_ultima_ok{tarea="backup"} <unix>` en el archivo `.prom` que lee Alloy. *(RNF-07, RNF-18, RNF-22; T-BD01, D12)*
- **FR-028**: El sistema MUST documentar en *runbooks*: despliegue, vuelta atrás, restauración (FR-033), recreación de la VM en ≤ 4 h, rotación de secretos y monitoreo (calendario de tareas). *(RNF-24, RNF-34)*
- **FR-033**: El runbook `restaurar.md` MUST, antes de reabrir: restaurar roles y datos, revocar todas las `sesion_refresh` (`UPDATE … SET revocado_utc = …`), rotar `KU_JWT_CLAVE_FIRMA`, arrancar la API (las purgas corren al arrancar, FR-042), comprobar 0 seudónimos de más de 72 h y revisar a mano las solicitudes de eliminación del último día. Las eliminaciones, desactivaciones y cambios de rol posteriores al backup se pierden: **riesgo aceptado**, acotado por el RPO de 24 h. *(RN-10, RNF-07, RNF-24; D9)*
- **FR-043**: Las verificaciones periódicas MUST estar en un calendario del runbook `simulacros.md` (qué, cada cuánto, dónde se anota): restauración mensual, revisión de privilegios mensual, revisión de costo y de uso de Grafana mensual, simulacro de incidente semestral (EQ-13) y pruebas de carga antes de cada versión y mensual. *(RNF-07, RNF-16, RNF-29, RNF-30, RNF-32; T-QA14, D11)*

**G. Reglas de arquitectura, CI/CD y pruebas de sistema**

- **FR-029**: La compilación o las pruebas MUST fallar ante: uso del interior de otro módulo, ciclos, `Domain` con EF Core o ASP.NET, uso de un `DbContext` ajeno, o un archivo de código de más de 199 líneas (`.cs`, `.sql` de consultas, `.sh` y `.js`; excluye migraciones `V*.sql`, generados y OpenAPI). *(RNF-28; constitución II y VIII; T-QA17)*
- **FR-032**: El arranque de la BD MUST hacerse con `V0000__arranque.sql`, que `migrar.sh --arranque` ejecuta una sola vez **como superusuario** (`KU_PG_SUPERUSUARIO`): roles `killauru_migrador`/`killauru_app`, extensiones `postgis` y `citext`, esquemas, tabla `migraciones.aplicada` y permisos. `V0001…V0007` corren con `killauru_migrador` y `psql -v ON_ERROR_STOP=1 --single-transaction`. Testcontainers aplica los mismos scripts con los mismos roles. Orden del primer despliegue: arranque → migraciones → `up`. *(RNF-18, `diseño-interno` §3.7; T-BD01 — Decisión SDD)*
- **FR-035**: Las pruebas de arquitectura MUST verificar la tabla de capas de `enfoque` §3: Adapters ↛ Domain; Domain ↛ `Contracts/` de otro módulo; Domain ↛ Application/Adapters/Infrastructure; Application ↛ Adapters/Infrastructure; Infrastructure ↛ Adapters. *(RNF-28, `enfoque-arquitectonico.md` §3; D4)*
- **FR-036**: El build MUST fallar si algún código usa `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now` o `DateTimeOffset.UtcNow` (`Microsoft.CodeAnalysis.BannedApiAnalyzers` con `BannedSymbols.txt` en `Directory.Build.props`). *(ADR-012, constitución III; T-QA16)*
- **FR-037**: Cada PR MUST pasar `.github/workflows/ci.yml`: `dotnet build` (advertencias = errores), pruebas unitarias, de arquitectura, de integración (Testcontainers) y de contrato OpenAPI, y cobertura con **coverlet** ≥ 90 % en los proyectos `Domain` (puerta). `main` está protegida: no se une sin CI en verde. *(RNF-28, EQ-05, `principios-de-diseño.md`; D10)*
- **FR-038**: Cada unión a `main` MUST ejecutar `.github/workflows/cd.yml`: imagen arm64 (runner `ubuntu-24.04-arm`) → `ghcr.io/<cuenta>/killauru-api:<sha>` → SSH por Cloudflare Access (sin puertos abiertos) a la VM → `desplegar.sh <sha>` (respaldo → migraciones → `docker compose up -d` → espera `/health/ready`), en el entorno de GitHub `produccion`. Volver atrás = `desplegar.sh <sha-anterior>`. Carga y estrés quedan fuera del CI. *(RNF-17, RNF-18, ADR-010; D10)*
- **FR-039**: 001 MUST ser la única dueña de las pruebas de sistema en `backend/tests/carga/`: `carga-combinada.k6.js` (300 sincronizando + 300 mapa, 10 min), `picos.k6.js` (ráfaga de 600), `estres.k6.js` (escalera de 100 en 100 hasta p95 > 2× la meta o errores > 1 %, registrando el punto de quiebre), `resistencia.k6.js` (4 h al 50 %) y `caos.md` + `caos.sh` (`kill -9` del contenedor de la API bajo carga, conciliación exacta). Corren fuera de la VM (PC del autor o workflow manual `carga.yml`) contra el entorno de pruebas, en horario de poco tráfico. Error = todo 5xx; 503 con `Retry-After` solo se acepta en `estres`; 429 no debe aparecer. *(RNF-03, RNF-06, RNF-23, EQ-06, EQ-09, EQ-14; D11)*
- **FR-044**: Una prueba de integración de punta a punta MUST recorrer lote → anomalía → mapa (invalidación) → indicador del día en `KillaUru.Integracion.Tests/PuntaAPunta/`. *(`enfoque` §6; T-QA20)*

### Reglas y parámetros que aplica esta feature

| Regla | Parámetro | Valor inicial | Dónde vive |
|---|---|---|---|
| RNF-11 anónimo, consultas | `LimiteTasa:AnonimoConsultasPorHora` | 60 | `ApiOptions` (host) |
| RNF-11 anónimo, lotes | `LimiteTasa:AnonimoLotesPorHora` | 120 | `ApiOptions` |
| RNF-11 registrado | `LimiteTasa:RegistradoPorHora` | 1 000 | `ApiOptions` |
| RNF-11 administrador | `LimiteTasa:AdministradorPorHora` | 3 000 | `ApiOptions` |
| D2 techo por IP, lotes anónimos | `LimiteTasa:TechoIpLotesPorHora` | 600 | `ApiOptions` |
| D2 techo por IP, consultas anónimas | `LimiteTasa:TechoIpConsultasPorHora` | 300 | `ApiOptions` |
| D8 registros por IP | `LimiteTasa:RegistrosPorHoraPorIp` | 5 | `ApiOptions` |
| RF-28 antigüedad máxima de la cola | `Ingesta:AntiguedadMaximaColaMinutos` | 5 | `IngestaOptions` (feature 005) |
| RNF-32 muestreo de trazas | `Telemetria:MuestreoTrazas` | 0,10 | `ApiOptions` |
| Constitución VIII | `LineasMaximasPorArchivo` | 199 | `KillaUru.Arquitectura.Tests` |
| D10 cobertura del dominio | umbral de coverlet en `ci.yml` | 90 % | `.github/workflows/ci.yml` |

### Key Entities

- **Rol**: `Anonimo`, `ConductorRegistrado`, `Administrador` (`10-glosario.md` §4).
- **Evento de dominio**: hecho publicado por un módulo tras su commit (por ejemplo `AnomaliaCambiada`).
- **Comprobación de salud**: aporte de cada módulo a `/health/ready`.
- **Tarea programada**: trabajo que corre al arrancar y luego por reloj (`ProgramaReloj`), con su métrica de última ejecución correcta.
- **Secreto**: clave de firma JWT, contraseña de la BD (por rol y del superusuario), clave del proveedor de correo, credenciales de R2, token de Grafana, token del túnel, clave de cifrado del respaldo (RNF-34) y, en GitHub, las credenciales de Cloudflare Access y la llave SSH de despliegue (D10).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El backend completo queda en marcha con 1 comando en un entorno limpio (EQ-05).
- **SC-002**: La vuelta a la versión anterior toma < 5 min en el simulacro (RNF-18).
- **SC-003**: Una caída genera alerta por correo en ≤ 10 min (RNF-20).
- **SC-004**: El 100 % de los endpoints de `/api/v1` aparecen en `/docs` con todos sus códigos y coinciden con su contrato.
- **SC-005**: El 100 % de las pruebas de arquitectura pasan y 0 archivos de código superan 199 líneas.
- **SC-006**: La restauración cumple RPO ≤ 24 h y RTO ≤ 2 h; la recreación de la VM, ≤ 4 h (RNF-24).
- **SC-007**: Disponibilidad mensual ≥ 99 % (RNF-05).
- **SC-008**: Costo mensual ≤ US$ 10, con alerta a US$ 8 (RNF-16).
- **SC-009**: 0 uniones a `main` sin CI en verde; cobertura de los proyectos `Domain` ≥ 90 % en cada PR (D10).
- **SC-010**: En la carga combinada: p95 del lote < 300 ms, p95 del mapa < 250 ms, retraso de cola ≤ 30 s (p95), error < 1 % y 0 respuestas 429 (RNF-03, RNF-23; D11).
- **SC-011**: En la prueba de caos: 0 eventos aceptados perdidos, 0 duplicados y reanudación ≤ 2 min (RNF-06, EQ-09).
- **SC-012**: Tras una restauración: 0 `sesion_refresh` sin revocar y 0 seudónimos de más de 72 h antes de reabrir (D9).

## Fuera de alcance

- Emitir, renovar o revocar tokens (feature 003); 003 aplica la política `registro` en su endpoint.
- Las comprobaciones de la cola y del corredor para `/health/ready` las **implementan** Ingesta (005) y Anomalías (004); aquí solo se deja el punto de extensión y la de migraciones.
- Procedimiento de incidentes de datos personales (RNF-29): feature 002.
- Las cabeceras CSP y `X-Frame-Options` del panel se configuran en Cloudflare Pages (`_headers`), en la feature del panel (010).
- Los scripts de carga de 005 y 006 se reemplazan por los de FR-039; esas features solo los referencian (D11).

## Assumptions

- Hay una VM Oracle Cloud ARM Always Free A1 de 4 OCPU / 24 GB en Santiago y una cuenta de Cloudflare con `killauru.pe` (R-04, R-05; D11).
- La imagen PostGIS arm64 se verifica antes de fijar el *compose* (RS-02; `research.md` R-008).
- Grafana Cloud sin tarjeta y UptimeRobot con su página pública gratuita (RNF-32, RS-03).
- El repositorio está en GitHub; GitHub Actions, GHCR y Cloudflare Access (plan gratuito) alcanzan para el CI/CD (D10).
