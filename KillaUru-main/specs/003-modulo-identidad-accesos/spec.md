# Feature Specification: Módulo Identidad y Accesos

**Feature Branch**: `003-modulo-identidad-accesos`

**Created**: 2026-10-20

**Status**: Draft

**Input**: User description: "Módulo Identidad y Accesos de KillaUru: cuentas de conductores y administradores, registro con consentimiento, inicio de sesión con bloqueo progresivo, sesiones cortas y revocables, verificación de correo, recuperación de contraseña, derechos del titular (ver mis datos, eliminar mi cuenta), gestión de usuarios y revisión mensual de privilegios."

---

## 0. Ficha del módulo

| Campo | Valor |
|---|---|
| Qué es | **Quién es quién**: cuentas, credenciales, roles, sesiones, consentimientos y correos transaccionales |
| Qué **no** es | No guarda nada de eventos ni trayectorias: **ninguna** tabla une una cuenta con eventos (RNF-10). No valida el token en cada petición (eso es el middleware de 001). No guarda el permiso local offline (vive en el celular, RN-21) |
| Componente C3 | Identidad y Accesos |
| Esquema de BD | `identidad` (dueño único) |
| Depende de | `Shared`; **Auditoría** (002) por `IAuditoriaService`; proveedor de correo por `IEnviadorCorreo` (único módulo que lo usa, regla 11) |
| Lo usan | Dashboard (007) por `IIdentidadConsultas`; la app y el panel por HTTP |
| Pantallas del prototipo | App: Términos y privacidad, ¿Creas una cuenta?, Crear cuenta, Revisa tu correo, Ajustes → Mi perfil, Eliminar cuenta («Esto pasará en este celular», «No se puede deshacer»). Panel: Iniciar sesión (bloqueo 5 min, sesión hasta 8 h), Usuarios (invitar, cambiar rol, reenviar verificación, desactivar/activar, revisión mensual) |
| Prioridad | *Must*: HU-11, HU-14, HU-18, HU-18b, HU-22, HU-23, HU-25, HU-27 · *Should no recortable*: HU-24 |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historias | HU-11, HU-14, HU-18, HU-18b, HU-22, HU-23, HU-24, HU-25, HU-27 | `02-historias-de-usuario.md` |
| Requisitos funcionales | RF-16, RF-19, RF-21, RF-22, RF-29, RF-30, RF-31, RF-32, RF-37, RF-38 (parte servidor) | `03-requisitos-funcionales.md` |
| Atributos de calidad | RNF-07 (restauración), RNF-10, RNF-11 (límite de tasa), RNF-22 (tareas programadas), RNF-26, RNF-29, RNF-30, RNF-31, RNF-34 | `04-atributos-de-calidad.md` |
| Reglas de negocio | RN-09 y RN-10 (retención y restauración: D3, D9), RN-14 (`con_sesion` solo con correo verificado: D8), RN-17 (conductor activo), RN-19 (bloqueo), RN-21 (c y d: lo que el servidor informa) | `09-reglas-de-negocio.md` |
| Restricciones y riesgos | R-06 (Ley 29733, consentimiento, ARCO, flujo transfronterizo), RS-04 (correo) | `05-restricciones.md` |
| Decisiones | ADR-009 (todo), ADR-013, ADR-007 (punto 4) | `07-decisiones-arquitectonicas.md` |
| Escenarios | EQ-04, EQ-13 | `08-escenarios-y-atributos-de-calidad.md` |
| Decisiones del autor (2026-10-21) | D3, D4, D8, D9, D13, D15 y correcciones T-OPS05, T-QA14, T-QA18, T-SEG09, T-SEG14, T-SEG19, T-SEG20 | `specs/revision/decisiones.md`, `specs/revision/plan-de-cambios.md` |
| Diseño | Tablas `usuario`, `sesion_refresh`, `consentimiento`, `token_correo`; §6.7 (contraseñas, tokens, bloqueo) | `diseño-interno-de-modulos.md` §3.4, §6.7 |
| Glosario | Token de acceso, token de actualización, derecho de acceso local, conductor activo, derechos ARCO, términos de uso | `10-glosario.md` §4 |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Registrarse como conductor con consentimiento (Priority: P1)

Como conductor, quiero crear mi cuenta con correo y contraseña aceptando la política de privacidad y los términos de uso, para ver los 333 km y recibir alertas (HU-18, HU-22, HU-27).

**Why this priority**: Sin cuenta no hay corredor completo ni alertas; es el paso que convierte al anónimo en registrado.

