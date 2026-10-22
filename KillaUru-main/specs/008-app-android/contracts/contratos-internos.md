# Contratos de la app KillaUru

La app no expone API. Este archivo fija (1) lo que consume del backend y cómo reacciona, (2) los puertos entre sus capas y (3) el manifiesto del modelo que comparte con 009. Los archivos de `cdn.killauru.pe` están en [`cdn.openapi.yaml`](cdn.openapi.yaml).

## 1. Lo que la app **consume** del backend

| Operación | Contrato | Rol | Uso en la app | Respuestas que trata |
|---|---|---|---|---|
| `POST /api/v1/anomalies/batch` | [`ingesta.openapi.yaml`](../../005-modulo-ingesta/contracts/ingesta.openapi.yaml) | anónimo / conductor | `ClienteLotesHttp` (FR-028…FR-038) | 200 · 400 · 401 · 403 · 413 · 429 · 503 · 5xx (`research.md` R-008) |
| `GET /api/v1/anomalies` | [`mapa.openapi.yaml`](../../006-modulo-consulta-mapa/contracts/mapa.openapi.yaml) | anónimo: sin `bbox` ni `updatedSince` · conductor: `updatedSince` | `RepositorioAnomaliasHttp` (FR-044…FR-048) | 200 · 304 (anónimo) · 400 (→ descarga completa) · 401 · 429 |
| `GET /api/v1/legal/versiones` | [`identidad.openapi.yaml`](../../003-modulo-identidad-accesos/contracts/identidad.openapi.yaml) | público | FR-003 | 200 · 429 |
| `POST /api/v1/auth/registro` | idem | público | FR-060 | 201 · 400 (`aceptacion_requerida`, `datos_invalidos`) · 409 `correo_en_uso` · 429 |
| `POST /api/v1/auth/inicio-sesion` (`canal = app`) | idem | público | FR-061, R-033 | 200 `SesionApp` · 401 `credenciales_invalidas` (mensaje genérico) · 429 |
| `POST /api/v1/auth/renovar` | idem | token de actualización | `RenovadorToken` (FR-062, FR-063) | 200 · 401 `sesion_invalida` / `cuenta_desactivada` (→ RN-21 c) · 429 |
| `POST /api/v1/auth/cerrar-sesion` | idem | conductor | FR-066, R-023 | 204 · 401 (se da por cerrada) · 429 |
| `POST /api/v1/auth/recuperar` | idem | público | FR-065 | 202 (siempre el mismo mensaje) · 400 · 429 |
| `GET /api/v1/me` | idem | conductor | FR-064, FR-068 | 200 `MisDatos` · 401 |
| `DELETE /api/v1/me` | idem | conductor | FR-067 | 204 · 401 · 403 `confirmacion_invalida` · 429 |
| `POST /api/v1/me/aceptaciones` | idem | conductor | FR-002, FR-003 | 204 · 400 · 401 · 429 |
| `POST /api/v1/me/reenviar-verificacion` | idem | conductor | FR-064 | 202 · 401 · 429 |

Reglas comunes ([`convenciones-api.md`](../../001-plataforma-backend/contracts/convenciones-api.md)): JSON `camelCase`, fechas ISO UTC con `Z`, errores `application/problem+json` con `codigo` estable (la app decide por `codigo`, nunca por `title`); `X-KillaUru-Seudonimo` solo sin token (FR-040); 429 en toda operación con `Retry-After`.

## 2. Puertos del dominio (`*/dominio/`, implementados en `*/datos/`)

