# Research: App Android KillaUru

Cada decisión dice qué se decidió, por qué, qué se descartó y de dónde sale. Lo que docs no decide va marcado **Decisión SDD**.

## R-001 · Un solo módulo Gradle, organizado por funcionalidad

- **Decision**: el proyecto Android vive en `app/` (raíz del repositorio) como **un solo proyecto Gradle** que aplica `com.android.application` en su propio `build.gradle.kts` (sin subcarpeta `app/app/`). El código se organiza en paquetes por funcionalidad, cada uno con `dominio`, `datos` y `ui`. La regla de capas se verifica con **Konsist** en las pruebas unitarias.
- **Rationale**: `diseño-interno-de-modulos.md` §7.1 lo decide: «Un solo módulo Gradle organizado por funcionalidad… Un solo módulo evita la complejidad de compilación de varios; la separación por paquetes y la regla de dependencia se revisan con Konsist». Una persona y 6–8 semanas (R-09, R-10).
- **Alternatives considered**: módulos Gradle por capa o por área (`:dominio`, `:datos`, `:ui`, o `:deteccion`, `:sincronizacion`…). Daría la regla de dependencia por el compilador, pero **contradice §7.1** (constitución I: gana docs). Queda en «Pendientes de confirmar por el autor».

## R-002 · Paquetes, capas y puertos

- **Decision**: paquete raíz `pe.killauru.app`. Funcionalidades: `comun`, `sesion`, `seudonimo`, `deteccion`, `sincronizacion`, `mapa`, `corredor`, `alertas`, `historial`, `actualizador` (las 7 de §7.1 más `comun`, `seudonimo` y `corredor`, que son componentes propios en C3). Reglas (Konsist): `dominio` solo usa Kotlin estándar, corrutinas y `comun.dominio` (nada de `android.*`, `androidx.*`, Room, Retrofit ni LiteRT); `ui` usa `dominio`, nunca `datos`; `datos` implementa los puertos de `dominio` y nunca usa `ui`; Hilt une todo en `*/datos/di/`.
- **Rationale**: `enfoque` §9: Presentación → Dominio ← Datos. En `Nivel4` §7 `ProximidadUseCase` usa `AnomaliaDao`; para respetar la regla de dependencia, el caso de uso usa el **puerto** `RepositorioAnomaliasLocales` (dominio) y `AnomaliaDao` vive en `mapa/datos`. Mismo comportamiento, misma consulta por rango de km.
- **Alternatives considered**: DAO de Room en el dominio (copia literal de Nivel4): ata el dominio a Room y rompe `enfoque` §9.

## R-003 · Reloj confiable (BD-A3) — Decisión SDD

- **Problema**: RF-03 desconfía del reloj del celular, pero la expiración de 168 h, `venceUtc`, la rotación del seudónimo, el permiso local y las purgas necesitan «ahora».
- **Decision**: puerto `Reloj` con `ahoraConfiable(): Instant?`. Fuentes, en orden: (1) último fix GPS (`Location.time`) + (`elapsedRealtimeNanos` actual − el del fix); (2) hora del servidor (`generadoUtc` o cabecera `Date` de la última respuesta 2xx/304) + tiempo monotónico transcurrido. El ancla `{utcMs, elapsedMs, bootCount, fuente}` se guarda en DataStore y vale mientras `Settings.Global.BOOT_COUNT` no cambie. Sin ancla válida, `ahoraConfiable()` es `null`: no se marca `EXPIRADO_LOCAL`, no se purga y no se rota el seudónimo hasta tenerla (el envío sigue: el servidor juzga). Las alertas usan la hora del **mismo fix** que se evalúa. `updatedSince` siempre es el `generadoUtc` del servidor (006 R-006).
- **Rationale**: cumple RF-03 y la propuesta de BD-A3; el reloj monotónico no cambia si el usuario mueve la hora.
- **Alternatives considered**: reloj de pared (lo prohíbe RF-03); NTP propio (red y complejidad).

## R-004 · `eventoId` UUIDv7 (BD-15) — Decisión SDD

