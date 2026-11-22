# Cierre de spec 008 — App Android

**Revisado:** 2026-11-15. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 100/128 tareas marcadas; 28 pendientes en tasks.md (T024, T107 y T112 reabiertas el 2026-11-22).
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Depende de 003/005/006; la detección final requiere el modelo aprobado de 009.

## Evidencia disponible

Implementación y tests JVM por funcionalidad. Configuración actual: Gradle 9.7.1, AGP 9.3.3, catálogo Kotlin 2.4.20, JDK 21 en CI y destino JVM 17.

## Qué falta localmente

**2026-11-22 — la app vuelve a compilar y aparecen las diferencias con la API.** Corregidos con TDD:
- Compilación: `obtenerSesion` → `obtener`; `@OptIn` en `TopAppBar`; `verificadoUtc` toma la **hora del servidor**
  (`data-model` §estado_copia, FR-026) y `ActualizarAnomaliasUseCase` deja de depender del reloj; `?.` innecesario (`-Werror`).
- Hilt: `MapaModulo` repetía un repositorio y dos DAO de `BdModulo`/`RepositoriosModulo`; faltaba proveer `WorkManager`.
- Pruebas: 3 archivos importaban el convertidor JSON antiguo de Jake Wharton; usan el de Square, como producción.
- FR-031 (T056): `duplicados` y rechazados con `id` → `SYNCED`; el modelo del dominio pasa a `RespuestaLote`/`Rechazo` con `motivo`.
- FR-031, conteo: `EnviarLoteUseCase` suma cada rechazo a `contador_local` (`rechazo.<Motivo>`) con el puerto
  `ContadorRechazos` y el adaptador `ContadorRechazosRoom`. `ContadorDao.incrementar` (`@Upsert`) **reemplazaba** el valor:
  `ContadorRechazosRoomTest` lo demostró (esperado 2, obtenido 1) y ahora `ContadorDao.sumar` usa `INSERT OR IGNORE`
  + `UPDATE valor = valor + n` en una transacción (`ON CONFLICT DO UPDATE` requiere Android 11; `minSdk` es 26).

- **Cliente de Ingesta alineado con el contrato 005** (`ContratoIngestaTest`, 6 casos, en rojo 5/6 antes del cambio):
  ruta `api/v1/anomalies/batch` (era `v1/ingest/batch`), campos `latitud`/`longitud`/`precisionMetros`, `tipo` como texto,
  `seudonimo` en cada evento y en la cabecera `X-KillaUru-Seudonimo` del lote (D2), `codigo` del ProblemDetails y
  `Retry-After` real en 429/503. La traducción vive en `MapeadorLote`; el cuerpo se valida contra el esquema `Lote` del YAML
  y se comparan los nombres de campo, porque `Evento` es permisivo (D5) y no detectaría `lat` en lugar de `latitud`.

- **Cliente del Mapa alineado con el contrato 006** (`ContratoMapaTest` reescrito, 8 casos, en rojo 7/8 antes del cambio):
  ruta `api/v1/anomalies` (era `v1/mapa/anomalias`), `updatedSince` ISO-8601 (era `desde` en ms), fechas ISO, `tipo` y
  `persistencia` como texto, `kmVia`; el 304 lee `Date` en formato HTTP (antes fallaba al tratarla como ISO) y, si falta,
  conserva la marca anterior en vez de usar el reloj del celular (FR-026; `RelojProhibidoTest` vuelve a verde).
  Los datos de prueba se validan contra `ColeccionAnomalias` del YAML. `ValidadorContrato.esquema` quita el `type: object`
  que el parser de Swagger añade junto a `const`.
- **Resultado:** 242 pruebas JVM, 0 fallidas, 0 omitidas (2026-11-22).
- **Cuenta alineada con el contrato 003** (`ContratoIdentidadTest` y pruebas por caso de uso, en rojo antes del cambio):
  `Sesion` = `SesionApp` (token de acceso de 5 min, `tokenRenovacion`, `sesionVenceUtc` como permiso local y `rol`);
  rutas `api/v1/auth/registro`, `inicio-sesion` (con `canal = app`), `renovar`, `cerrar-sesion`, `me/reenviar-verificacion`
  y `DELETE api/v1/me` con contraseña. El registro envía las versiones aceptadas; la app rechaza cuentas de administración
  (R-033); eliminar distingue 204 de 403 `confirmacion_invalida`. Ambos tokens se guardan cifrados y se renueva con
  `tokenRenovacion`. La prueba de contrato atrapó que `canal` no se enviaba (valor por defecto + `encodeDefaults = false`).
  Mensajes corregidos: el de credenciales estaba cambiado por el de R-033 y el registro pedía 8 caracteres en vez de 10.
  `LectorProblema` (común) lee el `codigo` de ProblemDetails para Ingesta e Identidad. **255 pruebas JVM, 0 fallidas.**
