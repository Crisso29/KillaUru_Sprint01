# Implementation Plan: Módulo Identidad y Accesos

**Branch**: `003-modulo-identidad-accesos` | **Date**: 2026-10-20 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-modulo-identidad-accesos/spec.md`

## Summary

Módulo con dominio propio (usuario, bloqueo RN-19, sesión rotativa con gracia, tokens de correo, consentimientos, revisión de privilegios), 23 casos de uso pequeños, 7 controladores HTTP, un comando de consola (`crear-admin`), una tarea diaria por reloj (`purga-identidad`), emisión de JWT de 5 min con el *claim* `verificado`, tokens de actualización rotativos derivados por HMAC (para devolver «el mismo par» en la gracia de 60 s sin guardar el token en claro), correo transaccional detrás de `IEnviadorCorreo`, auditoría en la misma transacción para cada acción sensible (`RegistrarAsync`) y de las lecturas de datos personales (`RegistrarLecturaAsync`), y horas de sesión truncadas para que no unan una cuenta con eventos (D3, D8, D15).

## Technical Context

**Language/Version**: C# 14 / .NET 10

**Primary Dependencies**: `Microsoft.Extensions.Identity.Core` (solo `PasswordHasher<T>`, PBKDF2), `System.IdentityModel.Tokens.Jwt` / `Microsoft.IdentityModel.JsonWebTokens`, EF Core 10 + Npgsql, `System.Security.Cryptography` (RNG, SHA-256, HMAC-SHA256), cliente HTTP del proveedor de correo, Cloudflare Turnstile (verificación por HTTP)

**Storage**: esquema `identidad` (5 tablas, `data-model.md`)

**Testing**: xUnit (dominio: bloqueo, rotación, vigencias), NSubstitute + `FakeTimeProvider` (casos de uso), Testcontainers (repositorios, transacción con auditoría), `WebApplicationFactory` (contratos HTTP, cookies, códigos)

**Performance Goals**: login < 500 ms p95 (SC-005); renovación < 50 ms p95

**Constraints**: RNF-26 (≤ 5 min), RN-19 (no revelar cuentas de conductores), RNF-10 (sin vínculo con eventos, ni inferible por la hora: D3), Adapters solo usa Application (D4), archivos ≤ 100 líneas; las transacciones se confirman con `IContextoTransaccion.ConfirmarAsync` (no con `CommitAsync` directo) para que corran los `AlConfirmar` (001, D15)

**Scale/Scope**: cientos a pocos miles de cuentas; ~300 conductores activos (S-01)

## Constitution Check

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Cada FR cita su fuente; lo que viene del prototipo o es decisión propia está marcado y en `research.md` | ✅ |
| II. Monolito modular | Solo usa `Auditoria/Contracts`; esquema y `DbContext` propios; expone `IIdentidadConsultas` | ✅ |
| III. Calibrable | Todos los valores en `IdentidadOptions` (y el tope de registros en `ApiOptions`, D8); reloj por `TimeProvider`; tarea por reloj con `ProgramaReloj` (T-OPS05) | ✅ |
| IV. Datos confiables | Operaciones atómicas por transacción; renovación idempotente en la gracia | ✅ |
| V. Privacidad | Sin seudónimos ni eventos; horas de sesión truncadas y purga a 30 días (D3); eliminación sin correo en auditoría; IP enmascarada; enlace del primer administrador fuera de los logs (T-SEG14) | ✅ |
| VI. Acceso en el servidor | JWT 5 min (`ClockSkew = 0`) + *refresh* revocable; políticas por rol (401/403, T-SEG20); auditoría en la misma transacción y lecturas auditadas (D15) | ✅ |
| VII. Pruebas primero | Dominio y casos de uso con valores límite (3.º intento, 59/60/61 s, 23:59/24:00 h) | ✅ |
| VIII. Código pequeño | 23 casos de uso de 1 archivo; controladores divididos por tema (`UsuariosController` partido en dos); un archivo por resultado (D4) | ✅ |
| IX. API documentada | `identidad.openapi.yaml` + XML | ✅ |
| X. Costo cero | Correo con plan gratuito; Turnstile gratis | ✅ |

## Project Structure

### Documentation

```text
specs/003-modulo-identidad-accesos/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/identidad.openapi.yaml · contracts/contratos-internos.md
└── checklists/requirements.md · checklists/trazabilidad.md
```

### Source Code — árbol exacto

```text
backend/src/Modules/KillaUru.Identidad/
├── KillaUru.Identidad.csproj                 # referencia: Shared, Auditoria
├── IdentidadModule.cs                        # AddIdentidadModule: options, DbContext, puertos, casos de uso
├── README.md
├── Contracts/
│   └── IIdentidadConsultas.cs                # ContarConductoresActivosAsync(dias)
├── Domain/
│   ├── Correo.cs                             # value object: recorta, minúsculas, formato
│   ├── RolUsuario.cs                         # enum: ConductorRegistrado = 1, Administrador = 2
│   ├── Usuario.cs                            # estado, rol, verificación, contraseña
│   ├── Usuario.Bloqueo.cs                    # partial: RegistrarFallo, RegistrarExito, EstaBloqueado (RN-19)
│   ├── ParametrosBloqueo.cs                  # record: intentos, ventana, duraciones
│   ├── SesionRenovacion.cs                   # vigencia, uso, revocación, ¿dentro de la gracia?
│   ├── PoliticaSesion.cs                     # calcula vencimiento (30 d deslizante / 8 h absolutas)
│   ├── TokenCorreo.cs                        # propósito, vigencia, un solo uso
│   ├── PropositoToken.cs                     # enum: Verificacion = 1, Recuperacion = 2, Invitacion = 3
│   ├── Consentimiento.cs · DocumentoLegal.cs # política = 1, términos = 2
│   ├── RevisionPrivilegios.cs                # fecha, actor, confirmados, retirados; Vence()
│   └── HorasGuardadas.cs                     # A la hora / al día (truncado hacia abajo, D3)
├── Application/
│   ├── IdentidadOptions.cs                   # tabla «Reglas y parámetros» de la spec
│   ├── Resultados/                           # (D4) un archivo por resultado; Adapters solo ve Application
│   │   ├── ResultadoLogin.cs · ResultadoRenovacion.cs · ResultadoEnlace.cs · ResultadoGestion.cs
│   │   └── UsuarioVista.cs                   # fila de usuario para listas (rol y estado como texto)
│   ├── Puertos/
│   │   ├── IUsuarioRepositorio.cs · ISesionRepositorio.cs · ITokenCorreoRepositorio.cs · IRevisionRepositorio.cs
│   │   ├── IPurgaIdentidadRepositorio.cs     # borrar vencidos > 30 d, truncar usado_utc (D3)
│   │   ├── ITransaccionIdentidad.cs          # solo abre la transacción y la une a IContextoTransaccion (ADR-013)
│   │   ├── IEnviadorCorreo.cs                # EnviarVerificacion, EnviarRecuperacion, EnviarInvitacion
│   │   ├── IEmisorTokens.cs                  # CrearAcceso(usuario) → JWT 5 min (sub, rol, verificado)
│   │   ├── IGeneradorSecretos.cs             # Aleatorio32(), Derivar(tokenAnterior) (HMAC), Hash(token)
│   │   ├── IHashContrasenas.cs               # Hash, Verificar
│   │   ├── IVerificadorAntiBot.cs            # Turnstile
│   │   └── ILimitadorRecuperacion.cs         # por correo y por IP
│   ├── Sesion/ EmisionSesion.cs              # servicio común: crea familia y par de tokens
│   ├── Sesion/ IniciarSesionUseCase.cs · RenovarSesionUseCase.cs · CerrarSesionUseCase.cs
│   ├── Cuenta/ RegistrarConductorUseCase.cs · VerificarCorreoUseCase.cs · ReenviarVerificacionUseCase.cs
│   ├── Cuenta/ SolicitarRecuperacionUseCase.cs · FijarContrasenaConEnlaceUseCase.cs   # recuperación e invitación
│   ├── Titular/ ObtenerMisDatosUseCase.cs · AceptarVersionUseCase.cs · EliminarMiCuentaUseCase.cs
│   ├── Legal/ ObtenerVersionesLegalesUseCase.cs
│   ├── Usuarios/ ListarUsuariosUseCase.cs · ResolverCorreosUseCase.cs · CrearUsuarioUseCase.cs
│   ├── Usuarios/ CambiarRolUseCase.cs · DesactivarUsuarioUseCase.cs · ActivarUsuarioUseCase.cs · ReenviarCorreoUsuarioUseCase.cs
│   ├── Usuarios/ CrearAdministradorInicialUseCase.cs   # solo si no hay administradores; devuelve el enlace (T-SEG14)
│   ├── Privilegios/ ConsultarRevisionUseCase.cs · RegistrarRevisionUseCase.cs
│   ├── Tareas/ PurgarIdentidadUseCase.cs     # D3; registra RegistrarTareaCorrecta("purga-identidad")
│   └── IdentidadConsultas.cs                 # implementa IIdentidadConsultas
├── Adapters/                                 # (docs: AuthController y UsuariosController, divididos para ≤ 100 líneas; solo usa Application, D4)
│   ├── SesionController.cs                   # /auth/inicio-sesion, /auth/renovar, /auth/cerrar-sesion
│   ├── CuentaController.cs                   # /auth/registro, /auth/verificar-correo, /auth/recuperar, /auth/fijar-contrasena
│   ├── MeController.cs                       # /me (GET, DELETE), /me/aceptaciones, /me/reenviar-verificacion
│   ├── LegalController.cs                    # /legal/versiones
│   ├── UsuariosController.cs                 # /usuarios (GET, POST)
│   ├── EstadoUsuarioController.cs            # /usuarios/{id}/rol|desactivar|activar|reenviar-correo
│   ├── RevisionPrivilegiosController.cs      # /usuarios/revision-privilegios (GET, POST)
│   ├── CookiesSesion.cs                      # escribe y borra ku_acceso / ku_renovacion
│   ├── Consola/ CrearAdminComando.cs         # `crear-admin <correo>`: imprime el enlace solo en la consola (T-SEG14)
│   └── Dtos/                                 # un archivo por DTO, con XML y <example>; rol, estado y documento como texto
└── Infrastructure/
    ├── IdentidadDbContext.cs
    ├── Configuraciones/ UsuarioConfig.cs · SesionConfig.cs · ConsentimientoConfig.cs · TokenCorreoConfig.cs · RevisionConfig.cs
    ├── Repositorios/ UsuarioRepositorio.cs · SesionRepositorio.cs · TokenCorreoRepositorio.cs · RevisionRepositorio.cs
    ├── Repositorios/ PurgaIdentidadRepositorio.cs   # SQL de data-model §1.1, en lotes
    ├── Tareas/ PurgaIdentidadTarea.cs        # BackgroundService: al arrancar y 03:45 local con ProgramaReloj (T-OPS05)
    ├── TransaccionIdentidad.cs
    ├── Seguridad/ EmisorJwt.cs · GeneradorSecretos.cs · HashContrasenas.cs · VerificadorTurnstile.cs · LimitadorRecuperacionEnMemoria.cs
    └── Correo/ EnviadorCorreoHttp.cs · EnviadorCorreoConsola.cs · PlantillasCorreo.cs