- **Decision**: `UuidV7.de(detectadoUtc, aleatorio)` en el dominio (48 bits de milisegundos de la hora GPS + 74 bits de `SecureRandom`). Formato canónico de 36 caracteres en minúsculas.
- **Rationale**: RF-03 pide «UUID»; v7 sigue siendo UUID válido para el contrato (`format: uuid`) y ordena las inserciones en la llave primaria de `evento_recibido` (BD-15).
- **Alternatives considered**: `UUID.randomUUID()` v4 (inserciones aleatorias en una PK de millones de filas).

## R-005 · Room: códigos estables y validación con disparadores (BD-A5)

- **Decision**: enumeraciones guardadas como `INTEGER` con un `TypeConverter` de códigos fijos (`EstadoLocal` 0…4, `TipoAnomalia` 1…2, `Alcance` 0…1) y una prueba que fija cada mapeo. Como Room no admite `CHECK` en `@Entity`, la validación vive en **disparadores** `BEFORE INSERT/UPDATE` con `RAISE(ABORT)` creados en `RoomDatabase.Callback.onCreate` y en cada migración (`data-model.md` §2). Room no valida disparadores, así que no rompen su verificación de esquema.
- **Rationale**: renombrar una constante de Kotlin no deja filas ilegibles; un valor imposible nunca entra (equivalente a los `CHECK` del backend).
- **Alternatives considered**: enums por nombre (`TEXT`, frágil); `CHECK` escrito a mano en una tabla creada fuera de Room (Room la rechaza al validar).

## R-006 · Migraciones de Room sin perder pendientes (BD-A1)

- **Decision**: `@Database(version = 1, exportSchema = true)`, esquemas JSON en `app/schemas/` (plugin `androidx.room` con `schemaDirectory`) y en git. Cada cambio: versión N+1 + `Migration(N, N+1)` explícita en `comun/datos/bd/migraciones/MigracionNaN1.kt`, registrada en `Migraciones.TODAS`. Reglas: `evento` y `recorrido` nunca se borran ni se recrean sin copiar sus filas; `anomalia` y `estado_copia` son caché: una migración puede vaciarlas y poner `generadoUtc = NULL` (fuerza descarga completa). `fallbackToDestructiveMigration*` prohibido (regla detekt `ForbiddenMethodCall` + prueba Konsist). Pruebas con `MigrationTestHelper`: por cada par N→N+1 (sembrar `PENDING`, `ENVIANDO`, recorridos; migrar; comprobar filas y disparadores) y la cadena 1→última. Una prueba falla si falta el JSON de la versión actual.
- **Rationale**: RNF-06 y RNF-33 (7 días de pendientes) frente a una actualización del APK; propuesta de BD-A1.
- **Alternatives considered**: `AutoMigration` para todo (no sirve para cambios de tipo o de disparadores); migración destructiva (pierde `PENDING`).

## R-007 · Selección del lote: un seudónimo por lote (D2, BD-A6)

- **Decision**: `EventoDao.tomarLote(limite)` con `@Transaction`: (1) seudónimo del `PENDING` más antiguo; (2) hasta 50 `PENDING` de ese seudónimo por `detectadoUtc, eventoId`; (3) `UPDATE … SET estado = 1, estadoDesde = :ahora`. El trabajo único `sync` evita dos lotes a la vez. Al rotar, los eventos del seudónimo viejo salen primero porque son los más antiguos.
- **Rationale**: RF-06 («primero los más antiguos»), D2 (el servidor rechaza `SeudonimoInconsistente`), BD-A6 (tomar y marcar es una transacción).

## R-008 · Cómo se interpreta cada respuesta del lote

| Respuesta | Acción | Fuente |
|---|---|---|
| 200 | `SYNCED` a todo id nombrado; `REJECTED_LOCAL` por `indice` si `id: null`; el resto → `PENDING`; ids ajenos al lote se ignoran; contador por motivo | Contrato del lote, D5 |
| 400 / 413 | Se parte en mitades `[0, n/2)` y `[n/2, n)` (pila en memoria dentro de la misma ejecución); 1 evento que falla solo → `REJECTED_LOCAL`; los demás siguen su respuesta | RF-06, R-026 |
| 401 | Renovar (`RenovadorToken`, una vez); éxito → reenviar el mismo lote; `sesion_invalida` / `cuenta_desactivada` → RN-21 c y reenviar sin token; renovación sin red → lote a `PENDING` y *backoff* | Contrato del lote, RN-21 c |
| 403 | Igual que la sesión inválida: RN-21 c y reenviar sin token | `01-actores.md` §4, 005 R-004 |
| 429 / 503 | Lote → `PENDING`; espera = `Retry-After` × (1 + U[0; 0,25]); sin cabecera → paso de *backoff* | RF-07 |
| 5xx / *timeout* / red | Lote → `PENDING`; siguiente paso de *backoff* | RF-07 |

