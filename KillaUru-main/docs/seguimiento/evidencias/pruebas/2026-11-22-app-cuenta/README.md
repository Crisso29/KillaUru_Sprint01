# App: pantallas de cuenta (navegación parte 2)

> Crear cuenta, revisa tu correo, iniciar sesión, olvidé, mi perfil, cerrar sesión y eliminar cuenta, junto al prototipo
> `referencia/prototipo/killauru-diseno.html` (pantallas 16–23). [Volver a evidencias](../../README.md).

| Dato | Valor |
|---|---|
| Fecha | 2026-11-22 |
| Entorno | Emulador `pixel_7_-_api_35` (Android 15), APK de depuración |
| API | **Sin API alcanzable** (Docker apagado; `api.killauru.pe` aún no desplegada). Por eso las capturas reales llegan hasta el error de red; el camino con API está cubierto por pruebas con MockWebServer |
| Pruebas automáticas | 350 JVM (Robolectric y JUnit), lint sin hallazgos, detekt sin hallazgos y `assembleDebug` en verde (ver [pruebas de la spec 008](../../../../../specs/008-app-android/pruebas.md)) |

## Pantallas

| Pantalla | Prototipo | Real (emulador) |
|---|---|---|
| ¿Cuenta o sin cuenta? con «Ya tengo cuenta» | — | [real](real-01-cuenta-o-no.png) |
| Crear cuenta (17) | [prototipo](prototipo-17-registro.png) | [vacío](real-02-registro.png) · [listo, con «(v1.2)»](real-10-registro-listo.png) · [sin red](real-11-registro-sin-red.png) |
| Iniciar sesión (19) | [prototipo](prototipo-19-login.png) | [con teclado: «Entrar» visible](real-05-login-teclado.png) · [sin red](real-06-login-sin-red.png) |
| Recuperar contraseña (21) | [prototipo](prototipo-21-olvide.png) | [real](real-07-olvide.png) · [sin red](real-08-olvide-sin-red.png) |
| Ajustes sin cuenta | — | [«Crear cuenta» e «Iniciar sesión»](real-03-ajustes-sin-cuenta.png) |
| Mis viajes sin cuenta | — | [«Crear cuenta gratis»](real-09-mis-viajes-sin-cuenta.png) |
| Revisa tu correo (18) | [prototipo](prototipo-18-verificar.png) | Necesita un registro con API: `CuentaUiTest` |
| Mi perfil (16) | [prototipo](prototipo-16-perfil.png) | Necesita sesión: `CuentaUiTest` (todos los campos), `NavGrafoCuentaTest` |
| Cerrar sesión (22) | [prototipo](prototipo-22-cerrar.png) | Necesita sesión: `CuentaUiTest` («3 detecciones por enviar»), `CerrarSesionUseCaseTest` |
| Eliminar mi cuenta (23) | [prototipo](prototipo-23-eliminar.png) | Necesita sesión: `CuentaUiTest` (contraseña + «Entiendo»), `CuentaViewModelsTest` |

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

- **Registro e inicio de sesión reales en el emulador**: requieren la API desplegada o local. Hoy la URL está fija en
  `RedModulo.BASE_URL` (`https://api.killauru.pe/`); para probar contra una API local hará falta que la variante de
  depuración pueda apuntar a otra URL.
- **Recortar el mapa al demo al cerrar sesión** (FR-051): depende de la CDN, siguiente paso.
- **Contrato de `POST /me/aceptaciones` y `GET /legal/versiones`** en la app (T109).
