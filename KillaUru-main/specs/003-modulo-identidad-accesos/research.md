# Research: Módulo Identidad y Accesos

Las decisiones marcadas **«Derivado del prototipo»** o **«Decisión de diseño SDD»** no estaban en `referencia/docs/`; el autor las confirmó el 2026-10-21 (lista al final) y pasan a docs (D1).

## R-001 · Contraseñas (`diseño-interno` §6.7)

- **Decision**: `PasswordHasher<Usuario>` de `Microsoft.Extensions.Identity.Core` (PBKDF2-HMAC-SHA512 con sal, iteraciones por defecto de .NET 10). Sin adoptar todo ASP.NET Identity. Si `VerifyHashedPassword` devuelve `SuccessRehashNeeded`, se vuelve a hashear al iniciar sesión.
- **Política**: 10–128 caracteres, sin reglas de composición (NIST SP 800-63B). Mínimo 10 por **D13** (igual que el prototipo, «Mínimo 10 caracteres»); máximo 128: *Decisión de diseño SDD*.
- **Alternatives considered**: Argon2 (paquete externo; PBKDF2 es lo que docs indica).

## R-002 · Token de acceso (ADR-009)

- **Decision**: JWT firmado con HMAC-SHA256 (clave de 256 bits del inventario de secretos, RNF-34), 5 min, *claims* solo `sub` (id interno), `rol`, `verificado` (bool, **D8**) y `jti`. Emisor `https://api.killauru.pe`, audiencia `killauru`. Se valida con `ClockSkew = TimeSpan.Zero` (T-SEG09): con un token de 5 min, la tolerancia por defecto de 5 min duplicaría su vida y rompería RNF-26.
- **`verificado` (D8)**: copia de `usuario.correo_verificado` al emitir. Ingesta pone `con_sesion = true` solo si `rol = ConductorRegistrado` **y** `verificado = true` (RN-14), para que una cuenta desechable sin verificar no confirme baches. Tras verificar el correo, el *claim* cambia en la siguiente renovación (≤ 5 min).
- **Rationale**: un solo servicio emite y valida (monolito); HMAC basta y es más simple que RSA.
- **Alternatives considered**: RS256 (útil solo si un tercero valida los tokens).

## R-003 · Token de actualización rotativo con «el mismo par» en la gracia (ADR-009)

- **Problema**: ADR-009 pide devolver **el mismo par nuevo** si el token viejo se reutiliza dentro de 60 s, pero `diseño-interno` §6.7 dice que el token nunca se guarda (solo su SHA-256). Sin guardarlo, no se puede devolver el mismo.
- **Decision (Decisión de diseño SDD)**: el primer token de una familia es aleatorio (32 bytes). Cada token siguiente se **deriva**: `nuevo = Base64Url(HMAC-SHA256(claveRotacion, tokenAnterior))`. Al renovar, el servidor guarda `SHA-256(nuevo)`. Si llega otra vez el token anterior dentro de la gracia, recalcula el mismo `nuevo` y emite un token de acceso nuevo (equivalente). Fuera de la gracia, revoca la familia.
- **Rationale**: cumple ADR-009 y §6.7 a la vez; un atacante que robe un token ya usado no puede derivar el siguiente sin la clave.
- **Consecuencia**: nuevo secreto en el inventario de RNF-34: `KU_IDENTIDAD_CLAVE_ROTACION`.
- **Alternatives considered**: guardar el sucesor cifrado durante 60 s (más columnas y limpieza); no tener gracia (rompe ADR-009 y castiga la mala señal de la sierra).

## R-004 · Vencimientos de la sesión

- **Decision**: `PoliticaSesion`:
  - Conductor: cada token nuevo vence `ahora + 30 días` (deslizante, RF-21, RN-21 a).
  - Administrador: `limite_absoluto_utc = inicio + 8 h` se copia a todos los tokens de la familia; ningún token vence después (RF-16).
- **Rationale**: «sesión de máximo 8 h absolutas» vs. «30 días que se extiende con el uso».

## R-005 · Bloqueo progresivo (RN-19)