- **Decision SDD**: el 403 no está en la tabla del contrato; tratarlo como sesión inválida evita perder eventos si un rol que no sincroniza llegara a la app (R-033).

## R-009 · *Backoff* con WorkManager y reanudación ≤ 60 s

- **Decision**: `PoliticaReintento.espera(paso, aleatorio)` = base[paso] × (1 + U[−0,25; +0,25]), base = 30 s, 2 min, 10 min, 30 min, 2 h (tope). El paso vive en DataStore. Tras un error, `ProgramadorSync` encola `sync` con `ExistingWorkPolicy.REPLACE` y `setInitialDelay`. Un 200 o la vuelta de la red ponen el paso en 0. `VigiaRed` (`ConnectivityManager.registerDefaultNetworkCallback`, registrado en `KillaUruApp` y en el servicio) en `onAvailable`: si `sync` está `ENQUEUED` con espera, lo reemplaza sin espera; si está `RUNNING`, no hace nada. Un evento nuevo encola con `KEEP`.
- **Rationale**: `diseño-interno` §7.5 (el *backoff* de WorkManager no sigue esa secuencia); RF-04 (reiniciar el *backoff*); RNF-33 (≤ 60 s).

## R-010 · Seudónimo aleatorio mensual — Decisión SDD

- **Decision**: cada rotación genera un UUID v4 **nuevo** con `SecureRandom`; no hay un identificador de instalación fijo del que se derive. Se guarda `{seudonimo, desdeUtc}`; rota cuando `ahoraConfiable ≥ desdeUtc + 30 días`.
- **Rationale**: ADR-007 («UUID aleatorio, seudónimo rotativo»). Derivarlo de un id fijo (por ejemplo HMAC) haría enlazables los meses si el id se filtra.

## R-011 · Copia local de anomalías

- **Decision**: tabla `estado_copia` (una fila) con `alcance`, `generadoUtc`, `etag` y `verificadoUtc`, en Room y no en DataStore, para guardarla **en la misma transacción** que las altas y bajas (si la app muere a la mitad, nada queda a medias). Registrado: `updatedSince = generadoUtc` (null → descarga completa sin `updatedSince`). Anónimo: siempre completo (006 R-007) con `If-None-Match`; 200 → `DELETE` + `INSERT` en una transacción; 304 → solo `verificadoUtc` = cabecera `Date`. Al iniciar sesión, al cerrarla o al cambiar el polígono demo o la polilínea, la copia se vacía y se baja completa (R-012, R-016).
- **Rationale**: RF-14, RF-26, 006 R-006 y R-007; la tabla de `diseño-interno` §7.2 pone «última sincronización» en DataStore: se mueve a Room por atomicidad (**Decisión SDD**).

## R-012 · Recorte al tramo demo con `enDemo` (BD-A2)

- **Decision**: columna `enDemo` calculada al guardar cada anomalía con `ZonaDemo.contiene(lat, lon)` sobre el polígono descargado (el mismo que usa el servidor, D16). RN-21 c/d: `DELETE FROM anomalia WHERE enDemo = 0` y `estado_copia.alcance = ANONIMO`, `generadoUtc = NULL`.
- **Rationale**: opción propuesta en BD-A2; borrar es inmediato, sin recalcular geometría al cerrar sesión.

## R-013 · Caducidad local y purga de anomalías vencidas

- **Decision**: el mapa y las alertas filtran `venceUtc > ahoraConfiable` (RF-26). La purga diaria borra `venceUtc < ahora − 7 días` (BD-A4) para que la tabla no crezca con lo que el servidor nunca dio de baja porque la app no se actualizó.
- **Rationale**: RF-26 y RN-07; 7 días es el mismo plazo que los estados finales de los eventos (**Decisión SDD**).

## R-014 · Datos del corredor en R2 (D16)

