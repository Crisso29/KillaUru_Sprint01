# App: fidelidad al prototipo

> La app real, pantalla por pantalla, junto al prototipo `referencia/prototipo/killauru-diseno.html`. [Volver a evidencias](../../README.md).

| Dato | Valor |
|---|---|
| Fecha | 2026-11-22 |
| Entorno | Emulador `pixel_7_-_api_35` (Android 15), APK de depuración instalado limpio |
| Pruebas automáticas | 302 JVM (Robolectric y JUnit), lint y `assembleDebug` en verde (ver [pruebas de la spec 008](../../../../../specs/008-app-android/pruebas.md)) |

## Pantallas

| Pantalla | Prototipo | Real |
|---|---|---|
| Ícono del lanzador y arranque («Señal») | — | [cajón](real-00-icono-en-cajon.png) · [arranque](real-00-arranque.png) |
| Bienvenida | [prototipo](prototipo-01-bienvenida.png) | [real](real-01-bienvenida.png) |
| Términos y privacidad (paso 1 de 5) | [prototipo](prototipo-02-terminos.png) | [real](real-02-terminos.png) |
| ¿Qué manejas? (paso 2 de 5) | [prototipo](prototipo-03-vehiculo.png) | [real](real-03-vehiculo.png) |
| Permisos (paso 3 de 5) | [prototipo](prototipo-04-permisos.png) | [real](real-04-permisos.png) |
| ¿Cuenta o sin cuenta? (paso 5 de 5) | [prototipo](prototipo-06-cuenta-o-no.png) | [real](real-06-cuenta-o-no.png) |
| Mis viajes | [prototipo](prototipo-14-historial.png) | [real](real-14-mis-viajes.png) |
| Ajustes | [prototipo](prototipo-15-ajustes.png) | [real](real-15-ajustes.png) |

## Qué se hizo

- **Marca:** ícono adaptativo y monocromo con el logo «Señal» (`herramientas/marca/generar_logo_android.py`).
- **Componentes del prototipo** (`comun/ui/`): `PantallaApp` (encabezado, «Paso N de 5», pie fijo), `TarjetaOpcion`, `Aviso`,
  `FilaAjuste`, `BotonPrincipal`, y los mismos íconos que el panel (`IconosKillaUru`, generados del prototipo).
- **Primer uso** con atrás en cada paso (`PrimerUsoViewModel.volver`) y el botón Atrás de Android.
- **Textos legales 1.2** del prototipo, iguales a la versión vigente del backend; antes el APK traía la 1.0 y el registro
  desde la app habría fallado con `aceptacion_requerida` (`VersionesLegalesCoherentesTest`).

## Lo que queda, con su motivo

- **Paso 4 «Descarga del mapa»**: depende de la CDN (siguiente paso); el indicador ya cuenta 5 pasos como el prototipo.
- **Mapa con estado, velocidad, hito y hoja «Bache en 1,2 km»**, **alertas a pantalla completa** y **estados de la detección**:
  dependen del mapa base de la CDN y de conectar la detección; se hacen con ese paso.
- **Cuenta** (crear, revisa tu correo, iniciar sesión, olvidé, perfil, cerrar sesión, eliminar): parte 2 de la navegación.
