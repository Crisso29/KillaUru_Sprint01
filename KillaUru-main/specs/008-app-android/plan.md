# Implementation Plan: App Android KillaUru

**Branch**: `008-app-android` | **Date**: 2026-10-22 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/008-app-android/spec.md`

## Summary

App Kotlin *offline-first* con las tres capas de `enfoque-arquitectonico.md` §9 en **un solo módulo Gradle** organizado por funcionalidad (`diseño-interno` §7.1, R-001). Un servicio en primer plano lee el GPS a 1 Hz y, solo cuando las políticas del dominio lo permiten (RN-03, RN-18, RN-22, RF-29), los sensores a 50 Hz; el detector LiteRT clasifica ventanas de 500 ms y `GuardarEventoUseCase` deja cada evento `PENDING` en Room con hora GPS y UUIDv7. `SincronizacionWorker` (WorkManager, trabajo único) envía lotes de ≤ 50 de un solo seudónimo y aplica el contrato del lote de 005 con *backoff* y *jitter*; `VigiaRed` lo reanuda en ≤ 60 s. El mapa MapLibre lee un PMTiles local; la copia de anomalías se actualiza con `updatedSince` (registrado) o completa (anónimo); `ProximidadUseCase` alerta a lo largo de la vía con la polilínea con km. La cuenta consume 003. Scripts de `app/herramientas/` y el workflow `app-datos-r2.yml` publican en R2 los datos del corredor (D16).

## Technical Context

**Language/Version**: Kotlin 2.x (JVM 17), Android Gradle Plugin 8.x; `minSdk` 26, `targetSdk`/`compileSdk` 35

**Primary Dependencies**: Jetpack Compose (Material 3, Navigation), Room (KSP, `room-ktx`), WorkManager, Hilt, Retrofit + OkHttp + kotlinx.serialization, LiteRT (`com.google.ai.edge.litert`), MapLibre Native Android (PMTiles), DataStore (Preferences) + Android Keystore, `androidx.core` (`LocationManagerCompat`). Versiones fijas en `gradle/libs.versions.toml`

**Storage**: Room `killauru.db` (5 tablas, `data-model.md`); DataStore `preferencias` y `sesion` (cifrado); archivos verificados en `filesDir` (PMTiles, polilínea, polígono demo, modelos)

**Testing**: JUnit 4 + kotlin.test + `kotlinx-coroutines-test` (dominio); Robolectric + Room en memoria; `MigrationTestHelper` (instrumentada); MockWebServer validado contra los YAML de 003, 005, 006 y `cdn.openapi.yaml`; `work-testing`; Compose testing; Konsist; Kover; campo (R-029, R-030)

**Target Platform**: Android 8.0+ (API 26), celulares de gama media (≤ 4 GB RAM), APK fuera de Play Store (S-02)

**Performance Goals**: inferencia < 50 ms p95 (RNF-04); ≤ 5 %/h de batería con pantalla apagada, < 1 MB/h de datos, < 100 MB (RNF-15); reanudar la sincronización ≤ 60 s (RNF-33); proyección del GPS de costo casi constante a 1 Hz (`diseño-interno` §7.3)

**Constraints**: nada del flujo de detección, guardado, mapa o alertas necesita red; ningún `.kt`/`.kts`/`.py`/`.sh` > 100 líneas; reloj del sistema prohibido fuera de `RelojSistema`; sin migración destructiva; sin copia de seguridad de Android

**Scale/Scope**: ~24 pantallas; ≤ 85 000 eventos en el peor caso de 14 días; ~ miles de anomalías; 1 PMTiles ≤ 80 MB

## Constitution Check

La constitución 1.1.0 está escrita para el backend; donde no encaja se aplica su **intención** y se propone una enmienda (`research.md`, «Propuesta de enmienda a la constitución»).

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Cada FR cita HU/RF/RN/RNF/ADR o D-xx; lo que no está en docs es **Decisión SDD** en `research.md`; el pedido de módulos Gradle por capa no se aplica porque choca con §7.1 (R-001) | ✅ |
| II. Capas | Tres capas de `enfoque` §9 por funcionalidad; Konsist en cada build (FR-075). El principio habla del monolito .NET: ver propuesta de enmienda | ✅ (adaptado) |
| III. Calibrable | Umbrales en `assets/parametros.json` (`ParametrosApp`) y en el manifiesto del modelo; reloj por puerto `Reloj` (R-003) | ✅ (ver Complexity Tracking) |
| IV. Ningún dato se pierde ni se duplica | Estados locales con disparadores; `ENVIANDO` → `PENDING` al abrir; mismo `eventoId` al reintentar; migraciones probadas (R-006) | ✅ |
| V. Privacidad | Seudónimo aleatorio mensual; historial solo local y borrado al cerrar sesión; sin copia de seguridad; tokens cifrados | ✅ (SQLCipher pendiente del autor, R-028) |
| VI. El servidor decide | La app no recorta por su cuenta lo que el servidor no recortó; el permiso local solo habilita lo que el servidor ya entregó (`enfoque` §9 principio 3) | ✅ |
| VII. Pruebas primero | Cada regla del dominio con valores límite antes del código; contrato contra los YAML; CI de la app (FR-077) | ✅ (herramientas adaptadas) |
| VIII. Código pequeño | Árbol de abajo con archivos ≤ 100 líneas; `verificar_lineas.py` y Konsist; detekt `ForbiddenMethodCall` | ✅ (amplía la regla a `.kt`, `.kts`, `.py`) |
| IX. Contratos | La app no publica endpoints; consume los YAML y publica `cdn.openapi.yaml` para R2 | ✅ |
| X. Costo cero | R2 público (plan gratuito), GitHub Actions, Protomaps/OSM; sin servicios de Google Play | ✅ |

## Project Structure

### Documentation

```text
specs/008-app-android/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/cdn.openapi.yaml · contracts/contratos-internos.md
└── checklists/requirements.md · checklists/trazabilidad.md
```

### Source Code — árbol exacto (rutas relativas a `app/`; `K/` = `src/main/java/pe/killauru/app/`)

```text
settings.gradle.kts · build.gradle.kts · gradle.properties · gradle/libs.versions.toml   # proyecto único (R-001); tarea copiarContratos
config/detekt/detekt.yml                       # ForbiddenMethodCall (reloj, migración destructiva), LongMethod 20
proguard-rules.pro
schemas/pe.killauru.app.comun.datos.bd.KillaUruBd/1.json      # generado por Room, en git (R-006)
mediciones/informe-campo.md                    # RNF-04, RNF-15, EQ-01 (R-030)
herramientas/
├── verificar_lineas.py                        # ≤ 100 líneas en .kt/.kts/.py/.sh (FR-076)
├── corredor/ generar_corredor.py · verificar_datos.py · generar_manifiesto.py · km_referencia.csv · requisitos.txt
└── mapa/ region_corredor.py · generar_pmtiles.sh
src/main/AndroidManifest.xml                   # permisos RF-35, servicio location, allowBackup=false (R-027)
src/main/assets/ parametros.json · legal/textos.json · modelo/manifiesto.json · modelo/killauru-1.0.0.tflite · mapa/estilo.json · mapa/sprites/ · mapa/glifos/
src/main/res/ raw/alerta_bache.ogg · raw/alerta_hundimiento.ogg · raw/alerta_varios.ogg · values/strings.xml · xml/reglas_extraccion.xml
K/
├── KillaUruApp.kt                             # Hilt; WorkManager a demanda; IniciarApp (ENVIANDO→PENDING antes de programar)
├── MainActivity.kt
├── comun/
│   ├── dominio/ Reloj.kt · Coordenada.kt · Resultado.kt · ParametrosApp.kt · TiempoRelativo.kt · UuidV7.kt · Geodesia.kt
│   ├── datos/ RelojSistema.kt · RelojConfiableAndroid.kt · AnclaTiempoDataStore.kt · CargadorParametros.kt · PurgaWorker.kt
│   ├── datos/bd/ KillaUruBd.kt · Convertidores.kt · TriggersValidacion.kt · PurgaDao.kt · ContadorDao.kt · ContadorEntity.kt
│   ├── datos/bd/migraciones/ Migraciones.kt   # lista TODAS (vacía en v1) y regla de recrear disparadores
│   ├── datos/red/ ClienteHttp.kt · ProblemaDto.kt · RetryAfter.kt · InterceptorSeudonimo.kt
│   ├── datos/almacen/ CifradorKeystore.kt · SerializadorCifrado.kt · PreferenciasDataStore.kt
│   ├── datos/di/ BdModulo.kt · RedModulo.kt · RelojModulo.kt · ParametrosModulo.kt
│   └── ui/ tema/Tema.kt · tema/Colores.kt · tema/Tipografia.kt · navegacion/Rutas.kt · navegacion/NavGrafo.kt · componentes/SeveridadChip.kt · componentes/BarraInferior.kt
├── sesion/
│   ├── dominio/ Sesion.kt · PermisoLocal.kt · CategoriaVehiculo.kt · Consentimiento.kt · ValidadorContrasena.kt · ResultadoCuenta.kt
│   ├── dominio/ RepositorioSesion.kt · ApiCuenta.kt · RepositorioPreferencias.kt
│   ├── dominio/casos/ RegistrarCuentaUseCase.kt · IniciarSesionUseCase.kt · CerrarSesionUseCase.kt · EliminarCuentaUseCase.kt · LimpiezaLocalSesion.kt
│   ├── dominio/casos/ ObtenerMisDatosUseCase.kt · RevisarVersionesLegalesUseCase.kt · AceptarVersionesUseCase.kt · AplicarSesionInvalidaUseCase.kt
│   ├── datos/ IdentidadApi.kt · IdentidadDtos.kt · ApiCuentaHttp.kt · RepositorioSesionCifrado.kt · InterceptorToken.kt · RenovadorToken.kt
│   ├── datos/ RepositorioPreferenciasDataStore.kt · RevocacionPendienteWorker.kt · di/SesionModulo.kt
│   └── ui/ BienvenidaPantalla.kt · TerminosPantalla.kt · TerminosViewModel.kt · VehiculoPantalla.kt · PermisosPantalla.kt · PermisosViewModel.kt
│       ·   CuentaONoPantalla.kt · PrimerUsoViewModel.kt · RegistroPantalla.kt · RegistroViewModel.kt · VerificarCorreoPantalla.kt
│       ·   LoginPantalla.kt · LoginViewModel.kt · OlvideContrasenaPantalla.kt · PerfilPantalla.kt · PerfilViewModel.kt
│       ·   CerrarSesionPantalla.kt · EliminarCuentaPantalla.kt · CuentaViewModel.kt · AjustesPantalla.kt · AjustesViewModel.kt
├── seudonimo/ dominio/ PoliticaRotacion.kt · RepositorioSeudonimo.kt · ObtenerSeudonimoUseCase.kt · datos/ RepositorioSeudonimoDataStore.kt
├── deteccion/
│   ├── dominio/ Evento.kt · EstadoLocal.kt · TipoAnomalia.kt · TransicionEvento.kt · Clasificacion.kt · Ventana.kt · Prefiltro.kt · FusionVentanas.kt
│   ├── dominio/ PoliticaGuardado.kt · PoliticaVelocidad.kt · PoliticaBateria.kt · PoliticaDeteccion.kt · DetectorViaje.kt · Viaje.kt
│   ├── dominio/ IndicadorEstado.kt · CalcularIndicadorUseCase.kt · GuardarEventoUseCase.kt · ProcesarVentanaUseCase.kt
│   ├── dominio/ RepositorioEventos.kt · Clasificador.kt · Fuentes.kt          # puertos: FuenteSensores, FuenteUbicacion, FuenteBateria
│   ├── datos/ EventoEntity.kt · EventoDao.kt · RepositorioEventosRoom.kt · ClasificadorLiteRt.kt · NormalizadorEntrada.kt
│   ├── datos/ LectorSensores.kt · LectorGps.kt · LectorBateria.kt · BufferVentanas.kt · di/DeteccionModulo.kt
│   └── ui/ ServicioDeteccion.kt · OrquestadorServicio.kt · NotificacionEstado.kt · IndicadorEstadoChip.kt · EstadosPantalla.kt
├── sincronizacion/
│   ├── dominio/ Lote.kt · RespuestaLote.kt · InterpretarRespuestaLote.kt · PlanParticion.kt · PoliticaReintento.kt · ApiLotes.kt
│   ├── dominio/ SincronizarEventosUseCase.kt · EnviarLoteUseCase.kt · ExpirarPendientesUseCase.kt · RecuperarEnviandoUseCase.kt · PurgarLocalUseCase.kt
│   └── datos/ IngestaApi.kt · LoteDtos.kt · ClienteLotesHttp.kt · SincronizacionWorker.kt · ProgramadorSync.kt · VigiaRed.kt
│       ·      EstadoBackoffDataStore.kt · di/SincronizacionModulo.kt
├── mapa/
│   ├── dominio/ AnomaliaLocal.kt · EstadoCopia.kt · ZonaDemo.kt · RepositorioAnomaliasLocales.kt · FuenteAnomalias.kt
│   ├── dominio/ ActualizarAnomaliasUseCase.kt · RecortarAlDemoUseCase.kt · AvisoFueraDemo.kt
│   ├── datos/ AnomaliaEntity.kt · AnomaliaDao.kt · EstadoCopiaEntity.kt · EstadoCopiaDao.kt · MapaApi.kt · GeoJsonDtos.kt · MapeadorGeoJson.kt
│   ├── datos/ RepositorioAnomaliasRoom.kt · RepositorioAnomaliasHttp.kt · ActualizarAnomaliasWorker.kt · di/MapaModulo.kt
│   └── ui/ MapaPantalla.kt · MapaViewModel.kt · CapaAnomalias.kt · CapaZonaDemo.kt · DetallePantalla.kt · SinMapaPantalla.kt
│       ·   AvisoFueraDemoBanner.kt · ActualizadoHace.kt
├── corredor/ dominio/ Corredor.kt · PosicionEnVia.kt · Vertice.kt · datos/ CargadorCorredor.kt · CargadorZonaDemo.kt
├── alertas/
│   ├── dominio/ ParametrosAviso.kt · Alerta.kt · ProximidadUseCase.kt · ElegibilidadAlertas.kt · AlertaSonora.kt
│   ├── datos/ AlertaSonoraAndroid.kt · NotificacionAlerta.kt
│   └── ui/ AlertaPantalla.kt
├── historial/
│   ├── dominio/ Recorrido.kt · AcumuladorRecorrido.kt · RepositorioRecorridos.kt
│   ├── datos/ RecorridoEntity.kt · RecorridoDao.kt · RepositorioRecorridosRoom.kt
│   └── ui/ HistorialPantalla.kt · HistorialViewModel.kt
└── actualizador/
    ├── dominio/ ManifiestoMapa.kt · ManifiestoModelo.kt · PoliticaDescarga.kt · PoliticaActivacionModelo.kt · CatalogoArchivos.kt
    ├── dominio/ ActualizarMapaUseCase.kt · ActualizarModeloUseCase.kt
    ├── datos/ CdnApi.kt · ManifiestoDtos.kt · DescargaReanudable.kt · VerificadorSha256.kt · AlmacenArchivos.kt · CdnHttp.kt
    ├── datos/ DescargarMapaWorker.kt · ActualizarModeloWorker.kt · di/ActualizadorModulo.kt
    └── ui/ DescargaMapaPantalla.kt · DescargaViewModel.kt

