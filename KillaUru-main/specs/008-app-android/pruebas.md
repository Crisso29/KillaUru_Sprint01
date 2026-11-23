# Pruebas 008 — App Android

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md) · [Cierre actual](cierre.md)

## Dónde buscar

| Tipo | Ubicación y propósito |
|---|---|
| Unitarias JVM | [Ejemplo: política de reintento](../../app/src/test/java/pe/killauru/app/sincronizacion/dominio/PoliticaReintentoTest.kt); inventario por funcionalidad al final |
| Integración local con Room | [Repositorio del mapa](../../app/src/test/java/pe/killauru/app/mapa/datos/RepositorioAnomaliasRoomTest.kt) |
| Contratos | [Lector de contratos](../../app/src/test/java/pe/killauru/app/contratos/ValidadorContratoTest.kt) [contrato de Ingesta](../../app/src/test/java/pe/killauru/app/sincronizacion/datos/ContratoIngestaTest.kt) y [contrato del mapa](../../app/src/test/java/pe/killauru/app/mapa/datos/ContratoMapaTest.kt) |
| Componentes UI en JVM | [Mapa](../../app/src/test/java/pe/killauru/app/mapa/ui/MapaUiTest.kt) |
| Instrumentadas: dispositivo/emulador | `app/src/androidTest/`, pendiente de crear e implementar |
| Arquitectura | [Paquete arquitectura](../../app/src/test/java/pe/killauru/app/arquitectura/) |
| Datos de prueba | Fixtures de cada funcionalidad; `src/test/resources/` aún no existe |
| Herramientas del repositorio | [herramientas](../../app/herramientas/) |
| Datos de la CDN (US8) | [test_datos_cdn.py](../../herramientas/cdn/test_datos_cdn.py): corredor, estilo, manifiesto y región generados desde el eje real y validados contra `cdn.openapi.yaml`; la verificación rechaza un tramo demo que no cubre la vía. Ejecutar desde `KillaUru-main`: `python -m unittest discover -s herramientas/cdn` (requiere `herramientas/cdn/requisitos.txt`) |

Robolectric prueba componentes Android en JVM. MockWebServer simula respuestas HTTP.
Ninguno demuestra por sí solo un flujo con el backend desplegado.
Clasificar seguridad por el comportamiento verificado: sesión, cifrado, permisos y privacidad.

## Cómo ejecutar

Los helpers de [soporte](../../app/src/test/java/pe/killauru/app/soporte/) no contienen pruebas ejecutables. La comprobación del
lector de contratos está enlazada en la tabla anterior y exige los cinco
archivos esperados. T017 sigue pendiente de validar sus ejemplos contra esquemas.
Las pruebas de mapa conservan sus casos; repositorio, fuente y reloj de memoria
viven en archivos separados junto al escenario que los utiliza.

Desde `KillaUru-main/app/`, con JDK/SDK configurados como indica el [README](../../app/README.md):

```powershell
.\gradlew.bat testDebugUnitTest
.\gradlew.bat detekt koverVerify
.\gradlew.bat connectedDebugAndroidTest
python -m unittest discover -s herramientas -p 'test_*.py'
```

La tercera orden requiere implementar la suite instrumentada y un emulador o dispositivo conectado; no acredita pruebas mientras no existan casos.
Conservar las rutas de Gradle y los paquetes por funcionalidad para localizar producción y test juntos.

## Qué falta acreditar

Consultar las casillas abiertas en [tasks.md](tasks.md): quedan pruebas instrumentadas,
sesión, sincronización, alertas y mediciones de campo.
La conciliación app → backend, batería, consumo de datos y latencia de inferencia en
celulares requieren evidencia real. No se validan reorganizando carpetas.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.

<details>
<summary>JVM — actualizador (6 archivos)</summary>

