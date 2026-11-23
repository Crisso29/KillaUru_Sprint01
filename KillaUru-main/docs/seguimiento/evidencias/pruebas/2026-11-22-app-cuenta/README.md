# App: pantallas de cuenta (navegación parte 2)

> Crear cuenta, revisa tu correo, iniciar sesión, olvidé, mi perfil, cerrar sesión y eliminar cuenta, junto al prototipo
> `referencia/prototipo/killauru-diseno.html` (pantallas 16–23). [Volver a evidencias](../../README.md).

| Dato | Valor |
|---|---|
| Fecha | 2026-11-22 |
| Entorno | Emulador `pixel_7_-_api_35` (Android 15), APK de depuración |
| API | Dos recorridos: **sin API** (errores de red) y **con la API local real** (`killauru-local` en Docker, `http://10.0.2.2:18080/`, APK de depuración con `-Pkillauru.apiUrlDepuracion`) |
| Pruebas automáticas | 353 JVM (Robolectric y JUnit), lint sin hallazgos, detekt sin hallazgos y `assembleDebug` en verde (ver [pruebas de la spec 008](../../../../../specs/008-app-android/pruebas.md)) |

## Pantallas

| Pantalla | Prototipo | Real (emulador) |
|---|---|---|
| ¿Cuenta o sin cuenta? con «Ya tengo cuenta» | — | [real](real-01-cuenta-o-no.png) |
| Crear cuenta (17) | [prototipo](prototipo-17-registro.png) | [vacío](real-02-registro.png) · [listo, con «(v1.2)»](real-10-registro-listo.png) · [sin red](real-11-registro-sin-red.png) |
| Iniciar sesión (19) | [prototipo](prototipo-19-login.png) | [con teclado: «Entrar» visible](real-05-login-teclado.png) · [sin red](real-06-login-sin-red.png) |
| Recuperar contraseña (21) | [prototipo](prototipo-21-olvide.png) | [real](real-07-olvide.png) · [sin red](real-08-olvide-sin-red.png) |
| Ajustes sin cuenta | — | [«Crear cuenta» e «Iniciar sesión»](real-03-ajustes-sin-cuenta.png) |
| Mis viajes sin cuenta | — | [«Crear cuenta gratis»](real-09-mis-viajes-sin-cuenta.png) |
| Revisa tu correo (18) | [prototipo](prototipo-18-verificar.png) | [con API](api-02-revisa-tu-correo.png) · [reenviado](api-03-correo-reenviado.png) |
| Mi perfil (16) | [prototipo](prototipo-16-perfil.png) | [datos reales de `GET /me`](api-05-perfil.png) · [Ajustes con cuenta](api-04-ajustes-con-cuenta.png) |
| Cerrar sesión (22) | [prototipo](prototipo-22-cerrar.png) | [aviso](api-06-cerrar-sesion.png) · [Ajustes después, sin cuenta](api-07-ajustes-tras-cerrar.png) |
| Eliminar mi cuenta (23) | [prototipo](prototipo-23-eliminar.png) | [pantalla](api-09-eliminar.png) · [contraseña equivocada (403)](api-10-eliminar-403.png) · [Ajustes después](api-11-ajustes-tras-eliminar.png) |
| Iniciar sesión con contraseña equivocada (API) | — | [401 → mensaje genérico](api-08-login-401.png) |

## Recorrido con la API local real (registro del servidor)

| Paso en la app | Petición | Respuesta |
|---|---|---|
| Crear cuenta | `POST /api/v1/auth/registro` | 201 |
| Reenviar el correo | `POST /api/v1/me/reenviar-verificacion` | 202 |
| Mi perfil | `GET /api/v1/me` | 200 |
| Cerrar sesión | `POST /api/v1/auth/cerrar-sesion` | 204 (revocada en el servidor) |
| Iniciar sesión con contraseña equivocada | `POST /api/v1/auth/inicio-sesion` | 401 → mensaje genérico |
| Iniciar sesión | `POST /api/v1/auth/inicio-sesion` | 200 |
| Eliminar con contraseña equivocada | `DELETE /api/v1/me` | 403 → «La contraseña no es correcta.» |
| Eliminar mi cuenta | `DELETE /api/v1/me` | 204 |
| Iniciar sesión después (con `curl`) | `POST /api/v1/auth/inicio-sesion` | 401: la cuenta ya no existe |

El correo de verificación **no sale** en el entorno local: la clave de Resend es de relleno y el proveedor responde 401;
la API lo maneja bien (202 y «disponible reenvío»). El enlace del correo se prueba en el entorno de pruebas.

Cómo repetirlo:

```
cd backend/deploy && docker compose --env-file .env.local -f docker-compose.local.yml up -d --build
cd ../../app && ./gradlew assembleDebug -Pkillauru.apiUrlDepuracion=http://10.0.2.2:18080/
```

`http` solo se permite en la variante de depuración y solo hacia `10.0.2.2` y `localhost`; la publicada es HTTPS fija
(`RedSeguraTest`).

## Fallos encontrados y corregidos (cada uno con su prueba)

- **El historial no se borraba al cerrar sesión** (FR-069), aunque el comentario de `LimpiezaLocalSesion` lo decía y la
  pantalla lo promete. `CerrarSesionUseCaseTest`.
- **Cerrar sesión no avisaba al servidor** (FR-066, RF-38) y la revocación pendiente nunca se encolaba; además habría
  enviado un token de acceso ya vencido (dura 5 min). Ahora renueva y luego cierra; el token pendiente se guarda cifrado.
  `CerrarSesionUseCaseTest`, `EnviarRevocacionPendienteTest`, `ContratoCuentaTest`.
- **El teclado tapaba el botón «Entrar»** (Android 15 dibuja de borde a borde) y la tecla ✓ no hacía nada:
  `imePadding` en `PantallaApp` y `alConfirmar` en `CampoFormulario`. [Captura](real-05-login-teclado.png), `CampoFormularioTest`.
- **TalkBack leía «Correo» dos veces** y tocar la etiqueta no enfocaba el campo (en la web sí, con `<label for>`).
  `CampoFormularioTest`.
- **Eliminar con contraseña equivocada no decía nada** (el ViewModel ignoraba el 403). `CuentaViewModelsTest`.
- **«Revisa tu correo» sin desplazamiento**: con letra grande el botón quedaba fuera (WCAG 1.4.10).
- **Cancelar una descarga del mapa no cancelaba la corrutina** (`DescargaReanudable` atrapaba `CancellationException`).

## Lo que queda, con su motivo

- **El enlace del correo de verificación y el de recuperación**: necesitan un proveedor de correo real (entorno de pruebas).
- **Recortar el mapa al demo al cerrar sesión** (FR-051): depende de la CDN, siguiente paso.
- **Contrato de `POST /me/aceptaciones` y `GET /legal/versiones`** en la app (T109).
