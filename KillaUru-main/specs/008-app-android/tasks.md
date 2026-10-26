---

description: "Tareas de la app Android KillaUru"
---

# Tasks: App Android KillaUru

**Input**: `specs/008-app-android/` · **Requiere**: contratos de 003, 005 y 006 (para MockWebServer); 004 T002 y 006 T002 (datos del corredor, US8); un modelo inicial de 009 (US2, con un modelo de prueba mientras tanto)

**Tests**: obligatorias y **primero** (constitución VII): cada regla del dominio con valores límite y cada respuesta del contrato del lote antes del código.

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `app/`, salvo `.github/` (raíz del repositorio). `K/` = `src/main/java/pe/killauru/app/` · `KT/` = `src/test/java/pe/killauru/app/` · `KA/` = `src/androidTest/java/pe/killauru/app/`. **Ningún `.kt`, `.kts`, `.py` ni `.sh` > 199 líneas.**

## Phase 1: Setup

- [ ] T001 Proyecto Gradle único (`com.android.application` en la raíz, `minSdk` 26, Compose, KSP, Hilt, Room con `schemaDirectory`, Kover, detekt, `allWarningsAsErrors`) y tarea `copiarContratos` (copia `../specs/00{1,3,5,6}-*/contracts/*` y `../specs/008-app-android/contracts/cdn.openapi.yaml` a `src/test/resources/contratos/`) — `settings.gradle.kts`, `build.gradle.kts`, `gradle.properties` · (FR-075, FR-078; R-001)
- [ ] T002 [P] Catálogo de versiones fijas (Compose, Room, WorkManager, Hilt, Retrofit, OkHttp, kotlinx.serialization, LiteRT, MapLibre, DataStore, Robolectric, MockWebServer, Konsist, Kover, swagger-parser, json-schema-validator) — `gradle/libs.versions.toml` · (FR-075; R-029)
- [ ] T003 [P] detekt: `ForbiddenMethodCall` (`System.currentTimeMillis`, `Instant.now`, `Clock.systemUTC`, `Date()`, `LocalDateTime.now`, `fallbackToDestructiveMigration*`), `LongMethod` 20, `LargeClass` — `config/detekt/detekt.yml` · (FR-024, FR-026, FR-076)
- [ ] T004 [P] `AndroidManifest.xml`: permisos de RF-35 (`ACCESS_FINE_LOCATION`, `ACCESS_BACKGROUND_LOCATION`, `POST_NOTIFICATIONS`, `REQUEST_IGNORE_BATTERY_OPTIMIZATIONS`, `FOREGROUND_SERVICE`, `FOREGROUND_SERVICE_LOCATION`, `INTERNET`, `ACCESS_NETWORK_STATE`), servicio `location`, `allowBackup="false"`, `fullBackupContent="false"`, `dataExtractionRules` — `src/main/AndroidManifest.xml`, `src/main/res/xml/reglas_extraccion.xml` · (FR-006, FR-009, FR-027; R-027)
- [ ] T005 [P] Script del límite de 200 líneas (`.kt`, `.kts`, `.py`, `.sh`; excluye `build/`, `schemas/`, generados) que falla con la lista — `herramientas/verificar_lineas.py` · (FR-076)
- [ ] T006 [P] Workflow de CI de la app (`pull_request` con `paths: app/**`): `assembleDebug` → `lint` → `detekt` → `testDebugUnitTest` (dominio, Robolectric, contrato, Konsist) → `koverVerify` (≥ 90 % de `*.dominio.*`) → `verificar_lineas.py`; si falla no se une a `main` — `../.github/workflows/app-ci.yml` · (FR-077)
- [ ] T007 [P] Pruebas de arquitectura con Konsist: `CapasTest` (dominio sin `android.*`/`androidx.*`/Room/Retrofit/LiteRT; `ui` ↛ `datos`; `datos` ↛ `ui`), `TamanoArchivosTest` (< 200 líneas), `RelojProhibidoTest`, `SinMigracionDestructivaTest` — `KT/arquitectura/` · (FR-075, FR-076, FR-024; R-002)

## Phase 2: Foundational — dominio común, Room, DataStore y red

### Pruebas (primero; deben fallar)

- [ ] T008 [P] `UuidV7Test`: versión 7, variante RFC 9562, 36 caracteres, orden por hora, 10 000 sin repetir — `KT/comun/dominio/UuidV7Test.kt` · (FR-013; R-004)
- [ ] T009 [P] `RelojConfiableTest` (`RelojFalso`): ancla GPS + monotónico; ancla del servidor; reinicio (otro `bootCount`) → `null`; mover el reloj de pared no cambia nada — `KT/comun/datos/RelojConfiableTest.kt` · (FR-026; R-003)
- [ ] T010 [P] `GeodesiaTest`: haversine con radio 6 371 008,8 m (1° de meridiano ≈ 111 195 m); proyección equirectangular en un segmento — `KT/comun/dominio/GeodesiaTest.kt` · (FR-055, FR-074; R-016)
- [ ] T011 [P] `TiempoRelativoTest` (UTC−5): 59 s «hace un momento»; 1 min; 59 min; 2 h «hace 2 horas»; ayer 15:30 «ayer a las 15:30»; anteayer «el 18 oct a las 09:05» — `KT/comun/dominio/TiempoRelativoTest.kt` · (FR-047; RNF-14, R-031)
- [ ] T012 [P] `CargadorParametrosTest`: lee `parametros.json`; rechaza `ConfidenceMinimo` 0 o 1.1, `BackoffSegundos` no creciente y `MaxEventosPorLote` 51 — `KT/comun/datos/CargadorParametrosTest.kt` · (FR-010, FR-029; III)
- [ ] T013 [P] `EsquemaInicialTest` (Robolectric, Room en memoria): 5 tablas, índices de `data-model.md` §1, `PRAGMA journal_mode = wal` en el archivo real — `KT/comun/datos/bd/EsquemaInicialTest.kt` · (FR-019)
- [ ] T014 [P] `DisparadoresTest`: insertar evento con estado ≠ 0, tipo 3, severidad 0/6, confidence 0.74, lat 91 → `evento_invalido`; transiciones prohibidas (2→0, 3→0, 4→1, 0→2, 0→3) → `transicion_invalida`; cambiar `eventoId` → `evento_inmutable`; `anomalia` con `persistente` 2; `estado_copia` id 2 — `KT/comun/datos/bd/DisparadoresTest.kt` · (FR-019, FR-020; R-005)
- [ ] T015 [P] `CodigosEstablesTest`: códigos fijos de `EstadoLocal`, `TipoAnomalia`, `Alcance` (`data-model.md` §4) — `KT/comun/datos/bd/CodigosEstablesTest.kt` · (FR-019; BD-A5)
- [ ] T016 [P] `SerializadorCifradoTest` (Robolectric): ida y vuelta; un byte alterado → error, no datos falsos — `KT/comun/datos/almacen/SerializadorCifradoTest.kt` · (FR-062; R-022)
- [ ] T017 [P] `ValidadorContrato` (soporte de pruebas): carga los YAML copiados con `swagger-parser` y valida peticiones y respuestas de MockWebServer con `json-schema-validator`; `ValidadorContratoTest` valida los `example` de los 4 YAML — `KT/soporte/ValidadorContrato.kt`, `KT/soporte/ValidadorContratoTest.kt` · (FR-078)