- [ContratoCdnTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/datos/ContratoCdnTest.kt)
- [DescargaReanudableTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/datos/DescargaReanudableTest.kt)
- [PoliticaActivacionModeloTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/dominio/PoliticaActivacionModeloTest.kt)
- [PoliticaDescargaTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/dominio/PoliticaDescargaTest.kt)
- [DescargaViewModelTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/ui/DescargaViewModelTest.kt): paso 4: pide la descarga sin cortar otra, «Usar datos móviles» y «Descargar ahora» reprograman (sigue del parcial)
- [MapaSinConexionPantallaTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/ui/MapaSinConexionPantallaTest.kt): prototipo 05: peso en MB con coma, nota por estado (no solo la barra), interruptor y «Continuar» al terminar

</details>

<details>
<summary>JVM — alertas (3 archivos)</summary>

- [ElegibilidadAlertasTest.kt](../../app/src/test/java/pe/killauru/app/alertas/dominio/ElegibilidadAlertasTest.kt)
- [ParametrosAvisoTest.kt](../../app/src/test/java/pe/killauru/app/alertas/dominio/ParametrosAvisoTest.kt)
- [ProximidadUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/alertas/dominio/ProximidadUseCaseTest.kt)

</details>

<details>
<summary>JVM — arquitectura (5 archivos)</summary>

- [CapasTest.kt](../../app/src/test/java/pe/killauru/app/arquitectura/CapasTest.kt)
- [RelojProhibidoTest.kt](../../app/src/test/java/pe/killauru/app/arquitectura/RelojProhibidoTest.kt)
- [SinMigracionDestructivaTest.kt](../../app/src/test/java/pe/killauru/app/arquitectura/SinMigracionDestructivaTest.kt)
- [TamanoArchivosTest.kt](../../app/src/test/java/pe/killauru/app/arquitectura/TamanoArchivosTest.kt)
- [RedSeguraTest.kt](../../app/src/test/java/pe/killauru/app/arquitectura/RedSeguraTest.kt): la versión publicada no permite `http` y su URL es HTTPS fija; depuración solo permite `http` hacia `10.0.2.2` y `localhost` (API local en Docker)

</details>

<details>
<summary>JVM — comun (22 archivos)</summary>

- [ComponentesPrototipoTest.kt](../../app/src/test/java/pe/killauru/app/comun/ui/ComponentesPrototipoTest.kt): íconos del prototipo, encabezado con pasos y atrás, tarjeta y fila pulsables enteras
- [TextoConNegritasTest.kt](../../app/src/test/java/pe/killauru/app/comun/ui/TextoConNegritasTest.kt): `**negrita**` de los textos legales
- [TemaCoherenteConPanelTest.kt](../../app/src/test/java/pe/killauru/app/comun/ui/tema/TemaCoherenteConPanelTest.kt): la paleta de la app (claro, oscuro y ventana) es la de `panel/.../tokens.css`
- [PaletaSeveridadTest.kt](../../app/src/test/java/pe/killauru/app/comun/ui/tema/PaletaSeveridadTest.kt): color de cada severidad y de su número (prototipo `.sev`)
- [NavGrafoTest.kt](../../app/src/test/java/pe/killauru/app/comun/ui/NavGrafoTest.kt): T024/T126 — primer uso → mapa sin volver atrás, pestañas en 1 toque, detalle en 2 (sustituye a `ToquesAccionesPrincipalesTest`, que solo comprobaba un conjunto definido en la misma prueba)
- [NavGrafoCuentaTest.kt](../../app/src/test/java/pe/killauru/app/comun/ui/NavGrafoCuentaTest.kt): T024 parte 2 — crear cuenta / ya tengo cuenta desde el primer uso (sobre el mapa), registro → revisa tu correo con el correo intacto (`+`), login y olvidé desde Ajustes, perfil a 2 toques, cerrar sesión, cancelar y eliminar vuelven al sitio correcto, Mis viajes sin cuenta → registro. Comparte [PantallasDobles.kt](../../app/src/test/java/pe/killauru/app/comun/ui/PantallasDobles.kt) y [PruebaNavegacion.kt](../../app/src/test/java/pe/killauru/app/comun/ui/PruebaNavegacion.kt) con `NavGrafoTest`
- [CampoFormularioTest.kt](../../app/src/test/java/pe/killauru/app/comun/ui/CampoFormularioTest.kt): campo con el nombre de su etiqueta sin repetirlo a TalkBack, tocar la etiqueta enfoca el campo y la tecla ✓ envía (WCAG 1.3.1, 4.1.2)
- [SerializadorCifradoTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/almacen/SerializadorCifradoTest.kt)
- [CodigosEstablesTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/bd/CodigosEstablesTest.kt)
- [DisparadoresTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/bd/DisparadoresTest.kt)
- [EsquemaInicialTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/bd/EsquemaInicialTest.kt)
- [TamanoBaseTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/bd/TamanoBaseTest.kt)
- [CargadorParametrosTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/CargadorParametrosTest.kt)
- [RelojConfiableTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/RelojConfiableTest.kt)
- [AnclaTiempoTest.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/AnclaTiempoTest.kt)
- [GeodesiaTest.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/GeodesiaTest.kt)
- [TiempoRelativoTest.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/TiempoRelativoTest.kt)
- [UuidV7Test.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/UuidV7Test.kt)
- [IniciarAppTest.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/IniciarAppTest.kt): T072: al arrancar recupera envíos cortados antes de programar sync y purga; mapa solo tras el primer uso
- [FormatoNumeroTest.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/FormatoNumeroTest.kt): números del prototipo: coma decimal y punto de miles en cualquier idioma del celular