**Independent Test**: Registrar una cuenta con las versiones vigentes aceptadas y recibir una sesión; intentar registrarse sin aceptar o con versiones viejas y ser rechazado.

**Acceptance Scenarios**:

1. **Given** un correo nuevo, una contraseña válida y la aceptación de las versiones **vigentes** de la política y de los términos, **When** me registro, **Then** se crea la cuenta con rol `ConductorRegistrado`, se guardan la fecha y la versión de ambos documentos aceptados, recibo una sesión (token de acceso de 5 min y token de actualización de 30 días) y se envía un correo de verificación (RF-21, RF-22, RF-29, RF-37).
2. **Given** que no marco alguna de las dos aceptaciones, o envío una versión distinta de la vigente, **When** me registro, **Then** no se crea la cuenta (`codigo = aceptacion_requerida`) (HU-22).
3. **Given** un correo ya registrado, **When** me registro, **Then** responde 409 `correo_en_uso` y no se modifica la cuenta existente (riesgo de enumeración **aceptado** por el autor, mitigado por el límite del escenario 5; D8).
4. **Given** la app sin cuenta, **When** consulta las versiones legales vigentes, **Then** recibe la versión y la URL de la política y de los términos, para mostrarlos antes de detectar (RF-29, RF-37).
5. **Given** 5 registros desde la misma IP en la última hora, **When** llega el 6.º, **Then** responde 429 `limite_excedido` con `Retry-After` y no se crea la cuenta (D8, RNF-11).

---

### User Story 2 - Iniciar sesión con bloqueo progresivo (Priority: P1)

Como conductor o administrador, quiero iniciar sesión con mi correo y contraseña, y que tras varios intentos fallidos la cuenta se bloquee un tiempo creciente, para proteger mi cuenta de ataques (HU-11, HU-18, RF-16, RF-21, RN-19).

**Why this priority**: Es la puerta de entrada al panel y al corredor completo.

**Independent Test**: Iniciar sesión bien; fallar 3 veces en 15 min y comprobar el bloqueo de 5 min, luego 15 min, luego 1 h, y que todo queda auditado.

**Acceptance Scenarios**:

1. **Given** credenciales válidas de un conductor desde la app, **When** inicio sesión, **Then** recibo un token de acceso de 5 min y un token de actualización de 30 días que se extiende con el uso (RF-21).
2. **Given** credenciales válidas de un administrador desde el panel, **When** inicio sesión, **Then** recibo la sesión en cookies `HttpOnly; Secure; SameSite=Strict`, con token de acceso de 5 min y una sesión de **máximo 8 h** absolutas, y el inicio queda auditado (RF-16, ADR-009).
3. **Given** un correo inexistente o una contraseña incorrecta, **When** intento iniciar sesión, **Then** la respuesta es **la misma** en ambos casos (`credenciales_invalidas`) (RN-19).
4. **Given** 3 intentos fallidos en 15 min, **When** fallo el 3.º, **Then** la cuenta queda bloqueada 5 min; el siguiente bloqueo dura 15 min y el siguiente 1 h; cada intento fallido y cada bloqueo se auditan (RN-19, HU-11).
5. **Given** una cuenta bloqueada, **When** intento entrar desde el **panel**, **Then** veo «Cuenta bloqueada» con la hora en que puedo volver a intentar (prototipo); **When** intento entrar desde la **app**, **Then** recibo la misma respuesta que con credenciales inválidas (RN-19: no revelar cuentas de conductores).
6. **Given** una cuenta desactivada, **When** intento iniciar sesión, **Then** no se crea sesión y la respuesta es `credenciales_invalidas`.
7. **Given** un conductor, **When** intenta entrar al **panel**, **Then** responde 403 `solo_administradores`.

---

### User Story 3 - Sesiones cortas, renovables y revocables (Priority: P1)

Como responsable de seguridad, quiero que el acceso dure 5 min y se renueve con un token rotativo que se puede revocar, para que una cuenta desactivada pierda el acceso en ≤ 5 min sin consultar la base de datos en cada petición (ADR-009, RNF-26).

**Why this priority**: Sin renovación la app pediría la contraseña cada 5 min; sin revocación no se cumple RNF-26.

**Independent Test**: Renovar varias veces; reutilizar un token viejo antes y después de 60 s; desactivar la cuenta y comprobar que la siguiente renovación falla.

**Acceptance Scenarios**:

1. **Given** un token de actualización válido, **When** renuevo, **Then** recibo un par nuevo, el anterior queda usado y, para el conductor, la sesión se extiende 30 días desde ahora; para el administrador, nunca más allá de las 8 h desde el inicio de sesión.
2. **Given** que reenvío un token ya usado **dentro de 60 s** (la respuesta anterior se perdió en la sierra), **When** renuevo, **Then** recibo **el mismo par** que la primera vez (ADR-009).
3. **Given** que reenvío un token ya usado **después de 60 s**, **When** renuevo, **Then** se revoca toda la sesión (familia) y responde 401 `sesion_invalida`.
4. **Given** una cuenta desactivada, **When** su app intenta renovar, **Then** responde 401 `cuenta_desactivada`; la app borra su permiso local (RN-21 c). El token de acceso vigente deja de servir en ≤ 5 min (RNF-26).
5. **Given** que cierro sesión, **When** el servidor recibe la petición, **Then** revoca mi sesión; lo demás (recortar al tramo demo, borrar el historial, enviar pendientes como anónimos) lo hace la app (RF-38, RN-21 d).
6. **Given** un token de actualización vencido, revocado o de una cuenta eliminada, **When** renuevo, **Then** responde 401 `sesion_invalida` y la app borra su permiso local.

---

### User Story 4 - Verificar el correo y recuperar la contraseña (Priority: P1)

Como conductor o administrador, quiero verificar mi correo y poder recuperar mi contraseña con un enlace, para no perder mi cuenta si la olvido (HU-18b, HU-25, RF-22, RF-32).

**Why this priority**: Ambas son *Must*; sin recuperación, una contraseña olvidada es una cuenta perdida.

**Independent Test**: Registrar, abrir el enlace de verificación; pedir recuperación para un correo existente y uno inexistente (misma respuesta); usar el enlace dos veces (la 2.ª falla).

**Acceptance Scenarios**:

1. **Given** el enlace de verificación recibido, **When** lo abro dentro de 24 h, **Then** mi correo queda verificado y el *claim* `verificado` de mi token de acceso pasa a `true` en la siguiente renovación (≤ 5 min; D8, RN-14); después de 24 h o si ya se usó, el enlace no sirve (HU-18b).
2. **Given** que pido «Olvidé mi contraseña», **When** el correo existe y está verificado, **Then** recibo un enlace de **un solo uso** que vence en 30 min; **When** el correo no existe o no está verificado, **Then** la respuesta en pantalla es **idéntica** y no se envía nada (HU-25, RF-32).
3. **Given** el enlace de recuperación vigente, **When** fijo una contraseña nueva, **Then** se cierran **todas** mis sesiones y la acción se audita (HU-25).
4. **Given** muchas solicitudes de recuperación para el mismo correo o desde la misma IP, **When** se supera el límite, **Then** se responde igual pero no se envían más correos (RF-32).
5. **Given** que no recibí el correo de verificación, **When** pido reenviarlo desde mi perfil, **Then** se envía un enlace nuevo y el anterior deja de servir (*derivado del prototipo*).

---

### User Story 5 - Ejercer mis derechos como titular de datos (Priority: P1)

Como conductor registrado, quiero ver los datos personales que el sistema tiene sobre mí, aceptar una nueva versión de la política o los términos, y eliminar mi cuenta, para ejercer mis derechos de la Ley 29733 (HU-23, HU-24, RF-30, RF-31, RF-37).

**Why this priority**: HU-23 es obligación legal (no se recorta); HU-24 es *Should* no recortable.

**Independent Test**: Consultar «mis datos»; aceptar una versión nueva; eliminar la cuenta y comprobar que no queda ningún dato personal y que la auditoría solo tiene el id.

**Acceptance Scenarios**:

1. **Given** una sesión de conductor, **When** consulto mis datos, **Then** veo **todos** los datos personales guardados: correo, rol, fecha de alta, si el correo está verificado y la versión aceptada de la política y de los términos (HU-24, RF-31).
2. **Given** que cambió la versión vigente de los términos o de la política, **When** la app me lo pide y acepto, **Then** se guarda la nueva fecha y versión (RF-37).
3. **Given** que confirmo la eliminación con mi contraseña, **When** se procesa, **Then** se borran mi correo, mis credenciales, mis consentimientos y mis tokens de correo, se revocan mis sesiones, y la auditoría guarda solo mi id interno (HU-23, RF-30). Los eventos que envié no se tocan, porque nunca estuvieron unidos a la cuenta (HU-21).

---

### User Story 6 - Gestionar usuarios y revisar privilegios cada mes (Priority: P1)