backend/db/migraciones/V0003__identidad.sql
backend/tests/KillaUru.Identidad.Tests/        Domain/ · Application/
backend/tests/KillaUru.Integracion.Tests/Identidad/   Repositorios, AuditoriaEnLaMismaTransaccion, PurgaIdentidad, HorasGuardadas, Api (Sesion, Cuenta, Me, Usuarios, Revision)
deploy/runbooks/restaurar.md · simulacros.md   # (de 001) sección «Identidad» y fila de la revisión mensual
```

**Structure Decision**: plantilla de `enfoque-arquitectonico.md` §4.1 («Identidad y Accesos: `Usuario`, `Rol`, política de bloqueo; registro, login, refresh con 60 s de gracia, cierre de sesión, gestión de usuarios y revisión mensual; `AuthController`, `UsuariosController`; repositorio EF, emisor de JWT, adaptador de correo»). Los dos controladores de docs se dividen en 7 por el límite de 100 líneas, sin cambiar las responsabilidades. Los resultados de los casos de uso viven en `Application/Resultados/` (D4): ningún controlador ni DTO referencia `Domain/`; los DTOs usan texto para rol, estado y documento y Application lo convierte a los enums del dominio. `ITransaccionIdentidad` (antes `IUnidadTrabajoIdentidad`) se renombra porque `patrones` descarta un *Unit of Work* propio (ARQ-12).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| Columnas nuevas respecto de `diseño-interno` §3.4 (`ventana_fallos_desde_utc`, `creado_utc`, `limite_absoluto_utc`, `hash_contrasena` nulo) y tabla `revision_privilegios` | RN-19 necesita el inicio de la ventana de 15 min; RN-17 necesita saber cuándo se renovó; la sesión de 8 h es absoluta; la invitación aún no tiene contraseña; RNF-30 necesita la fecha de la última revisión | Calcularlo desde la auditoría haría que Identidad lea Auditoría (no hay contrato de lectura y no se debe crear) |
| Tarea `purga-identidad` y horas truncadas (D3) | Sin purga, `sesion_refresh` gana una fila por renovación y las horas al segundo permiten unir una renovación con un lote (R-06) | Borrar cada fila usada al terminar la gracia (SEG-02): el autor eligió truncar y purgar a 30 días (D3) |
| Propósito de token `Invitacion` (3) | Crear usuarios (RF-19) sin que el administrador conozca la contraseña (prototipo «Invitar usuario») | Usar el enlace de recuperación de 30 min es muy corto para una invitación |