- **Decision**: dominio `cdn.killauru.pe` (R2 público). Rutas: `mapa/manifiesto.json` (único punto de entrada, `Cache-Control: max-age=300`) y archivos **inmutables** con versión en el nombre (`Cache-Control: max-age=31536000, immutable`): `mapa/corredor-pe28a.v<N>.pmtiles`, `mapa/corredor_pe28a.v<N>.json` (polilínea con km), `mapa/tramo_demo.v<N>.geojson`, `mapa/estilo.v<N>.json` (para el panel, 010). El manifiesto lista cada archivo con `tipo`, `version`, `url`, `bytes`, `sha256` y la `atribucion` (`contracts/cdn.openapi.yaml`). Un archivo nuevo descargado durante un viaje se activa al terminar el viaje.
- **Rationale**: ADR-011 punto 3 (manifiesto igual al del modelo); RF-26; un solo manifiesto = una sola consulta. Los nombres versionados permiten caché larga y reanudar con `Range` sin que cambie el archivo a la mitad.
- **Alternatives considered**: un manifiesto por archivo (más peticiones); nombres fijos (la caché del borde serviría mezclas de versiones).

## R-015 · Cómo se genera el PMTiles y dónde viven estilo y glifos — Decisión SDD

- **Decision**: `herramientas/mapa/generar_pmtiles.sh` usa la CLI `pmtiles extract` sobre un *build* diario público de Protomaps (OpenStreetMap, ODbL) con la región = franja de 5 km alrededor de la polilínea (`region_corredor.py`), `--maxzoom=14`; falla si pasa de 80 MB (RNF-15). El estilo MapLibre, los *sprites* y los glifos (solo latino) van **dentro del APK** (`assets/mapa/`) para funcionar sin red; el workflow publica una copia del estilo para el panel.
- **Rationale**: ADR-011 (PMTiles en R2, MapLibre, atribución OSM, costo cero); los glifos en línea romperían el modo offline.
- **Alternatives considered**: Planetiler sobre un extracto de Perú (más pesado de correr en Actions); teselas raster (rompe RNF-15).

## R-016 · El mismo km en la app y en el servidor (D13, BD-19)

- **Decision**: `generar_corredor.py` calcula el km acumulado con **haversine** (radio 6 371 008,8 m, el mismo que debe usar `CorredorVia` de 004, R-001 de 004), km 0 en el primer vértice (Pisco). `Corredor.proyectar` usa la misma proyección local equirectangular por segmento. `herramientas/corredor/km_referencia.csv` (20 puntos: inicio, fin, curvas cerradas, km 268 y 335.073) guarda el km esperado; `CorredorKmReferenciaTest` exige diferencia ≤ 1 m. **Nota para 004**: su prueba de `CorredorVia.KmSobreVia` debe leer el mismo CSV.
- **Rationale**: el `kmVia` que llega del servidor y el km del vehículo se comparan para la alerta: si el algoritmo difiere, la alerta se corre (BD-19 punto 1).

## R-017 · Contrato del manifiesto del modelo, compartido con 009

- **Decision**: `cdn.killauru.pe/modelo/manifiesto.json` con `esquema = 1` y los campos de `contracts/cdn.openapi.yaml` (`ManifiestoModelo`): `version` (semver; mayor si cambia el catálogo, ADR-004 punto 4), `url`, `bytes`, `sha256`, `versionMinimaApp` (`versionCode`), `etiquetas` (orden de la salida de clase, debe ser `[Normal, Bache, Hundimiento]`), `entrada` (`frecuenciaHz`, `muestrasPorVentana`, `ejes`, `pasoMuestras`, `media[6]`, `desviacion[6]`, `umbralPrefiltro`), `salidas` (`clase`: índice del tensor de probabilidades; `severidad`: índice del tensor de 5 probabilidades 1…5), `matrizConfusionUrl`, `publicadoUtc`. El APK lleva el mismo archivo para su modelo inicial. **009 lo produce y lo publica; 008 lo consume y no lo cambia sin cambiar 009.**
- **Rationale**: ADR-004 puntos 1, 2 y 4; RF-33; la app no debe tener la forma del modelo escrita en el código.

## R-018 · Ventanas, prefiltro, fusión y fix — Decisión SDD

