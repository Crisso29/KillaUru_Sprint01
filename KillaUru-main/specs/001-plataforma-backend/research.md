# Research: Plataforma base del backend

Cada decisión indica de qué documento sale. Las marcadas **«Decisión de diseño SDD»** no están en `referencia/docs/` y se justifican aquí; las marcadas **«D#»** son decisiones del autor del 2026-10-21 (`specs/revision/decisiones.md`).

## R-001 · Documentación de la API

- **Decision**: `Microsoft.AspNetCore.OpenApi` (incluido en .NET 10, OpenAPI 3.1) genera `/openapi/v1.json`; **Scalar** (`Scalar.AspNetCore`) lo muestra en `/docs`. Comentarios XML (`GenerateDocumentationFile`) y atributos `[EndpointSummary]`, `[EndpointDescription]`, `[ProducesResponseType]` en cada acción. Un *transformer* agrega a cada operación el rol permitido y los esquemas de seguridad. En producción, `Api:DocumentacionPublica = false` → 404.
- **Rationale**: es la vía oficial de .NET 10 (Swashbuckle ya no viene en las plantillas); costo cero; la documentación sale del código, así no se desactualiza. Ocultarla en producción reduce la superficie expuesta (R-07).
- **Alternatives considered**: Swashbuckle + Swagger UI (mantenimiento incierto, duplica lo que .NET ya trae); NSwag (más pesado); documentación escrita a mano (se desactualiza).
- **Contract-first**: el YAML de `specs/*/contracts/` es la fuente. `KillaUru.Integracion.Tests` compara el documento generado con el YAML (rutas, métodos, códigos de respuesta y esquemas). *(Constitución IX — Decisión de diseño SDD.)*

## R-002 · Validación de la sesión y roles (ADR-009)

- **Decision**: `JwtBearer` valida firma, emisor, audiencia y vencimiento (5 min) con `ClockSkew = TimeSpan.Zero`: el mismo proceso emite y valida con el mismo `TimeProvider`, así que no hace falta tolerancia (RNF-26, T-SEG09). El token se lee de `Authorization: Bearer` o, si no viene, de la cookie `ku_acceso` (panel). Políticas: `SoloAdministrador`, `SoloConductorRegistrado`, `Autenticado` (cualquiera de los dos). Sin token = rol `Anonimo`. Un token con un rol que no está en `Shared/Domain/Roles` → 401. Sin token en un endpoint con política → 401; con token y rol insuficiente → 403 `sin_permiso` (T-SEG20).
- **Rationale**: ADR-009 punto 2 (middleware transversal) y punto 5 (cookie `HttpOnly; Secure; SameSite=Strict` para el panel). `panel.killauru.pe` y `api.killauru.pe` son el mismo sitio (`killauru.pe`), así que la cookie `SameSite=Strict` viaja; CORS solo permite `https://panel.killauru.pe` con credenciales. RNF-26 pide perder el acceso en ≤ 5 min: una tolerancia de 30 s lo rompía.
- **Alternatives considered**: consultar la BD en cada petición (descartado en ADR-009); sesiones de servidor (estado extra).

## R-003 · Controladores `internal` (hallazgo de la revisión de coherencia)

- **Decision**: los controladores siguen siendo `internal` (regla «todo internal salvo `Contracts/`», `diseño-interno-de-modulos.md` 2). El host registra un `ControladoresInternosFeatureProvider` que acepta tipos no públicos que heredan de `ControllerBase`, y cada módulo agrega su ensamblado con `AddApplicationPart`.
- **Rationale**: ASP.NET Core, por defecto, **solo descubre controladores públicos**; sin esto los endpoints no existirían. Así se respeta docs sin hacer públicos los adaptadores.
- **Alternatives considered**: hacer públicos los controladores (rompe la regla de visibilidad); Minimal APIs (docs define controladores).

## R-004 · Límite de tasa (RNF-11, D2, D8)

