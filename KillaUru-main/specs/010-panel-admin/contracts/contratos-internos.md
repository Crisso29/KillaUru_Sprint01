# Contratos del panel «KillaUru Ops»

El panel **no tiene API propia** (no hay `*.openapi.yaml` en esta carpeta): es un cliente de los contratos de 001–007. Este archivo fija, operación por operación, qué envía y qué campos lee, para que la prueba de contrato (tipos generados + Ajv, `research.md` R-024) y la revisión campo por campo tengan una lista cerrada.

## 1. Lo que el panel **ofrece**

| Qué | A quién | Fuente |
|---|---|---|
| Páginas públicas `/cuenta/verificar#token=…`, `/cuenta/restablecer#token=…`, `/cuenta/invitacion#token=…` | Enlaces de los correos de 003 (`UrlBaseEnlaces = https://panel.killauru.pe/cuenta`) | 003 R-006, `data-model.md` §5 |
| Cabeceras de seguridad de `panel.killauru.pe` (`_headers`) | Navegadores | FR-011, `research.md` R-008 |
| Nada más | — | Ningún módulo depende del panel |

## 2. Lo que el panel **usa** de la API

Todas las llamadas van a `VITE_URL_API` con `credentials: 'include'` (cookie `ku_acceso`, `securitySchemes.cookieAcceso` de 001). Respuestas de error: `Problema` (001) y `ProblemaBloqueo` (003), leídas por `codigo`, `status`, `correlacionId` y `bloqueadoHastaUtc` (`data-model.md` §2). Todas tratan 429 (`LimiteExcedido`).

### 2.1 Identidad y Accesos (`003/contracts/identidad.openapi.yaml`)

| `operationId` | Método y ruta | Envía | Lee | Códigos tratados | Pantalla |
|---|---|---|---|---|---|
| `iniciarSesion` | `POST /api/v1/auth/inicio-sesion` | `InicioSesionSolicitud`: `correo`, `contrasena`, `canal = "panel"`, `turnstileToken` | `SesionPanel`: `expiraAccesoUtc`, `sesionVenceUtc`, `rol` | 200, 401 `credenciales_invalidas`, 403 `solo_administradores` / `antibot_fallido`, 423 `cuenta_bloqueada` (`bloqueadoHastaUtc`), 429 | Login |
| `renovarSesion` | `POST /api/v1/auth/renovar` | sin cuerpo (cookie `ku_renovacion`) | `SesionPanel` | 200, 401 `sesion_invalida` / `cuenta_desactivada`, 429 | todas (R-006) |
| `cerrarSesion` | `POST /api/v1/auth/cerrar-sesion` | sin cuerpo | — | 204, 401, 429 | Marco |
| `solicitarRecuperacion` | `POST /api/v1/auth/recuperar` | `{ correo }` | — | 202, 400, 429 | Login → recuperar |
| `verificarCorreo` | `POST /api/v1/auth/verificar-correo` | `TokenEnlace`: `token` | — | 204, 400 `enlace_invalido`, 429 | `/cuenta/verificar` |
| `fijarContrasenaConEnlace` | `POST /api/v1/auth/fijar-contrasena` | `{ token, nuevaContrasena }` (10–128) | — | 204, 400 `enlace_invalido` / `datos_invalidos`, 429 | `/cuenta/restablecer`, `/cuenta/invitacion` |
| `obtenerMisDatos` | `GET /api/v1/me` | — | `MisDatos.correo` (solo eso) | 200, 401, 429 | Marco, Usuarios (fila propia), Exportación («tú») |
| `listarUsuarios` | `GET /api/v1/usuarios` | `busqueda` (≤ 100) **o** `ids` (≤ 200), `rol`, `estado`, `pagina`, `tamano` (50) | `PaginaUsuarios`: `elementos[]` (`Usuario`: `id`, `correo`, `rol`, `estado`, `correoVerificado`, `fechaAltaUtc`, `ultimaSesionUtc`), `pagina`, `tamano`, `total` | 200, 400, 401, 403, 429 | Usuarios; actores (Auditoría, Resumen, Mapa); filtro por correo |
| `crearUsuario` | `POST /api/v1/usuarios` | `{ correo, rol }` | `Usuario` | 201, 400, 401, 403, 409 `correo_en_uso`, 429 | Usuarios |
| `cambiarRol` | `PUT /api/v1/usuarios/{id}/rol` | `{ rol }` | — | 204, 401, 403, 404, 409 `accion_sobre_si_mismo` / `ultimo_administrador`, 429 | Usuarios |
| `desactivarUsuario` | `POST /api/v1/usuarios/{id}/desactivar` | `{ motivo }` (≤ 200) | — | 204, 401, 403, 404, 409, 429 | Usuarios |
| `activarUsuario` | `POST /api/v1/usuarios/{id}/activar` | sin cuerpo | — | 204, 401, 403, 404, 429 | Usuarios |
| `reenviarCorreoUsuario` | `POST /api/v1/usuarios/{id}/reenviar-correo` | sin cuerpo | — | 202, 401, 403, 404, 429 | Usuarios |
| `consultarRevisionPrivilegios` | `GET /api/v1/usuarios/revision-privilegios` | — | `EstadoRevision`: `ultimaRevisionUtc`, `venceUtc`, `administradores[]` (`id`, `correo`, `ultimaSesionUtc`) | 200, 401, 403, 429 | Usuarios, Resumen |
| `registrarRevisionPrivilegios` | `POST /api/v1/usuarios/revision-privilegios` | `{ confirmados[], retirados[] }` | `EstadoRevision` | 201, 400 `revision_incompleta`, 401, 403, 409, 429 | Usuarios |