- **Decision**: ventana de `muestrasPorVentana` (25 a 50 Hz = 500 ms), paso `pasoMuestras`; la vertical se estima con un filtro pasa-bajos de la gravedad; el prefiltro deja pasar la ventana si la varianza vertical supera `umbralPrefiltro` (del manifiesto). Ventanas positivas a ≤ `FusionVentanasMs` (1 000 ms) de la anterior se funden (se queda la de mayor confidence). El fix usado es el más cercano en tiempo a la ventana y no más viejo que `AntiguedadMaximaFixMs` (2 s); sin él no se guarda y el indicador pasa a «Sin GPS».
- **Rationale**: `diseño-interno` §7.4 (prefiltro); un mismo golpe ocupa 1–2 ventanas y contarlo dos veces inflaría RN-15a; RF-03 exige coordenadas con su precisión.

## R-019 · Qué es un viaje — Decisión SDD

- **Decision**: `DetectorViaje` (dominio): empieza cuando se cumple RN-03 (> 5 km/h ≥ 10 s); termina tras `ParadaFinViajeMinutos` (30) con velocidad ≤ 5 km/h, o al detener el servicio. Lo usan: RN-21 b (el permiso no se corta durante un viaje), el aviso fuera del demo (una vez por viaje) y el historial (un recorrido por viaje).
- **Rationale**: RN-21 b define el fin del viaje con «se detiene más de 30 min»; usar la misma definición en los tres lugares evita incoherencias.

## R-020 · Ciclo de vida del servicio en primer plano — Decisión SDD

- **Decision**: `ServicioDeteccion` (tipo `location`) arranca al abrir la app con el primer uso completo y queda activo hasta que el usuario pulsa «Detener» en la notificación. GPS a 1 Hz mientras hay movimiento; tras la parada de fin de viaje, cada 10 s hasta volver a moverse. `START_STICKY`. No arranca solo al encender el celular.
- **Rationale**: HU-01 («la app está en ejecución»), RS-06, RNF-15; arrancar al encender exigiría iniciar un servicio de ubicación en segundo plano, que Android 12+ restringe.

## R-021 · Sonido de la alerta (RN-20) — Decisión SDD

- **Decision**: `AlertaSonoraAndroid` pide foco de audio `AUDIOFOCUS_GAIN_TRANSIENT_MAY_DUCK` (la música baja), reproduce con `AudioAttributes.USAGE_ALARM` (suena en modo vibración con el volumen de alarma) un tono y una frase grabada en español del APK («Bache adelante», «Hundimiento adelante», «Varios baches adelante»), vibra y publica una notificación de alta prioridad que se cierra sola. Sin TextToSpeech.
- **Rationale**: RN-20 (no hace falta mirar la pantalla); TTS depende de que el celular tenga voz en español descargada, que no se puede asegurar sin red.

## R-022 · Tokens cifrados y renovación

- **Decision**: clave AES-256-GCM en Android Keystore (`CifradorKeystore`); DataStore con un `Serializer` que cifra todo el contenido. `InterceptorToken` agrega `Authorization`; `RenovadorToken` (`okhttp3.Authenticator`) renueva con un `Mutex` (una renovación a la vez) y reintenta la petición una sola vez. Si la respuesta de la renovación se pierde, reenviar el mismo token de actualización dentro de 60 s devuelve el mismo par (ADR-009).
- **Rationale**: `enfoque` §9 («DataStore cifrado con Android Keystore»); `androidx.security:security-crypto` está obsoleto.

## R-023 · Cerrar sesión sin señal — Decisión SDD

- **Decision**: lo local de RN-21 d se hace al instante; si `POST /auth/cerrar-sesion` falla por red, el token de actualización queda en `revocacionPendiente` (DataStore cifrado) y `RevocacionPendienteWorker` (red conectada) lo envía; 204 o 401 lo borran.
- **Rationale**: RF-38 pide revocar en el servidor; RN-21 d no puede esperar a tener red.

## R-024 · Términos sin red y versión nueva durante un viaje

- **Decision**: el APK trae los textos y la versión vigente al compilar; el primer uso funciona sin red. Al abrir con red, si `GET /legal/versiones` trae otra versión, se pide aceptarla antes de mostrar el mapa o iniciar un **nuevo** viaje con detección; un viaje en curso del servicio no se corta (como RN-21 b).
- **Rationale**: HU-27/RF-37 piden aceptar antes de usar y volver a pedir si cambia; cortar la detección a mitad de la vía no protege a nadie. **Pendiente de confirmar por el autor** (suaviza HU-27 durante un viaje).