- **Decision**: `Microsoft.AspNetCore.RateLimiting` con particiones: anónimo-consulta (60/h, clave `seudónimo:IP`), anónimo-lote (120/h, misma clave), registrado (1 000/h, clave `sub`), administrador (3 000/h, clave `sub`). Ventana deslizante en memoria (una instancia, S-03). El seudónimo llega en la cabecera `X-KillaUru-Seudonimo` (RF-27). La IP real: `ForwardedHeaders` acepta `CF-Connecting-IP` solo cuando la conexión viene de `cloudflared` (red interna de Docker).
- **Techo por IP (D2)**: para las peticiones **anónimas**, un segundo limitador con clave = IP real, encadenado con `PartitionedRateLimiter.CreateChained`: `TechoIpLotesPorHora = 600` y `TechoIpConsultasPorHora = 300`. Rotar el seudónimo ya no da cuota nueva; las cuentas (CGNAT) no cuentan para el techo.
- **Registro (D8)**: política `registro` con clave = IP real y `RegistrosPorHoraPorIp = 5`; la aplica 003 en su endpoint de registro. «Correo en uso» (409) queda como riesgo aceptado mitigado por este tope.
- **Rationale**: es nativo de ASP.NET Core y sin costo; los techos cierran SEG-01/SEG-08/QA-15.
- **Alternatives considered**: límites en Cloudflare (el plan gratis no distingue roles); Redis (descartado, ADR-006).

## R-005 · Auditoría en la misma transacción (ADR-013, D15) — Decisión de diseño SDD

- **Decision**: `Shared/Persistencia/IContextoTransaccion` (servicio *scoped*) guarda la `DbConnection` y la `DbTransaction` (tipos de la BCL, no de EF) abiertas por el módulo que actúa. `AuditoriaDbContext` se crea con esa conexión y llama `Database.UseTransaction(tx)`. `IAuditoriaService.RegistrarAsync` **exige** esa transacción (si falta, `InvalidOperationException`); las lecturas sensibles usan `RegistrarLecturaAsync` con transacción propia (002).
- **`AlConfirmar` y liberación (D15, ARQ-10)**: `AlConfirmar(Func<Task>)` guarda acciones que se ejecutan **después** del commit (la métrica de acciones de riesgo de Auditoría). El módulo confirma con `ConfirmarAsync(ct)` del contexto (commit → acciones de `AlConfirmar` → liberar) o revierte con `RevertirAsync(ct)` (rollback → descartar acciones → liberar). Tras liberar, `Conexion` y `Transaccion` vuelven a `null`, así una segunda transacción del mismo *scope* no usa una ya cerrada.
- **Rationale**: cumple «acción y auditoría en la misma transacción» sin compartir el `DbContext` (regla 3) y sin cambiar la firma documentada; la métrica no se emite si la acción se revierte (SEG-18).
- **Alternatives considered**: pasar la transacción como parámetro (cambia el contrato); *Outbox* (descartado en ADR-013); detectar el commit con interceptores de EF (no sirve para transacciones de ADO.NET).

## R-006 · Observabilidad (ADR-010, RNF-21, RNF-32)

- **Decision**: OpenTelemetry con exportador OTLP/HTTP a Grafana Cloud (métricas, trazas y logs). Instrumentación de ASP.NET Core con `http.route` (plantilla, nunca la ruta cruda), de donde salen p95 y 5xx por ruta. `TraceIdRatioBasedSampler(0.1)`. Serilog con salida JSON compacta a consola y al *sink* OTLP. Filtro que excluye `/health/*` de logs y trazas. `IMetricas` lo implementa el host con `System.Diagnostics.Metrics` (contadores e histogramas sin etiquetas de alta cardinalidad).
- **Logs sin datos personales (T-SEG03)**: `FiltroDatosPersonales` (política de Serilog + procesador de OpenTelemetry) quita propiedades y atributos con seudónimo, IP, `Authorization`, cookies, tokens, correos y cuerpos de petición, y nunca registra `sub` junto a `loteId`. Una prueba captura los logs de un lote enviado con token y verifica que no aparece ninguno.
- **Alertas de p95 por ruta (T-OPS17)**: mapa > 250 ms (RNF-01/RNF-20), lote > 300 ms (RNF-02); resto 250 ms (meta SDD); rutas de sesión de 003 500 ms (meta SDD de 003); 5xx > 1 % global.
- **Alternatives considered**: Prometheus + Grafana propios (descartado en ADR-010).

## R-007 · Salud (RF-28, T-OPS19)

- **Decision**: `AddHealthChecks()`; `/health/live` sin comprobaciones (solo el proceso); `/health/ready` con etiqueta `ready` y cuatro aportes: PostgreSQL (`SELECT 1`, resultado guardado 5 s) y migraciones al día (`ChequeoMigraciones`: versión esperada por la imagen frente a `migraciones.aplicada`), ambos de 001; la cola, que **aporta Ingesta** (005); y el corredor cargado, que **aporta Anomalías** (004, reintenta la carga mientras no esté listo). Respuesta sin cuerpo detallado.
- **Rationale**: FR-019: cada módulo aporta su chequeo sin que el host lo conozca. Con el orden arranque → migraciones → `up` del primer despliegue, `ready` queda en 200 sin reiniciar.

