---

description: "Tareas de la plataforma base del backend"
---

# Tasks: Plataforma base del backend

**Input**: `specs/001-plataforma-backend/` (spec, plan, research, data-model, contracts)

**Tests**: obligatorias (constitución VII). En cada historia, las pruebas se escriben primero y deben **fallar** antes de implementar.

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. `[P]` = puede hacerse en paralelo (otro archivo, sin dependencia pendiente). Las rutas son relativas a `backend/`, salvo `.github/` (raíz del repositorio). **Ningún archivo de código (`.cs`, `.sql` de consultas, `.sh`, `.js`) puede pasar de 100 líneas.** Tareas T058+ agregadas en la revisión 2026-10-21.

## Phase 1: Setup (solución y proyectos)

- [ ] T001 Crear `global.json` (SDK .NET 10), `Directory.Build.props` (net10.0, Nullable, TreatWarningsAsErrors, AnalysisLevel=latest-recommended, GenerateDocumentationFile, InvariantGlobalization, `Microsoft.CodeAnalysis.BannedApiAnalyzers`) y `.editorconfig` — raíz de `backend/` · (Constitución VIII, FR-036)
- [ ] T002 Crear `Directory.Packages.props` con versiones centralizadas de los paquetes de `plan.md` — `Directory.Packages.props`
- [ ] T003 Crear `KillaUru.sln` y los proyectos `src/KillaUru.Api`, `src/KillaUru.Shared` y los 6 `src/Modules/KillaUru.<Modulo>` con sus carpetas `Contracts/ Domain/ Application/ Adapters/ Infrastructure/` · (ADR-001, ADR-002)
- [ ] T004 Agregar `InternalsVisibleTo` de cada módulo solo hacia `KillaUru.<Modulo>.Tests` y `KillaUru.Integracion.Tests` — `src/Modules/*/KillaUru.<Modulo>.csproj` · (Constitución II)
- [ ] T005 [P] Crear los proyectos de prueba `tests/KillaUru.Shared.Tests`, `tests/KillaUru.Arquitectura.Tests`, `tests/KillaUru.Integracion.Tests` y los 6 `tests/KillaUru.<Modulo>.Tests`, con `coverlet.collector` · (FR-037)
- [ ] T006 [P] Crear `<Modulo>Module.cs` vacío y `README.md` (plantilla: propósito, endpoints, ejemplos) en cada módulo · (FR-017, RNF-19)
- [ ] T058 [P] `BannedSymbols.txt` con `DateTime.Now/UtcNow` y `DateTimeOffset.Now/UtcNow`; comprobar que una llamada de prueba rompe el build y revertirla — `BannedSymbols.txt` · (FR-036, T-QA16)

**Checkpoint**: `dotnet build` sin advertencias.

---

## Phase 2: User Story 6 — Reglas de arquitectura y de tamaño (P1) 🛡️ primero, para que todo lo demás nazca vigilado

### Pruebas

- [ ] T007 [P] [US6] `LimiteDeLineasTests`: recorre `src/**/*.cs`, `tests/**/*.cs`, `db/**/*.sql` (excepto `db/migraciones/V*.sql`), `deploy/**/*.sh` y `tests/carga/**/*.js`; excluye generados y OpenAPI; falla si algún archivo > 100 líneas, mostrando ruta y conteo — `tests/KillaUru.Arquitectura.Tests/LimiteDeLineasTests.cs` · (FR-029, T-QA17)
- [ ] T008 [P] [US6] `ReglasDeCapasTests`: `Adapters` ↛ `Domain` e `Infrastructure`; `Application` ↛ `Adapters`, `Infrastructure` y EF Core; `Infrastructure` ↛ `Adapters` — `tests/KillaUru.Arquitectura.Tests/ReglasDeCapasTests.cs` · (FR-035, D4)
- [ ] T059 [P] [US6] `ReglasDeDominioTests`: `Domain` ↛ `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore`, `Application`, `Adapters`, `Infrastructure` y `Contracts/` de **otro** módulo — `tests/KillaUru.Arquitectura.Tests/ReglasDeDominioTests.cs` · (FR-029, FR-035, D4)
- [ ] T009 [P] [US6] `ReglasDeModulosTests`: un módulo solo usa tipos `Contracts` de otro; el grafo permitido es exactamente el de `componentes-arquitectonicos.md` regla 4; ningún módulo referencia el `DbContext` de otro; `Shared` no referencia módulos — `tests/KillaUru.Arquitectura.Tests/ReglasDeModulosTests.cs` · (FR-029, RNF-28)

