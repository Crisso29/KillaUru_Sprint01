# Implementation Plan: Plataforma base del backend

**Branch**: `001-plataforma-backend` | **Date**: 2026-10-20 (revisado 2026-10-21) | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-plataforma-backend/spec.md`

## Summary

Crear la solución .NET 10 con sus 8 proyectos de producción y sus proyectos de prueba; el host de la API (sesión, roles, límite de tasa con techo por IP, errores, cabeceras, CORS, logs sin datos personales); el kernel `Shared` (value objects, eventos de dominio, métricas, contexto de transacción con `AlConfirmar`, programa de tareas por reloj); la documentación viva de la API (OpenAPI + Scalar); la observabilidad (OpenTelemetry + Serilog → Grafana Cloud, y Grafana Alloy para la VM); `/health/live` y `/health/ready`; el despliegue con Docker Compose (API, PostGIS, `cloudflared`, respaldo, Alloy); CI/CD con GitHub Actions; el entorno de pruebas con límites; las pruebas de arquitectura (incluido el límite de 100 líneas por archivo) y las pruebas de sistema (carga, picos, estrés, resistencia y caos).

## Technical Context

**Language/Version**: C# 14 sobre .NET 10 LTS (R-02)

**Primary Dependencies**: ASP.NET Core 10 (controladores, `RateLimiting`, `Authentication.JwtBearer`, `HealthChecks`, `ProblemDetails`), `Microsoft.AspNetCore.OpenApi`, `Scalar.AspNetCore`, EF Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` + `NetTopologySuite`, `Microsoft.Extensions.Caching.Hybrid`, OpenTelemetry (.NET, exportador OTLP), Serilog (`Serilog.AspNetCore`, `Serilog.Formatting.Compact`, `Serilog.Sinks.OpenTelemetry`), `Microsoft.CodeAnalysis.BannedApiAnalyzers`

**Storage**: PostgreSQL 17 + PostGIS 3.5 en arm64; un esquema por módulo; scripts SQL versionados en `backend/db/migraciones/` (`V0000` como superusuario, el resto con `killauru_migrador`)

**Testing**: xUnit, NSubstitute, `FakeTimeProvider`, Testcontainers (PostgreSQL + PostGIS, mismos scripts y roles), `WebApplicationFactory`, NetArchTest.Rules, coverlet (cobertura ≥ 90 % de `Domain`); k6 para carga, picos, estrés y resistencia, y `caos.sh` (dueña: 001; 005 y 006 los referencian)

**CI/CD**: GitHub Actions (`ci.yml` en cada PR, `cd.yml` al unir a `main`, `carga.yml` manual), GHCR, SSH por Cloudflare Access (D10)

**Target Platform**: Linux arm64 en contenedor (Oracle Cloud ARM Always Free A1, 4 OCPU / 24 GB, Santiago), detrás de Cloudflare Tunnel

**Project Type**: web-service (monolito modular)

**Performance Goals**: el middleware común agrega ≤ 2 ms por petición; las metas de negocio (RNF-01, RNF-02, RNF-03, RNF-23) se miden con las pruebas de sistema de esta feature

**Constraints**: costo ≤ US$ 10/mes (RNF-16); telemetría dentro del plan gratuito (RNF-32); archivos de código (`.cs`, `.sql` de consultas, `.sh`, `.js`) ≤ 100 líneas; sin puertos abiertos (ADR-010); límites de CPU y memoria por contenedor