### Implementación

- [ ] T018 [P] `Reloj`, `Coordenada`, `Resultado`, `ParametrosApp`, `TiempoRelativo`, `UuidV7`, `Geodesia` — `K/comun/dominio/` · (FR-013, FR-026, FR-047, FR-055)
- [ ] T019 [P] `RelojSistema` (único que lee el reloj del sistema), `RelojConfiableAndroid`, `AnclaTiempoDataStore`, `CargadorParametros` y `parametros.json` — `K/comun/datos/`, `src/main/assets/parametros.json` · (FR-026; R-003)
- [ ] T020 Entidades y DAO base: `EventoEntity`, `AnomaliaEntity`, `EstadoCopiaEntity`, `RecorridoEntity`, `ContadorEntity`; `KillaUruBd` (v1, `exportSchema`, WAL, `Callback.onCreate` → `TriggersValidacion.crear`), `Convertidores`, `PurgaDao`, `ContadorDao`, `Migraciones.TODAS` (vacía) — `K/comun/datos/bd/`, `K/deteccion/datos/EventoEntity.kt`, `K/mapa/datos/AnomaliaEntity.kt`, `K/mapa/datos/EstadoCopiaEntity.kt`, `K/historial/datos/RecorridoEntity.kt` · (FR-019, FR-020, FR-024; R-005, R-006)
- [ ] T021 Esquema `1.json` generado y versionado — `schemas/pe.killauru.app.comun.datos.bd.KillaUruBd/1.json` · (FR-024; BD-A1)
- [ ] T022 [P] `CifradorKeystore` (AES-256-GCM), `SerializadorCifrado`, `PreferenciasDataStore` — `K/comun/datos/almacen/` · (FR-062; R-022)
- [ ] T023 [P] `ClienteHttp` (OkHttp: TLS, *timeouts* 15/30 s), `ProblemaDto` (`codigo`), `RetryAfter`, `InterceptorSeudonimo` (solo sin `Authorization`) — `K/comun/datos/red/` · (FR-030, FR-040)
- [ ] T024 `KillaUruApp` (Hilt, WorkManager a demanda), `MainActivity`, módulos Hilt `BdModulo`, `RedModulo`, `RelojModulo`, `ParametrosModulo`; tema, colores, tipografía, `Rutas`, `NavGrafo`, `SeveridadChip`, `BarraInferior` — `K/KillaUruApp.kt`, `K/MainActivity.kt`, `K/comun/datos/di/`, `K/comun/ui/` · (FR-008)

**Checkpoint**: unitarias y Konsist en verde; Room crea el esquema v1 con disparadores.

---

## Phase 3: User Story 1 — Primer uso, consentimiento y ajustes (P1) 🎯 MVP

### Pruebas

- [ ] T025 [P] [US1] `ConsentimientoTest`: sin una de las dos casillas no se acepta; versión distinta a la vigente → pedir otra vez — `KT/sesion/dominio/ConsentimientoTest.kt` · (FR-001, FR-003)
- [ ] T026 [P] [US1] `RevisarVersionesLegalesUseCaseTest` (dobles): sin red usa la versión del APK; vigente ≠ aceptada → `PedirAceptacion`; registrado → `POST /me/aceptaciones` al aceptar — `KT/sesion/dominio/RevisarVersionesLegalesUseCaseTest.kt` · (FR-002, FR-003; R-024)
- [ ] T027 [P] [US1] `PrimerUsoUiTest` (Compose + Robolectric): botón inactivo hasta marcar las dos casillas; Liviano/Pesado/Moto con su texto; guía del soporte y del cargador; permiso negado → texto de lo que se pierde — `KT/sesion/ui/PrimerUsoUiTest.kt` · (FR-001, FR-004, FR-006)
- [ ] T028 [P] [US1] `AjustesUiTest`: «Solo ver el mapa» no cambia la categoría; filas de vehículo, modelo, mapa, perfil, términos y estado del servicio — `KT/sesion/ui/AjustesUiTest.kt` · (FR-005, FR-007)

### Implementación

