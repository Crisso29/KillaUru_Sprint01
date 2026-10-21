---

description: "Tareas de la plataforma base del backend"
---

# Tasks: Plataforma base del backend

**Input**: `specs/001-plataforma-backend/` (spec, plan, research, data-model, contracts)

**Tests**: obligatorias (constitución VII). En cada historia, las pruebas se escriben primero y deben **fallar** antes de implementar.

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. `[P]` = puede hacerse en paralelo (otro archivo, sin dependencia pendiente). Todas las rutas son relativas a `backend/`. **Ningún `.cs` puede pasar de 100 líneas.**

## Phase 1: Setup (solución y proyectos)

- [ ] T001 Crear `global.json` (SDK .NET 10), `Directory.Build.props` (net10.0, Nullable, TreatWarningsAsErrors, AnalysisLevel=latest-recommended, GenerateDocumentationFile, InvariantGlobalization) y `.editorconfig` — raíz de `backend/` · (Constitución VIII)
- [ ] T002 Crear `Directory.Packages.props` con versiones centralizadas de los paquetes de `plan.md` — `Directory.Packages.props`
- [ ] T003 Crear `KillaUru.sln` y los proyectos `src/KillaUru.Api`, `src/KillaUru.Shared` y los 6 `src/Modules/KillaUru.<Modulo>` con sus carpetas `Contracts/ Domain/ Application/ Adapters/ Infrastructure/` · (ADR-001, ADR-002)
- [ ] T004 Agregar `InternalsVisibleTo` de cada módulo solo hacia `KillaUru.<Modulo>.Tests` y `KillaUru.Integracion.Tests` — `src/Modules/*/KillaUru.<Modulo>.csproj` · (Constitución II)
- [ ] T005 [P] Crear los proyectos de prueba `tests/KillaUru.Shared.Tests`, `tests/KillaUru.Arquitectura.Tests`, `tests/KillaUru.Integracion.Tests` y los 6 `tests/KillaUru.<Modulo>.Tests` (las carpetas ya existen)
- [ ] T006 [P] Crear `<Modulo>Module.cs` vacío y `README.md` (plantilla: propósito, endpoints, ejemplos) en cada módulo · (FR-017, RNF-19)

**Checkpoint**: `dotnet build` sin advertencias.

---

## Phase 2: User Story 6 — Reglas de arquitectura y de tamaño (P1) 🛡️ primero, para que todo lo demás nazca vigilado

### Pruebas

- [ ] T007 [P] [US6] `LimiteDeLineasTests`: recorre `src/**/*.cs` y falla si algún archivo > 100 líneas, mostrando ruta y conteo — `tests/KillaUru.Arquitectura.Tests/LimiteDeLineasTests.cs` · (FR-029)
- [ ] T008 [P] [US6] `ReglasDeCapasTests`: `Domain` no referencia `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore` ni `Application/Adapters/Infrastructure`; `Adapters` no referencia `Domain` ni `Infrastructure`; ningún tipo usa `DateTime.UtcNow`/`DateTime.Now` — `tests/KillaUru.Arquitectura.Tests/ReglasDeCapasTests.cs` · (FR-029, regla 1 y 5)
- [ ] T009 [P] [US6] `ReglasDeModulosTests`: un módulo solo usa tipos `Contracts` de otro; el grafo permitido es exactamente el de `componentes-arquitectonicos.md` regla 4; ningún módulo referencia el `DbContext` de otro; `Shared` no referencia módulos — `tests/KillaUru.Arquitectura.Tests/ReglasDeModulosTests.cs` · (FR-029, RNF-28)

**Checkpoint**: las 3 pruebas pasan con el esqueleto vacío y fallan si se introduce una violación a propósito (comprobarlo y revertir).

---

## Phase 3: Kernel compartido (bloquea a todos los módulos)

### Pruebas