src/test/java/pe/killauru/app/                 # mismos paquetes (dominio, Robolectric, contrato) + arquitectura/ + soporte/
src/test/resources/ contratos/ (copiados por copiarContratos) · fixtures/ (lotes, GeoJSON, manifiestos) · trayectorias/
src/androidTest/java/pe/killauru/app/          # migraciones, InferenciaBenchmarkTest, Compose en dispositivo

../.github/workflows/app-ci.yml                # cada PR que toque app/ (FR-077)
../.github/workflows/app-datos-r2.yml          # workflow_dispatch: publicar en R2 (FR-073)
```

**Structure Decision**: `diseño-interno` §7.1: un módulo, paquetes por funcionalidad con `dominio`/`datos`/`ui`. Se agregan `comun`, `seudonimo` y `corredor` porque C3 los tiene como componentes propios. El proyecto Gradle es `app/` mismo (sin `app/app/`), así todas las rutas de `tasks.md` son relativas a `app/`. Las pantallas del prototipo se reparten por funcionalidad; cada `ViewModel` usa solo casos de uso del dominio.

## Dependencias

- **Backend**: 003 (cuenta), 005 (lote), 006 (mapa) desplegados en el entorno de pruebas para las pruebas de punta a punta; las unitarias y de contrato usan MockWebServer con sus YAML.
- **Datos**: `backend/db/datos/corredor_pe28a.geojson` (004 T002) y `backend/db/datos/tramo_demo.geojson` (006 T002) antes de US8.
- **Modelo**: un `.tflite` inicial y su manifiesto de 009 antes de cerrar US2; mientras tanto, un modelo de prueba con la misma forma.
- **Infraestructura**: *bucket* R2 público en `cdn.killauru.pe` y secretos `KU_R2_ACCESS_KEY_ID`, `KU_R2_SECRET_ACCESS_KEY`, `KU_R2_ENDPOINT` en GitHub (RNF-34).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| `estado_copia` en Room y no en DataStore (`diseño-interno` §7.2 pone «última sincronización» en DataStore) | El `generadoUtc` debe guardarse en la **misma** transacción que las altas y bajas; si no, un corte deja la copia y la marca desalineadas | DataStore no participa en transacciones de SQLite (R-011) |
| Columnas `ultimaDeteccionUtc` y `enDemo` en `anomalia`; `kmRecorridos` en `recorrido` | RF-09 (tiempo relativo), RN-21 c/d (recorte) y totales del prototipo | Recortar por geometría al cerrar sesión, o no mostrar el último reporte, incumple docs (BD-A2) |
| Disparadores en lugar de `CHECK` | Room no admite `CHECK` en `@Entity` | Sin validación en la BD, un error de código guarda estados imposibles (BD-A5) |
| Umbrales en un archivo del APK, no «sin recompilar» (III) | `enfoque` §5 los pone «en el APK»; cambiar el archivo no cambia código | Configuración remota: servicio nuevo, fuera de docs (X) |
| `ProximidadUseCase` usa el puerto `RepositorioAnomaliasLocales` y no `AnomaliaDao` como en Nivel4 §7 | Regla de dependencia de `enfoque` §9 | Room en el dominio rompe la regla (R-002) |