</details>

<details>
<summary>JVM — contratos (1 archivo)</summary>

- [ValidadorContratoTest.kt](../../app/src/test/java/pe/killauru/app/contratos/ValidadorContratoTest.kt)

</details>

<details>
<summary>JVM — corredor (3 archivos)</summary>

- [CorredorKmReferenciaTest.kt](../../app/src/test/java/pe/killauru/app/corredor/dominio/CorredorKmReferenciaTest.kt)
- [CorredorTest.kt](../../app/src/test/java/pe/killauru/app/corredor/dominio/CorredorTest.kt)
- [CargadoresMapaTest.kt](../../app/src/test/java/pe/killauru/app/corredor/datos/CargadoresMapaTest.kt): corredor `[lon, lat, km]` (esquema `Corredor`) y tramo demo leídos de los archivos descargados; sin mapa no se cae

</details>

<details>
<summary>JVM — deteccion (12 archivos)</summary>

- [ClasificadorLiteRtTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/datos/ClasificadorLiteRtTest.kt)
- [CalcularIndicadorUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/CalcularIndicadorUseCaseTest.kt)
- [DetectorViajeTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/DetectorViajeTest.kt)
- [FusionVentanasTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/FusionVentanasTest.kt)
- [GuardarEventoUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/GuardarEventoUseCaseTest.kt)
- [PoliticaBateriaTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/PoliticaBateriaTest.kt)
- [PoliticaDeteccionTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/PoliticaDeteccionTest.kt)
- [PoliticaGuardadoTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/PoliticaGuardadoTest.kt)
- [PoliticaVelocidadTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/PoliticaVelocidadTest.kt)
- [PrefiltroTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/PrefiltroTest.kt)
- [ProcesamientoVentanaTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/ProcesamientoVentanaTest.kt)
- [TransicionEventoTest.kt](../../app/src/test/java/pe/killauru/app/deteccion/dominio/TransicionEventoTest.kt)

</details>

<details>
<summary>JVM — historial (2 archivos)</summary>

- [TextosViajeTest.kt](../../app/src/test/java/pe/killauru/app/historial/ui/TextosViajeTest.kt): tramo por pueblos y duración de «Mis viajes»
- [AcumuladorRecorridoTest.kt](../../app/src/test/java/pe/killauru/app/historial/dominio/AcumuladorRecorridoTest.kt)

</details>

<details>
<summary>JVM — mapa (13 archivos)</summary>