- [ ] T029 [US1] `Consentimiento`, `CategoriaVehiculo`, `RepositorioPreferencias`; `RevisarVersionesLegalesUseCase`, `AceptarVersionesUseCase` — `K/sesion/dominio/`, `K/sesion/dominio/casos/` · (FR-002, FR-003, FR-004, FR-005)
- [ ] T030 [US1] `RepositorioPreferenciasDataStore`; textos legales del APK — `K/sesion/datos/RepositorioPreferenciasDataStore.kt`, `src/main/assets/legal/textos.json` · (FR-002, FR-003, FR-004, FR-005)
- [ ] T031 [US1] Pantallas del primer uso: `BienvenidaPantalla`, `TerminosPantalla` + `TerminosViewModel`, `VehiculoPantalla`, `PermisosPantalla` + `PermisosViewModel` (ubicación «todo el tiempo» en API 29+, notificaciones en 33+, batería), `CuentaONoPantalla`, `PrimerUsoViewModel` — `K/sesion/ui/` · (FR-001, FR-004, FR-006, FR-008)
- [ ] T032 [US1] `AjustesPantalla` + `AjustesViewModel` (enlace a la página de estado de UptimeRobot) — `K/sesion/ui/AjustesPantalla.kt`, `K/sesion/ui/AjustesViewModel.kt` · (FR-005, FR-007)

**Checkpoint**: el primer uso se completa sin cuenta y sin red.

---

## Phase 4: User Story 7 — Seudónimo rotativo (P1)

- [ ] T033 [P] [US7] `PoliticaRotacionTest`: 29 d 23 h 59 min → mismo; 30 d → nuevo; sin hora confiable → no rota; UUID v4 aleatorio, distinto en cada rotación — `KT/seudonimo/dominio/PoliticaRotacionTest.kt` · (FR-039; R-010)
- [ ] T034 [US7] `PoliticaRotacion`, `RepositorioSeudonimo`, `ObtenerSeudonimoUseCase`, `RepositorioSeudonimoDataStore` (`SecureRandom`) — `K/seudonimo/` · (FR-039)
- [ ] T035 [US7] `InterceptorSeudonimoTest` (MockWebServer): sin token lleva `X-KillaUru-Seudonimo`; con token no — `KT/comun/datos/red/InterceptorSeudonimoTest.kt` · (FR-040)

---

## Phase 5: User Story 2 — Detectar sin tocar el celular (P1)

### Pruebas

- [ ] T036 [P] [US2] `PoliticaVelocidadTest`: 6 km/h 9 s → no; 10 s → sí; 5,0 km/h → no; caída a 4 km/h apaga y vuelve a contar — `KT/deteccion/dominio/PoliticaVelocidadTest.kt` · (FR-010; RN-03)
- [ ] T037 [P] [US2] `PoliticaDeteccionTest`: tabla de verdad de categoría, «Solo ver el mapa», ahorro, permiso y velocidad — `KT/deteccion/dominio/PoliticaDeteccionTest.kt` · (FR-010, FR-005; RN-18)
- [ ] T038 [P] [US2] `PoliticaBateriaTest`: 14 % sin cargar → ahorro; 15 % → no; 5 % cargando → no — `KT/deteccion/dominio/PoliticaBateriaTest.kt` · (FR-017; RN-22)
- [ ] T039 [P] [US2] `PoliticaGuardadoTest`: 0.74 → no; 0.75 → sí; 0.76 → sí; `Normal` 0.99 → no — `KT/deteccion/dominio/PoliticaGuardadoTest.kt` · (FR-012; RN-04)
- [ ] T040 [P] [US2] `PrefiltroTest` y `FusionVentanasTest`: varianza bajo el umbral → no invoca el modelo; dos positivas a 500 ms → 1 evento (mayor confidence); a 1 001 ms → 2 — `KT/deteccion/dominio/` · (FR-011, FR-014; R-018)
- [ ] T041 [P] [US2] `GuardarEventoUseCaseTest`: hora = fix GPS + monotónico; fix de 2 001 ms → no guarda y marca «Sin GPS»; seudónimo vigente; `modeloVersion` del modelo activo; estado `PENDING` — `KT/deteccion/dominio/GuardarEventoUseCaseTest.kt` · (FR-013, FR-015; R-003, R-018)
- [ ] T042 [P] [US2] `DetectorViajeTest`: inicio con RN-03; 29 min detenido sigue; 30 min termina; detener el servicio termina — `KT/deteccion/dominio/DetectorViajeTest.kt` · (FR-018; R-019)
- [ ] T043 [P] [US2] `CalcularIndicadorUseCaseTest`: prioridad de R-032 en los 7 estados y «N por enviar» — `KT/deteccion/dominio/CalcularIndicadorUseCaseTest.kt` · (FR-016; RF-36)
- [ ] T044 [P] [US2] `ClasificadorLiteRtTest` (Robolectric con el modelo de prueba): normalización del manifiesto; salida de 3 clases y 5 severidades; intérprete y *buffers* reutilizados (sin asignaciones por ventana) — `KT/deteccion/datos/ClasificadorLiteRtTest.kt` · (FR-011, FR-015)
- [ ] T045 [P] [US2] `EventoDaoTest` (Room en memoria): `guardar`, `pendientes()` como `Flow`, conteo del indicador — `KT/deteccion/datos/EventoDaoTest.kt` · (FR-019, FR-016)

### Implementación