**Checkpoint**: las 4 pruebas pasan con el esqueleto vacío y fallan si se introduce una violación a propósito (comprobarlo y revertir).

---

## Phase 3: Kernel compartido (bloquea a todos los módulos)

### Pruebas

- [ ] T010 [P] `SeveridadTests`: `Crear(0|6) = null`, `Crear(1..5)` válido, `MedianaHaciaArriba({2,3,3,5}) = 3`, `({3,4}) = 4`, `({5}) = 5` — `tests/KillaUru.Shared.Tests/Domain/SeveridadTests.cs` · (RN-02, RN-05)
- [ ] T011 [P] `CeldaTests`: misma coordenada → misma celda; punto a 10 m de un borde → 2 celdas en `Alrededor(c,100)`; cerca de una esquina → 4; siempre en orden ascendente; `Limites()` contiene la coordenada que generó la celda — `tests/KillaUru.Shared.Tests/Domain/CeldaTests.cs` · (FR-023)
- [ ] T012 [P] `CoordenadaTests`, `ConfidenceTests`, `SeudonimoInstalacionTests` con valores límite — `tests/KillaUru.Shared.Tests/Domain/` · (FR-023)
- [ ] T013 [P] `PublicadorEnMemoriaTests`: entrega a todos los manejadores del tipo, en orden; una falla en uno no impide a los demás — `tests/KillaUru.Shared.Tests/Eventos/PublicadorEnMemoriaTests.cs` · (FR-024)
- [ ] T060 [P] `ContextoTransaccionTests`: `AlConfirmar` corre solo después de `ConfirmarAsync`, nunca tras `RevertirAsync`; tras confirmar o revertir, `Conexion` y `Transaccion` son `null`; `Unir` dos veces sin liberar → `InvalidOperationException` — `tests/KillaUru.Shared.Tests/Persistencia/ContextoTransaccionTests.cs` · (FR-025, D15)
- [ ] T061 [P] `ProgramaRelojTests` (`FakeTimeProvider`): 10:04:59 → `:05` de la misma hora; 10:05:00 → 11:05; diaria 03:30 local: 08:29:59Z → hoy 08:30Z, 08:30:01Z → mañana — `tests/KillaUru.Shared.Tests/Tareas/ProgramaRelojTests.cs` · (FR-042, T-OPS05)

### Implementación

- [ ] T014 [P] `Coordenada`, `Confidence`, `SeudonimoInstalacion` — `src/KillaUru.Shared/Domain/` · (FR-023)
- [ ] T015 [P] `Severidad` (con `MedianaHaciaArriba`) — `src/KillaUru.Shared/Domain/Severidad.cs` · (FR-023)
- [ ] T016 [P] `Celda` (paso 0,005°, `De`, `Alrededor`, `Limites`) — `src/KillaUru.Shared/Domain/Celda.cs` · (FR-023)
- [ ] T017 [P] `Roles` (3 constantes) — `src/KillaUru.Shared/Domain/Roles.cs` · (plan, Complexity Tracking)
- [ ] T018 [P] `IEventoDominio`, `IManejador<T>`, `IPublicadorEventos`, `PublicadorEnMemoria` — `src/KillaUru.Shared/Eventos/` · (FR-024)
- [ ] T019 [P] `IMetricas` con los 9 métodos de `data-model.md` §3.2 (incluido `RegistrarRetrasoCola`) — `src/KillaUru.Shared/Metricas/IMetricas.cs` · (FR-022)
- [ ] T020 [P] `IContextoTransaccion` y `ContextoTransaccion` (`Unir`, `AlConfirmar`, `ConfirmarAsync`, `RevertirAsync`, liberación) — `src/KillaUru.Shared/Persistencia/` · (FR-025, ADR-013, D15)
- [ ] T062 [P] `ProgramaReloj` (`ProximaHoraria`, `ProximaDiaria`, UTC−5 fijo) — `src/KillaUru.Shared/Tareas/ProgramaReloj.cs` · (FR-042)

