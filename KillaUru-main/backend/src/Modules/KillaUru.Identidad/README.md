# Identidad y Accesos

Implementa el spec 003: cuentas, consentimiento, credenciales, sesiones, correos y administración. No vincula usuarios con eventos ni trayectorias. La API valida JWT en el host de 001 y registra auditoría mediante contratos de 002.

## Diseño

Domain contiene políticas puras; Application coordina casos de uso y puertos; Infrastructure implementa EF, criptografía y servicios externos; Adapters traduce HTTP y consola. Tipos internos salvo módulo y contratos públicos, un tipo por archivo, reloj inyectado, mapeo explícito y comentarios de responsabilidad y decisiones.

`TransaccionIdentidad` une escritura y auditoría. Bloqueos por cuenta protegen login, canje y rotación; el bloqueo administrativo serializa cambios que podrían retirar al último administrador. Los correos se envían después del commit. Las lecturas administrativas también se auditan.

## API

Prefijo `/api/v1`. Cuerpos y errores completos: [identidad.openapi.yaml](../../../../specs/003-modulo-identidad-accesos/contracts/identidad.openapi.yaml).

| Ruta | Métodos | Acceso |
|---|---|---|
| `/legal/versiones` | GET | Público |
| `/auth/registro`, `/auth/inicio-sesion`, `/auth/renovar` | POST | Público; renovar exige refresh |
| `/auth/verificar-correo`, `/auth/recuperar`, `/auth/fijar-contrasena` | POST | Público; canje exige enlace válido |
| `/auth/cerrar-sesion` | POST | Autenticado |
| `/me` | GET / DELETE | Autenticado / conductor con contraseña |
| `/me/aceptaciones` | POST | Conductor |
| `/me/reenviar-verificacion` | POST | Autenticado |
| `/usuarios` | GET / POST | Administrador |
| `/usuarios/{id}/rol` | PUT | Administrador |
| `/usuarios/{id}/desactivar`, `/activar`, `/reenviar-correo` | POST | Administrador |
| `/usuarios/revision-privilegios` | GET / POST | Administrador |

Registro de ejemplo (obtener previamente las versiones vigentes):

```json
{"correo":"conductor@example.com","contrasena":"una-clave-larga","versionPoliticaAceptada":"1.2","versionTerminosAceptada":"1.2"}
```

Login de app:

```json
{"correo":"conductor@example.com","contrasena":"una-clave-larga","canal":"app"}
```

La app recibe `tokenAcceso`, `tokenRenovacion`, `expiraAccesoUtc`, `sesionVenceUtc` y `rol`. Usa `Authorization: Bearer <tokenAcceso>` y renueva con `{"tokenRenovacion":"..."}`. El panel usa `canal: "panel"` y `turnstileToken`; recibe cookies `HttpOnly; Secure; SameSite=Strict`. `ku_renovacion` solo se envía a `/api/v1/auth/renovar`; el cierre identifica la familia mediante el `jti` firmado y elimina ambas cookies en sus rutas originales.

JWT dura como máximo cinco minutos. El refresh del conductor extiende treinta días; el del administrador nunca supera ocho horas desde el login. La expiración persistida se trunca hacia abajo a la hora. La rotación admite reutilización durante sesenta segundos con el mismo sucesor; después revoca la familia. Solo se guardan hashes de refresh y enlaces.

Los errores usan ProblemDetails con `codigo` y correlación: `aceptacion_requerida` (400), `correo_en_uso` (409), `credenciales_invalidas` (401), `antibot_fallido`/`solo_administradores` (403), `cuenta_bloqueada` (423, panel), `sesion_invalida`/`cuenta_desactivada` (401), `enlace_invalido` (400), `confirmacion_invalida` (403), `accion_sobre_si_mismo`/`ultimo_administrador` (409). Límite de tasa: 429 y `Retry-After`.

## Operación y verificación

Aplicar V0003 después de V0001/V0002. Configurar firma JWT, clave HMAC distinta, Turnstile y correo según [secretos](../../../deploy/runbooks/secretos.md) y [correo](../../../deploy/runbooks/correo.md). El adaptador de consola no imprime destinatarios ni enlaces. `crear-admin <correo>` escribe el enlace exclusivamente en su salida; usar el override sin logs de Docker descrito en [desplegar](../../../deploy/runbooks/desplegar.md).

La purga arranca con el host y a las 03:45 de Lima, retiene treinta días y reduce precisión de `usado_utc` después de dos minutos. Ante restauración, revocar todas las sesiones y rotar firma JWT: [procedimiento](../../../deploy/runbooks/restaurar.md).

Desde `backend/`:

```powershell
dotnet test --project tests/KillaUru.Identidad.Tests
dotnet test --project tests/KillaUru.Integracion.Tests
dotnet test --project tests/KillaUru.Arquitectura.Tests
dotnet test --project tests/KillaUru.Integracion.Tests -- --filter-class '*RendimientoIdentidadTests' --explicit on
```

Integración requiere Docker. Rendimiento requiere k6 y usa cuentas sintéticas. Evidencia y pendientes: [revisión](../../../../specs/003-modulo-identidad-accesos/revision-implementacion.md).