Como administrador, quiero crear usuarios, cambiar su rol, desactivarlos o activarlos y registrar la revisión mensual de privilegios, para mantener el control de acceso (HU-14, RF-19, RNF-30).

**Why this priority**: HU-14 es *Must*; RNF-30 lo exige el D.S. 016-2024-JUS.

**Independent Test**: Invitar a un usuario, cambiarle el rol, desactivarlo (pierde el acceso en ≤ 5 min), activarlo y hacer la revisión mensual; todo auditado.

**Acceptance Scenarios**:

1. **Given** una sesión de administrador, **When** listo los usuarios con búsqueda por correo y filtros por rol y estado, **Then** veo correo, rol, estado (activa, desactivada, invitación pendiente), correo verificado, fecha de alta y última sesión; la consulta se audita como `AccesoAdministrativoDatos` con `RegistrarLecturaAsync` (D15).
2. **Given** un correo y un rol, **When** creo un usuario, **Then** se crea una cuenta sin contraseña y se envía un correo de invitación de 24 h para que la persona fije su contraseña; queda auditado (`UsuarioCreado`) (RF-19; *invitación derivada del prototipo*).
3. **Given** un usuario, **When** cambio su rol, **Then** se revocan sus sesiones (el rol nuevo aplica en su próximo inicio) y queda auditado (`RolCambiado`, acción de riesgo).
4. **Given** un usuario, **When** lo desactivo con un motivo, **Then** se revocan sus sesiones, pierde el acceso a la API en ≤ 5 min y queda auditado (HU-14, RNF-26).
5. **Given** que el administrador intenta desactivarse o quitarse el rol a sí mismo, o dejar el sistema sin ningún administrador activo, **When** confirma, **Then** responde 409 `ultimo_administrador` o `accion_sobre_si_mismo`.
6. **Given** el inicio de un mes, **When** hago la revisión, **Then** veo la lista de administradores, confirmo o retiro (desactivo) a cada uno, y la revisión queda registrada con fecha; el panel muestra cuándo vence la próxima (RNF-30).
7. **Given** una lista de ids (por ejemplo, actores de la auditoría), **When** el panel pide sus correos, **Then** recibe los que existen; los eliminados no aparecen (prototipo: «cuentas eliminadas solo con su id»); la consulta se audita como `AccesoAdministrativoDatos` (D15).

---

### User Story 7 - Dar datos agregados a otros módulos sin revelar identidades (Priority: P3)

Como módulo Dashboard, quiero saber cuántos conductores registrados están activos, sin recibir ninguna identidad, para mostrar el indicador de RN-17.

**Independent Test**: Con sesiones sembradas, `ContarConductoresActivosAsync(7)` devuelve el número correcto.

**Acceptance Scenarios**:

1. **Given** cuentas de conductor con sesiones renovadas en distintos días, **When** se cuentan las activas en 7 días, **Then** se cuentan las cuentas (no las sesiones) que renovaron su sesión en ese periodo (RN-17).

---

### User Story 8 - Guardar solo lo necesario y sobrevivir a una restauración (Priority: P2)

Como responsable de privacidad, quiero que las horas de las sesiones no permitan unir una cuenta con un lote de eventos, que lo vencido se borre y que, tras restaurar un respaldo, nadie conserve una sesión revivida (D3, D9, RNF-10, RN-10).

**Why this priority**: R-06 prohíbe **inferir** el vínculo cuenta ↔ eventos; las sesiones rotan cada 5 min y, sin purga, crecen sin límite.

**Independent Test**: Renovar y revisar las horas guardadas; correr la purga con `FakeTimeProvider` a 30 y 31 días; aplicar los pasos de restauración y comprobar que toda renovación falla.

**Acceptance Scenarios**:

1. **Given** una renovación a las 10:17:43, **When** se guarda la sesión, **Then** `creado_utc`, `expira_utc` y `revocado_utc` quedan en la hora en punto (truncadas hacia abajo), el `usado_utc` del token anterior queda exacto y el `usuario.creado_utc` de una cuenta nueva queda en el día (D3).
2. **Given** la purga diaria de las 03:45 (y al arrancar), **When** corre, **Then** borra las sesiones con `expira_utc` o `revocado_utc` de hace más de 30 días y los tokens de correo vencidos o usados hace más de 30 días, trunca a la hora todo `usado_utc` con más de 2 min y registra `purga-identidad` (D3, T-OPS05).
3. **Given** un respaldo restaurado, **When** se siguen los pasos de Identidad del *runbook*, **Then** todas las sesiones quedan revocadas, la clave de firma JWT es nueva y todos vuelven a iniciar sesión (D9).