**Checkpoint**: `KillaUru.Shared.Tests` en verde; cobertura de `Shared/Domain` ≥ 90 %.

---

## Phase 4: User Story 1 — Levantar con un comando (P1) 🎯 MVP

### Pruebas

- [ ] T021 [US1] `FabricaApi`: `WebApplicationFactory` + contenedor PostGIS de Testcontainers que aplica `V0000__arranque.sql` como superusuario y luego `V0001…` como `killauru_migrador`, igual que `migrar.sh`; la API se conecta como `killauru_app` — `tests/KillaUru.Integracion.Tests/Infraestructura/FabricaApi.cs` · (FR-032)
- [ ] T022 [US1] `MigracionesTests`: tras arranque + V0001 existen los 5 esquemas de módulo, `migraciones`, los 2 roles, `postgis`, `citext` y las filas `V0000`/`V0001`; `killauru_app` no puede `CREATE TABLE`; una tabla creada por `killauru_migrador` es legible y escribible por `killauru_app`; un script con error no deja nada aplicado — `tests/KillaUru.Integracion.Tests/Migraciones/MigracionesTests.cs` · (FR-002, FR-004, FR-032)
- [ ] T063 [US1] `SinFkEntreEsquemasTests`: con todas las migraciones aplicadas, ninguna FK de `information_schema` apunta a otro esquema — `tests/KillaUru.Integracion.Tests/Migraciones/SinFkEntreEsquemasTests.cs` · (FR-004)

### Implementación

- [ ] T023 [US1] Scripts `V0000__arranque.sql` y `V0001__privilegios.sql` (de `data-model.md` §1) — `db/migraciones/` · (FR-002, FR-004, FR-032)
- [ ] T024 [US1] `ApiOptions` (sección `Api`, incluidos `TechoIpLotesPorHora`, `TechoIpConsultasPorHora` y `RegistrosPorHoraPorIp`) y `appsettings.Pruebas.json` — `src/KillaUru.Api/Configuracion/ApiOptions.cs` · (FR-010, FR-030, FR-031, FR-006)
- [ ] T025 [US1] `ModulosExtensions` (llama a los 6 `Add<Modulo>Module` y `AddApplicationPart`) y `ControladoresInternosFeatureProvider` — `src/KillaUru.Api/Modulos/` · (R-003)
- [ ] T026 [US1] `Program.cs` mínimo: `TimeProvider.System`, `IContextoTransaccion` *scoped*, `IPublicadorEventos` *singleton*, módulos y controladores — `src/KillaUru.Api/Program.cs` · (FR-026)
- [ ] T027 [P] [US1] `Dockerfile` de la API (multi-etapa, `linux/arm64`, con `curl` para el *healthcheck*) — `deploy/Dockerfile` · (FR-001)
- [ ] T028 [US1] Verificar la imagen PostGIS arm64 (R-008); si no existe, `deploy/postgres/Dockerfile` — (RS-02)
- [ ] T029 [US1] `docker-compose.yml`: `api` (sin puertos publicados, *healthcheck* `/health/live`), `postgres` (volumen persistente), `cloudflared` (token del túnel), `backup`, `alloy`; red interna; en todos `restart: unless-stopped`, logs `json-file` (`max-size: 10m`, `max-file: 3`) y límites de CPU/memoria — `deploy/docker-compose.yml` · (FR-001, FR-041, ADR-010)
- [ ] T030 [P] [US1] `.env.example` con los nombres de `data-model.md` §5 (incluido `KU_PG_SUPERUSUARIO`) — `deploy/.env.example` · (FR-007)
- [ ] T031 [US1] `migrar.sh`: `--arranque` aplica `V0000` como superusuario; sin opción: respaldo previo + aplica en orden los `V*.sql` pendientes conectado como `killauru_migrador` con `psql -v ON_ERROR_STOP=1 --single-transaction` y registra en `migraciones.aplicada` — `deploy/migrar.sh` · (FR-002, FR-003, FR-032)
- [ ] T064 [US1] `desplegar.sh <sha>`: respaldo → `migrar.sh` → `docker compose up -d` con la imagen `ghcr.io/<cuenta>/killauru-api:<sha>` → espera `/health/ready` (falla si no llega a 200); volver atrás = `desplegar.sh <sha-anterior>` — `deploy/desplegar.sh` · (FR-005, FR-038)