- **Decision**: en `Usuario`: `intentos_fallidos`, `ventana_fallos_desde_utc`, `nivel_bloqueo`, `bloqueado_hasta`. Fallo: si la ventana empezó hace más de 15 min, se reinicia en 1; al llegar a 3 se bloquea con `DuracionesBloqueoMinutos[min(nivel, 2)]`, se sube el nivel y el contador vuelve a 0. Éxito: contador y nivel a 0. Restablecer la contraseña también desbloquea.
- **Respuesta (RN-19)**: app → siempre `credenciales_invalidas` (401). Panel → si la cuenta existe, es administrador y está bloqueada: 423 `cuenta_bloqueada` con `bloqueadoHasta` (prototipo «Cuenta bloqueada 5 min… vuelve a intentar a las 10:47»). *El prototipo prevalece para el panel porque son cuentas internas; para conductores manda RN-19.*
- **Intentos con un correo inexistente**: se auditan como `InicioSesionFallido` con objetivo nulo; no se bloquea nada.

## R-006 · Enlaces de correo

- **Decision**: 32 bytes aleatorios en Base64Url; se guarda solo su SHA-256 en `token_correo`. El enlace lleva el token en el **fragmento**: `https://panel.killauru.pe/cuenta/verificar#token=…`, `…/cuenta/restablecer#token=…`, `…/cuenta/invitacion#token=…`. La página estática (Cloudflare Pages) hace el `POST` a la API.
- **Rationale**: el fragmento no viaja al servidor ni queda en logs de Cloudflare; el token es de un solo uso. *Decisión de diseño SDD.*
- Al emitir un token nuevo del mismo propósito para la misma cuenta, los anteriores sin usar se marcan usados (solo un enlace vivo por propósito).

## R-007 · Proveedor de correo (RS-04, regla 11)

- **Decision**: puerto `IEnviadorCorreo` con 3 operaciones de negocio (no un `Enviar(html)` genérico). Implementaciones: `EnviadorCorreoConsola` (desarrollo y pruebas) y `EnviadorCorreoHttp` para el proveedor elegido. Candidato: **Resend** (el nombre ya aparece en `principios-de-diseño.md`); alternativas: Amazon SES, Brevo. **Verificar límites vigentes** del plan gratuito antes de elegir (RS-04). SPF, DKIM y DMARC en `killauru.pe`.
- **Falla del proveedor**: la operación principal no falla; se registra el error y se reintenta 3 veces con espera corta; el usuario puede pedir reenviar.

## R-008 · Anti-bot en el login del panel (ADR-009 punto 6)

- **Decision**: **Cloudflare Turnstile** (gratis, ya se usa Cloudflare). El panel envía `turnstileToken`; la API lo valida con `siteverify` antes de verificar la contraseña. En la app no se usa: se apoya en el límite de tasa (RNF-11) y en RN-19.
- *Decisión de diseño SDD: docs dice «protección anti-bot» sin nombrar herramienta.*

## R-009 · Invitación de usuarios — Derivado del prototipo

- **Decision**: `POST /api/v1/usuarios` crea la cuenta con `hash_contrasena = NULL` y envía un enlace de propósito `Invitacion` de 24 h. Al fijar la contraseña con ese enlace, la cuenta queda con contraseña **y correo verificado** (el enlace demuestra que el correo es suyo). Mientras no la fije: estado «Invitación pendiente» y no puede iniciar sesión.
- **Rationale**: RF-19 pide «crear» usuarios; el prototipo muestra «Invitar usuario: le enviaremos un correo para que cree su contraseña».

## R-010 · Registro con correo ya usado

- **Decision**: 409 `correo_en_uso`. RN-19 y RF-32 piden no revelar cuentas en el **login** y en la **recuperación**; docs no lo pide en el registro, y la app necesita decirle al conductor que ya tiene cuenta.
- **Riesgo aceptado (D8)**: el 409 permite enumerar correos; se mitiga con el tope `LimiteTasa:RegistrosPorHoraPorIp = 5` (`ApiOptions`, 001; 429 con `Retry-After`), calibrable. Se anota en la sección de riesgos de docs.
- **Alternatives considered**: 202 genérico con correo «ya tienes cuenta» (SEG-06, PO-30): descartado por el autor (D8).

## R-011 · Revisión mensual de privilegios (RNF-30)