### Edge Cases

- Correo con mayúsculas o espacios: se compara sin distinguir mayúsculas y sin espacios (`citext` + recorte).
- Dos renovaciones simultáneas con el mismo token (dos pestañas del panel): la segunda cae en la gracia de 60 s y recibe el mismo par.
- Restablecer la contraseña de una cuenta bloqueada: la desbloquea y reinicia el contador.
- Enlace de verificación usado después de cambiar el correo: no aplica (v1.0 no permite cambiar el correo; *Rectificación* de ARCO queda fuera, ver «Fuera de alcance»).
- El proveedor de correo falla: la acción principal (registro, recuperación) no falla; el envío se reintenta y se registra como error técnico; `reenviar` permite pedirlo otra vez.
- Un administrador invitado que nunca fija su contraseña: la invitación vence a las 24 h; el administrador puede reenviarla.
- Reloj del servidor: todas las vigencias se calculan con `TimeProvider`; el token de acceso se valida sin tolerancia (`ClockSkew = TimeSpan.Zero`, T-SEG09).
- Administrador que inicia sesión a las 09:12: `limite_absoluto_utc` = 17:12 (exacto), pero cada `expira_utc` se guarda truncado (17:00); la sesión dura como máximo 8 h, nunca más (D3, RF-16).
- Reuso de un token dentro de la gracia de 60 s: funciona porque `usado_utc` se guarda exacto; la purga solo lo trunca pasados 2 min (D3).

## Requirements *(mandatory)*

### Functional Requirements

**A. Cuentas y consentimiento**

- **FR-001**: El sistema MUST registrar conductores con correo y contraseña, asignando el rol `ConductorRegistrado`. *(RF-21, HU-18)*
- **FR-002**: El registro MUST exigir la aceptación explícita de las versiones **vigentes** de la política de privacidad y de los términos de uso, y guardar la fecha y la versión de cada una. *(RF-29, RF-37, HU-22, HU-27)*
- **FR-003**: El sistema MUST publicar las versiones vigentes y las URL de la política y de los términos, sin autenticación, para que la app pida la aceptación (también al anónimo, que la guarda en el celular). *(RF-29, RF-37)*
- **FR-004**: El sistema MUST permitir a un conductor aceptar una versión nueva y guardar su fecha y versión. *(RF-37)*
- **FR-005**: Las contraseñas MUST guardarse solo como hash PBKDF2 con sal (`PasswordHasher`), nunca en claro; longitud mínima de 10 y máxima de 128 caracteres. *(`diseño-interno` §6.7; mínimo 10 por D13)*
- **FR-006**: El correo MUST compararse sin distinguir mayúsculas y ser único. *(Decisión SDD; `diseño-interno` §3.4 `citext UNIQUE`)*
- **FR-037**: El registro MUST limitarse a `LimiteTasa:RegistrosPorHoraPorIp` (5) registros por hora por IP con la política de límite de tasa del host (001) y responder 429 `limite_excedido`; el 409 `correo_en_uso` se mantiene como riesgo aceptado, mitigado por ese límite. *(D8, RNF-11)*

**B. Inicio de sesión y bloqueo**

- **FR-007**: El sistema MUST iniciar sesión con correo y contraseña para conductores (app) y administradores (panel). *(RF-16, RF-21)*
- **FR-008**: La respuesta a correo inexistente, contraseña incorrecta o cuenta desactivada MUST ser idéntica. *(RN-19)*
- **FR-009**: El sistema MUST bloquear la cuenta tras `IntentosMaximos` fallos en `VentanaIntentosMinutos`, con duraciones crecientes `DuracionesBloqueoMinutos`; un inicio correcto reinicia contador y nivel. *(RN-19)*
- **FR-010**: Cada intento fallido, cada bloqueo y cada inicio de sesión de administrador MUST auditarse. *(RF-15, HU-11)*
- **FR-011**: El panel MUST mostrar el bloqueo y su fin; la app MUST recibir la misma respuesta que con credenciales inválidas. *(RN-19; prototipo)*
- **FR-012**: El inicio de sesión del panel MUST exigir una verificación anti-bot. *(ADR-009 punto 6)*
- **FR-013**: Un conductor MUST NOT poder iniciar sesión en el panel. *(RF-16, ADR-009)*

**C. Sesiones**

