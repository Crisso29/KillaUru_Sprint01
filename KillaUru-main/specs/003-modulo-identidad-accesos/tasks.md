---

description: "Tareas del módulo Identidad y Accesos"
---

# Tasks: Módulo Identidad y Accesos

**Input**: `specs/003-modulo-identidad-accesos/` · **Requiere**: 001 (fases 1–6) y 002 (fases 1–3)

**Tests**: obligatorias; primero la prueba (debe fallar), luego el código.

**Formato**: `- [x] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `backend/`. Módulo: `src/Modules/KillaUru.Identidad/` (abreviado `Id/`). Pruebas unitarias en `tests/KillaUru.Identidad.Tests/` (`IdT/`); de integración en `tests/KillaUru.Integracion.Tests/Identidad/` (`IntId/`). **Ningún `.cs` > 199 líneas.**

**Cierre local (2026-10-24): 65/65 tareas implementadas o documentadas.** Los nombres de pruebas del plan se agruparon por responsabilidad; consultar la evidencia en [revision-implementacion.md](revision-implementacion.md). T041 acredita adaptador, plan gratuito y documentación DNS, no entrega real; T055 acredita 300 cuentas y 5 VU locales. El checklist operativo externo permanece abierto en la revisión.

## Phase 1: Setup

- [x] T001 Migración `V0003__identidad.sql` (5 tablas e índices de `data-model.md` §1, incluidos `ix_sesion_usuario_fk`, `ix_token_usuario_fk` e `ix_sesion_expira`) — `db/migraciones/V0003__identidad.sql` · (FR-001…FR-036, FR-040)
- [x] T002 [P] `IdentidadOptions` con los valores de la spec (`ContrasenaMinima = 10`, `RetencionIdentidadDias = 30`, `TruncarUsoTrasMinutos = 2`, `HoraPurgaIdentidad = 03:45`) y su sección en `appsettings.json` — `Id/Application/IdentidadOptions.cs`, `src/KillaUru.Api/appsettings.json` · (ADR-012, FR-005, FR-040)
- [x] T003 [P] Agregar `KU_IDENTIDAD_CLAVE_ROTACION` y `KU_TURNSTILE_SECRETO` a `deploy/.env.example` y al *runbook* de secretos · (RNF-34)

## Phase 2: Foundational (dominio y puertos que usan todas las historias)

### Pruebas

- [x] T004 [P] `CorreoTests` (recorte, minúsculas, formato, 254 caracteres) — `IdT/Domain/CorreoTests.cs` · (FR-006)
- [x] T005 [P] `UsuarioBloqueoTests`: 2 fallos no bloquean; 3.º en 15 min bloquea 5 min; fallo a los 15:01 reinicia la ventana; 2.º bloqueo 15 min; 3.º y siguientes 60 min; éxito reinicia nivel — `IdT/Domain/UsuarioBloqueoTests.cs` · (FR-009, RN-19)
- [x] T006 [P] `PoliticaSesionTests` y `HorasGuardadasTests`: conductor ahora+30 d en cada renovación; administrador nunca pasa de inicio+8 h; `expira_utc` truncado hacia abajo (10:59:59 → 10:00; 11:00:00 → 11:00); `creado_utc` de la cuenta al día local — `IdT/Domain/PoliticaSesionTests.cs`, `IdT/Domain/HorasGuardadasTests.cs` · (FR-015, FR-039)
- [x] T007 [P] `SesionRenovacionTests`: tabla de decisión de `data-model.md` §2.3, con reuso a 59, 60 y 61 s — `IdT/Domain/SesionRenovacionTests.cs` · (FR-016, FR-017)
- [x] T008 [P] `TokenCorreoTests`: vigencias 24 h / 30 min / 24 h en el límite exacto; un solo uso — `IdT/Domain/TokenCorreoTests.cs` · (FR-021, FR-022)

### Implementación

- [x] T009 [P] `Correo`, `RolUsuario`, `DocumentoLegal`, `PropositoToken`, `ParametrosBloqueo`, `HorasGuardadas` — `Id/Domain/`; resultados (`ResultadoLogin`, `ResultadoRenovacion`, `ResultadoEnlace`, `ResultadoGestion`, `UsuarioVista`), un archivo cada uno — `Id/Application/Resultados/` · (D4, FR-039)
- [x] T010 `Usuario` y `PoliticaBloqueo` (clase propia, SRP) — `Id/Domain/Usuario.cs`, `Id/Domain/PoliticaBloqueo.cs` · (FR-005, FR-009)
- [x] T011 [P] `SesionRenovacion` y `PoliticaSesion` — `Id/Domain/` · (FR-015, FR-016)
- [x] T012 [P] `TokenCorreo`, `Consentimiento`, `RevisionPrivilegios` — `Id/Domain/` · (FR-002, FR-021, FR-034)
- [x] T013 [P] Puertos (11 interfaces de `plan.md`, incluidos `ITransaccionIdentidad` e `IPurgaIdentidadRepositorio`) — `Id/Application/Puertos/`
- [x] T014 `IdentidadDbContext` + 5 configuraciones EF — `Id/Infrastructure/` · (ADR-012)
- [x] T015 [P] Repositorios + prueba de integración de cada uno con Testcontainers — `Id/Infrastructure/Repositorios/`, `IntId/PersistenciaTests.cs`, `IntId/UsuariosApiTests.cs`
- [x] T016 `TransaccionIdentidad` (abre transacción y `IContextoTransaccion.Unir`) + prueba «acción y auditoría juntas, o ninguna» y «`RegistrarAsync` sin transacción unida lanza `InvalidOperationException`» — `Id/Infrastructure/TransaccionIdentidad.cs`, `IntId/PersistenciaTests.cs` · (RNF-31, FR-038)
- [x] T017 [P] `HashContrasenas` (PasswordHasher, rehash) — `Id/Infrastructure/Seguridad/HashContrasenas.cs` · (FR-005)
- [x] T018 [P] `GeneradorSecretos` (aleatorio 32 B, SHA-256, derivación HMAC) + prueba: misma entrada → misma salida; distinta clave → distinta salida — `Id/Infrastructure/Seguridad/GeneradorSecretos.cs` · (R-003)
- [x] T019 [P] `EmisorJwt` (5 min, `sub`, `rol`, `verificado`, `jti`) + prueba de que el middleware de 001 lo acepta a las 4:59 y lo rechaza a las 5:01 (`ClockSkew = TimeSpan.Zero`) — `Id/Infrastructure/Seguridad/EmisorJwt.cs` · (FR-014, D8, T-SEG09)
- [x] T020 [P] `EnviadorCorreoConsola` y `PlantillasCorreo` (español, enlaces con `#token=`) — `Id/Infrastructure/Correo/` · (FR-024, FR-025)
- [x] T021 `EmisionSesion` (crea familia y par según el rol) — `Id/Application/Sesion/EmisionSesion.cs` · (FR-014, FR-015)
- [x] T022 `AddIdentidadModule` — `Id/IdentidadModule.cs`