**Scale/Scope**: 300 dispositivos simultáneos (S-01), una sola instancia (S-03)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principio | Cómo lo cumple esta feature | Estado |
|---|---|---|
| I. Docs manda y trazabilidad | Cada FR cita HU/RF/RNF/ADR o la decisión D# del autor; cada tarea cita su FR | ✅ |
| II. Monolito modular + Clean | 6 proyectos de módulo; todo `internal` salvo `Contracts/`; NetArchTest con la tabla de capas completa (D4) | ✅ |
| III. Reglas calibrables | `TimeProvider` único; `BannedApiAnalyzers`; patrón `<Modulo>Options`; techos en `ApiOptions` | ✅ |
| IV. Datos confiables | El publicador y `AlConfirmar` corren **tras** el commit; prueba de caos con conciliación | ✅ |
| V. Privacidad | Logs, trazas y métricas sin seudónimo, IP, tokens, correos ni `sub` + `loteId` (FR-034) | ✅ |
| VI. Acceso en el servidor | Middleware de sesión sin tolerancia de reloj + políticas por rol; regla 401/403 única; `private, no-store` por defecto | ✅ |
| VII. Pruebas primero | Pruebas de value objects, de arquitectura y de límites antes del código; cobertura ≥ 90 % como puerta de CI | ✅ |
| VIII. Código pequeño | Prueba de 100 líneas (`.cs`, `.sql`, `.sh`, `.js`); analizadores; advertencias como errores | ✅ |
| IX. API documentada | OpenAPI + Scalar; prueba de contrato en CI; `ProblemDetails` con `codigo` | ✅ |
| X. Costo cero | GitHub Actions, GHCR, Cloudflare Access y Grafana Alloy en planes gratuitos | ✅ |

**Re-evaluación tras el diseño:** sin violaciones; los agregados a `Shared` y el contenedor `alloy` se justifican abajo en «Complexity Tracking».

## Project Structure

### Documentation (this feature)

```text
specs/001-plataforma-backend/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/
│   ├── plataforma.openapi.yaml      # /health/live, /health/ready y componentes comunes (ProblemDetails, seguridad)
│   └── convenciones-api.md          # reglas que siguen TODOS los contratos de 002–007 (incluida la regla 401/403)
└── checklists/
    ├── requirements.md
    └── trazabilidad.md
```

### Source Code — árbol exacto (cada archivo de código ≤ 100 líneas)