## R-007b · Errores

- **Decision**: `AddProblemDetails()` + un manejador global que agrega `codigo` (por ejemplo `token_invalido`, `sin_permiso`, `limite_excedido`, `error_interno`) y `correlacionId`. Solo ese manejador captura excepciones (`principios-de-diseño.md` 6).

## R-008 · Imagen de PostgreSQL + PostGIS en arm64 (RS-02)

- **Decision**: verificar al inicio que la imagen oficial `postgis/postgis` publique la etiqueta arm64 de la versión elegida. Si no existe, construir una imagen propia `FROM postgres:17` + paquetes `postgresql-17-postgis-3` de Debian (que sí existen para arm64), fijada por *digest*. Testcontainers usa la misma versión (amd64 en el runner de CI).
- **Rationale**: RS-02 pide verificarlo antes de fijar el *compose*.

## R-009 · Migraciones y arranque de la BD (RNF-18, T-BD01) — Decisión de diseño SDD

- **Decision**: `V0000__arranque.sql` lo corre `migrar.sh --arranque` **una sola vez como superusuario** (`KU_PG_SUPERUSUARIO`): crea los roles `killauru_migrador` y `killauru_app` (contraseñas por variables de `psql`), las extensiones `postgis` y `citext`, los esquemas, la tabla `migraciones.aplicada` (dueño `killauru_migrador`) y los permisos. `V0001…V0007` los aplica `migrar.sh` conectado **como** `killauru_migrador`, con `psql -v ON_ERROR_STOP=1 --single-transaction`, después de un respaldo. EF Core **no** genera migraciones (`diseño-interno-de-modulos.md` 3.7). Testcontainers aplica los mismos scripts con los mismos roles, así `ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador` vale igual en pruebas. Orden del primer despliegue: arranque → migraciones → `up`.
- **Rationale**: `CREATE ROLE` y `CREATE EXTENSION postgis` exigen superusuario: el rol migrador no puede crearse a sí mismo (BD-01, OPS-10).
- **Alternatives considered**: `dotnet ef database update` al arrancar (prohibido por RNF-18); herramientas como Flyway (otro componente); migrar todo como superusuario (los privilegios por defecto no se aplicarían a `killauru_app`).

## R-010 · Límite de 200 líneas por archivo y API prohibidas (pedido del autor, T-QA16, T-QA17)

- **Decision**: prueba en `KillaUru.Arquitectura.Tests` que recorre `backend/src/**/*.cs`, `backend/tests/**/*.cs`, los `.sql` de consultas (`db/**/*.sql` salvo `db/migraciones/V*.sql`), `deploy/**/*.sh` y `tests/carga/**/*.js`, y falla si un archivo supera 199 líneas; excluye migraciones, generados y OpenAPI (constitución VIII). Además `Microsoft.CodeAnalysis.BannedApiAnalyzers` (RS0030) con `BannedSymbols.txt` (`DateTime.Now/UtcNow`, `DateTimeOffset.Now/UtcNow`) en `Directory.Build.props`: con advertencias como errores, rompe el build. NetArchTest queda para las capas.
- **Rationale**: NetArchTest revisa dependencias entre tipos y no distingue la llamada a `UtcNow` (QA-16); la constitución cuenta también SQL y scripts (QA-17, ARQ-04).

## R-011 · Respaldo (RNF-07, RNF-34, T-BD01, D12)

- **Decision**: contenedor `backup` con cron interno a las 02:00 hora local (después de los indicadores de 00:30 y antes de las purgas de 03:00): `pg_dumpall --roles-only` + `pg_dump -Fc` → comprobación con `pg_restore --list` → cifrado con `age` (clave pública en el servidor, clave privada **solo fuera de la VM**) → subida a un *bucket* privado de R2 con `rclone`; regla de ciclo de vida de 30 días en el *bucket*. Al terminar bien escribe `killauru_tarea_ultima_ok{tarea="backup"} <unix>` en un archivo `.prom` de un volumen compartido con `alloy`, que lo envía a Grafana (alerta a las 26 h).
- **Rationale**: `pg_dump` no guarda roles; sin ellos la restauración falla o deja dueños incorrectos (OPS-12). `IMetricas` vive en el proceso de la API y no sirve para otro contenedor (OPS-03).
- **Alternatives considered**: `gpg` simétrico (la clave tendría que estar en la VM); POST OTLP desde el script (más código que mantener).