- [ ] T046 [US2] `Evento`, `EstadoLocal`, `TipoAnomalia`, `TransicionEvento`, `Clasificacion`, `Ventana`, `Prefiltro`, `FusionVentanas` — `K/deteccion/dominio/` · (FR-011, FR-014, FR-020)
- [ ] T047 [US2] `PoliticaGuardado`, `PoliticaVelocidad`, `PoliticaBateria`, `PoliticaDeteccion`, `DetectorViaje`, `Viaje` — `K/deteccion/dominio/` · (FR-010, FR-012, FR-017, FR-018)
- [ ] T048 [US2] Puertos `RepositorioEventos`, `Clasificador`, `Fuentes`; `GuardarEventoUseCase`, `ProcesarVentanaUseCase`, `IndicadorEstado`, `CalcularIndicadorUseCase` — `K/deteccion/dominio/` · (FR-012, FR-013, FR-016)
- [ ] T049 [US2] `EventoDao` y `RepositorioEventosRoom` — `K/deteccion/datos/` · (FR-019)
- [ ] T050 [US2] `ClasificadorLiteRt` + `NormalizadorEntrada` (lee `assets/modelo/manifiesto.json`), modelo inicial en el APK — `K/deteccion/datos/`, `src/main/assets/modelo/` · (FR-011, FR-015, FR-071)
- [ ] T051 [US2] `LectorSensores` (50 Hz, `maxReportLatencyUs` 1 s, se registra y desregistra según `PoliticaDeteccion`), `LectorGps` (`LocationManagerCompat`, 1 Hz; 0,1 Hz tras fin de viaje), `LectorBateria`, `BufferVentanas`, `DeteccionModulo` — `K/deteccion/datos/` · (FR-009, FR-010, FR-017; R-020)
- [ ] T052 [US2] `ServicioDeteccion` (`location`, `START_STICKY`, botón «Detener»), `OrquestadorServicio`, `NotificacionEstado`, `IndicadorEstadoChip`, `EstadosPantalla` — `K/deteccion/ui/` · (FR-009, FR-016; R-020)
- [ ] T053 [US2] `ServicioDeteccionTest` (Robolectric, reproductor de sensores y GPS grabados): 10 min de ruta con 6 baches etiquetados → 6 eventos `PENDING`, 0 toques; sensores apagados con Pesado — `KT/deteccion/ui/ServicioDeteccionTest.kt` · (FR-008, FR-009, FR-010; HU-01, SC-007)

**Checkpoint**: la app detecta y guarda sin red.

---

## Phase 6: User Story 3 — Guardar sin señal y sincronizar (P1)

### Pruebas

- [ ] T054 [P] [US3] `TransicionEventoTest`: las 6 transiciones válidas y las prohibidas de `data-model.md` §3 — `KT/deteccion/dominio/TransicionEventoTest.kt` · (FR-020)
- [ ] T055 [P] [US3] `PoliticaReintentoTest` (aleatorio fijo): pasos 30/120/600/1 800/7 200 s; tope en 7 200 s; jitter −25 % (22,5 s) y +25 % (37,5 s) en el paso 0; `Retry-After` 120 → [120, 150] s; reinicio a 0 — `KT/sincronizacion/dominio/PoliticaReintentoTest.kt` · (FR-035, FR-036; RF-07)
- [ ] T056 [P] [US3] `InterpretarRespuestaLoteTest`: ids en las tres listas → `SYNCED`; `{id: null, indice: 7}` → `REJECTED_LOCAL` el de la posición 7; no mencionado → `PENDING`; id ajeno ignorado; contador por motivo — `KT/sincronizacion/dominio/InterpretarRespuestaLoteTest.kt` · (FR-031; D5)
- [ ] T057 [P] [US3] `PlanParticionTest`: 400 en 50 → 25 + 25 → … → solo el culpable `REJECTED_LOCAL`; 413 igual; un lote de 1 que da 400 → `REJECTED_LOCAL` — `KT/sincronizacion/dominio/PlanParticionTest.kt` · (FR-032, FR-033; R-026)
- [ ] T058 [P] [US3] `EnviarLoteUseCaseTest` (dobles): 401 → renovar y reenviar el mismo lote; `sesion_invalida` / `cuenta_desactivada` / 403 → `AplicarSesionInvalida` y reenviar sin token; 401 sin red al renovar → `PENDING`; nunca `REJECTED_LOCAL` por 401/403 — `KT/sincronizacion/dominio/EnviarLoteUseCaseTest.kt` · (FR-034; R-008)
- [ ] T059 [P] [US3] `SincronizarEventosUseCaseTest`: 120 pendientes → 50, 50, 20, más antiguos primero; dos seudónimos → lotes separados; 168 h exactas → `EXPIRADO_LOCAL` y contador; sin hora confiable no expira; «Solo ver el mapa» o ahorro → no envía — `KT/sincronizacion/dominio/SincronizarEventosUseCaseTest.kt` · (FR-022, FR-029, FR-037; D2)
- [ ] T060 [P] [US3] `EventoDaoLoteTest` (Room en memoria): `tomarLote` en una transacción, un solo seudónimo, orden `detectadoUtc, eventoId`; `recuperarEnviando`; `expirar` — `KT/deteccion/datos/EventoDaoLoteTest.kt` · (FR-021, FR-022, FR-029; BD-A6)
- [ ] T061 [P] [US3] `ContratoLoteTest` (MockWebServer + `ValidadorContrato` con `ingesta.openapi.yaml`): cuerpo sin `loteId`, un seudónimo, ≤ 50, cabecera solo sin token; respuestas 200/400/401/403/413/429/503 del YAML interpretadas — `KT/sincronizacion/datos/ContratoLoteTest.kt` · (FR-030, FR-031…FR-035, FR-078)
- [ ] T062 [P] [US3] `SincronizacionWorkerTest` (`work-testing`, `TestDriver`): restricción de red; tras 500 → nuevo trabajo con espera del paso; `VigiaRed.onAvailable` reemplaza la espera y corre en ≤ 60 s; trabajo detenido → `ENVIANDO` vuelve a `PENDING`; un solo `sync` — `KT/sincronizacion/datos/SincronizacionWorkerTest.kt` · (FR-028, FR-036, FR-038; R-009)
- [ ] T063 [P] [US3] `CorteDeRedTest` (MockWebServer `DISCONNECT_AFTER_REQUEST`, respuesta 200 perdida): reenvío con los mismos `eventoId`; el servidor simulado los cuenta una sola vez — `KT/sincronizacion/datos/CorteDeRedTest.kt` · (FR-031, FR-036; EQ-02, SC-003)
- [ ] T064 [P] [US3] `PurgaLocalTest` (Room + reloj falso): 7 días − 1 ms se conserva, 7 días se borra; `PENDING` y `ENVIANDO` nunca; recorrido de 6 meses + 1 día; anomalía vencida hace 7 días — `KT/comun/datos/PurgaLocalTest.kt` · (FR-023; BD-A4)
- [ ] T065 [P] [US3] `TamanoBaseTest` (Robolectric, archivo real): 85 000 eventos + 5 000 anomalías + 400 recorridos → `killauru.db` + `-wal` < 25 MB; 14 días sin red sin perder `PENDING` — `KT/comun/datos/bd/TamanoBaseTest.kt` · (FR-025; RNF-15, RNF-33, SC-002)
- [ ] T066 [P] [US3] `CadenaMigracionesTest` (instrumentada, `MigrationTestHelper`): con v1 sembrada (`PENDING`, `ENVIANDO`, recorridos) abre con `Migraciones.TODAS` y verifica filas y disparadores; falla si falta el JSON de la versión actual — `KA/comun/datos/bd/CadenaMigracionesTest.kt` · (FR-024; SC-008)