- **FR-014**: Todos los roles MUST recibir un token de acceso (JWT) de 5 min con solo `sub`, `rol` y `verificado` (bool: correo verificado al emitirlo; Ingesta lo usa para `con_sesion`, RN-14), validado sin tolerancia de reloj (`ClockSkew = TimeSpan.Zero`). *(ADR-009, `diseño-interno` §6.7, D8, T-SEG09)*
- **FR-015**: El token de actualización MUST ser aleatorio de 32 bytes, guardarse solo como SHA-256, rotar en cada uso dentro de su familia y durar: conductor 30 días desde el último uso; administrador como máximo 8 h desde el inicio. *(ADR-009, RF-16, RF-21)*
- **FR-016**: Si un token ya usado se reutiliza dentro de `GraciaRotacionSegundos`, MUST devolverse el mismo par; después, MUST revocarse toda la familia. *(ADR-009)*
- **FR-017**: Cada renovación MUST verificar que la cuenta sigue activa y responder `cuenta_desactivada` si no. *(ADR-009, RN-21 c)*
- **FR-018**: El cierre de sesión MUST revocar la familia de la sesión. *(RF-38)*
- **FR-019**: La desactivación, la eliminación, el cambio de rol y el cambio de contraseña MUST revocar todas las sesiones de la cuenta. *(RNF-26, HU-25)*
- **FR-020**: En el panel, los tokens MUST viajar en cookies `HttpOnly; Secure; SameSite=Strict` (`ku_acceso` y `ku_renovacion`, esta última solo en la ruta de renovación); en la app, en el cuerpo de la respuesta. *(ADR-009 punto 5)*

**D. Correo**

- **FR-021**: El sistema MUST enviar un correo de verificación con un enlace de un solo uso vigente 24 h al registrarse, y permitir reenviarlo invalidando el anterior. *(RF-22, HU-18b)*
- **FR-022**: El sistema MUST enviar, solo a correos verificados, un enlace de recuperación de un solo uso que vence en 30 min, responder igual exista o no la cuenta, y limitar las solicitudes por correo y por IP. *(RF-32, HU-25)*
- **FR-023**: Restablecer la contraseña MUST cerrar todas las sesiones, desbloquear la cuenta y auditarse. *(HU-25)*
- **FR-024**: Los enlaces MUST llevar el token en el **fragmento** de la URL (`#token=`) para que no quede en logs de servidores; el token se guarda solo como SHA-256. *(Decisión de diseño SDD, R-006)*
- **FR-025**: El proveedor de correo MUST integrarse solo desde este módulo, detrás de `IEnviadorCorreo`. *(Regla 11, ADR-009 punto 3)*

**E. Derechos del titular (Ley 29733)**

- **FR-026**: El conductor MUST poder ver todos sus datos personales: correo, rol, fecha de alta, correo verificado, versión y fecha aceptadas de la política y de los términos. *(RF-31, HU-24)*
- **FR-027**: El conductor MUST poder eliminar su cuenta confirmando con su contraseña; se borran correo, credenciales, consentimientos y tokens de correo, se revocan las sesiones y se audita **sin correo**. *(RF-30, HU-23)*
- **FR-028**: Al eliminar una cuenta, MUST escribirse un log estructurado `CuentaEliminada` con el id interno (sin correo) para poder repetir la eliminación tras restaurar un respaldo. *(RN-10; `002/research.md` R-009)*
- **FR-029**: Ninguna tabla del módulo MUST guardar seudónimos, eventos, ubicaciones ni trayectorias. *(RNF-10, ADR-007 punto 4)*

**F. Gestión de usuarios (administrador)**

- **FR-030**: El administrador MUST poder listar usuarios con búsqueda por correo, filtro por rol y estado, y paginación; la consulta se audita como `AccesoAdministrativoDatos` con `RegistrarLecturaAsync`. *(RF-19; prototipo; D15)*
- **FR-031**: El administrador MUST poder crear un usuario (correo + rol) que recibe una invitación de 24 h para fijar su contraseña. *(RF-19; invitación derivada del prototipo)*
- **FR-032**: El administrador MUST poder cambiar el rol, desactivar (con motivo) y activar una cuenta, y reenviar la verificación o la invitación; todo auditado. *(RF-19, HU-14; activar y reenviar derivados del prototipo)*
- **FR-033**: El sistema MUST impedir que un administrador actúe sobre sí mismo (desactivarse, quitarse el rol) o deje el sistema sin administradores activos. *(RF-19, RNF-30; Decisión SDD)*
- **FR-034**: El administrador MUST poder registrar la revisión mensual de privilegios: ver la lista de administradores, confirmar o retirar (desactivar) a cada uno; se guarda la fecha y se audita. El sistema MUST informar cuándo vence la próxima revisión (el día 1 del mes siguiente a la última) y la revisión figura como verificación periódica mensual en el calendario del *runbook* `simulacros.md`. Consultar el estado de la revisión (lista de administradores con su correo) se audita igual que la lista de usuarios. *(RF-19, RNF-30, T-QA14, D15)*
- **FR-035**: El sistema MUST devolver los correos de una lista de ids (máximo 200) solo al administrador, para mostrar actores en la auditoría; la consulta se audita como `AccesoAdministrativoDatos` con `RegistrarLecturaAsync`. *(002 R-006, D15)*

