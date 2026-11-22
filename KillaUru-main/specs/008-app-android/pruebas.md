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
<summary>JVM — actualizador (4 archivos)</summary>

- [ContratoCdnTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/datos/ContratoCdnTest.kt)
- [DescargaReanudableTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/datos/DescargaReanudableTest.kt)
- [PoliticaActivacionModeloTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/dominio/PoliticaActivacionModeloTest.kt)
- [PoliticaDescargaTest.kt](../../app/src/test/java/pe/killauru/app/actualizador/dominio/PoliticaDescargaTest.kt)

</details>

<details>
<summary>JVM — alertas (3 archivos)</summary>

- [ElegibilidadAlertasTest.kt](../../app/src/test/java/pe/killauru/app/alertas/dominio/ElegibilidadAlertasTest.kt)
- [ParametrosAvisoTest.kt](../../app/src/test/java/pe/killauru/app/alertas/dominio/ParametrosAvisoTest.kt)
- [ProximidadUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/alertas/dominio/ProximidadUseCaseTest.kt)

</details>

<details>
<summary>JVM — arquitectura (4 archivos)</summary>

- [CapasTest.kt](../../app/src/test/java/pe/killauru/app/arquitectura/CapasTest.kt)
- [RelojProhibidoTest.kt](../../app/src/test/java/pe/killauru/app/arquitectura/RelojProhibidoTest.kt)
- [SinMigracionDestructivaTest.kt](../../app/src/test/java/pe/killauru/app/arquitectura/SinMigracionDestructivaTest.kt)
- [TamanoArchivosTest.kt](../../app/src/test/java/pe/killauru/app/arquitectura/TamanoArchivosTest.kt)

</details>

<details>
<summary>JVM — comun (13 archivos)</summary>

- [SerializadorCifradoTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/almacen/SerializadorCifradoTest.kt)
- [CodigosEstablesTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/bd/CodigosEstablesTest.kt)
- [DisparadoresTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/bd/DisparadoresTest.kt)
- [EsquemaInicialTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/bd/EsquemaInicialTest.kt)
- [TamanoBaseTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/bd/TamanoBaseTest.kt)
- [CargadorParametrosTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/CargadorParametrosTest.kt)
- [InterceptorSeudonimoTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/red/InterceptorSeudonimoTest.kt)
- [RelojConfiableTest.kt](../../app/src/test/java/pe/killauru/app/comun/datos/RelojConfiableTest.kt)
- [AnclaTiempoTest.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/AnclaTiempoTest.kt)
- [GeodesiaTest.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/GeodesiaTest.kt)
- [TiempoRelativoTest.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/TiempoRelativoTest.kt)
- [UuidV7Test.kt](../../app/src/test/java/pe/killauru/app/comun/dominio/UuidV7Test.kt)
- [ToquesAccionesPrincipalesTest.kt](../../app/src/test/java/pe/killauru/app/comun/ui/ToquesAccionesPrincipalesTest.kt)

</details>

<details>
<summary>JVM — contratos (1 archivo)</summary>

- [ValidadorContratoTest.kt](../../app/src/test/java/pe/killauru/app/contratos/ValidadorContratoTest.kt)

</details>

<details>
<summary>JVM — corredor (2 archivos)</summary>

- [CorredorKmReferenciaTest.kt](../../app/src/test/java/pe/killauru/app/corredor/dominio/CorredorKmReferenciaTest.kt)
- [CorredorTest.kt](../../app/src/test/java/pe/killauru/app/corredor/dominio/CorredorTest.kt)

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
<summary>JVM — historial (1 archivo)</summary>

- [AcumuladorRecorridoTest.kt](../../app/src/test/java/pe/killauru/app/historial/dominio/AcumuladorRecorridoTest.kt)

</details>

<details>
<summary>JVM — mapa (7 archivos)</summary>

- [ContratoMapaTest.kt](../../app/src/test/java/pe/killauru/app/mapa/datos/ContratoMapaTest.kt)
- [RepositorioAnomaliasRoomTest.kt](../../app/src/test/java/pe/killauru/app/mapa/datos/RepositorioAnomaliasRoomTest.kt)
- [ActualizacionesTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/ActualizacionesTest.kt)
- [ActualizarAnomaliasUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/ActualizarAnomaliasUseCaseTest.kt)
- [AvisoFueraDemoTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/AvisoFueraDemoTest.kt)
- [ZonaDemoTest.kt](../../app/src/test/java/pe/killauru/app/mapa/dominio/ZonaDemoTest.kt)
- [MapaUiTest.kt](../../app/src/test/java/pe/killauru/app/mapa/ui/MapaUiTest.kt)

</details>

<details>
<summary>JVM — sesion (6 archivos)</summary>

- [CasosCuentaTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/CasosCuentaTest.kt)
- [CicloSesionTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/CicloSesionTest.kt)
- [ConsentimientoTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/ConsentimientoTest.kt)
- [PermisoLocalTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/PermisoLocalTest.kt)
- [RevisarVersionesLegalesUseCaseTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/RevisarVersionesLegalesUseCaseTest.kt)
- [ValidadorContrasenaTest.kt](../../app/src/test/java/pe/killauru/app/sesion/dominio/ValidadorContrasenaTest.kt)

</details>

<details>
<summary>JVM — seudonimo (2 archivos)</summary>

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