**Checkpoint**: quickstart §2 completo (orden arranque → migraciones → `up`).

---

## Phase 5: User Story 3 — Seguridad común (P1)

### Pruebas

- [ ] T032 [US3] `SeguridadTests` (con un controlador de prueba solo en el proyecto de pruebas): sin token → `Anonimo`; sin token en endpoint con política → 401; token de rol sin permiso → 403 `sin_permiso`; token vencido hace 1 s / mal firmado / rol desconocido → 401 `token_invalido`; token por cookie `ku_acceso` → aceptado — `tests/KillaUru.Integracion.Tests/Seguridad/SeguridadTests.cs` · (FR-008, FR-009, FR-045, T-SEG09)
- [ ] T033 [US3] `LimiteTasaTests` (valores límite): 60.ª consulta anónima → 200 y 61.ª → 429 con `Retry-After`; 120.º lote → 200 y 121.º → 429; registrado 1 000/1 001; administrador 3 000/3 001; cuota de lotes independiente; `CF-Connecting-IP` ignorada fuera de `RedTunel` — `tests/KillaUru.Integracion.Tests/Seguridad/LimiteTasaTests.cs` · (FR-010, FR-011, T-QA18)
- [ ] T065 [US3] `TechoPorIpTests`: una IP con un seudónimo distinto por petición: 600.º lote → 200, 601.º → 429; 300.ª consulta → 200, 301.ª → 429; peticiones con token no cuentan; registro: 5.º → permitido, 6.º → 429 — `tests/KillaUru.Integracion.Tests/Seguridad/TechoPorIpTests.cs` · (FR-030, FR-031, T-QA18)
- [ ] T034 [US3] `ErroresYCabecerasTests`: excepción → 500 `ProblemDetails` con `codigo = error_interno` y `correlacionId`, sin traza; toda respuesta con `Cache-Control: private, no-store`, HSTS y `nosniff`; CORS solo para `OrigenPanel` — `tests/KillaUru.Integracion.Tests/Seguridad/ErroresYCabecerasTests.cs` · (FR-012…FR-014)

### Implementación

- [ ] T035 [US3] `AutenticacionExtensions` (JwtBearer, lectura de cookie, `ClockSkew = TimeSpan.Zero`, rol válido) — `src/KillaUru.Api/Seguridad/AutenticacionExtensions.cs` · (FR-008, FR-009)
- [ ] T036 [P] [US3] `PoliticasExtensions` (401 sin token, 403 `sin_permiso` con rol insuficiente) — `src/KillaUru.Api/Seguridad/PoliticasExtensions.cs` · (FR-008, FR-045)
- [ ] T037 [P] [US3] `IpRealExtensions` — `src/KillaUru.Api/Seguridad/IpRealExtensions.cs` · (FR-011)
- [ ] T038 [US3] `ClaveLimiteTasa` y `LimiteTasaExtensions` (políticas `consultas`, `lotes` y `registro`) — `src/KillaUru.Api/Seguridad/` · (FR-010, FR-031)
- [ ] T066 [US3] `TechoPorIp`: limitador por IP real para anónimos, encadenado a la cuota por seudónimo + IP — `src/KillaUru.Api/Seguridad/TechoPorIp.cs` · (FR-030, D2)
- [ ] T039 [P] [US3] `CabecerasMiddleware` y CORS — `src/KillaUru.Api/Seguridad/CabecerasMiddleware.cs` · (FR-013, FR-014)
- [ ] T040 [P] [US3] `ManejadorErrores` y `ProblemDetailsExtensions` — `src/KillaUru.Api/Errores/` · (FR-012)