### 2.2 Consulta de Mapa (`006/contracts/mapa.openapi.yaml`)

| `operationId` | Método y ruta | Envía | Lee | Códigos tratados | Pantalla |
|---|---|---|---|---|---|
| `consultarMapa` | `GET /api/v1/anomalies` | `estado` (lista, siempre), `tipo` (uno o nada), `severidad` (lista o nada), `desde`. **Nunca** `bbox`, `hasta`, `updatedSince`, `If-None-Match` ni `X-KillaUru-Seudonimo` | `ColeccionAnomalias`: `generadoUtc` («Actualizado HH:MM:SS»), `features[]` (`AnomaliaFeature`: `id`, `geometry.coordinates` `[lon, lat]`, `properties`: `id`, `kmVia`, `tipo`, `severidad`, `persistencia`, `ultimaDeteccionUtc`, `venceUtc`, `estado`). `BajaFeature` no llega (no se usa `updatedSince`) | 200, 400, 401, 403, 429 | Mapa; Resumen (franja, `estado=Activa`); búsqueda rápida |

### 2.3 Anomalías (`004/contracts/anomalias.openapi.yaml`)

| `operationId` | Método y ruta | Envía | Lee | Códigos tratados | Pantalla |
|---|---|---|---|---|---|
| `obtenerDetalleAnomalia` | `GET /api/v1/anomalies/{id}` | — | `DetalleAnomalia`: los 17 campos requeridos (`id`, `tipo`, `severidadMostrada`, `estado`, `persistencia`, `latitud`, `longitud`, `kmVia`, `diasDistintos15d`, `reportes`, `ultimaDeteccionUtc`, `venceUtc`, `visibleConductores`, `porRevisar`, `vehiculosRevision`, `reportesPorDia[] {dia, cantidad}`, `historial[] {desde, hacia, ocurridaUtc, actorId}`) | 200, 401, 403, 404, 429 | Mapa (detalle) |
| `moderarAnomalia` | `POST /api/v1/anomalies/{id}/moderacion` | `{ accion }` ∈ `MarcarReparada`, `Descartar`, `Reabrir`, `MantenerDescartada` | `DetalleAnomalia` | 200, 401, 403, 404, 409 `transicion_no_permitida`, 429 | Mapa (detalle), Moderación |
| `listarTransicionesRecientes` | `GET /api/v1/anomalies/transiciones` | `limite = 5` | `TransicionReciente[]`: `anomaliaId`, `tipo`, `kmVia`, `desde`, `hacia`, `ocurridaUtc`, `actorId` | 200, 401, 403, 429 | Resumen |
| `listarPorRevisar` | `GET /api/v1/moderacion/por-revisar` | — | `ElementoPorRevisar[]`: `id`, `tipo`, `kmVia`, `severidadMostrada`, `vehiculos7d`, `reportes`, `ultimaDeteccionUtc` | 200, 401, 403, 429 | Moderación, Resumen, contador del menú |
| `listarPendientes` | `GET /api/v1/moderacion/pendientes` | — | `ElementoPendiente[]`: `id`, `tipo`, `kmVia`, `severidadMostrada`, `reportes`, `primerReporteUtc`, `venceUtc` | 200, 401, 403, 429 | Moderación, Resumen |
| `exportarDataset` | `POST /api/v1/exportaciones` | `SolicitudExportacion`: `formato` (`CSV` / `GeoJSON`), `desdeUtc`, `hastaUtc`, `tipo` (o `null`), `estados` (`[]` = todos); `Accept: text/csv` o `application/geo+json` | cuerpo como `Blob` (no se interpreta) | 200, 400, 401, 403, 429 | Exportación |