```text
.github/workflows/                           # en la raíz del repositorio (D10)
├── ci.yml                                   # cada PR: build, unitarias, arquitectura, integración, contrato, cobertura ≥ 90 % Domain
├── cd.yml                                   # merge a main: imagen arm64 → GHCR :<sha> → SSH por Cloudflare Access → desplegar.sh <sha>
└── carga.yml                                # manual (workflow_dispatch): corre un script de tests/carga contra «killauru-pruebas»

backend/
├── KillaUru.sln
├── global.json                              # fija el SDK de .NET 10
├── Directory.Build.props                    # net10.0, Nullable, TreatWarningsAsErrors, AnalysisLevel, GenerateDocumentationFile, InvariantGlobalization, BannedApiAnalyzers
├── Directory.Packages.props                 # versiones centralizadas
├── BannedSymbols.txt                        # DateTime.Now/UtcNow, DateTimeOffset.Now/UtcNow (T-QA16)
├── .editorconfig                            # estilo y severidad de analizadores
│
├── src/KillaUru.Api/                        # HOST: solo composición y asuntos transversales
│   ├── KillaUru.Api.csproj
│   ├── Program.cs                           # arma el pipeline llamando a las extensiones de abajo
│   ├── appsettings.json · appsettings.Development.json · appsettings.Pruebas.json   # Pruebas: límites de tasa altos (D11)
│   ├── Configuracion/ApiOptions.cs          # LimiteTasa (cuotas, techos por IP, registros), Telemetria, DocumentacionPublica, OrigenPanel, RedTunel, Jwt
│   ├── Modulos/ModulosExtensions.cs         # Add<Modulo>Module(...) de los 6 + AddApplicationPart
│   ├── Modulos/ControladoresInternosFeatureProvider.cs   # descubre controladores internal (R-003)
│   ├── Seguridad/AutenticacionExtensions.cs # JwtBearer (ClockSkew = 0); lee Bearer o cookie ku_acceso
│   ├── Seguridad/PoliticasExtensions.cs     # SoloAdministrador, SoloConductorRegistrado, Autenticado
│   ├── Seguridad/IpRealExtensions.cs        # ForwardedHeaders con CF-Connecting-IP solo desde cloudflared
│   ├── Seguridad/LimiteTasaExtensions.cs    # políticas "consultas", "lotes" y "registro"
│   ├── Seguridad/ClaveLimiteTasa.cs         # calcula la partición (rol + seudónimo/IP o cuenta)
│   ├── Seguridad/TechoPorIp.cs              # techo por IP de anónimos (600 lotes/h, 300 consultas/h) encadenado a la cuota (D2)
│   ├── Seguridad/CabecerasMiddleware.cs     # HSTS, nosniff, Cache-Control private,no-store por defecto
│   ├── Errores/ManejadorErrores.cs          # IExceptionHandler → ProblemDetails 500
│   ├── Errores/ProblemDetailsExtensions.cs  # agrega codigo y correlacionId a todo ProblemDetails
│   ├── Documentacion/DocumentacionExtensions.cs   # AddOpenApi + MapScalarApiReference; 404 en producción
│   ├── Documentacion/TransformadorSeguridad.cs    # rol permitido, esquemas de seguridad y 401/403/429 comunes
│   ├── Salud/SaludExtensions.cs             # /health/live y /health/ready
│   ├── Salud/ChequeoBaseDatos.cs            # SELECT 1 con caché de 5 s
│   ├── Salud/ChequeoMigraciones.cs          # versión esperada frente a migraciones.aplicada (T-OPS19)
│   ├── Observabilidad/TelemetriaExtensions.cs     # OpenTelemetry: métricas (ASP.NET Core con http.route), trazas (10 %), OTLP
│   ├── Observabilidad/LogsExtensions.cs     # Serilog JSON + filtro de /health
│   ├── Observabilidad/FiltroDatosPersonales.cs    # quita seudónimo, IP, tokens, cookies, correos y sub+loteId de logs y trazas (T-SEG03)
│   └── Observabilidad/MetricasOpenTelemetry.cs    # implementa IMetricas con System.Diagnostics.Metrics
│
├── src/KillaUru.Shared/                     # KERNEL: no depende de ningún módulo ni de ASP.NET/EF
│   ├── KillaUru.Shared.csproj
│   ├── Domain/Coordenada.cs
│   ├── Domain/Severidad.cs                  # Crear, MedianaHaciaArriba
│   ├── Domain/Confidence.cs
│   ├── Domain/SeudonimoInstalacion.cs
│   ├── Domain/Celda.cs                      # De, Alrededor, Limites
│   ├── Domain/Roles.cs                      # nombres de rol: Anonimo, ConductorRegistrado, Administrador
│   ├── Eventos/IEventoDominio.cs
│   ├── Eventos/IManejador.cs                # IManejador<TEvento>
│   ├── Eventos/IPublicadorEventos.cs
│   ├── Eventos/PublicadorEnMemoria.cs
│   ├── Metricas/IMetricas.cs                # 9 métodos (data-model §3.2)
│   ├── Persistencia/IContextoTransaccion.cs # Unir, AlConfirmar, ConfirmarAsync, RevertirAsync (D15)
│   ├── Persistencia/ContextoTransaccion.cs
│   └── Tareas/ProgramaReloj.cs              # próximo instante horario o diario con TimeProvider (T-OPS05)
│
├── src/Modules/KillaUru.<Modulo>/           # los 6: en esta feature solo el esqueleto
│   ├── KillaUru.<Modulo>.csproj             # InternalsVisibleTo solo hacia KillaUru.<Modulo>.Tests e Integracion.Tests
│   ├── <Modulo>Module.cs                    # Add<Modulo>Module(IServiceCollection, IConfiguration) vacío
│   ├── README.md                            # plantilla: propósito, endpoints, ejemplos
│   └── Contracts/ · Domain/ · Application/ · Adapters/ · Infrastructure/   (carpetas)
│
├── tests/
│   ├── KillaUru.Shared.Tests/               # value objects, publicador, ContextoTransaccion, ProgramaReloj
│   ├── KillaUru.Arquitectura.Tests/         # ReglasDeCapasTests, ReglasDeDominioTests, ReglasDeModulosTests, LimiteDeLineasTests
│   ├── KillaUru.Integracion.Tests/          # FabricaApi (WebApplicationFactory + Testcontainers con V0000 y roles), Migraciones/, Salud/, Seguridad/, Contratos/, PuntaAPunta/
│   ├── KillaUru.<Modulo>.Tests/             # proyectos vacíos, listos para 002–007
│   └── carga/                               # pruebas de sistema (D11), fuera del CI
│       ├── comun.js                         # seudónimo por VU, eventos sobre la polilínea, criterio de error
│       ├── carga-combinada.k6.js · picos.k6.js · estres.k6.js · resistencia.k6.js
│       └── caos.md · caos.sh                # kill -9 de la API bajo carga + conciliación
│
├── db/
│   ├── migraciones/V0000__arranque.sql      # superusuario, una sola vez (T-BD01)
│   ├── migraciones/V0001__privilegios.sql   # privilegios por defecto, con killauru_migrador
│   └── semillas/LEEME.md                    # datos sintéticos (se llenan en cada módulo)
│
└── deploy/
    ├── Dockerfile                           # imagen de la API (arm64), con curl para el healthcheck
    ├── docker-compose.yml                   # api, postgres, cloudflared, backup, alloy; restart, healthcheck, logs, límites
    ├── docker-compose.pruebas.yml           # proyecto «killauru-pruebas» (R-11): cpus 2, mem_limit 6g, volumen aparte
    ├── .env.example                         # solo NOMBRES de los secretos (RNF-34)
    ├── postgres/Dockerfile                  # solo si la imagen oficial no tiene arm64 (R-008)
    ├── migrar.sh                            # --arranque (superusuario, V0000) · normal: respaldo + V0001… con killauru_migrador
    ├── desplegar.sh                         # <sha>: respaldo → migraciones → up -d → espera /health/ready (D10)
    ├── backup/Dockerfile · backup/respaldar.sh    # pg_dumpall --roles-only + pg_dump → age → R2; escribe el .prom (D12)
    ├── alloy/config.alloy                   # host, logs de Docker y archivo .prom → Grafana Cloud (D12)
    └── runbooks/ desplegar.md · volver-atras.md · restaurar.md · recrear-vm.md · secretos.md · monitoreo.md · simulacros.md
```