## R-012 · Entorno de pruebas (R-11, D11)

- **Decision**: `docker-compose.pruebas.yml` con nombre de proyecto `killauru-pruebas`, otra BD en un **volumen aparte**, otros puertos, sin `cloudflared` público, con `db/semillas/` sintéticas, `ASPNETCORE_ENVIRONMENT=Pruebas` (`appsettings.Pruebas.json` con límites de tasa altos) y límites `cpus: 2` y `mem_limit: 6g` para API + BD de pruebas. Admite simulacros **y pruebas de carga**, en horario de poco tráfico. Se levanta solo para eso y se apaga al terminar. La VM es una A1 de 4 OCPU / 24 GB (Always Free) y producción también tiene límites por contenedor.
- **Rationale**: R-11 (no restar recursos a producción) y OPS-21/QA-04 (k6 en la misma VM competiría con producción).

## R-013 · CI/CD con GitHub Actions (D10)

- **Decision**: `ci.yml` en cada PR: `dotnet build` (advertencias = errores) → unitarias → arquitectura (incluye límite de líneas) → integración (Testcontainers) → contrato OpenAPI → cobertura con coverlet ≥ 90 % en los proyectos `Domain` (puerta). La rama `main` está protegida con el *check* de CI obligatorio. `cd.yml` al unir a `main`: imagen arm64 en el runner `ubuntu-24.04-arm` → `ghcr.io/<cuenta>/killauru-api:<sha>` → SSH por **Cloudflare Access** (token de servicio `KU_CF_ACCESS_CLIENT_ID`/`KU_CF_ACCESS_CLIENT_SECRET` y llave `KU_DEPLOY_SSH_KEY`, secretos del entorno de GitHub `produccion`) → `desplegar.sh <sha>`: respaldo → migraciones → `docker compose up -d` → espera `/health/ready`. Volver atrás = `desplegar.sh <sha-anterior>`. Carga y estrés quedan fuera del CI.
- **Rationale**: RNF-28 («Pipeline de CI»), EQ-05, ADR-010 (*tag* inmutable, sin puertos abiertos).
- **Alternatives considered**: despliegue manual (D10 eligió automático); *pull* desde la VM (otro agente en la VM).

## R-014 · Pruebas de carga, estrés, resistencia y caos (D11)

- **Decision**: 001 es la **única dueña** de `backend/tests/carga/` (005 y 006 los referencian): `carga-combinada.k6.js` (300 sincronizando + 300 mapa, 10 min), `picos.k6.js` (ráfaga de 600), `estres.k6.js` (escalera de 100 en 100 hasta p95 > 2× la meta o errores > 1 %; se registra el punto de quiebre), `resistencia.k6.js` (4 h al 50 %), `caos.md` + `caos.sh` (`kill -9` del contenedor de la API bajo carga; conciliación exacta). Corren **fuera de la VM** (PC del autor o `carga.yml` manual) contra «killauru-pruebas». Error = todo 5xx; 503 con `Retry-After` solo aceptado en `estres`; 429 no debe aparecer. Antes de cada versión y mensual (calendario en `simulacros.md`). El retraso de cola se mide con `killauru.ingesta.retraso_cola_s`.
- **Rationale**: QA-03…QA-09 y OPS-21: dos scripts separados nunca medían RNF-03 completo, y k6 en la VM competía con producción.

## R-015 · Grafana Alloy (D12)

- **Decision**: contenedor `alloy` con la imagen oficial `grafana/alloy` fija por versión, que **solo envía** (cumple ADR-010): métricas de la VM (disco, CPU, memoria), logs de Docker y el archivo `.prom` del backup. Alertas: disco > 80 % y tarea > 26 h. Rotación de logs de Docker (`json-file`, `max-size: 10m`, `max-file: 3`). Dentro del plan gratuito sin tarjeta (RNF-32).
- **Alternatives considered**: *gauge* de disco en la API (no ve CPU ni memoria del host); script `df` en el backup (no ve CPU ni memoria).

## R-016 · Tareas por reloj (T-OPS05)