### Implementación

- [ ] T067 [US3] `Lote`, `RespuestaLote`, `InterpretarRespuestaLote`, `PlanParticion`, `PoliticaReintento`, puerto `ApiLotes` — `K/sincronizacion/dominio/` · (FR-031, FR-032, FR-033, FR-035, FR-036)
- [ ] T068 [US3] `SincronizarEventosUseCase`, `EnviarLoteUseCase`, `ExpirarPendientesUseCase`, `RecuperarEnviandoUseCase`, `PurgarLocalUseCase` — `K/sincronizacion/dominio/` · (FR-021, FR-022, FR-023, FR-029, FR-034, FR-037)
- [ ] T069 [US3] `EventoDao.tomarLote/expirar/recuperarEnviando/aplicar` con `@Transaction` — `K/deteccion/datos/EventoDao.kt`, `K/deteccion/datos/RepositorioEventosRoom.kt` · (FR-021, FR-022, FR-029, FR-031)
- [ ] T070 [US3] `IngestaApi`, `LoteDtos` (ISO UTC con `Z`), `ClienteLotesHttp` (mapea 200/400/401/403/413/429/503/5xx a `RespuestaLote`) — `K/sincronizacion/datos/` · (FR-030, FR-031…FR-036)
- [ ] T071 [US3] `SincronizacionWorker`, `ProgramadorSync` (`REPLACE` + `setInitialDelay`; `KEEP` al guardar un evento), `VigiaRed`, `EstadoBackoffDataStore`, `SincronizacionModulo` — `K/sincronizacion/datos/` · (FR-028, FR-036, FR-038; R-009)
- [ ] T072 [US3] `IniciarApp` en `KillaUruApp`: `RecuperarEnviandoUseCase` antes de inicializar WorkManager; `PurgaWorker` diario — `K/KillaUruApp.kt`, `K/comun/datos/PurgaWorker.kt` · (FR-021, FR-023)

**Checkpoint**: EQ-02 del lado de la app en verde con MockWebServer.

---

## Phase 7: User Story 8 — Publicar los datos del corredor en R2 (P1)

- [ ] T073 [P] [US8] Puntos de referencia: 20 coordenadas con su km esperado (Pisco km 0, km 268, km 335.073, curvas cerradas) — `herramientas/corredor/km_referencia.csv` · (FR-074; R-016)
- [ ] T074 [P] [US8] `generar_corredor.py`: lee `backend/db/datos/corredor_pe28a.geojson`, km acumulado con haversine (radio 6 371 008,8 m), escribe `corredor_pe28a.v<N>.json` con `fuenteSha256`; dependencias fijas — `herramientas/corredor/generar_corredor.py`, `herramientas/corredor/requisitos.txt` · (FR-072, FR-074)
- [ ] T075 [P] [US8] `verificar_datos.py`: km creciente; todos los vértices del km 268 al 335.073 dentro de `tramo_demo.geojson`; polígono cerrado ≤ 60 vértices; km de `km_referencia.csv` ≤ 1 m — `herramientas/corredor/verificar_datos.py` · (FR-072, FR-074; BD-19)
- [ ] T076 [P] [US8] `region_corredor.py` (franja de 5 km) y `generar_pmtiles.sh` (`pmtiles extract` de un *build* de Protomaps, `--maxzoom=14`, falla si > 80 MB) — `herramientas/mapa/` · (FR-072; R-015, RNF-15)
- [ ] T077 [P] [US8] `generar_manifiesto.py`: `mapa/manifiesto.json` con versión, `bytes`, `sha256` y atribución; valida contra `ManifiestoMapa` de `cdn.openapi.yaml` — `herramientas/corredor/generar_manifiesto.py` · (FR-072; R-014)
- [ ] T078 [US8] Workflow `workflow_dispatch` (entrada `version`, `ensayo: true/false`): instala requisitos → genera → verifica → sube archivos versionados con `Cache-Control: immutable` → sube el manifiesto al final (`max-age=300`), con `KU_R2_ACCESS_KEY_ID`, `KU_R2_SECRET_ACCESS_KEY`, `KU_R2_ENDPOINT` — `../.github/workflows/app-datos-r2.yml` · (FR-073; D16, RNF-34)
- [ ] T079 [US8] `CorredorKmReferenciaTest`: `Corredor.proyectar` sobre la polilínea generada y `km_referencia.csv` → diferencia ≤ 1 m — `KT/corredor/dominio/CorredorKmReferenciaTest.kt` · (FR-074; SC-012)

---

## Phase 8: User Story 4 — Mapa sin señal según el rol (P1)

### Pruebas

