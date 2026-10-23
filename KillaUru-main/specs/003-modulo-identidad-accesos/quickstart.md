# Quickstart: comprobar el módulo Identidad y Accesos

## Requisitos previos

001 y 002 funcionando; migración `V0003` aplicada; `EnviadorCorreoConsola` activo en desarrollo (los enlaces salen en el log, **solo en desarrollo**); el primer administrador creado con `docker compose run --rm api crear-admin <correo>`, que imprime el enlace de invitación solo en la consola (T-SEG14).

## 1. Pruebas automáticas

```powershell
cd backend
dotnet test tests/KillaUru.Identidad.Tests
dotnet test tests/KillaUru.Integracion.Tests --filter "FullyQualifiedName~Identidad"
```

## 2. Recorrido del conductor (app)

| Paso | Petición | Esperado |
|---|---|---|
| 1 | `GET /api/v1/legal/versiones` | 200 con versión `1.2` de ambos documentos |
| 2 | `POST /api/v1/auth/registro` con versiones `1.2` | 201 con `tokenAcceso` (5 min) y `tokenRenovacion`, `sesionVenceUtc` ≈ hoy + 30 días; en el log, el enlace de verificación |
| 3 | `POST /api/v1/auth/registro` sin aceptar | 400 `aceptacion_requerida` |
| 3b | 6 registros desde la misma IP en 1 h | los 5 primeros 201 (o 409); el 6.º → 429 con `Retry-After` (D8, T-QA18) |
| 3c | Decodificar `tokenAcceso` | *claims* `sub`, `rol`, `verificado = false`, `jti`; tras el paso 4 y una renovación, `verificado = true` (D8) |
| 4 | `POST /api/v1/auth/verificar-correo` con el token del log | 204; repetirlo → 400 `enlace_invalido` |
| 5 | `GET /api/v1/me` | 200 con correo, rol, alta, verificado y las 2 aceptaciones |
| 6 | `POST /api/v1/auth/renovar` dos veces con el **mismo** token en < 60 s | ambas 200 con el **mismo** `tokenRenovacion` |
| 7 | Repetir el paso 6 pasados 61 s (con `FakeTimeProvider` en la prueba) | 401 `sesion_invalida` y la familia revocada |
| 8 | 3 inicios de sesión fallidos | el 4.º, aun con la contraseña correcta, → 401 `credenciales_invalidas` (bloqueo de 5 min, oculto en la app) |
| 9 | `POST /api/v1/auth/recuperar` con un correo inexistente y con el propio | ambos 202 idénticos; solo el propio genera enlace |
| 10 | `POST /api/v1/auth/fijar-contrasena` | 204; el `tokenRenovacion` anterior → 401 `sesion_invalida` |
| 10b | Revisar `identidad.sesion_refresh` | `creado_utc`, `expira_utc` y `revocado_utc` en la hora en punto; `usado_utc` exacto (D3) |
| 11 | `DELETE /api/v1/me` con la contraseña | 204; en `identidad.usuario` no queda el correo; en `auditoria.registro` hay `CuentaEliminada` con el id y sin correo |

## 3. Recorrido del administrador (panel)

| Paso | Esperado |
|---|---|
| Login con `canal = panel` y Turnstile de prueba | 200, cookies `ku_acceso` y `ku_renovacion` `HttpOnly; Secure; SameSite=Strict`; `sesionVenceUtc` = ahora + 8 h; auditado `InicioSesionAdministrador` con IP enmascarada |
| 3 fallos | 423 `cuenta_bloqueada` con `bloqueadoHastaUtc` |
| `POST /api/v1/usuarios` | 201, estado `InvitacionPendiente`, enlace de invitación en el log |
| `POST /api/v1/usuarios/{id}/desactivar` | 204; la siguiente renovación de ese usuario → 401 `cuenta_desactivada`; su JWT deja de servir en ≤ 5 min |
| Desactivarse a sí mismo | 409 `accion_sobre_si_mismo` |
| `GET /api/v1/usuarios/revision-privilegios` → `POST` | `venceUtc` = día 1 del mes siguiente; auditado `RevisionPrivilegios` (y el `GET` como `AccesoAdministrativoDatos`, D15) |
| `GET /api/v1/usuarios` y `GET /api/v1/usuarios?ids=…` | 200; cada consulta deja un `AccesoAdministrativoDatos` (D15) |
| `GET /api/v1/usuarios` sin token / con token de conductor | 401 / 403 (T-SEG20) |

## 3b. Purga y restauración (D3, D9)

- Con `FakeTimeProvider`: sesión vencida hace 30 días → se conserva; hace 31 días → se borra; `usado_utc` de hace 1:59 min → exacto; de hace 2:01 min → truncado. La tarea registra `purga-identidad`.
- Restauración (en «killauru-pruebas»): aplicar `data-model.md` §1.2, rotar `KU_JWT_CLAVE_FIRMA`, arrancar → toda renovación responde 401 `sesion_invalida` y los JWT anteriores, 401.

## 4. Auditoría del esquema (EQ-04)

```sql
-- No debe existir ninguna columna con seudónimo, ubicación, evento o trayectoria en el esquema identidad
SELECT table_name, column_name FROM information_schema.columns
WHERE table_schema = 'identidad' AND column_name ~ '(seudonimo|evento|posicion|lat|lon|ubicacion)';
-- Esperado: 0 filas
```