- **Red autenticada conectada** (`RedAutenticadaTest` sobre la cadena real de OkHttp, en rojo 4/5 antes del cambio):
  `InterceptorToken` no añadía nada y ni él ni `RenovadorToken` estaban en el cliente; `InterceptorSeudonimo` recibía
  `{ null }`. Ahora `ClienteHttp` encadena token → seudónimo → renovador; las rutas públicas de `/api/v1/auth/` no llevan
  el token (la API da 401 a un token vencido incluso ahí); el seudónimo solo va en anónimas y no pisa el del lote (D2);
  varios 401 simultáneos hacen **una** renovación (ADR-009) y una renovación rechazada invalida la sesión (RN-21 c).
  Mutaciones verificadas: sin la exclusión, `inicio-sesion` llevaba `Bearer acceso-vencido`; sin la comprobación de
  concurrencia, 4 peticiones hacían 4 renovaciones. Cada funcionalidad entrega su interceptor por `@Named`
  (`NombresInterceptor`), así `comun` no depende de sesión ni de seudónimo. **261 pruebas JVM, lint y compilación en verde.**

**⚠️ Bloqueante para «primera versión funcional» (T024 reabierta):** `NavGrafo` solo registra Bienvenida, Mapa, Ajustes e
Historial, y cada una muestra un texto fijo. Ninguna de las 19 pantallas programadas (Mapa, Login, Registro, Perfil,
Términos, Permisos, Detalle, Historial, Descarga…) se llama desde la navegación, y `PerfilViewModel` no lo usa nadie.
Las capas de dominio y datos están probadas, pero el usuario no puede llegar a ellas.

**Parte 1 resuelta (2026-11-22):** `NavGrafo` recibe sus pantallas como `PantallasApp` (las reales en
`PantallasReales`, dobles en `NavGrafoTest`) y conecta primer uso → mapa (fuera de la pila), pestañas Mapa · Mis viajes ·
Ajustes y detalle. `PrimerUsoViewModel` ahora muestra la bienvenida solo la primera vez, pide una versión nueva de los
términos sin repetir vehículo ni permisos (R-024) y retoma en el vehículo si faltaba. `PermisosRuta` pide los permisos
al sistema (antes nada lo hacía), con la ubicación en dos pasos (`PermisosUbicacion`). Se corrigió un ciclo de
inicialización en `Rutas` (un conjunto en su `companion` podía quedar en null) y se sustituyó la prueba tautológica
`ToquesAccionesPrincipalesTest`. Recorrido en emulador: [evidencia](../../docs/seguimiento/evidencias/pruebas/2026-11-22-app-navegacion/README.md),
con 5 diferencias de pantalla frente a la spec anotadas allí. **271 pruebas JVM y lint en verde.**

**Tema y pantallas del primer uso (2026-11-22):** la app no tenía tema (T024 nombraba `Tema`, `Colores` y
`Tipografia`, que no existían): usaba el morado de Material. Ahora `comun/ui/tema/` trae `Paleta` (los tokens del
panel, claro y oscuro), `Tipografia` (Overpass, Source Sans 3 y Overpass Mono en el APK, licencia OFL en
`assets/licencias/`) y `KillaUruTema`; `TemaCoherenteConPanelTest` lee `panel/src/compartido/estilos/tokens.css` y
falla si un color se aparta (mutación: `--s2` con el valor del prototipo → «Distintos del panel en claro: [--s2]»).
`--s2` y `--s3` siguen al panel, no al prototipo, porque el prototipo no llega a 3:1 (WCAG 1.4.11); el motivo quedó
escrito en `tokens.css` y en `Paleta`. `SeveridadChip` usaba verde/ámbar/rojo genéricos y pintaba igual 1 y 2: ahora
es el círculo `.sev`. Términos y vehículo siguen al prototipo (textos, «Acepto y continúo», guía del soporte y del
cargador, RN-18) con filas pulsables enteras (`FilaMarcable`). Verificado en emulador, claro y oscuro. Pendiente: los
marcadores del mapa son todos rojos (`MapaPantalla`), se rehace con el paso de la CDN. **283 pruebas JVM y lint en verde.**