- [ContratoMapaTest.kt](../../app/src/test/java/pe/killauru/app/mapa/datos/ContratoMapaTest.kt)
- [RepositorioAnomaliasRoomTest.kt](../../app/src/test/java/pe/killauru/app/mapa/datos/RepositorioAnomaliasRoomTest.kt)
- [ActualizacionesTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/ActualizacionesTest.kt)
- [ActualizarAnomaliasUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/ActualizarAnomaliasUseCaseTest.kt)
- [AvisoFueraDemoTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/AvisoFueraDemoTest.kt)
- [ZonaDemoTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/ZonaDemoTest.kt)
- [MapaUiTest.kt](../../app/src/test/java/pe/killauru/app/mapa/ui/MapaUiTest.kt)
- [ContratoManifiestoMapaTest.kt](../../app/src/test/java/pe/killauru/app/mapa/datos/ContratoManifiestoMapaTest.kt): T088: ejemplo del YAML, 4 archivos, ETag/304, sin red y sin credenciales hacia la CDN
- [ArchivosMapaLocalTest.kt](../../app/src/test/java/pe/killauru/app/mapa/datos/ArchivosMapaLocalTest.kt): activar deja la versión y borra la anterior; estado ilegible = sin mapa; tamaño y huella del manifiesto
- [EstiloMapaLocalTest.kt](../../app/src/test/java/pe/killauru/app/mapa/datos/EstiloMapaLocalTest.kt): T096: el estilo publicado pasa a leer el PMTiles del celular y los glifos del APK
- [ActualizarMapaUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/ActualizarMapaUseCaseTest.kt): FR-041…FR-044: activa solo con los 4 archivos íntegros, 304, misma versión, espera Wi-Fi, datos móviles, sin red
- [EnDemoAnomaliasTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/EnDemoAnomaliasTest.kt): FR-051: `enDemo` con el polígono descargado; lo del anónimo siempre es demo
- [MapaViewModelTest.kt](../../app/src/test/java/pe/killauru/app/mapa/ui/MapaViewModelTest.kt): sin mapa / con mapa, recarga al terminar una descarga y sin carrera entre cargas

</details>

<details>
<summary>JVM — sesion (21 archivos)</summary>

- [VersionesLegalesCoherentesTest.kt](../../app/src/test/java/pe/killauru/app/sesion/datos/VersionesLegalesCoherentesTest.kt): la versión legal del APK es la vigente del backend (estaba 1.0 contra 1.2) y nombra a los proveedores
- [TextoAceptacionTest.kt](../../app/src/test/java/pe/killauru/app/sesion/ui/TextoAceptacionTest.kt): «Aceptaste la v1.2 el 14 oct 2026» en hora de Lima
- [PrimerUsoPantallasTest.kt](../../app/src/test/java/pe/killauru/app/sesion/ui/PrimerUsoPantallasTest.kt): textos del prototipo, «Acepto y continúo» y filas pulsables enteras (términos y vehículo)
- [PrimerUsoViewModelTest.kt](../../app/src/test/java/pe/killauru/app/sesion/ui/PrimerUsoViewModelTest.kt): bienvenida solo la primera vez, versión nueva sin repetir vehículo (R-024), retoma en el vehículo
- [PermisosUbicacionTest.kt](../../app/src/test/java/pe/killauru/app/sesion/ui/PermisosUbicacionTest.kt): FR-006 — ubicación en dos pasos (precisa y luego «todo el tiempo»)
- [RedAutenticadaTest.kt](../../app/src/test/java/pe/killauru/app/sesion/datos/RedAutenticadaTest.kt): cadena real de OkHttp — token, rutas públicas, renovación única y sesión invalidada
- [ContratoIdentidadTest.kt](../../app/src/test/java/pe/killauru/app/sesion/datos/ContratoIdentidadTest.kt): registro, inicio de sesión, reenvío y eliminación contra `identidad.openapi.yaml`
- [ContratoCuentaTest.kt](../../app/src/test/java/pe/killauru/app/sesion/datos/ContratoCuentaTest.kt): T109 — `GET /me` con el esquema `MisDatos`, `recuperar` (202/400/429) y `cerrar-sesion` (Bearer + `RenovarSolicitud`; 204/401 terminan, 503 y sin red reintentan)
- [EnviarRevocacionPendienteTest.kt](../../app/src/test/java/pe/killauru/app/sesion/datos/EnviarRevocacionPendienteTest.kt): T107 / R-023 — la revocación pendiente renueva y cierra con los tokens nuevos; 401 la da por terminada; sin red conserva el token
- [CerrarSesionUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/CerrarSesionUseCaseTest.kt): T107 — cerrar sesión con red, sin red (revocación programada) y sin sesión; siempre borra tokens, preferencias e historial (FR-069)
- [CasosCuentaNuevosTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/casos/CasosCuentaNuevosTest.kt): HU-25 — un correo sin «@» no gasta una petición; FR-068 — mis datos
- [CuentaViewModelsTest.kt](../../app/src/test/java/pe/killauru/app/sesion/ui/CuentaViewModelsTest.kt): perfil con datos y reintento, recuperar sin revelar cuentas, eliminar con contraseña equivocada, login con mensaje genérico (RN-19), Ajustes con y sin cuenta, reenviar verificación una vez, pendientes al cerrar sesión, versión de privacidad en el registro
- [CuentaUiTest.kt](../../app/src/test/java/pe/killauru/app/sesion/ui/CuentaUiTest.kt): T110 — las 7 pantallas de cuenta del prototipo: consentimiento obligatorio, «Revisa tu correo» sin bloquear, error genérico, olvidé, perfil completo, «3 detecciones por enviar», eliminar con contraseña y «Entiendo»
- [EliminarCuentaUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/EliminarCuentaUseCaseTest.kt)
- [IniciarSesionUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/IniciarSesionUseCaseTest.kt)
- [RegistrarCuentaUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/RegistrarCuentaUseCaseTest.kt)
- [CicloSesionTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/CicloSesionTest.kt): consentimiento y sesión inválida (cerrar sesión pasó a `CerrarSesionUseCaseTest`)
- [ConsentimientoTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/ConsentimientoTest.kt)
- [PermisoLocalTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/PermisoLocalTest.kt)
- [RevisarVersionesLegalesUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/RevisarVersionesLegalesUseCaseTest.kt)
- [ValidadorContrasenaTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/ValidadorContrasenaTest.kt)