**G. Consultas para otros módulos**

- **FR-036**: El módulo MUST ofrecer `ContarConductoresActivosAsync(dias)`: número de cuentas `ConductorRegistrado` que renovaron o iniciaron su sesión en los últimos N días, sin revelar cuáles. *(RN-17, RF-18)*

**H. Auditoría, retención y operación**

- **FR-038**: Toda acción sensible MUST auditarse con `IAuditoriaService.RegistrarAsync` **dentro** de la transacción del caso de uso (`TransaccionIdentidad` unida a `IContextoTransaccion`, también el inicio fallido con un correo inexistente); las lecturas sensibles (FR-030, FR-034, FR-035) MUST usar `RegistrarLecturaAsync`. El módulo MUST NOT llamar `IMetricas.RegistrarAccionRiesgo` (la emite solo Auditoría). *(D15, ADR-013, RNF-31)*
- **FR-039**: El módulo MUST guardar `sesion_refresh.creado_utc`, `expira_utc` y `revocado_utc` truncados hacia abajo a la hora, `usado_utc` exacto (lo necesita la gracia de 60 s) y `usuario.creado_utc` truncado al día. *(D3, RNF-10, R-06)*
- **FR-040**: Una tarea diaria por reloj (`ProgramaReloj`: al arrancar y a las 03:45 hora local) MUST borrar las `sesion_refresh` con `expira_utc` o `revocado_utc` de hace más de 30 días y los `token_correo` vencidos o usados hace más de 30 días, truncar a la hora todo `usado_utc` con más de 2 min y registrar `IMetricas.RegistrarTareaCorrecta("purga-identidad")`. *(D3, T-OPS05, RNF-22; BD-05)*
- **FR-041**: Tras restaurar un respaldo y antes de reabrir, el *runbook* MUST revocar todas las `sesion_refresh` y rotar `KU_JWT_CLAVE_FIRMA` (todos vuelven a iniciar sesión); las eliminaciones, desactivaciones y cambios de rol posteriores al respaldo que se pierdan son riesgo aceptado (RPO de 24 h) y se revisan a mano las solicitudes de eliminación del último día. *(D9, RN-10, RNF-07)*
- **FR-042**: Todas las operaciones del contrato MUST declarar 429; sin token en un endpoint que exige sesión → 401; con token y rol insuficiente → 403. *(T-SEG19, T-SEG20, RNF-11)*
- **FR-043**: El enlace de invitación del primer administrador MUST imprimirse solo en la consola del comando de semilla (`crear-admin`), nunca en los logs ni en Grafana Cloud. *(T-SEG14, RNF-34, R-06)*

### Reglas y parámetros (`IdentidadOptions`, sección `Identidad`)