- [ ] T080 [P] [US4] `CorredorTest`: proyección en recta y en curva cerrada; búsqueda incremental desde el último segmento; sentido +1/−1; a 51 m de la vía → `null` — `KT/corredor/dominio/CorredorTest.kt` · (FR-055; RN-16)
- [ ] T081 [P] [US4] `ZonaDemoTest`: punto dentro, fuera y sobre el borde; polígono de 60 vértices — `KT/mapa/dominio/ZonaDemoTest.kt` · (FR-049, FR-051)
- [ ] T082 [P] [US4] `ActualizarAnomaliasUseCaseTest`: registrado sin copia → sin `updatedSince`; con copia → `updatedSince` = `generadoUtc`; bajas borradas; `venceUtc` null → baja; anónimo → completo con `If-None-Match`; 304 → conserva y actualiza `verificadoUtc`; 400 → descarga completa; al iniciar sesión → copia vaciada — `KT/mapa/dominio/ActualizarAnomaliasUseCaseTest.kt` · (FR-044, FR-045, FR-052; R-011)
- [ ] T083 [P] [US4] `AvisoFueraDemoTest`: primera vez fuera en el viaje → aviso; segunda → no; viaje nuevo → sí; registrado → nunca — `KT/mapa/dominio/AvisoFueraDemoTest.kt` · (FR-049; R-035)
- [ ] T084 [P] [US4] `RepositorioAnomaliasRoomTest`: altas, bajas y `estado_copia` en una transacción (falla a la mitad → nada cambia); `recortarAlDemo`; filtro `venceUtc > ahora` — `KT/mapa/datos/RepositorioAnomaliasRoomTest.kt` · (FR-044, FR-048, FR-051)
- [ ] T085 [P] [US4] `ContratoMapaTest` (MockWebServer + `mapa.openapi.yaml`): anónimo sin `bbox` ni `updatedSince` y con `X-KillaUru-Seudonimo`; registrado con `Authorization` y `updatedSince`; `AnomaliaFeature` y `BajaFeature` del YAML mapeadas — `KT/mapa/datos/ContratoMapaTest.kt` · (FR-044, FR-045, FR-078; SC-009)
- [ ] T086 [P] [US4] `DescargaReanudableTest` (MockWebServer con `Range`): corte al 62 % → reanuda con `Range: bytes=…-`; 416 → empieza de cero; SHA-256 incorrecto → borra y no activa; tamaño distinto → no activa — `KT/actualizador/datos/DescargaReanudableTest.kt` · (FR-041; SC-011)
- [ ] T087 [P] [US4] `PoliticaDescargaTest`: Wi-Fi → sí; datos móviles sin permiso → no; con «Usar datos móviles» → sí; versión igual → no; activación durante un viaje → al terminar — `KT/actualizador/dominio/PoliticaDescargaTest.kt` · (FR-041, FR-042; R-014)
- [ ] T088 [P] [US4] `ContratoCdnTest` (MockWebServer + `cdn.openapi.yaml`): manifiesto del mapa, 206 con `Content-Range` — `KT/actualizador/datos/ContratoCdnTest.kt` · (FR-041, FR-078)
- [ ] T089 [P] [US4] `MapaUiTest` (Compose): «Actualizado hace 12 min»; detalle con «permanente», «Último reporte hace 2 horas», km y distancia por la vía; anónimo con sombreado y aviso; sin mapa → «Aún no tienes el mapa» — `KT/mapa/ui/MapaUiTest.kt` · (FR-047, FR-049, FR-050, FR-053)

### Implementación

- [ ] T090 [US4] `Corredor`, `PosicionEnVia`, `Vertice`; `CargadorCorredor`, `CargadorZonaDemo` — `K/corredor/` · (FR-042, FR-055)
- [ ] T091 [US4] `AnomaliaLocal`, `EstadoCopia`, `ZonaDemo`, puertos `RepositorioAnomaliasLocales` y `FuenteAnomalias`, `ActualizarAnomaliasUseCase`, `RecortarAlDemoUseCase`, `AvisoFueraDemo` — `K/mapa/dominio/` · (FR-044, FR-045, FR-049, FR-051, FR-052)
- [ ] T092 [US4] `AnomaliaDao`, `EstadoCopiaDao`, `RepositorioAnomaliasRoom` (`enDemo` con `ZonaDemo`), `MapaApi`, `GeoJsonDtos`, `MapeadorGeoJson`, `RepositorioAnomaliasHttp`, `MapaModulo` — `K/mapa/datos/` · (FR-044, FR-045, FR-048, FR-051)
- [ ] T093 [US4] `ActualizarAnomaliasWorker` (periódico 15 min con red; al abrir y al volver la red) — `K/mapa/datos/ActualizarAnomaliasWorker.kt` · (FR-046; R-034)
- [ ] T094 [US4] Actualizador del mapa: `ManifiestoMapa`, `PoliticaDescarga`, `CatalogoArchivos`, `ActualizarMapaUseCase`; `CdnApi`, `ManifiestoDtos`, `DescargaReanudable`, `VerificadorSha256`, `AlmacenArchivos`, `CdnHttp`, `DescargarMapaWorker` (sin medición salvo permiso; `setForeground` con progreso), `ActualizadorModulo` — `K/actualizador/` · (FR-041, FR-042)
- [ ] T095 [US4] `DescargaMapaPantalla` + `DescargaViewModel` («Si se corta, sigue desde donde quedó») — `K/actualizador/ui/` · (FR-041, FR-008)
- [ ] T096 [US4] Estilo, *sprites* y glifos en el APK; `MapaPantalla`, `MapaViewModel`, `CapaAnomalias` (color por severidad), `CapaZonaDemo`, `AvisoFueraDemoBanner`, `ActualizadoHace`, `DetallePantalla`, `SinMapaPantalla`; atribución OSM visible — `src/main/assets/mapa/`, `K/mapa/ui/` · (FR-043, FR-047, FR-049, FR-050, FR-053)

**Checkpoint**: mapa y copia de anomalías funcionando en modo avión.

---