- [ ] T010 [P] `SeveridadTests`: `Crear(0|6) = null`, `Crear(1..5)` válido, `MedianaHaciaArriba({2,3,3,5}) = 3`, `({3,4}) = 4`, `({5}) = 5` — `tests/KillaUru.Shared.Tests/Domain/SeveridadTests.cs` · (RN-02, RN-05)
- [ ] T011 [P] `CeldaTests`: misma coordenada → misma celda; punto a 10 m de un borde → 2 celdas en `Alrededor(c,100)`; cerca de una esquina → 4; siempre en orden ascendente; `Limites()` contiene la coordenada que generó la celda — `tests/KillaUru.Shared.Tests/Domain/CeldaTests.cs` · (FR-023)
- [ ] T012 [P] `CoordenadaTests`, `ConfidenceTests`, `SeudonimoInstalacionTests` con valores límite — `tests/KillaUru.Shared.Tests/Domain/` · (FR-023)
- [ ] T013 [P] `PublicadorEnMemoriaTests`: entrega a todos los manejadores del tipo, en orden; una falla en uno no impide a los demás — `tests/KillaUru.Shared.Tests/Eventos/PublicadorEnMemoriaTests.cs` · (FR-024)

### Implementación

- [ ] T014 [P] `Coordenada`, `Confidence`, `SeudonimoInstalacion` — `src/KillaUru.Shared/Domain/` · (FR-023)
- [ ] T015 [P] `Severidad` (con `MedianaHaciaArriba`) — `src/KillaUru.Shared/Domain/Severidad.cs` · (FR-023)
- [ ] T016 [P] `Celda` (paso 0,005°, `De`, `Alrededor`, `Limites`) — `src/KillaUru.Shared/Domain/Celda.cs` · (FR-023)
- [ ] T017 [P] `Roles` (3 constantes) — `src/KillaUru.Shared/Domain/Roles.cs` · (plan, Complexity Tracking)
- [ ] T018 [P] `IEventoDominio`, `IManejador<T>`, `IPublicadorEventos`, `PublicadorEnMemoria` — `src/KillaUru.Shared/Eventos/` · (FR-024)
- [ ] T019 [P] `IMetricas` con los 8 métodos de `data-model.md` §3.2 — `src/KillaUru.Shared/Metricas/IMetricas.cs` · (FR-022)
- [ ] T020 [P] `IContextoTransaccion` y `ContextoTransaccion` — `src/KillaUru.Shared/Persistencia/` · (FR-025, ADR-013)

**Checkpoint**: `KillaUru.Shared.Tests` en verde; cobertura de `Shared/Domain` ≥ 90 %.

---

## Phase 4: User Story 1 — Levantar con un comando (P1) 🎯 MVP

### Pruebas

- [ ] T021 [US1] `FabricaApi`: `WebApplicationFactory` + contenedor PostGIS de Testcontainers que aplica `db/migraciones/*.sql` antes de arrancar — `tests/KillaUru.Integracion.Tests/Infraestructura/FabricaApi.cs`
- [ ] T022 [US1] `MigracionesTests`: tras aplicar V0001 existen los 5 esquemas, los 2 roles, `postgis`, `citext` y la fila `V0001`; `killauru_app` no puede `CREATE TABLE` — `tests/KillaUru.Integracion.Tests/Migraciones/MigracionesTests.cs` · (FR-002, FR-004)

### Implementación

- [ ] T023 [US1] Script `V0001__esquemas_y_roles.sql` (de `data-model.md` §1) — `db/migraciones/V0001__esquemas_y_roles.sql` · (FR-002, FR-004)
- [ ] T024 [US1] `ApiOptions` (sección `Api`) — `src/KillaUru.Api/Configuracion/ApiOptions.cs` · (data-model §4)
- [ ] T025 [US1] `ModulosExtensions` (llama a los 6 `Add<Modulo>Module` y `AddApplicationPart`) y `ControladoresInternosFeatureProvider` — `src/KillaUru.Api/Modulos/` · (R-003)
- [ ] T026 [US1] `Program.cs` mínimo: `TimeProvider.System`, `IContextoTransaccion` *scoped*, `IPublicadorEventos` *singleton*, módulos y controladores — `src/KillaUru.Api/Program.cs` · (FR-026)
- [ ] T027 [P] [US1] `Dockerfile` de la API (multi-etapa, `linux/arm64`) — `deploy/Dockerfile` · (FR-001)
- [ ] T028 [US1] Verificar la imagen PostGIS arm64 (R-008); si no existe, `deploy/postgres/Dockerfile` — (RS-02)
- [ ] T029 [US1] `docker-compose.yml`: `api` (sin puertos publicados), `postgres` (volumen persistente), `cloudflared` (token del túnel), `backup`; red interna; *healthcheck* de `api` con `/health/live` — `deploy/docker-compose.yml` · (FR-001, ADR-010)
- [ ] T030 [P] [US1] `.env.example` con los nombres de `data-model.md` §5 — `deploy/.env.example` · (FR-007)
- [ ] T031 [US1] `migrar.sh`: `pg_dump` previo + aplica en orden los `V*.sql` pendientes con `killauru_migrador` + registra en `migracion_aplicada` — `deploy/migrar.sh` · (FR-002, FR-003)