## R-025 · «Solo ver el mapa» y los eventos ya guardados

- **Decision**: con la opción activa, los eventos `PENDING` **no** se envían (RF-29: «apaga la detección y el envío»); si pasan 168 h, expiran (RF-06). Al desactivarla, se envían.
- **Rationale**: lectura literal de RF-29 y del estado «Envío desactivado por el usuario» (RF-36). **Pendiente de confirmar por el autor**: ¿la opción debe borrar los pendientes o enviarlos antes de apagarse?

## R-026 · Qué hace la app con el 413 (005 R-002, `no-aplicado-docs01.md`)

- **Decision**: igual que el 400: partir el lote; un evento que falla solo → `REJECTED_LOCAL`. Un lote de 50 pesa ~20 KB; el 413 (> 256 KB) solo aparece si un evento está corrupto (por ejemplo un `modeloVersion` enorme), y partir lo aísla.
- **Rationale**: no se pierde ningún evento bueno; reutiliza el camino del 400. **Pendiente de confirmar por el autor** y, si lo confirma, agregar el 413 al «Contrato de respuesta del lote» de `03-requisitos-funcionales.md`.

## R-027 · Sin copia de seguridad de Android — Decisión SDD

- **Decision**: `android:allowBackup="false"`, `dataExtractionRules` que excluye todo y `fullBackupContent="false"`.
- **Rationale**: la copia en la nube de Google llevaría posiciones, historial y tokens fuera del celular (RNF-10, RN-21 d «para que otra persona… no lo vea»); las claves de Keystore no se restauran, así que la sesión restaurada no serviría.

## R-028 · ¿Cifrar Room con SQLCipher? (BD-A7)

- **Decision**: v1.0 **sin** SQLCipher; la base vive en el almacenamiento privado de la app, sin copia de seguridad (R-027), con eventos ≤ 14 días (168 h + 7 días) y un historial que se borra al cerrar sesión. Riesgo anotado: un celular *rooteado* o robado y desbloqueado expone posiciones recientes.
- **Rationale**: SQLCipher suma ~4–7 MB al APK y costo de CPU en cada escritura (RNF-15). **Pendiente de confirmar por el autor** (BD-A7 lo marca como decisión del autor).

## R-029 · Herramientas de prueba

| Capa | Herramienta | Qué se aísla |
|---|---|---|
| Dominio | JUnit 4 + kotlin.test + `kotlinx-coroutines-test`; dobles escritos a mano (`Fake…`); `RelojFalso` | Nada: sin Android, sin red, sin reloj real |
| Datos (Room) | Robolectric + `Room.inMemoryDatabaseBuilder`; `MigrationTestHelper` (instrumentada, `androidTest`) | SQL, índices, disparadores, transacciones, migraciones |
| Datos (red) | OkHttp `MockWebServer` + validación contra los YAML (`swagger-parser` 2.1 para OpenAPI 3.1 + `networknt json-schema-validator` 2020-12) | Contrato HTTP real, sin servidor |
| WorkManager | `work-testing` (`WorkManagerTestInitHelper`, `TestDriver`, `TestListenableWorkerBuilder`) | Restricciones, esperas, reemplazos |
| UI | Compose testing (`createComposeRule`) con Robolectric y en dispositivo | Textos, toques, estados |
| Arquitectura | Konsist | Capas, 200 líneas, reloj prohibido, sin migración destructiva |
| Campo | Reproductor de sensores, modo avión, `dumpsys batterystats`, `TrafficStats` | EQ-01, RNF-15 |

- **Rationale**: equivalentes Android de la pirámide de la constitución VII (xUnit, NSubstitute, `FakeTimeProvider`, Testcontainers, `WebApplicationFactory`, NetArchTest). Ver la propuesta de enmienda.

## R-030 · Cómo se mide cada atributo de calidad