## Phase 9: User Story 5 — Alertas a lo largo de la vía (P1)

### Pruebas

- [ ] T097 [P] [US5] `ParametrosAvisoTest`: Liviano y Moto 8 s / 100 m; Pesado 12 s / 150 m; fin de pasada 500 m; a 30 km/h Liviano = 100 m (no 66,7); a 60 km/h = 133,3 m; Pesado 60 km/h = 200 m — `KT/alertas/dominio/ParametrosAvisoTest.kt` · (FR-056; RN-16)
- [ ] T098 [P] [US5] `ProximidadUseCaseTest` (reloj falso, corredor de prueba): solo por delante; una por pasada; 501 m → nueva pasada; 3 dentro → 1 alerta con cantidad y peor severidad; `venceUtc` ≤ hora del fix → no; fuera de la vía → no — `KT/alertas/dominio/ProximidadUseCaseTest.kt` · (FR-055, FR-056, FR-057, FR-048)
- [ ] T099 [P] [US5] `ElegibilidadAlertasTest`: anónimo → no; registrado con permiso → sí con Pesado, «Solo ver el mapa» y ahorro; permiso vencido a mitad de viaje → sí hasta la parada de 30 min — `KT/alertas/dominio/ElegibilidadAlertasTest.kt` · (FR-054, FR-059, FR-063)
- [ ] T100 [P] [US5] `ProximidadTrayectoriasTest` (fixtures de `src/test/resources/trayectorias/`: recta, curva de 80 m / 1 km, ida y vuelta, 30/60/90 km/h): alerta a 8 s ± 1 s y 12 s ± 1 s, respetando mínimos — `KT/alertas/dominio/ProximidadTrayectoriasTest.kt` · (FR-056, FR-057; SC-006)
- [ ] T101 [P] [US5] `AlertaSonoraAndroidTest` (Robolectric): foco `MAY_DUCK`, `USAGE_ALARM`, vibración, clip según tipo y cantidad, notificación de alta prioridad que se cierra sola — `KT/alertas/datos/AlertaSonoraAndroidTest.kt` · (FR-058; R-021)

### Implementación

- [ ] T102 [US5] `ParametrosAviso`, `Alerta`, `ElegibilidadAlertas`, puerto `AlertaSonora`, `ProximidadUseCase` (puerto `RepositorioAnomaliasLocales`) — `K/alertas/dominio/` · (FR-054…FR-057, FR-059)
- [ ] T103 [US5] `AlertaSonoraAndroid`, `NotificacionAlerta`, clips en español — `K/alertas/datos/`, `src/main/res/raw/` · (FR-058)
- [ ] T104 [US5] `AlertaPantalla` («Bache · 130 m · Frena…», «3 baches seguidos»); el servicio evalúa la proximidad con cada fix, también en pausa de detección — `K/alertas/ui/AlertaPantalla.kt`, `K/deteccion/ui/OrquestadorServicio.kt` · (FR-057, FR-058, FR-059)

**Checkpoint**: HU-09 verificada con trayectorias simuladas.

---

## Phase 10: User Story 6 — Mi cuenta desde la app (P1)

### Pruebas

- [ ] T105 [P] [US6] `ValidadorContrasenaTest`: 9 → error «Mínimo 10 caracteres»; 10 → ok; 128 → ok; 129 → error — `KT/sesion/dominio/ValidadorContrasenaTest.kt` · (FR-060; D13)
- [ ] T106 [P] [US6] `PermisoLocalTest`: vigente hasta `sesionVenceUtc`; vencido sin viaje → no; vencido en viaje → sí hasta 30 min detenido; sin hora confiable → usa la hora del fix — `KT/sesion/dominio/PermisoLocalTest.kt` · (FR-063; RN-21 a, b)
- [ ] T107 [P] [US6] `CasosCuentaTest` (dobles): registro 201/409/429; login 401 → mensaje genérico idéntico; rol `Administrador` → rechazo y cierre (R-033); cerrar sesión con y sin red (revocación pendiente); eliminar 204/403; `LimpiezaLocalSesion` borra tokens, permiso e historial, recorta al demo y deja `PENDING` para envío sin token — `KT/sesion/dominio/CasosCuentaTest.kt` · (FR-060, FR-061, FR-062, FR-066, FR-067; R-023)
- [ ] T108 [P] [US6] `RenovadorTokenTest` (MockWebServer): 401 → una sola renovación aunque haya 3 peticiones en paralelo; reintenta la petición; `cuenta_desactivada` → `AplicarSesionInvalidaUseCase` (permiso borrado y recorte al demo) — `KT/sesion/datos/RenovadorTokenTest.kt` · (FR-062, FR-063, FR-051; RN-21 c)
- [ ] T109 [P] [US6] `ContratoIdentidadTest` (MockWebServer + `identidad.openapi.yaml`): registro, inicio de sesión (`canal = app`), renovar, cerrar sesión, recuperar, `GET`/`DELETE /me`, aceptaciones, reenviar verificación y versiones legales — `KT/sesion/datos/ContratoIdentidadTest.kt` · (FR-002, FR-060…FR-068, FR-078)
- [ ] T110 [P] [US6] `CuentaUiTest` (Compose): registro, «Revisa tu correo» sin bloquear, login con mensaje genérico, olvidé, perfil con todos los campos, aviso de cerrar sesión (3 pendientes), eliminar con contraseña y casilla — `KT/sesion/ui/CuentaUiTest.kt` · (FR-060, FR-061, FR-064…FR-068)

### Implementación