**Checkpoint**: dominio con cobertura ≥ 90 %; repositorios y transacción con auditoría en verde.

---

## Phase 3: User Story 1 — Registrarse con consentimiento (P1) 🎯 MVP

- [x] T023 [P] [US1] `RegistrarConductorUseCaseTests` (aceptaciones vigentes, versión vieja, sin aceptar, correo en uso, correo de verificación enviado, sesión emitida con `verificado = false`, contraseña de 9/10 caracteres) — `IdT/Application/RegistrarConductorTests.cs`, `IntId/CuentaApiTests.cs` · (FR-001, FR-002, FR-021)
- [x] T024 [P] [US1] `ObtenerVersionesLegalesUseCase` + prueba — `Id/Application/Legal/` · (FR-003)
- [x] T025 [US1] `RegistrarConductorUseCase` — `Id/Application/Cuenta/RegistrarConductorUseCase.cs` · (FR-001, FR-002)
- [x] T026 [US1] `LegalController` y `CuentaController.Registrar` con DTOs documentados y la política de límite de tasa del registro del host (`LimiteTasa:RegistrosPorHoraPorIp`) — `Id/Adapters/` · (Constitución IX, FR-037)
- [x] T027 [US1] `CuentaApiTests.Registro*` (201, 400 `aceptacion_requerida`, 409 `correo_en_uso`; valores límite del tope por IP: 5.º registro aceptado, 6.º → 429 con `Retry-After`) — `IntId/CuentaApiTests.cs` · (FR-037, SC-008, T-QA18)