**Pendiente, en curso:** `enDemo` no viene en el contrato; debe calcularse con `ZonaDemo` (data-model §1). El polígono
se descarga de la CDN (FR-042, research `cdn.killauru.pe`, `cdn.openapi.yaml` con `tipo: tramoDemo`), pero
`CargadorZonaDemo` lo busca en los assets del APK y nadie lo usa; además el archivo depende de 006/T002. Mientras tanto
vale `false`, y FR-051 (borrar `enDemo = 0` al cerrar sesión) vaciaría toda la copia. CDN por revisar.
Cerrar sesión (FR-066) no llama a `cerrar-sesion`; la revocación diferida sin red debe diseñarse, porque `cerrar-sesion` exige el
token de acceso, que dura 5 min. Faltan `ObtenerMisDatosUseCase` (FR-068) y que `LimpiezaLocalSesion` recorte al demo y borre el
historial (FR-051, FR-069), aunque su comentario lo afirme.
FR-035: el adaptador ya entrega el `Retry-After`, pero ningún caso de uso usa `reintentarEn` para programar el reintento.
Detekt marca 2 funciones de más de 20 líneas (`DescargaMapaPantalla`, `DescargaReanudable.descargar`); `RepositorioAnomaliasHttp.obtener` ya se dividió.


El 21 de noviembre se intentó ejecutar los tests de contratos, actualización de mapa y tamaño.
Gradle se detuvo antes de los tests por errores de compilación en producción:
`DescargaMapaPantalla.kt:28` (API experimental), `ActualizarAnomaliasWorker.kt:26`
(`obtenerSesion` inexistente) y `ActualizarAnomaliasUseCase.kt:80` (`ahoraMs` inexistente).
El SDK local existe, pero requirió definir ANDROID_HOME para la ejecución.
No existen src/androidTest ni src/test/resources. ClasificadorLiteRtTest existe pero comprueba normalización/mapeo: falta el alcance LiteRT completo de T044. TamanoBaseTest existe: T065 requiere revisar y ejecutar su evidencia, no recrear el archivo.

## Qué falta en el entorno desplegado