| Atributo | Medida | Cómo | Dónde queda |
|---|---|---|---|
| RNF-04 | p95 < 50 ms | `InferenciaBenchmarkTest` (instrumentada): 1 000 ventanas reales con el intérprete caliente, p95 con `System.nanoTime`, en 3 celulares de gama media (≤ 4 GB) | `mediciones/informe-campo.md` (T121) |
| RNF-15 batería | ≤ 5 %/h pantalla apagada | 1 h de manejo real o reproductor, pantalla apagada, sin cargador; `dumpsys batterystats --reset` antes y `dumpsys batterystats` + porcentaje al final; 3 celulares | `mediciones/informe-campo.md` |
| RNF-15 datos | < 1 MB/h | `TrafficStats.getUidTxBytes/RxBytes` antes y después de 1 h (sin la descarga del mapa) | `mediciones/informe-campo.md` |
| RNF-15 almacenamiento | < 100 MB a 6 meses | `TamanoBaseTest`: llena 85 000 eventos, 5 000 anomalías, 400 recorridos y mide `killauru.db` + `-wal` (meta < 25 MB) | CI |
| RNF-33 / EQ-01 | 5 h, 7 días, ≤ 60 s | Recorrido en modo avión; reproductor de 5 h; reloj del emulador +7 días; cronómetro desde que vuelve la red hasta el primer lote en MockWebServer o en el servidor | Informe de campo |
| EQ-02 | 0 pérdidas / 0 duplicados | Corte de red con `MockWebServer.SocketPolicy.DISCONNECT_AFTER_REQUEST`; en campo, conteo app vs `evento_recibido` | CI + campo |
| RN-16 | 8 s / 12 s ± 1 s | Trayectorias simuladas en `ProximidadTrayectoriasTest` | CI |
| RNF-12 / EQ-08 | ≤ 3 toques, 0 para detectar | `ToquesAccionesPrincipalesTest` (Compose) cuenta toques desde el mapa | CI |

## R-031 · Tiempos relativos (RNF-14) — Decisión SDD

- **Decision**: `TiempoRelativo.formatear(instante, ahora)` en hora de Perú (UTC−5 fijo, como RN-12): < 1 min «hace un momento»; < 60 min «hace N min»; mismo día «hace N horas»; día anterior «ayer a las HH:mm»; más «el d MMM a las HH:mm».
- **Rationale**: los ejemplos de RNF-14 («hace 2 horas», «ayer a las 15:30»).

## R-032 · Prioridad del indicador (RF-36) — Decisión SDD

- **Decision**: se muestra el primero que se cumple: «Falta un permiso» → «Sin GPS» → «Detección apagada (vehículo Pesado o Moto)» → «Envío desactivado por el usuario» → «En pausa: batería baja» → «En pausa: velocidad baja» → «Detectando». Siempre con «N por enviar» (`PENDING` + `ENVIANDO`).
- **Rationale**: primero lo que el usuario puede arreglar; luego sus elecciones; luego las pausas automáticas.

## R-033 · Una cuenta de administrador en la app — Decisión SDD

- **Decision**: si el inicio de sesión devuelve `rol = Administrador`, la app no guarda la sesión, llama a `cerrar-sesion` y muestra «Esta cuenta es de administración. Usa el panel».
- **Rationale**: `01-actores.md` §1 (interfaz del administrador: panel web) y §4 (el administrador no detecta, no sincroniza ni recibe alertas); 005 responde 403 a sus lotes.

## R-034 · Cada cuánto se actualiza la copia — Decisión SDD

- **Decision**: al abrir la app, al volver la red y cada 15 min con red (`PeriodicWorkRequest`, mínimo de Android).
- **Rationale**: RNF-15 (< 1 MB/h): el demo completo pesa unos KB y la caché de borde dura 60 s; los cambios del registrado son pequeños.

## R-035 · Aviso fuera del demo — Decisión SDD

- **Decision**: se muestra la primera vez en el viaje que un fix (≤ 50 m de la vía) cae fuera del polígono demo, ya sea al salir o porque el viaje empezó fuera; se guarda el id del viaje avisado. Dura 8 s y no pide toques.
- **Rationale**: RN-08 y HU-07 («al salir del tramo demo… una sola vez por viaje»); el caso de empezar fuera no lo cubre docs y tiene el mismo riesgo (confundir «sin datos» con «sin baches»).

## Propuesta de enmienda a la constitución (no aplicada)

La constitución 1.1.0 está escrita para el backend. Para la app se propone una enmienda **MENOR** (1.2.0), que el autor debe aprobar:

1. **II.** Agregar un párrafo «App Android»: tres capas de `enfoque` §9 (`dominio` ← `ui`, `dominio` ← `datos`), un solo módulo Gradle por funcionalidad (`diseño-interno` §7.1), verificado con **Konsist** en cada build.
2. **III.** «Ningún umbral es una constante»: en la app, los umbrales viven en `assets/parametros.json` (`ParametrosApp`) y en el manifiesto del modelo; cambiarlos exige un APK nuevo pero no cambiar código (`enfoque` §5: «en el APK»). El reloj es el puerto `Reloj` (R-003), no `TimeProvider`.
3. **VII.** Pirámide de la app: JUnit + `kotlinx-coroutines-test` (dominio) → Robolectric + Room en memoria y `MigrationTestHelper` → MockWebServer contra los YAML → `work-testing` → Compose testing → Konsist → campo. Cobertura con **Kover** ≥ 90 % de los paquetes `dominio`. CI de la app en cada PR que toque `app/` (`app-ci.yml`).
4. **VIII.** La regla de 200 líneas incluye `.kt`, `.kts` y `.py` (excepto generados por Room/KSP y `schemas/*.json`). El equivalente de `BannedApiAnalyzers` es detekt `ForbiddenMethodCall` (`System.currentTimeMillis`, `Instant.now`, `Clock.systemUTC`, `Date()`, `LocalDateTime.now`, `fallbackToDestructiveMigration`). Métodos ≤ 20 líneas (`LongMethod`), `allWarningsAsErrors = true`, resultados con `sealed interface` en lugar de excepciones para el flujo de negocio.
5. **IX.** La app no publica endpoints: consume los YAML de `specs/*/contracts/` y los verifica con pruebas de contrato; los archivos de `cdn.killauru.pe` tienen su contrato en `008/contracts/cdn.openapi.yaml`.
6. **Restricciones.** Fila «App móvil»: el stack de `estilo-arquitectonico.md` (Kotlin, Compose, Room, WorkManager, Hilt, Retrofit + OkHttp, LiteRT, MapLibre Native, DataStore + Keystore, `minSdk` 26) y «APK fuera de Play Store (S-02)».

## Pendientes de confirmar por el autor

1. **R-001**: el pedido de módulos Gradle por capa o área choca con `diseño-interno` §7.1 («un solo módulo»). Se sigue docs; si el autor prefiere varios módulos, primero se cambia §7.1.
2. **R-024**: una versión nueva de los términos no corta un viaje en curso; se pide al abrir la app.
3. **R-025**: con «Solo ver el mapa», los pendientes ya guardados no se envían (y expiran a las 168 h).
4. **R-026**: el 413 se trata como el 400 (partir el lote); documentarlo en el contrato del lote de `03`.
5. **R-028**: Room sin SQLCipher en v1.0, como riesgo aceptado (BD-A7).

## Confirmados por el autor el 2026-10-22 (D18)

Todos los «Pendientes de confirmar por el autor» de este archivo y la «Propuesta de enmienda a la constitución» quedaron aprobados tal como se recomendaron (`specs/revision/decisiones.md`, D18). La enmienda se aplicó en la constitución 1.2.0.

## Ajuste geométrico autorizado — 2026-10-24

**Revisión geométrica 2026-10-24 (004)**: el eje canónico `backend/db/datos/corredor_pe28a.geojson` mide **335,072963461 km** (335,073 km al mostrarlo). El km 0 es el extremo seleccionado en el enlace PE-1S, no el centro de Pisco; el km final es el extremo seleccionado en Ayacucho. Son distancias acumuladas del eje con radio esférico 6 371 008,8 m, no progresivas oficiales. El demo conserva el inicio geométrico km 268 y termina en `properties.demo_km_fin = properties.longitud_km`: **67,072963461 km**. El nombre histórico «Ayacucho–Vinchos» no certifica que el km 268 sea el centro de Vinchos. Los cálculos y límites usan el valor completo del GeoJSON; los valores redondeados de documentos e interfaz son solo de presentación. La generación del polígono de 200 m y su validación de ≤ 60 vértices corresponden a 006; no se reemplaza ese recorte por una comprobación de kilometraje.

Referencia reproducible: `backend/db/datos/README.md`; 20 puntos compartidos en `backend/db/datos/km_referencia.csv`. Backend y Android deben usar ese mismo CSV, radio y eje. `TramosTotales = ceil(longitud_km) = 336`. El panel no debe conservar las progresivas de pueblos del prototipo sin verificarlas contra el eje.