**Checkpoint**: quickstart §4 completo.

---

## Phase 6: User Story 2 — Documentación viva de la API (P1)

- [ ] T041 [US2] `DocumentacionTests`: en Development `/openapi/v1.json` y `/docs` responden 200 y el documento contiene `/health/live` y `/health/ready`; en Production ambos 404 — `tests/KillaUru.Integracion.Tests/Documentacion/DocumentacionTests.cs` · (FR-015)
- [ ] T042 [US2] `ContratoOpenApiTests`: compara el documento generado con **todos** los `specs/*/contracts/*.openapi.yaml` (rutas, métodos, códigos —429 en toda operación de `/api/v1`—, propiedades requeridas) — `tests/KillaUru.Integracion.Tests/Contratos/ContratoOpenApiTests.cs` · (FR-016)
- [ ] T043 [US2] `DocumentacionExtensions` (OpenAPI 3.1 + Scalar en `/docs`, apagado en producción) — `src/KillaUru.Api/Documentacion/DocumentacionExtensions.cs` · (FR-015)
- [ ] T044 [US2] `TransformadorSeguridad` (agrega «Rol: …», esquemas `tokenAcceso`/`cookieAcceso` y respuestas 401/403/429 comunes) — `src/KillaUru.Api/Documentacion/TransformadorSeguridad.cs` · (FR-016, FR-045)

---

## Phase 7: User Story 4 — Salud y monitoreo (P2, recortable al final: HU-17)

- [ ] T045 [US4] `SaludTests`: `/health/live` 200 sin consultar la BD; `/health/ready` 503 con la BD detenida o con una migración pendiente; un chequeo extra registrado por un módulo de prueba se suma a `ready`; ninguna respuesta incluye datos internos — `tests/KillaUru.Integracion.Tests/Salud/SaludTests.cs` · (FR-018, FR-019)
- [ ] T046 [US4] `SaludExtensions` (etiqueta `ready`), `ChequeoBaseDatos` (caché de 5 s) y `ChequeoMigraciones` (versión esperada frente a `migraciones.aplicada`) — `src/KillaUru.Api/Salud/` · (FR-018, FR-019, T-OPS19)
- [ ] T067 [US4] `LogsSinDatosPersonalesTests`: capturar logs y trazas de un lote enviado con token y seudónimo, de un login y de un error; no aparecen seudónimo, IP, `Authorization`, cookies, tokens, correos ni `sub` junto a `loteId` — `tests/KillaUru.Integracion.Tests/Seguridad/LogsSinDatosPersonalesTests.cs` · (FR-034, T-SEG03)
- [ ] T047 [P] [US4] `LogsExtensions` (Serilog JSON, correlación, sin `/health`) y `FiltroDatosPersonales` — `src/KillaUru.Api/Observabilidad/` · (FR-020, FR-034)
- [ ] T048 [P] [US4] `TelemetriaExtensions` (OTLP, muestreo 10 %, instrumentación de ASP.NET Core con `http.route`, sin exponer `/metrics`) — `src/KillaUru.Api/Observabilidad/TelemetriaExtensions.cs` · (FR-020, FR-021)
- [ ] T049 [US4] `MetricasOpenTelemetry` implementa `IMetricas` (9 métodos, `retraso_cola_s` en segundos) sin etiquetas de alta cardinalidad + prueba unitaria con `MeterListener` — `src/KillaUru.Api/Observabilidad/MetricasOpenTelemetry.cs` · (FR-022, RNF-32)
- [ ] T068 [US4] Contenedor `alloy`: `config.alloy` con métricas del host (disco, CPU, memoria), logs de Docker y el archivo `.prom` del volumen `metricas-backup` → Grafana Cloud; imagen `grafana/alloy` fija por versión; sin puertos — `deploy/alloy/config.alloy` · (FR-040, D12)
- [ ] T050 [US4] Configurar en Grafana Cloud (sin tarjeta) los paneles y alertas: p95 por ruta (mapa 250 ms, lote 300 ms, resto 250 ms y sesión 500 ms como metas SDD), 5xx > 1 %, disco > 80 %, tarea > 26 h (incluido `backup`), uso del plan ≥ 80 %; en UptimeRobot el monitor de `/health/ready` y su página pública; documentarlo con el calendario de tareas de `ProgramaReloj` — `deploy/runbooks/monitoreo.md` · (HU-17, FR-022, FR-040, FR-042, FR-046)