**Checkpoint**: quickstart §2 completo.

---

## Phase 5: User Story 3 — Seguridad común (P1)

### Pruebas

- [ ] T032 [US3] `SeguridadTests` (con un controlador de prueba solo en el proyecto de pruebas): sin token → `Anonimo`; token vencido/mal firmado/rol desconocido → 401 `token_invalido`; token por cookie `ku_acceso` → aceptado — `tests/KillaUru.Integracion.Tests/Seguridad/SeguridadTests.cs` · (FR-008, FR-009)
- [ ] T033 [US3] `LimiteTasaTests`: 61.ª consulta anónima → 429 con `Retry-After`; cuota de lotes independiente; registrado y administrador con su clave de cuenta; `CF-Connecting-IP` ignorada fuera de `RedTunel` — `tests/KillaUru.Integracion.Tests/Seguridad/LimiteTasaTests.cs` · (FR-010, FR-011)
- [ ] T034 [US3] `ErroresYCabecerasTests`: excepción → 500 `ProblemDetails` con `codigo = error_interno` y `correlacionId`, sin traza; toda respuesta con `Cache-Control: private, no-store`, HSTS y `nosniff`; CORS solo para `OrigenPanel` — `tests/KillaUru.Integracion.Tests/Seguridad/ErroresYCabecerasTests.cs` · (FR-012…FR-014)

### Implementación

- [ ] T035 [US3] `AutenticacionExtensions` (JwtBearer, lectura de cookie, tolerancia ≤ 30 s, rol válido) — `src/KillaUru.Api/Seguridad/AutenticacionExtensions.cs` · (FR-008, FR-009)
- [ ] T036 [P] [US3] `PoliticasExtensions` — `src/KillaUru.Api/Seguridad/PoliticasExtensions.cs` · (FR-008)
- [ ] T037 [P] [US3] `IpRealExtensions` — `src/KillaUru.Api/Seguridad/IpRealExtensions.cs` · (FR-011)
- [ ] T038 [US3] `ClaveLimiteTasa` y `LimiteTasaExtensions` (políticas `consultas` y `lotes`) — `src/KillaUru.Api/Seguridad/` · (FR-010)
- [ ] T039 [P] [US3] `CabecerasMiddleware` y CORS — `src/KillaUru.Api/Seguridad/CabecerasMiddleware.cs` · (FR-013, FR-014)
- [ ] T040 [P] [US3] `ManejadorErrores` y `ProblemDetailsExtensions` — `src/KillaUru.Api/Errores/` · (FR-012)

**Checkpoint**: quickstart §4 completo.

---

## Phase 6: User Story 2 — Documentación viva de la API (P1)

- [ ] T041 [US2] `DocumentacionTests`: en Development `/openapi/v1.json` y `/docs` responden 200 y el documento contiene `/health/live` y `/health/ready`; en Production ambos 404 — `tests/KillaUru.Integracion.Tests/Documentacion/DocumentacionTests.cs` · (FR-015)
- [ ] T042 [US2] `ContratoOpenApiTests`: compara el documento generado con **todos** los `specs/*/contracts/*.openapi.yaml` (rutas, métodos, códigos, propiedades requeridas) — `tests/KillaUru.Integracion.Tests/Contratos/ContratoOpenApiTests.cs` · (FR-016)
- [ ] T043 [US2] `DocumentacionExtensions` (OpenAPI 3.1 + Scalar en `/docs`, apagado en producción) — `src/KillaUru.Api/Documentacion/DocumentacionExtensions.cs` · (FR-015)
- [ ] T044 [US2] `TransformadorSeguridad` (agrega «Rol: …», esquemas `tokenAcceso`/`cookieAcceso` y respuestas 401/403/429 comunes) — `src/KillaUru.Api/Documentacion/TransformadorSeguridad.cs` · (FR-016)