| Regla | Parámetro | Valor inicial | Fuente |
|---|---|---|---|
| Token de acceso | `TokenAccesoMinutos` | 5 | ADR-009 |
| Sesión del administrador (absoluta) | `SesionAdministradorHoras` | 8 | RF-16 |
| Sesión del conductor (desde el último uso) | `SesionConductorDias` | 30 | RF-21, RN-21 |
| Gracia de la rotación | `GraciaRotacionSegundos` | 60 | ADR-009 |
| RN-19 intentos | `IntentosMaximos` | 3 | RN-19 |
| RN-19 ventana | `VentanaIntentosMinutos` | 15 | RN-19 |
| RN-19 duraciones | `DuracionesBloqueoMinutos` | [5, 15, 60] | RN-19 |
| Verificación de correo | `VerificacionCorreoHoras` | 24 | HU-18b |
| Recuperación | `RecuperacionMinutos` | 30 | HU-25 |
| Invitación | `InvitacionHoras` | 24 | *Derivado del prototipo* (igual que la verificación) |
| Límite de recuperaciones por correo | `RecuperacionesPorCorreoPorHora` | 3 | RF-32 (valor: *Decisión de diseño SDD*) |
| Límite de recuperaciones por IP | `RecuperacionesPorIpPorHora` | 10 | RF-32 (valor: *Decisión de diseño SDD*) |
| Longitud de contraseña | `ContrasenaMinima`, `ContrasenaMaxima` | 10, 128 | D13 (mínimo 10, como el prototipo); máximo: *Decisión de diseño SDD* (NIST SP 800-63B) |
| Registros por IP | `LimiteTasa:RegistrosPorHoraPorIp` (en `ApiOptions`, 001) | 5 | D8 (calibrable) |
| Retención de sesiones y tokens de correo | `RetencionIdentidadDias` | 30 | D3 |
| Truncado de `usado_utc` | `TruncarUsoTrasMinutos` | 2 | D3 |
| Hora de la purga de Identidad | `HoraPurgaIdentidad` | 03:45 (hora local, UTC−5) | T-OPS05 |
| Versiones vigentes | `VersionPolitica`, `VersionTerminos` | `"1.2"`, `"1.2"` | RF-29, RF-37; prototipo («Versión 1.2») |
| Conductor activo | (lo pasa Dashboard) | 7 días | RN-17 (`DiasConductorActivo` vive en `DashboardOptions`) |

### Key Entities

- **Usuario**: id interno, correo, hash de la contraseña (vacío si la invitación está pendiente), rol, activo, correo verificado, intentos fallidos, inicio de la ventana de fallos, nivel de bloqueo, bloqueado hasta, fecha de alta.
- **Sesión de renovación**: token (solo su hash), familia, cuenta, creación, vencimiento, límite absoluto (admin), uso, revocación. Horas truncadas a la hora, salvo el uso (exacto hasta la purga) y el límite absoluto (D3).
- **Consentimiento**: cuenta, documento (política o términos), versión, fecha de aceptación.
- **Token de correo**: hash, cuenta, propósito (verificación, recuperación, invitación), vencimiento, uso.
- **Revisión de privilegios**: fecha, quién la hizo, administradores confirmados y retirados.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Una cuenta desactivada pierde el acceso a la API en ≤ 5 min (RNF-26, EQ-04).
- **SC-002**: 100 % de los intentos sin token, con token vencido o por fuerza bruta son rechazados; 0 respuestas que permitan distinguir un correo de conductor registrado de uno inexistente en login y recuperación (RN-19, RF-32).
- **SC-003**: 0 tablas con un vínculo entre una cuenta y eventos o trayectorias (auditoría del esquema, RNF-10).
- **SC-004**: Tras eliminar una cuenta, 0 rastros de su correo en cualquier tabla (RF-30).
- **SC-005**: El login responde en < 500 ms p95 (el hash PBKDF2 domina el tiempo).
- **SC-006**: El 100 % de las acciones sensibles de este módulo quedan auditadas en la misma transacción (RNF-31).
- **SC-007**: Tras la purga, 0 sesiones o tokens de correo fuera de su plazo de 30 días, 0 `usado_utc` sin truncar con más de 2 min y 0 horas de sesión fuera de la hora en punto (D3, RNF-10).
- **SC-008**: El 6.º registro desde una misma IP en 1 h responde 429 en el 100 % de los casos; el 5.º se acepta (D8, T-QA18).

## Fuera de alcance

- Cambiar el correo de la cuenta (derecho de **rectificación**) y oponerse a un tratamiento (**oposición**): docs solo exige acceso (RF-31) y cancelación (RF-30) en v1.0 (`10-glosario.md`, «Derechos ARCO»).
- El permiso local offline y su vigencia en el celular (RN-21 a, b, d): es de la app.
- Las páginas web que abren los enlaces de los correos: son de la feature del panel (Cloudflare Pages); aquí solo se define el contrato.
- Inicio de sesión con proveedores externos (Google, etc.): no está en docs.

## Assumptions

- Proveedor de correo con plan gratuito (RS-04); SPF, DKIM y DMARC configurados en `killauru.pe`.
- La verificación anti-bot del panel usa Cloudflare Turnstile (gratis); ver `research.md` R-008.
- Las decisiones marcadas **«Derivado del prototipo»** o **«Decisión de diseño SDD»** fueron **confirmadas por el autor el 2026-10-21** (D1, D8, D13; ver `research.md`) y se reflejan en `docs/`.