- [ ] T111 [US6] `Sesion`, `PermisoLocal`, `ValidadorContrasena`, `ResultadoCuenta`, puertos `RepositorioSesion` y `ApiCuenta` — `K/sesion/dominio/` · (FR-060, FR-062, FR-063)
- [ ] T112 [US6] Casos de uso: `RegistrarCuentaUseCase`, `IniciarSesionUseCase`, `CerrarSesionUseCase`, `EliminarCuentaUseCase`, `LimpiezaLocalSesion`, `ObtenerMisDatosUseCase`, `AplicarSesionInvalidaUseCase` — `K/sesion/dominio/casos/` · (FR-051, FR-060…FR-068)
- [ ] T113 [US6] `IdentidadApi`, `IdentidadDtos`, `ApiCuentaHttp`, `RepositorioSesionCifrado`, `InterceptorToken`, `RenovadorToken`, `RevocacionPendienteWorker`, `SesionModulo` — `K/sesion/datos/` · (FR-062, FR-063, FR-066; R-022, R-023)
- [ ] T114 [US6] Pantallas: `RegistroPantalla` + `RegistroViewModel`, `VerificarCorreoPantalla`, `LoginPantalla` + `LoginViewModel`, `OlvideContrasenaPantalla`, `PerfilPantalla` + `PerfilViewModel`, `CerrarSesionPantalla`, `EliminarCuentaPantalla`, `CuentaViewModel` — `K/sesion/ui/` · (FR-060, FR-061, FR-064…FR-068)

**Checkpoint**: registro → corredor completo y alertas; cerrar sesión → demo, sin historial y pendientes enviados sin cuenta.

---

## Phase 11: User Story 10 — Mis viajes (P3, *Could*, recortable 2.º)

- [ ] T115 [P] [US10] `AcumuladorRecorridoTest`: viaje 335.073 → 268 con 7 eventos; ida y vuelta suma `kmRecorridos`; anónimo → no guarda; purga a 6 meses + 1 día — `KT/historial/dominio/AcumuladorRecorridoTest.kt` · (FR-069)
- [ ] T116 [US10] `Recorrido`, `AcumuladorRecorrido`, `RepositorioRecorridos`; `RecorridoDao`, `RepositorioRecorridosRoom` — `K/historial/dominio/`, `K/historial/datos/` · (FR-069)
- [ ] T117 [US10] `HistorialPantalla` + `HistorialViewModel` («Se guarda solo en este celular por 6 meses…», totales, lista; anónimo → explicación) — `K/historial/ui/` · (FR-069, FR-008)

---

## Phase 12: User Story 9 — Modelo nuevo sin reinstalar (P3, *Should*, recortable 3.º)

- [ ] T118 [P] [US9] `PoliticaActivacionModeloTest`: versión mayor y compatible → descarga; `versionMinimaApp` mayor → no; etiquetas distintas → no; `esquema` 2 → no; activar con viaje en curso → espera; carga fallida → vuelve al anterior — `KT/actualizador/dominio/PoliticaActivacionModeloTest.kt` · (FR-070, FR-071; R-017)
- [ ] T119 [US9] `ContratoManifiestoModeloTest` (MockWebServer + `ManifiestoModelo` de `cdn.openapi.yaml`, con el `assets/modelo/manifiesto.json` del APK) — `KT/actualizador/datos/ContratoManifiestoModeloTest.kt` · (FR-071, FR-078)
- [ ] T120 [US9] `ManifiestoModelo`, `PoliticaActivacionModelo`, `ActualizarModeloUseCase`, `ActualizarModeloWorker` (diario, `UNMETERED`), activación al terminar el viaje — `K/actualizador/dominio/`, `K/actualizador/datos/ActualizarModeloWorker.kt` · (FR-070, FR-071)

---

## Phase 13: Polish — campo y mediciones

- [ ] T121 Informe de campo (plantilla y resultados): dispositivos (3 de gama media), ruta Ayacucho–Vinchos, cronómetro y registro de cada medición de `research.md` R-030 — `mediciones/informe-campo.md` · (FR-025; SC-001, SC-004, SC-005)
- [ ] T122 `InferenciaBenchmarkTest` (instrumentada): 1 000 ventanas, p95 < 50 ms; correr en los 3 celulares y anotar en T121 — `KA/deteccion/InferenciaBenchmarkTest.kt` · (FR-011; RNF-04, SC-004)
- [ ] T123 Prueba de campo EQ-01: 5 h en modo avión (o reproductor de 5 h), conteo de SQLite antes y después; cronómetro de la reanudación ≤ 60 s; reloj del emulador +7 días · (FR-028, FR-025; SC-001, SC-002)
- [ ] T124 Batería, datos y almacenamiento (RNF-15) con la pantalla apagada en 3 celulares (`dumpsys batterystats`, `TrafficStats`) y ahorro de RN-22 · (FR-017, FR-025; SC-005)
- [ ] T125 Conciliación de punta a punta contra el entorno de pruebas (R-11): eventos enviados por la app = filas de `evento_recibido`; 0 duplicados · (FR-031; SC-003)
- [ ] T126 [P] `ToquesAccionesPrincipalesTest` (Compose): ver mapa, ver detalle, abrir historial, cambiar vehículo, cerrar sesión en ≤ 3 toques; 100 % de `strings.xml` en español — `KT/comun/ui/ToquesAccionesPrincipalesTest.kt` · (FR-008; RNF-12, SC-007)
- [ ] T127 Correr `app-datos-r2.yml` con `ensayo: true` y luego real; anotar versión y huellas publicadas · (FR-073)
- [ ] T128 Marcar `checklists/trazabilidad.md` y llevar a `research.md` las respuestas del autor a los pendientes · (FR-001…FR-078; constitución I)

## Dependencies & Execution Order

Phase 1 → Phase 2 → US1 → US7 → US2 → US3 → US8 → US4 → US5 → US6 → (US10, US9) → Polish. US6 puede avanzar en paralelo con US4/US5 usando una sesión falsa; US4 necesita los archivos de US8 (o los *fixtures*); US2 se cierra con el modelo de 009 (mientras tanto, un modelo de prueba de la misma forma). Recorte: US10 (HU-10) y luego US9 (RF-33), en el orden de `02-historias-de-usuario.md`.