---

## Phase 7: User Story 4 — Salud y monitoreo (P2)

- [ ] T045 [US4] `SaludTests`: `/health/live` 200 sin consultar la BD; `/health/ready` 503 con la BD detenida; un chequeo extra registrado por un módulo de prueba se suma a `ready`; ninguna respuesta incluye datos internos — `tests/KillaUru.Integracion.Tests/Salud/SaludTests.cs` · (FR-018, FR-019)
- [ ] T046 [US4] `SaludExtensions` (etiqueta `ready`) y `ChequeoBaseDatos` (caché de 5 s) — `src/KillaUru.Api/Salud/` · (FR-018, FR-019)
- [ ] T047 [P] [US4] `LogsExtensions` (Serilog JSON, correlación, sin `/health`) — `src/KillaUru.Api/Observabilidad/LogsExtensions.cs` · (FR-020)
- [ ] T048 [P] [US4] `TelemetriaExtensions` (OTLP, muestreo 10 %, sin exponer `/metrics`) — `src/KillaUru.Api/Observabilidad/TelemetriaExtensions.cs` · (FR-020, FR-021)
- [ ] T049 [US4] `MetricasOpenTelemetry` implementa `IMetricas` sin etiquetas de alta cardinalidad + prueba unitaria con `MeterListener` — `src/KillaUru.Api/Observabilidad/MetricasOpenTelemetry.cs` · (FR-022, RNF-32)
- [ ] T050 [US4] Configurar en Grafana Cloud (sin tarjeta) los paneles y alertas de RNF-20/RNF-22 y en UptimeRobot el monitor de `/health/ready` y su página pública; documentarlo — `deploy/runbooks/monitoreo.md` · (HU-17)

---

## Phase 8: User Story 5 — Vuelta atrás y recuperación (P2)

- [ ] T051 [US5] Contenedor de respaldo: `pg_dump -Fc` → `age` (clave pública) → R2 privado con `rclone`; publica su última ejecución correcta — `deploy/backup/Dockerfile`, `deploy/backup/respaldar.sh` · (FR-027)
- [ ] T052 [P] [US5] `docker-compose.pruebas.yml` (proyecto `killauru-pruebas`, otra BD, otros puertos, sin túnel) y `db/semillas/LEEME.md` — `deploy/` · (FR-006, R-11)
- [ ] T053 [P] [US5] *Runbooks* `desplegar.md`, `volver-atras.md`, `restaurar.md` (incluye repetir purgas y eliminaciones, RN-10), `recrear-vm.md` (≤ 4 h), `secretos.md` (inventario, copia cifrada fuera, rotación) — `deploy/runbooks/` · (FR-028, RNF-24, RNF-34)
- [ ] T054 [US5] Simulacro en «killauru-pruebas»: vuelta atrás < 5 min y restauración con RTO ≤ 2 h; anotar tiempos en `deploy/runbooks/simulacros.md` · (SC-002, SC-006)

---

## Phase 9: Polish

- [ ] T055 [P] Completar los `README.md` de los 6 módulos con la sección «Cómo se prueba» · (FR-017)
- [ ] T056 Ejecutar `quickstart.md` completo y marcar `checklists/trazabilidad.md`
- [ ] T057 Revisar costo: alertas de presupuesto en Oracle y Cloudflare a US$ 8 — `deploy/runbooks/monitoreo.md` · (RNF-16)

## Dependencies & Execution Order

- Phase 1 → Phase 2 (US6) → Phase 3 (kernel) → Phase 4 (US1) → Phase 5 (US3) → Phase 6 (US2) → Phase 7 (US4) → Phase 8 (US5) → Phase 9.
- Las features 002–007 pueden empezar cuando terminen las fases 1–6 (US4 y US5 pueden completarse en paralelo con ellas).
- Dentro de cada fase: pruebas → implementación; las tareas `[P]` de una misma fase pueden hacerse juntas.