- **Decision**: tabla `revision_privilegios`; vence el día 1 del mes siguiente a la última revisión (prototipo: «Vence el 1 nov»). Retirar a un administrador = desactivar su cuenta (y revocar sus sesiones). Se audita `RevisionPrivilegios` con los ids confirmados y retirados. La revisión es una verificación periódica mensual en el calendario del *runbook* `simulacros.md` (T-QA14), no una tarea única.

## R-012 · Eliminación de la cuenta

- **Decision**: exige la contraseña (confirmación fuerte de una acción irreversible; prototipo «No se puede deshacer»). Se mantiene por **D13**; el prototipo agrega el campo. Borra la fila de `usuario` (las tablas hijas se borran en cascada **dentro del esquema**), audita `CuentaEliminada` con el id y escribe el log `CuentaEliminada {UsuarioId}` (002, R-009).

## R-013 · Horas de sesión truncadas y purga de Identidad (D3, T-OPS05)

- **Decision**: al escribir, `sesion_refresh.creado_utc`, `expira_utc` y `revocado_utc` se truncan hacia abajo a la hora y `usuario.creado_utc` al día (`HorasGuardadas`, en el dominio). `usado_utc` se guarda exacto porque la gracia de 60 s lo necesita; la purga lo trunca a la hora cuando tiene más de 2 min (`TruncarUsoTrasMinutos`). La purga diaria borra las sesiones con `expira_utc` o `revocado_utc` de hace más de 30 días y los `token_correo` vencidos o usados hace más de 30 días (`RetencionIdentidadDias`).
- **Rationale**: R-06 prohíbe **inferir** el vínculo cuenta ↔ eventos; al segundo, una renovación y el reenvío del mismo lote caen en el mismo instante (SEG-02). `creado_utc` y `revocado_utc` se truncan igual que `expira_utc` porque D3 pide «las horas de sesión redondeadas a la hora» (el cierre de sesión envía pendientes como anónimos, RN-21 d). `limite_absoluto_utc` queda exacto: solo lo tienen administradores, que no envían eventos con sesión, y truncarlo cambiaría las 8 h. RN-17 (7 días) funciona con horas truncadas y la purga a 30 días no le quita datos. El `usado_utc` exacto vive como máximo hasta la siguiente purga (≤ 24 h), dentro de las 72 h en que Ingesta conserva `recibido_utc` exacto.
- **Consecuencia**: con `expira_utc` truncado, la sesión del administrador termina hasta 59 min antes de las 8 h (sigue siendo «máximo 8 h», RF-16). Índices completos por `usuario_id` para que el `ON DELETE CASCADE` de RF-30 no recorra la tabla (BD-05).
- **Programación (T-OPS05)**: `PurgaIdentidadTarea` corre al arrancar y cada día a las 03:45 hora local con `Shared/Tareas/ProgramaReloj` (no `PeriodicTimer` de 24 h) y registra `RegistrarTareaCorrecta("purga-identidad")` (alerta a las 26 h, RNF-22).
- **Alternatives considered**: borrar cada fila usada al terminar la gracia y guardar solo el día de última actividad (SEG-02): el autor eligió D3.

## R-014 · Restauración de un respaldo (D9)

- **Decision**: la sección «Identidad» del *runbook* `restaurar.md` (001) pide, antes de reabrir: `UPDATE identidad.sesion_refresh SET revocado_utc = date_trunc('hour', <ahora>) WHERE revocado_utc IS NULL;` y rotar `KU_JWT_CLAVE_FIRMA` (los JWT emitidos antes dejan de validar). Al arrancar la API corre `purga-identidad` (T-OPS05). Todos vuelven a iniciar sesión.
- **Riesgo aceptado**: eliminaciones, desactivaciones y cambios de rol posteriores al respaldo se pierden (RPO de 24 h); el operador revisa a mano las solicitudes de eliminación del último día (log `CuentaEliminada`, FR-028) y las repite.

## R-015 · Auditoría de escrituras y lecturas (D15)