| Puerto (dominio) | Operaciones | Implementación (datos) |
|---|---|---|
| `Reloj` | `ahoraConfiable(): Instant?` · `monotonico(): Duration` · `anclarGps(utc, elapsed)` · `anclarServidor(utc)` | `RelojConfiableAndroid` (R-003) |
| `RepositorioEventos` | `guardar(Evento)` · `tomarLote(limite): Lote?` · `aplicar(ResultadoLote)` · `expirar(limite)` · `recuperarEnviando()` · `pendientes(): Flow<Int>` | `RepositorioEventosRoom` |
| `ApiLotes` | `enviar(lote, sesion?): RespuestaLote` | `ClienteLotesHttp` |
| `RepositorioAnomaliasLocales` | `enTramo(desde, hasta, ahora)` · `visibles(ahora): Flow` · `aplicarCambios(altas, bajas, generadoUtc)` · `reemplazar(lista, etag, verificado)` · `recortarAlDemo()` · `estadoCopia()` | `RepositorioAnomaliasRoom` |
| `FuenteAnomalias` | `cambiosDesde(generadoUtc?, sesion)` · `demoCompleto(etag?)` | `RepositorioAnomaliasHttp` |
| `Clasificador` | `clasificar(Ventana): Clasificacion` · `modeloVersion` | `ClasificadorLiteRt` |
| `FuenteSensores` / `FuenteUbicacion` / `FuenteBateria` | flujos de muestras, fixes y estado de batería | `LectorSensores`, `LectorGps`, `LectorBateria` |
| `AlertaSonora` | `emitir(alerta: Alerta)` | `AlertaSonoraAndroid` (R-021) |
| `RepositorioSesion` | `sesion(): Flow<Sesion?>` · `guardar(SesionApp)` · `borrar()` · `permisoLocal()` | `RepositorioSesionCifrado` (R-022) |
| `ApiCuenta` | `registrar` · `iniciarSesion` · `renovar` · `cerrarSesion` · `recuperar` · `misDatos` · `eliminar` · `aceptar` · `reenviarVerificacion` · `versionesLegales` | `ApiCuentaHttp` |
| `RepositorioPreferencias` | categoría, «Solo ver el mapa», datos móviles, versiones aceptadas, paso del *backoff* | `RepositorioPreferenciasDataStore` |
| `RepositorioSeudonimo` | `vigente(ahora): String` | `RepositorioSeudonimoDataStore` (R-010) |
| `RepositorioRecorridos` | `guardar(Recorrido)` · `ultimos6Meses()` · `borrarTodo()` | `RepositorioRecorridosRoom` |
| `CatalogoArchivos` | `manifiestoMapa()` · `manifiestoModelo()` · `descargar(archivo, progreso)` · `activar(archivo)` | `CdnHttp` + `DescargaReanudable` + `VerificadorSha256` |

## 3. Manifiesto del modelo (contrato con 009)

- **Dueño:** 009 lo genera al publicar cada `modeloVersion` y lo sube a `cdn.killauru.pe/modelo/manifiesto.json`; el APK incluye una copia para el modelo inicial en `assets/modelo/manifiesto.json`.
- **Esquema:** `ManifiestoModelo` de [`cdn.openapi.yaml`](cdn.openapi.yaml) (`esquema = 1`).
- **Reglas que la app verifica antes de activar** (`PoliticaActivacionModelo`, FR-070, FR-071): `esquema == 1`; `version` > activa (semver); `versionMinimaApp` ≤ `versionCode`; `etiquetas == [Normal, Bache, Hundimiento]`; `entrada.frecuenciaHz == 50`; tamaño y SHA-256 del archivo; carga de prueba del intérprete con una ventana cero sin error y salidas del tamaño esperado.
- **Cambio de contrato:** un campo nuevo opcional no cambia `esquema`; uno obligatorio o un cambio de forma sube `esquema` y exige un APK que lo entienda.

## 4. Formato del lote que envía la app

Igual a `Lote` de `ingesta.openapi.yaml`: `{ "eventos": [ { eventoId, detectadoUtc, latitud, longitud, precisionMetros, tipo, severidad, confidence, modeloVersion, seudonimo } ] }`, sin `loteId` (D3), todos con el mismo `seudonimo` (D2), ≤ 50, ordenados por `detectadoUtc`. `tipo` en texto (`Bache` / `Hundimiento`); `detectadoUtc` en ISO UTC con `Z` y milisegundos.