---

## Phase 4: User Story 2 — Iniciar sesión con bloqueo (P1)

- [x] T028 [P] [US2] `IniciarSesionUseCaseTests`: misma respuesta para correo inexistente, contraseña mala y cuenta desactivada; bloqueo progresivo; auditoría de fallos, bloqueos e inicio de administrador; conductor en panel → `solo_administradores`; invitación pendiente no puede entrar — `IdT/Domain/UsuarioBloqueoTests.cs`, `IntId/SesionApiTests.cs`, `IntId/RespuestasComunesApiTests.cs` · (FR-007…FR-013)
- [x] T029 [P] [US2] `VerificadorTurnstile` + doble de prueba — `Id/Infrastructure/Seguridad/VerificadorTurnstile.cs` · (FR-012)
- [x] T030 [US2] `IniciarSesionUseCase` — `Id/Application/Sesion/IniciarSesionUseCase.cs` · (FR-007…FR-013)
- [x] T031 [US2] `CookiesSesion` y `SesionController.IniciarSesion` (app: cuerpo; panel: cookies; 423 solo en panel) — `Id/Adapters/` · (FR-011, FR-020)
- [x] T032 [US2] `SesionApiTests.InicioSesion*` (incluye atributos de las cookies) — `IntId/SesionApiTests.cs`

---

## Phase 5: User Story 3 — Sesiones renovables y revocables (P1)

- [x] T033 [P] [US3] `RenovarSesionUseCaseTests` (todas las filas de la tabla de decisión) y `CerrarSesionUseCaseTests` — `IdT/Domain/SesionRenovacionTests.cs`, `IdT/Domain/PoliticaSesionTests.cs`, `IntId/SesionApiTests.cs` · (FR-015…FR-018)
- [x] T034 [US3] `RenovarSesionUseCase` y `CerrarSesionUseCase` — `Id/Application/Sesion/` · (FR-015…FR-018)
- [x] T035 [US3] `SesionController.Renovar` y `CerrarSesion` — `Id/Adapters/SesionController.cs`
- [x] T036 [US3] `SesionApiTests.Renovacion*`: gracia de 60 s devuelve el mismo token; a los 61 s revoca; desactivada → `cuenta_desactivada`; prueba de extremo a extremo de RNF-26 (desactivar → JWT inútil en ≤ 5 min) — `IntId/SesionApiTests.cs` · (SC-001)

---

## Phase 6: User Story 4 — Verificación y recuperación (P1)

- [x] T037 [P] [US4] Pruebas de `VerificarCorreoUseCase`, `ReenviarVerificacionUseCase`, `SolicitarRecuperacionUseCase` (respuesta idéntica, solo verificados, límites por correo e IP) y `FijarContrasenaConEnlaceUseCase` (recuperación e invitación; revoca sesiones; desbloquea; audita) — `IntId/CuentaApiTests.cs`, `IdT/Infrastructure/LimitadorRecuperacionTests.cs` · (FR-021…FR-023)
- [x] T038 [P] [US4] `LimitadorRecuperacionEnMemoria` — `Id/Infrastructure/Seguridad/LimitadorRecuperacionEnMemoria.cs` · (FR-022)
- [x] T039 [US4] Los 4 casos de uso — `Id/Application/Cuenta/` · (FR-021…FR-023)
- [x] T040 [US4] `CuentaController` (verificar, recuperar, fijar contraseña) y `MeController.ReenviarVerificacion` — `Id/Adapters/`
- [x] T041 [US4] `EnviadorCorreoHttp` para el proveedor elegido (verificar plan gratuito, RS-04) + reintentos; SPF/DKIM/DMARC documentados — `Id/Infrastructure/Correo/EnviadorCorreoHttp.cs`, `deploy/runbooks/correo.md` · (FR-025)
- [x] T042 [US4] `CuentaApiTests.Recuperacion*` (202 idéntico con y sin cuenta) — `IntId/CuentaApiTests.cs` · (SC-002)