### 2.4 Dashboard (`007/contracts/dashboard.openapi.yaml`)

| `operationId` | Método y ruta | Envía | Lee | Códigos tratados | Pantalla |
|---|---|---|---|---|---|
| `consultarIndicadores` | `GET /api/v1/dashboard/indicadores` | `dias` ∈ 7, 14, 30 | `ResumenDashboard`: `ultimoDiaCerrado`, `calculadoUtc`, `urlPanelesTecnicos`, `indicadores` (6 × `Indicador`: `valor`, `comparacion`, `variacion`, `unidad`, `serie[7]`), `eventosPorDia[] {dia, eventos, eventosPeriodoAnterior}` | 200, 400, 401, 403, 429 | Resumen |

### 2.5 Auditoría (`002/contracts/auditoria.openapi.yaml`)

| `operationId` | Método y ruta | Envía | Lee | Códigos tratados | Pantalla |
|---|---|---|---|---|---|
| `consultarAuditoria` | `GET /api/v1/auditoria` | `desde`, `hasta`, `categoria`, `accion`, `actorId`, `pagina`, `tamano` | `PaginaAuditoria`: `elementos[]` (`RegistroAuditoria`: `id`, `ocurridoUtc`, `actorId`, `actorRol`, `categoria`, `accion`, `objetivo`, `contexto`), `pagina`, `tamano`, `total` | 200, 400, 401, 403, 429 | Auditoría; Exportación («Recientes», `accion=DatasetExportado`, `tamano=5`; lee `contexto.formato`, `desde`, `hasta`, `tipo`, `estado`, `filas`) |

### 2.6 Operaciones de los contratos que el panel **no** usa

`saludViva`, `saludLista` (las mira UptimeRobot), `obtenerVersionesLegales`, `registrarConductor`, `eliminarMiCuenta`, `aceptarVersion`, `reenviarMiVerificacion` (son de la app, 008) y todo `ingesta.openapi.yaml` (005). El panel **no** llama a ningún endpoint que no esté en los YAML.

## 3. Lo que el panel usa fuera de la API

| Recurso | Uso | Fuente |
|---|---|---|
| `https://cdn.killauru.pe/…` (PMTiles, estilo, glifos, *sprite*) | Mapa base por rangos HTTP | ADR-011; rutas de 008 (D16); H-04 |
| `https://challenges.cloudflare.com/turnstile/v0/api.js` | Anti-bot del login | 003 R-008 |
| Página pública de UptimeRobot (`VITE_URL_ESTADO`) | Enlace «Estado del servicio» | `01-actores.md`; PO-21 |
| `urlPanelesTecnicos` (Grafana Cloud) | Enlace «Paneles técnicos» | 007; ADR-010 |

## 4. Garantías que espera de la API

- Las cookies `ku_acceso` y `ku_renovacion` son `HttpOnly; Secure; SameSite=Strict` y `ku_renovacion` solo viaja a la ruta de renovación (003 FR-020).
- CORS permite `https://panel.killauru.pe` con credenciales (001 FR-014) y la API comprueba `Origin` en las operaciones con cookie que cambian estado (**hueco H-01**).
- Toda respuesta con sesión lleva `Cache-Control: private, no-store` (001, ADR-006): el panel no cachea nada fuera de la memoria.
- Sin `estado`, el administrador recibe los 5 estados (006 R-004): el panel **siempre** envía `estado` (D6).
- Las lecturas de `/usuarios`, `/usuarios/revision-privilegios` y `/auditoria` quedan auditadas (D15): el panel las agrupa y no las refresca solas (R-027).