**Structure Decision**: se respeta la estructura de `diseño-interno-de-modulos.md` §2 y las carpetas que ya existen en `backend/`. Se agregan `KillaUru.Shared.Tests`, `tests/carga/`, `db/semillas/`, `deploy/` (docs no fija dónde va el *compose*) y `.github/workflows/` (D10). `V0001__esquemas_y_roles.sql` pasa a `V0000__arranque.sql` + `V0001__privilegios.sql` (T-BD01).

## Dependencias entre proyectos

| Proyecto | Referencia a |
|---|---|
| `KillaUru.Shared` | solo la BCL |
| `KillaUru.<Modulo>` | `KillaUru.Shared` y los proyectos de los módulos que usa (solo verá su `Contracts/` porque el resto es `internal`) |
| `KillaUru.Api` | `KillaUru.Shared` y los 6 módulos |

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| `Shared/Persistencia/IContextoTransaccion` (Shared solo tenía kernel, eventos y métricas) | ADR-013: auditoría en la **misma** transacción sin cambiar la firma `RegistrarAsync(acción, actor, objetivo, contexto)`; `AlConfirmar` para la métrica de riesgo después del commit (D15) | Pasar la transacción como parámetro cambia el contrato de Nivel3; compartir el `DbContext` rompe la regla 3 |
| `Shared/Domain/Roles.cs` | Los controladores de cada módulo necesitan el nombre del rol para `[Authorize]` sin depender de Identidad (evita la flecha Mapa → Identidad, que no existe en el grafo) | Poner los roles en `Identidad/Contracts` crearía dependencias nuevas desde Mapa, Anomalías y Auditoría |
| `Shared/Tareas/ProgramaReloj.cs` | Cinco módulos (003, 004, 005, 007 y el backup) programan tareas por reloj; un solo cálculo evita cinco variantes (T-OPS05) | Copiar `ProgramaDiario` de 007 en cada módulo duplica lógica con `TimeProvider` |
| Contenedor `alloy` (ADR-010 no lo nombraba) | La API no puede medir disco, CPU ni memoria de la VM, ni la tarea de backup, que es otro contenedor (D12) | Prometheus + Grafana propios (descartado en ADR-010); un *gauge* en la API no ve el host |