---

## Phase 7: User Story 5 — Derechos del titular (P1)

- [x] T043 [P] [US5] Pruebas de `ObtenerMisDatosUseCase`, `AceptarVersionUseCase` y `EliminarMiCuentaUseCase` (contraseña incorrecta → `confirmacion_invalida`; borra todo; audita sin correo; escribe log `CuentaEliminada {UsuarioId}`) — `IntId/CuentaApiTests.cs`, `IntId/ConsultasYPrivacidadTests.cs` · (FR-026…FR-028)
- [x] T044 [US5] Los 3 casos de uso — `Id/Application/Titular/` · (FR-026…FR-028)
- [x] T045 [US5] `MeController` (GET, DELETE, aceptaciones) — `Id/Adapters/MeController.cs`
- [x] T046 [US5] `MeApiTests` + consulta de verificación: tras eliminar, el correo no aparece en ninguna tabla — `IntId/CuentaApiTests.cs`, `IntId/ConsultasYPrivacidadTests.cs` · (SC-004)

---

## Phase 8: User Story 6 — Usuarios y revisión mensual (P1)

- [x] T047 [P] [US6] Pruebas de `ListarUsuariosUseCase` (filtros, `ids`, audita `AccesoAdministrativoDatos` con `RegistrarLecturaAsync`, también con `ids`), `CrearUsuarioUseCase`, `CambiarRolUseCase`, `DesactivarUsuarioUseCase`, `ActivarUsuarioUseCase`, `ReenviarCorreoUsuarioUseCase`, `ResolverCorreosUseCase`, `ConsultarRevisionUseCase` (audita la lectura) y `RegistrarRevisionUseCase` (incluye «sobre sí mismo» y «último administrador») — `IdT/Application/` (ConsultasAdministrativas, GestionUsuarios, Invitaciones, ProteccionAdministradores, RevisionPrivilegios) · (FR-030…FR-035)
- [x] T048 [US6] Los 9 casos de uso — `Id/Application/Usuarios/`, `Id/Application/Privilegios/` · (FR-030…FR-035)
- [x] T049 [US6] `UsuariosController` (GET, POST), `EstadoUsuarioController` (rol, desactivar, activar, reenviar; restricción `{id:guid}`) y `RevisionPrivilegiosController`, sin tipos de `Domain/` — `Id/Adapters/` · (D4, FR-030…FR-035)
- [x] T050 [US6] `UsuariosApiTests` y `RevisionApiTests` (401 sin token, 403 a conductores, 409, auditorías de escrituras y lecturas) — `IntId/` · (FR-038, FR-042)
- [x] T051 [P] [US6] Comando `crear-admin <correo>`: `CrearAdministradorInicialUseCase` (solo si no hay administradores; audita `UsuarioCreado` con actor sistema) + `CrearAdminComando`, que imprime el enlace solo en `Console.Out` (nunca `ILogger`); prueba de que el enlace no aparece en los *sinks* de Serilog; paso en el *runbook* — `Id/Application/Usuarios/CrearAdministradorInicialUseCase.cs`, `Id/Adapters/Consola/CrearAdminComando.cs`, `deploy/runbooks/desplegar.md` · (FR-043, T-SEG14)

---

## Phase 9: User Story 7 — Consultas para Dashboard (P3)

- [x] T052 [US7] `IIdentidadConsultas` + `IdentidadConsultas` + prueba de integración (cuenta cuentas, no sesiones; solo conductores; ventana de N días) — `Id/Contracts/`, `Id/Application/IdentidadConsultas.cs`, `IntId/ConsultasYPrivacidadTests.cs` · (FR-036, RN-17)