- **Decision**: cada caso de uso que escribe abre `ITransaccionIdentidad` (la une a `IContextoTransaccion`) y llama `RegistrarAsync` dentro; si falta la transacción, `RegistrarAsync` lanza `InvalidOperationException` (falla en pruebas y desarrollo). El inicio de sesión fallido con un correo inexistente también abre la transacción para auditar. Las lecturas con datos personales —lista de usuarios, `GET /usuarios?ids=` y el estado de la revisión de privilegios (lista de administradores con su correo)— usan `RegistrarLecturaAsync` con `AccesoAdministrativoDatos`. 003 no tiene endpoint de detalle de usuario.
- **Contexto**: `UsuarioCreado` y `CuentaActivada` llevan `{ "rol": "Administrador" | "ConductorRegistrado" }` en el contexto (dato no personal), para que Auditoría pueda tratarlas como riesgo si el autor amplía RNF-29 (SEG-13, ver `specs/revision/no-aplicado-003.md`).
- **Métrica de riesgo**: la emite solo Auditoría con `AlConfirmar` (SEG-18, OPS-16); 003 tiene una prueba de que nunca llama `RegistrarAccionRiesgo`.

## R-016 · Resultados en Application (D4)

- **Decision**: `ResultadoLogin`, `ResultadoRenovacion`, `ResultadoEnlace` y `ResultadoGestion` pasan de `Domain/Resultados.cs` a `Application/Resultados/`, un archivo cada uno (< 200 líneas). Los controladores y DTOs no referencian `Domain/` (`enfoque` §3); rol, estado y documento viajan como texto y Application los convierte. `UsuariosController` se parte en `UsuariosController` (listar, crear) y `EstadoUsuarioController` (rol, desactivar, activar, reenviar) (ARQ-08).

## R-017 · Primer administrador sin pasar por los logs (T-SEG14)

- **Decision**: comando de un solo uso `docker compose run --rm api crear-admin <correo>` (`CrearAdminComando` → `CrearAdministradorInicialUseCase`): crea la cuenta invitada solo si no hay administradores, audita `UsuarioCreado` con actor sistema e **imprime el enlace solo en la consola** (`Console.Out`, no `ILogger`), así no llega a Grafana Cloud (RNF-34, R-06). Reemplaza la semilla SQL y «el enlace se obtiene del log».

## Confirmados por el autor el 2026-10-21

1. R-003 derivación HMAC del token de actualización (y el nuevo secreto) — **D1**.
2. R-005 respuesta 423 visible solo en el panel — **D1**.
3. R-006 tokens en el fragmento y páginas en `panel.killauru.pe/cuenta/…` — **D1**.
4. R-008 Turnstile solo en el panel — **D1**.
5. R-009 invitación de 24 h y estado «Invitación pendiente» — **D1**.
6. R-010 409 en el registro, como riesgo aceptado con tope de 5 registros/h por IP — **D8**.
7. Valores de RF-32 (3/h por correo, 10/h por IP) — **D1**; política de contraseñas **10–128** — **D13**.
8. Eliminar la cuenta pide la contraseña (R-012), se mantiene y se agrega al prototipo — **D13**.
# Decisiones de implementación verificadas (2026-10-24)

- Concurrencia: bloqueo de fila por cuenta; para correos aún inexistentes, advisory lock por correo normalizado. Los cambios administrativos usan un mutex transaccional separado. Evita serializar todos los logins y protege al último administrador.
- Canje y refresh leen el token después de bloquear su cuenta. PostgreSQL conserva microsegundos; `usado_utc` y la comparación de gracia usan esa precisión para aceptar exactamente sesenta segundos.
- El `jti` firmado identifica la familia al cerrar sesión: la cookie refresh tiene Path restringido a renovar. No se amplía su exposición al resto de rutas.
- Correos después del commit, con idempotencia y reintentos. Si el proceso termina entre commit y envío, se usa reenvío; no se agrega un outbox fuera del alcance acordado. La consola de desarrollo omite enlaces y correos.
- Reenvío administrativo usa el código existente `RecuperacionSolicitada` con contexto de operación `Reenvio` y propósito; no cambia el catálogo de 002 ni genera métrica de riesgo.
- Los archivos de pruebas se agrupan por responsabilidad; los nombres tentativos de tasks.md se corresponden con la tabla de evidencia de la revisión. La matriz distingue validación local de verificaciones operativas externas.