---

## Phase 8: User Story 5 — Vuelta atrás y recuperación (P2)

- [ ] T051 [US5] Contenedor de respaldo: cron 02:00 local; `pg_dumpall --roles-only` + `pg_dump -Fc` → `pg_restore --list` → `age` (clave pública) → R2 privado con `rclone` (regla de ciclo de vida de 30 días); al terminar bien escribe `killauru_tarea_ultima_ok{tarea="backup"} <unix>` en el `.prom` — `deploy/backup/Dockerfile`, `deploy/backup/respaldar.sh` · (FR-027, D12)
- [ ] T052 [P] [US5] `docker-compose.pruebas.yml` (proyecto `killauru-pruebas`, otra BD en volumen aparte, otros puertos, sin túnel, `cpus: 2` y `mem_limit: 6g` para API + BD, `ASPNETCORE_ENVIRONMENT=Pruebas`) y `db/semillas/LEEME.md` — `deploy/` · (FR-006, R-11, D11)
- [ ] T053 [P] [US5] *Runbooks* `desplegar.md` (orden del primer despliegue, CI/CD), `volver-atras.md`, `restaurar.md` (FR-033: roles + datos, revocar sesiones, rotar `KU_JWT_CLAVE_FIRMA`, arrancar, 0 seudónimos > 72 h, revisar a mano las eliminaciones del último día, riesgo aceptado), `recrear-vm.md` (≤ 4 h), `secretos.md` (inventario con los secretos de D10 y T-BD01, copia cifrada fuera, rotación) — `deploy/runbooks/` · (FR-028, FR-033, RNF-24, RNF-34)
- [ ] T054 [US5] Simulacro en «killauru-pruebas»: vuelta atrás con `desplegar.sh <sha-anterior>` < 5 min y restauración con RTO ≤ 2 h, verificando 0 sesiones sin revocar y 0 seudónimos > 72 h; anotar tiempos en `deploy/runbooks/simulacros.md` · (FR-005, FR-033, SC-002, SC-006, SC-012)

---

## Phase 9: User Story 7 — CI/CD (P1)

- [ ] T069 [US7] `ci.yml` (cada PR): `dotnet build` (advertencias = errores) → unitarias → arquitectura → integración (Testcontainers) → contrato OpenAPI → cobertura con coverlet ≥ 90 % de los proyectos `Domain` (puerta) — `.github/workflows/ci.yml` · (FR-037, D10)
- [ ] T070 [US7] Proteger `main` en GitHub: *check* `ci` obligatorio, sin uniones directas; documentarlo — `deploy/runbooks/desplegar.md` · (FR-037)
- [ ] T071 [US7] `cd.yml` (merge a `main`, entorno `produccion`): imagen arm64 en `ubuntu-24.04-arm` → `ghcr.io/<cuenta>/killauru-api:<sha>` → SSH por Cloudflare Access → `desplegar.sh <sha>` — `.github/workflows/cd.yml` · (FR-038, D10)
- [ ] T072 [US7] Configurar Cloudflare Access para SSH a la VM (token de servicio, sin puertos abiertos) y los secretos `KU_CF_ACCESS_CLIENT_ID`, `KU_CF_ACCESS_CLIENT_SECRET`, `KU_DEPLOY_SSH_KEY`; agregarlos a `secretos.md` — `deploy/runbooks/secretos.md` · (FR-007, FR-038)