---

## Phase 10: User Story 8 — Retención, restauración y operación (P2)

- [x] T058 [P] [US8] `PurgaIdentidadTests` (Testcontainers): sesión vencida o revocada hace 30 d → se conserva, 31 d → se borra; token de correo vencido o usado hace 31 d → se borra; `usado_utc` de hace 1:59 min → exacto, 2:01 min → truncado; borrar una cuenta usa los índices `_fk` — `IntId/PurgaIdentidadTests.cs` · (FR-040, SC-007)
- [x] T059 [US8] `IPurgaIdentidadRepositorio`, `PurgaIdentidadRepositorio` (SQL de `data-model.md` §1.1, en lotes) y `PurgarIdentidadUseCase` (registra `RegistrarTareaCorrecta("purga-identidad")`) — `Id/Application/Puertos/`, `Id/Infrastructure/Repositorios/`, `Id/Application/Tareas/` · (FR-040)
- [x] T060 [US8] `PurgaIdentidadTarea` (`BackgroundService` con `ProgramaReloj`: al arrancar y 03:45 hora local) + prueba con `FakeTimeProvider` del próximo instante — `Id/Infrastructure/Tareas/PurgaIdentidadTarea.cs`, `IdT/Application/PurgaIdentidadTests.cs`, `IntId/PurgaIdentidadTests.cs` · (FR-040, T-OPS05)
- [x] T061 [P] [US8] `HorasGuardadasTests` de integración: tras registrar, renovar, cerrar sesión y desactivar, `creado_utc`, `expira_utc` y `revocado_utc` están en la hora en punto y `usuario.creado_utc` en el día — `IntId/HorasGuardadasTests.cs` · (FR-039, SC-007)
- [x] T062 [US8] Sección «Identidad» del *runbook* `restaurar.md` (de 001): revocar todas las sesiones (`data-model.md` §1.2), rotar `KU_JWT_CLAVE_FIRMA`, arrancar (corre `purga-identidad`), revisar a mano las eliminaciones del último día; prueba de que tras el `UPDATE` toda renovación da 401 — `deploy/runbooks/restaurar.md`, `IntId/DesactivacionRestauracionTests.cs` · (FR-041, D9)
- [x] T063 [P] [US8] Prueba de que ningún caso de uso de Identidad llama `IMetricas.RegistrarAccionRiesgo` — `tests/KillaUru.Arquitectura.Tests/ReglasIdentidadTests.cs` · (FR-038)
- [x] T064 [P] [US8] Fila «Revisión mensual de privilegios» (responsable, día 1 de cada mes) en el calendario de verificaciones periódicas — `deploy/runbooks/simulacros.md` · (FR-034, T-QA14)
- [x] T065 [P] [US8] `RespuestasComunesApiTests`: cada operación del contrato declara 429; sin token → 401; conductor en operación de administrador → 403 — `IntId/RespuestasComunesApiTests.cs` · (FR-042, T-SEG19, T-SEG20)

---

## Phase 11: Polish

- [x] T053 [P] `README.md` del módulo (endpoints, ejemplos, cookies, códigos de error) — `Id/README.md` · (RNF-19)
- [x] T054 Prueba de contrato con `contracts/identidad.openapi.yaml` en verde
- [x] T055 Prueba de rendimiento: login p95 < 500 ms y renovación p95 < 50 ms con 300 usuarios (k6) · (SC-005)
- [x] T056 Ejecutar la consulta de auditoría del esquema de `quickstart.md` §4 · (SC-003)
- [x] T057 Marcar `checklists/trazabilidad.md` (los pendientes de `research.md` ya fueron confirmados por el autor el 2026-10-21)

## Dependencies & Execution Order

Phase 1 → Phase 2 → US1 → US2 → US3 → US4 → US5 → US6 → US7 → US8 → Polish. US4, US5 y US6 pueden avanzar en paralelo una vez terminada US3; US8 necesita US3 (sesiones) y US6 (T051).