Modelo aprobado; migraciones instrumentadas; trayectoria/alertas; 5 h offline y conciliación; batería/datos/almacenamiento en 3 celulares; R2 y benchmark p95.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T017 | Local | Presencia y parseo corregidos; falta validar ejemplos frente a sus esquemas y ejecutar Gradle con SDK Android configurado. |
| T027 | Local | `PrimerUsoUiTest` (Compose + Robolectric): botón inactivo hasta marcar las dos casillas; Liviano/Pesado/Moto con su texto; guía del soporte y del cargador; permiso negado → texto de lo que se pierde |
| T028 | Local | `AjustesUiTest`: «Solo ver el mapa» no cambia la categoría; filas de vehículo, modelo, mapa, perfil, términos y estado del servicio |
| T044 | Local | `ClasificadorLiteRtTest` (Robolectric con el modelo de prueba): normalización del manifiesto; salida de 3 clases y 5 severidades; intérprete y *buffers* reutilizados (sin asignaciones por ventana) |
| T045 | Local | `EventoDaoTest` (Room en memoria): `guardar`, `pendientes()` como `Flow`, conteo del indicador |
| T053 | Local | `ServicioDeteccionTest` (Robolectric, reproductor de sensores y GPS grabados): 10 min de ruta con 6 baches etiquetados → 6 eventos `PENDING`, 0 toques; sensores apagados con Pesado |
| T060 | Local | `EventoDaoLoteTest` (Room en memoria): `tomarLote` en una transacción, un solo seudónimo, orden `detectadoUtc, eventoId`; `recuperarEnviando`; `expirar` |
| T061 | Local | `ContratoLoteTest` (MockWebServer + `ValidadorContrato` con `ingesta.openapi.yaml`): cuerpo sin `loteId`, un seudónimo, ≤ 50, cabecera solo sin token; respuestas 200/400/401/403/413/429/503 del YAML interpretadas |
| T062 | Local | `SincronizacionWorkerTest` (`work-testing`, `TestDriver`): restricción de red; tras 500 → nuevo trabajo con espera del paso; `VigiaRed.onAvailable` reemplaza la espera y corre en ≤ 60 s; trabajo detenido → `ENVIANDO` vuelve a `PENDING`; un solo `sync` |
| T063 | Local | `CorteDeRedTest` (MockWebServer `DISCONNECT_AFTER_REQUEST`, respuesta 200 perdida): reenvío con los mismos `eventoId`; el servidor simulado los cuenta una sola vez |
| T064 | Local | `PurgaLocalTest` (Room + reloj falso): 7 días − 1 ms y 7 días se conservan, 7 días + 1 ms se borra; `PENDING` y `ENVIANDO` nunca; recorrido de 6 meses + 1 día; anomalía vencida hace 7 días |
| T065 | Local | `TamanoBaseTest` (Robolectric, archivo real): 85 000 eventos + 5 000 anomalías + 400 recorridos → `killauru.db` + `-wal` < 25 MB; 14 días sin red sin perder `PENDING` |
| T066 | Local | `CadenaMigracionesTest` (instrumentada, `MigrationTestHelper`): con v1 sembrada (`PENDING`, `ENVIANDO`, recorridos) abre con `Migraciones.TODAS` y verifica filas y disparadores; falla si falta el JSON de la versión actual |
| T100 | Local | `ProximidadTrayectoriasTest` (fixtures de `src/test/resources/trayectorias/`: recta, curva de 80 m / 1 km, ida y vuelta, 30/60/90 km/h): alerta a 8 s ± 1 s y 12 s ± 1 s, respetando mínimos |
| T101 | Local | `AlertaSonoraAndroidTest` (Robolectric): foco `MAY_DUCK`, `USAGE_ALARM`, vibración, clip según tipo y cantidad, notificación de alta prioridad que se cierra sola |
| T108 | Local | `RenovadorTokenTest` (MockWebServer): 401 → una sola renovación aunque haya 3 peticiones en paralelo; reintenta la petición; `cuenta_desactivada` → `AplicarSesionInvalidaUseCase` (permiso borrado y recorte al demo) |
| T109 | Local | `ContratoIdentidadTest` (MockWebServer + `identidad.openapi.yaml`): registro, inicio de sesión (`canal = app`), renovar, cerrar sesión, recuperar, `GET`/`DELETE /me`, aceptaciones, reenviar verificación y versiones legales |
| T110 | Local | `CuentaUiTest` (Compose): registro, «Revisa tu correo» sin bloquear, login con mensaje genérico, olvidé, perfil con todos los campos, aviso de cerrar sesión (3 pendientes), eliminar con contraseña y casilla |
| T119 | Local | `ContratoManifiestoModeloTest` (MockWebServer + `ManifiestoModelo` de `cdn.openapi.yaml`, con el `assets/modelo/manifiesto.json` del APK) |
| T121 | Desplegado/campo | Informe de campo (plantilla y resultados): dispositivos (3 de gama media), ruta Ayacucho–Vinchos, cronómetro y registro de cada medición de `research.md` R-030 |
| T122 | Desplegado/campo | `InferenciaBenchmarkTest` (instrumentada): 1 000 ventanas, p95 < 50 ms; correr en los 3 celulares y anotar en T121 |
| T123 | Desplegado/campo | Prueba de campo EQ-01: 5 h en modo avión (o reproductor de 5 h), conteo de SQLite antes y después; cronómetro de la reanudación ≤ 60 s; reloj del emulador +7 días |
| T124 | Desplegado/campo | Batería, datos y almacenamiento (RNF-15) con la pantalla apagada en 3 celulares (`dumpsys batterystats`, `TrafficStats`) y ahorro de RN-22 |
| T125 | Desplegado/campo | Conciliación de punta a punta contra el entorno de pruebas (R-11): eventos enviados por la app = filas de `evento_recibido`; 0 duplicados |
| T127 | Desplegado/campo | Correr `app-datos-r2.yml` con `ensayo: true` y luego real; anotar versión y huellas publicadas |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