**Checkpoint**: un PR con una prueba rota no se puede unir; un PR verde se despliega solo.

---

## Phase 10: User Story 8 — Carga, estrés, resistencia y caos (P2; requiere 005 y 006)

- [ ] T073 [P] [US8] `comun.js`: seudónimo por VU, eventos sobre la polilínea de `db/datos/`, criterio de error (todo 5xx; 429 prohibido) — `tests/carga/comun.js` · (FR-039)
- [ ] T074 [P] [US8] `carga-combinada.k6.js`: 300 sincronizando + 300 mapa, 10 min; umbrales lote p95 < 300 ms, mapa p95 < 250 ms, errores < 1 % — `tests/carga/carga-combinada.k6.js` · (FR-039, RNF-03, EQ-14)
- [ ] T075 [P] [US8] `picos.k6.js` (ráfaga de 600) y `estres.k6.js` (escalera de 100 en 100 hasta p95 > 2× la meta o errores > 1 %; 503 con `Retry-After` aceptado solo aquí; registra el punto de quiebre) — `tests/carga/` · (FR-039, EQ-06)
- [ ] T076 [P] [US8] `resistencia.k6.js` (4 h al 50 %) — `tests/carga/resistencia.k6.js` · (FR-039)
- [ ] T077 [US8] `caos.md` + `caos.sh`: `kill -9` del contenedor de la API bajo carga, espera la reanudación y corre la conciliación (0 perdidos, 0 duplicados) — `tests/carga/` · (FR-039, RNF-06, EQ-09)
- [ ] T078 [US8] `carga.yml` (`workflow_dispatch`, elige el script) contra «killauru-pruebas» — `.github/workflows/carga.yml` · (FR-039)
- [ ] T079 [US8] Correr las 5 pruebas fuera de la VM, en horario de poco tráfico, y anotar resultados (incluido el retraso de cola de `retraso_cola_s`) en `deploy/runbooks/simulacros.md` · (FR-039, SC-010, SC-011)

---

## Phase 11: Polish

- [ ] T055 [P] Completar los `README.md` de los 6 módulos con la sección «Cómo se prueba» · (FR-017)
- [ ] T080 Prueba de punta a punta: lote → anomalía → mapa (invalidación, `ETag` nuevo) → indicador del día — `tests/KillaUru.Integracion.Tests/PuntaAPunta/FlujoCompletoTests.cs` · (FR-044, T-QA20; se completa cuando existan 004–007)
- [ ] T081 Calendario de verificaciones periódicas (restauración mensual, revisión de privilegios mensual, costo y uso de Grafana mensual, simulacro de incidente semestral, pruebas de carga antes de cada versión y mensual) — `deploy/runbooks/simulacros.md` · (FR-043, T-QA14)
- [ ] T056 Ejecutar `quickstart.md` completo y marcar `checklists/trazabilidad.md`
- [ ] T057 Revisar costo: alertas de presupuesto en Oracle y Cloudflare a US$ 8 — `deploy/runbooks/monitoreo.md` · (RNF-16)

## Dependencies & Execution Order

- Phase 1 → Phase 2 (US6) → Phase 3 (kernel) → Phase 4 (US1) → Phase 5 (US3) → Phase 6 (US2) → Phase 7 (US4) → Phase 8 (US5) → Phase 9 (US7) → Phase 10 (US8) → Phase 11.
- Las features 002–007 pueden empezar cuando terminen las fases 1–6. US7 conviene hacerla justo después de la Phase 6 para que todo lo demás pase por CI. US8 necesita 005 y 006; T080 necesita 004–007.
- Dentro de cada fase: pruebas → implementación; las tareas `[P]` de una misma fase pueden hacerse juntas.