</details>

<details>
<summary>JVM — seudonimo (3 archivos)</summary>

- [InterceptorSeudonimoTest.kt](../../app/src/test/java/pe/killauru/app/seudonimo/datos/InterceptorSeudonimoTest.kt): solo en anónimas y sin pisar el del lote (D2)
- [ObtenerSeudonimoTest.kt](../../app/src/test/java/pe/killauru/app/seudonimo/dominio/ObtenerSeudonimoTest.kt)
- [PoliticaRotacionTest.kt](../../app/src/test/java/pe/killauru/app/seudonimo/dominio/PoliticaRotacionTest.kt)

</details>

<details>
<summary>JVM — sincronizacion (8 archivos)</summary>

- [ContadorRechazosRoomTest.kt](../../app/src/test/java/pe/killauru/app/sincronizacion/datos/ContadorRechazosRoomTest.kt): suma real en Room (FR-031)
- [ContratoIngestaTest.kt](../../app/src/test/java/pe/killauru/app/sincronizacion/datos/ContratoIngestaTest.kt): petición y respuestas contra `ingesta.openapi.yaml`
- [EnviarLoteUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/sincronizacion/dominio/EnviarLoteUseCaseTest.kt)
- [InterpretarRespuestaLoteTest.kt](../../app/src/test/java/pe/killauru/app/sincronizacion/dominio/InterpretarRespuestaLoteTest.kt)
- [MantenimientoEventosTest.kt](../../app/src/test/java/pe/killauru/app/sincronizacion/dominio/MantenimientoEventosTest.kt)
- [PlanParticionTest.kt](../../app/src/test/java/pe/killauru/app/sincronizacion/dominio/PlanParticionTest.kt)
- [PoliticaReintentoTest.kt](../../app/src/test/java/pe/killauru/app/sincronizacion/dominio/PoliticaReintentoTest.kt)
- [SincronizarEventosUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/sincronizacion/dominio/SincronizarEventosUseCaseTest.kt)

</details>

<details>
<summary>Herramientas del repositorio (1 archivo)</summary>

- [test_verificar_lineas.py](../../app/herramientas/test_verificar_lineas.py)

</details>