- **Decision**: `Shared/Tareas/ProgramaReloj` calcula el próximo instante con `TimeProvider` (cada hora en un minuto fijo, o cada día a una hora local UTC−5). Toda tarea corre **al arrancar** y luego por reloj: caducidad `:05`, purgas de seudónimo `:10`; 00:30 indicadores (007), 02:00 backup (host), 03:00 purga de recepción 30 d (005), 03:30 purga histórica (004), 03:45 purga de Identidad (003). Nombres de métrica: `caducidad`, `purga-seudonimos`, `purga-seudonimos-recepcion`, `purga-recepcion`, `purga-historica`, `purga-identidad`, `indicadores`, `backup`. El calendario se publica en `monitoreo.md`.
- **Rationale**: con `PeriodicTimer` de 24 h, una tarea diaria no corre nunca si se despliega más de una vez al día, y el *gauge* en memoria queda vacío tras cada reinicio (OPS-05).

## R-017 · Restauración (D9)

- **Decision**: `restaurar.md`: restaurar roles y datos → antes de reabrir, `UPDATE identidad.sesion_refresh SET revocado_utc = …` para todas, rotar `KU_JWT_CLAVE_FIRMA`, arrancar la API (las purgas corren al arrancar, R-016) y comprobar 0 seudónimos de más de 72 h → revisar a mano las solicitudes de eliminación del último día. Las eliminaciones, desactivaciones y cambios de rol posteriores al backup se pierden: **riesgo aceptado**, acotado por el RPO de 24 h (RN-10, RNF-07). El simulacro mensual lo verifica.
- **Rationale**: una restauración revivía sesiones revocadas y privilegios retirados (SEG-10, SEG-11).

## Confirmados por el autor el 2026-10-21

001 no tenía pendientes abiertos. Quedan confirmadas por el autor las decisiones que esta feature aplica: techo por IP y tope de registros (R-004, D2 y D8), `AlConfirmar` y liberación del contexto (R-005, D15), restauración (R-017, D9), CI/CD (R-013, D10), pruebas de sistema y entorno de pruebas (R-012, R-014, D11) y Grafana Alloy (R-011, R-015, D12).

## Notas de implementación (2026-10-22)

Diferencias con el plan, todas por una razón técnica comprobada al programar:

1. **Cobertura con Microsoft.Testing.Platform**: .NET 10 ya no corre `dotnet test` con VSTest, y `coverlet.collector` solo funciona con VSTest. Se usa `Microsoft.Testing.Extensions.CodeCoverage` (oficial y gratuito), que genera el mismo informe Cobertura; la puerta ≥ 90 % del dominio la calcula `deploy/ci/cobertura_dominio.py`.
2. **Sin `PoliticasExtensions`**: los módulos protegen sus endpoints con `[Authorize(Roles = Roles.X)]` (los nombres viven en Shared). Políticas con nombre no tendrían ningún uso real (YAGNI). El 401/403 en ProblemDetails lo dan los eventos de JwtBearer en `AutenticacionExtensions`.
3. **`RechazoTokenInvalido` y `CorsPanelExtensions`** (archivos nuevos, una responsabilidad cada uno). El primero es necesario porque ASP.NET trata como anónimo, en silencio, un token inválido en un endpoint público; FR-008 exige 401.
4. **Shared usa las abstracciones de DI y de logging** de Microsoft.Extensions: el publicador de eventos resuelve los manejadores en un scope propio (pueden necesitar un DbContext) y registra sus fallas sin frenar a los demás. No son ASP.NET ni EF.
5. **PostgreSQL arm64**: `postgis/postgis:17-3.5` solo existe para amd64 (comprobado); `deploy/postgres/Dockerfile` arma la imagen sobre `postgres:17` + `postgresql-17-postgis-3` (R-008).

Defectos que encontraron las pruebas y se corrigieron: `CF-Connecting-IP` se aceptaba de cualquiera sin red del túnel; un token inválido en un endpoint público pasaba como anónimo; las respuestas de error salían con `no-cache` en vez de `private, no-store`; el contrato `plataforma.openapi.yaml` tenía YAML inválido en la línea 105.

Quedan para el autor (acciones fuera del código): configurar Grafana y UptimeRobot (T050), simulacro de restauración y vuelta atrás (T054), proteger `main` en GitHub (T070), Cloudflare Access para el SSH (T072), correr las pruebas de carga cuando existan 005 y 006 (T079), quickstart completo (T056), alertas de costo (T057) y la prueba de punta a punta cuando existan 004–007 (T080).
