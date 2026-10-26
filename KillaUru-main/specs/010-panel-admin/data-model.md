# Data Model: Panel de administración «KillaUru Ops»

El panel **no tiene base de datos** ni guarda nada en el navegador (FR-006). Este archivo describe el modelo **del cliente**: qué guarda en memoria, cómo nombra cada consulta, cómo traduce enumeraciones a textos y qué deriva. Los tipos de datos son los generados desde los YAML (`src/api/generado/`); aquí no se redefinen.

## 1. Sesión (en memoria, `src/sesion/`)

```ts
type EstadoSesion =
  | { tipo: 'desconocida' }                                  // arranque, antes de renovar
  | { tipo: 'anonima'; motivo?: 'sesion' | 'salida' }        // sin sesión → /login
  | { tipo: 'activa'; expiraAccesoUtc: Date; sesionVenceUtc: Date;
      desfaseMs: number; correo?: string };                  // correo llega después, de GET /me
```

| Transición | Disparador | Efecto |
|---|---|---|
| `desconocida → activa` | `POST /auth/renovar` 200 al arrancar | programa la renovación; pide `GET /me` |
| `desconocida → anonima` | renovar 401 | `/login` sin aviso |
| `anonima → activa` | `POST /auth/inicio-sesion` 200 | `/resumen` (o la ruta pedida antes) |
| `activa → activa` | renovar 200 (planificada, al enfocar o tras 401) | nuevos tiempos; reprograma |
| `activa → anonima('sesion')` | renovar 401 o 401 tras el reintento | `queryClient.clear()`; `/login?motivo=sesion` con «Tu sesión terminó. Vuelve a entrar.» |
| `activa → anonima('salida')` | «Salir» o mensaje `cerrada` del canal | `POST /auth/cerrar-sesion` (solo la pestaña que sale); `clear()`; `/login` |

- `desfaseMs = (expiraAccesoUtc − VIDA_ACCESO_MIN) − ahoraLocal` al recibir la respuesta (R-006).
- Renovación planificada a `expiraAccesoUtc − MARGEN_RENOVACION_S` (hora corregida).
- Barra de sesión: `restante / SESION_MAXIMA_HORAS`, acotada a [0, 1].
- Canal: `BroadcastChannel('ku-sesion')`, único mensaje `'cerrada'`.

## 2. Errores de la API (`src/api/problema.ts`)

`ErrorApi = { status, codigo, correlacionId?, bloqueadoHastaUtc? }`, armado desde `Problema` / `ProblemaBloqueo` (001, 003). Sin cuerpo legible (red, CORS) → `{ status: 0, codigo: 'sin_conexion' }`.

| `status` · `codigo` | Mensaje | Acción |
|---|---|---|
| 401 `token_invalido` | — | renovar y reintentar (FR-007) |
| 401 `credenciales_invalidas` | «Correo o contraseña incorrectos.» | — |
| 401 `sesion_invalida` · `cuenta_desactivada` | «Tu sesión terminó. Vuelve a entrar.» | `/login` |
| 403 `sin_permiso` | «No tienes permiso para esto.» | se queda |
| 403 `solo_administradores` | «Esta cuenta no tiene acceso al panel.» | — |
| 403 `antibot_fallido` | «No pudimos comprobar que eres una persona. Vuelve a intentarlo.» | nuevo desafío |
| 404 `no_encontrado` | «Esta anomalía ya no existe.» / «Esta cuenta ya no existe.» | quita la selección |
| 409 `transicion_no_permitida` | «Ya no se puede: la anomalía cambió de estado. Actualizamos el detalle.» | recarga el detalle |
| 409 `correo_en_uso` | «Ese correo ya tiene una cuenta.» | — |
| 409 `accion_sobre_si_mismo` | «No puedes cambiar tu propio rol ni desactivar tu propia cuenta.» | — |
| 409 `ultimo_administrador` | «Debe quedar al menos un administrador activo.» | — |
| 400 `revision_incompleta` | «La lista de administradores cambió. La cargamos de nuevo.» | recarga la revisión |
| 400 `enlace_invalido` | «El enlace no es válido o ya venció. Pide uno nuevo desde la app.» | — |
| 400 `datos_invalidos` | «Revisa los datos marcados.» (+ `errores` por campo) | — |
| 423 `cuenta_bloqueada` | «Cuenta bloqueada N min por intentos fallidos. Vuelve a intentar a las HH:MM.» | bloquea el formulario hasta esa hora (R-020) |
| 429 `limite_excedido` | «Hiciste muchas consultas seguidas. Espera un momento y vuelve a intentar.» | — |
| 0 `sin_conexion` | «No pudimos conectar con el servidor. Revisa tu conexión y vuelve a intentar.» | «Reintentar» |
| 5xx / otro | «Algo falló de nuestro lado. Código de soporte: `<correlacionId>`.» | «Reintentar» |

## 3. Consultas (TanStack Query) y su política de refresco

| Clave | Operación (contrato) | `staleTime` | Refresco automático | ¿La API la audita? |
|---|---|---|---|:---:|
| `['me']` | `GET /api/v1/me` (003 `obtenerMisDatos`) | ∞ (hasta salir) | no | no |
| `['mapa', filtros]` | `GET /api/v1/anomalies` (006 `consultarMapa`) | 30 s | 60 s y al enfocar | no |
| `['mapa', 'franja']` | `GET /api/v1/anomalies?estado=Activa` | 30 s | 60 s y al enfocar | no |
| `['anomalia', id]` | `GET /api/v1/anomalies/{id}` (004 `obtenerDetalleAnomalia`) | 30 s | al enfocar | no |
| `['transiciones', 5]` | `GET /api/v1/anomalies/transiciones?limite=5` (004) | 30 s | 60 s y al enfocar | no |
| `['por-revisar']` | `GET /api/v1/moderacion/por-revisar` (004) | 30 s | 60 s y al enfocar | no |
| `['pendientes']` | `GET /api/v1/moderacion/pendientes` (004) | 30 s | 60 s y al enfocar | no |
| `['indicadores', dias]` | `GET /api/v1/dashboard/indicadores` (007) | 10 min | al enfocar | no |
| `['usuarios', consulta]` | `GET /api/v1/usuarios` (003 `listarUsuarios`) | 10 min | no | **sí** |
| `['actores', ids]` | `GET /api/v1/usuarios?ids=` | 10 min | no | **sí** |
| `['revision']` | `GET /api/v1/usuarios/revision-privilegios` (003) | 10 min | no | **sí** |
| `['auditoria', filtros]` | `GET /api/v1/auditoria` (002 `consultarAuditoria`) | 10 min | no | **sí** |
| `['exportaciones-recientes']` | `GET /api/v1/auditoria?accion=DatasetExportado&tamano=5` | 10 min | no | **sí** |

**Mutaciones e invalidación**

| Mutación | Invalida |
|---|---|
| `moderarAnomalia` | `['anomalia', id]` (se reemplaza con la respuesta), `['mapa']`, `['por-revisar']`, `['pendientes']`, `['transiciones']` |
| `crearUsuario`, `cambiarRol`, `desactivarUsuario`, `activarUsuario` | `['usuarios']`, `['revision']`, `['actores']` |
| `registrarRevisionPrivilegios` | `['revision']` (se reemplaza con la respuesta), `['usuarios']` |
| `exportarDataset` | `['exportaciones-recientes']` |
| `reenviarCorreoUsuario`, `solicitarRecuperacion`, `verificarCorreo`, `fijarContrasenaConEnlace` | nada |

## 4. Filtros del mapa (dirección ↔ estado ↔ petición)

| En la dirección | Valores | Sin el parámetro (D6) | En la petición |
|---|---|---|---|
| `estado` | lista de `EstadoAnomalia` separada por comas | `Activa,PendienteConfirmacion` | `estado` (siempre) |
| `tipo` | `Bache`, `Hundimiento` (lista) | ambos | `tipo` solo si hay uno |
| `severidad` | `1`…`5` (lista) | las 5 | `severidad` solo si faltan algunas |
| `periodo` | `24h`, `7d`, `30d` | `7d` | `desde = ahora − periodo` |
| `anomalia` | `uuid` | — | no (abre el detalle) |
| `vista` | `detalle`, `lista` | `detalle` | no |

Valores desconocidos se descartan; un grupo vacío → sin petición y lista vacía. Rótulos de estado: `Activa` → «Activa», `PendienteConfirmacion` → «Pendiente de confirmar», `Expirada`, `Reparada`, `Descartada` (iguales).

## 5. Derivaciones (funciones puras en `modelo/`)

**Acciones permitidas (RN-07; solo ayuda, decide la API)**

| Acción (`AccionModeracion`) | Botón | Desde | Confirmación (título · texto · botón) |
|---|---|---|---|
| `MarcarReparada` | «Marcar reparada» | `Activa`, `PendienteConfirmacion` | «Marcar `<id>` como reparada» · «Dejará de verse en el mapa y no generará alertas. Si alguien vuelve a reportarla, reaparece y pasa otra vez por la confirmación.» · «Marcar reparada» |
| `Descartar` | «Descartar» (peligro) | `Activa`, `PendienteConfirmacion`, `Expirada` | «Descartar `<id>`» · «Para falsos positivos, por ejemplo un rompemuelles. Los reportes nuevos no la reactivan; si llegan de 3 o más vehículos en 7 días, aparecerá en Moderación.» · «Descartar» |
| `Reabrir` | «Reabrir» | `Reparada`, `Descartada` | «Reabrir `<id>`» · «Vuelve a Activa de inmediato, sin esperar confirmación, porque tú ya la verificaste. Se verá en el mapa y generará alertas.» · «Reabrir» |
| `MantenerDescartada` | «Mantener descartada» | `Descartada` con `porRevisar` (lista «Por revisar») | sin diálogo (como el prototipo); aviso «`<id>` sigue descartada» |

**«Qué pasó» de una transición (R-014)** — `actor` = `actorId ≠ null`

| `desde` → `hacia` | Actor | Texto |
|---|:---:|---|
| `null` → `Activa` | no | «Creada con el primer reporte» |
| `null` → `PendienteConfirmacion` | no | «Nueva: espera confirmación» |
| `PendienteConfirmacion` → `Activa` | no | «Confirmada por 2.º vehículo» |
| `Activa` / `PendienteConfirmacion` → `Expirada` | no | «Expiró sin reportes nuevos» |
| `Expirada` / `Reparada` → `Activa` | no | «Volvió con un reporte nuevo» |
| `Expirada` / `Reparada` → `PendienteConfirmacion` | no | «Volvió con un reporte grave: espera confirmación» |
| cualquiera → `Reparada` | sí | «Marcada reparada» |
| cualquiera → `Descartada` | sí | «Descartada» |
| `Reparada` / `Descartada` → `Activa` | sí | «Reabierta» |
| otra combinación | — | «Cambió a `<estado>`» |

**Indicadores (FR-015)**

| Campo de 007 | Rótulo | Unidad mostrada | Variación | Subir es | Ayuda |
|---|---|---|---|---|---|
| `eventosDia` | Eventos válidos (ayer) | — | «+8,1 %» | bueno | Eventos válidos (reportes procesados) del último día cerrado. |
| `conductoresActivos` | Conductores activos | — | «+12» | bueno | Cuentas que renovaron su sesión en los últimos 7 días. |
| `dispositivosDia` | Dispositivos 24 h | — | «−4» | bueno | Seudónimos distintos con eventos en 24 h. Solo se guarda el número. |
| `coberturaPct` | Cobertura | % | «+4 pts» | bueno | Tramos de 1 km con al menos un evento válido en 7 días. |
| `confirmacionPct` | Confirmación | % | «+1 pt» | bueno | Pendientes que pasaron a activa antes de expirar. |
| `reaparicionPct` | Reaparición | % | «+2 pts» | **malo** | Vuelven a activa tras expirar. Si sube, la caducidad es corta. |

`valor`/`variacion` `null` → «—» y «Sin calcular». Sufijo «vs. 7 d previos».

**Requiere atención (FR-018)**

| Fila | Dato | Número de la derecha | Destino |
|---|---|---|---|
| Puntos descartados con reportes nuevos · «3 o más vehículos en 7 días» | `por-revisar` | cantidad | `/moderacion?pestana=por-revisar` |
| Esperan confirmación · «`<id>` expira en N h» (el primero) | `pendientes` | cantidad | `/moderacion?pestana=pendientes` |
| Revisión mensual de privilegios · «Vence el 1 nov · 2 administradores» | `revision` | días que faltan («12 d»; «vencida» en rojo) | `/usuarios` |
| Estado del servicio · «Página pública de UptimeRobot» | — | ícono externo | `VITE_URL_ESTADO` |

«N temas» = cantidad por revisar + cantidad pendientes + 1 si la revisión vence en ≤ 15 días o ya venció. (El texto «3 o más vehículos en 7 días» es el del prototipo y describe RN-07; si el parámetro cambia en la API, el texto se ajusta en `textos.ts`.)

**Franja del corredor (R-013)**: `tramo(k) = floor(min(kmVia, 332.999) / 10)`, 34 tramos; `{ desdeKm, cantidad, severidadMax | null }`.

**Tramo demo (R-012)**: `dentroDelDemo(km) = TRAMO_DEMO_KM.desde ≤ km ≤ TRAMO_DEMO_KM.hasta`.

**Actor de auditoría (R-016)**: `interpretarActor(texto) → { tipo: 'id', id } | { tipo: 'correo', correo } | { tipo: 'invalido' }`; `elegirCuenta(correo, usuarios) → id | null` por igualdad sin mayúsculas.

**Exportación (R-017)**: `aSolicitud({ desde, hasta, tipo, estado, formato }) → SolicitudExportacion`; `nombreArchivo(desde, hasta, formato)`.

**Revisión (FR-034)**: `diasRestantes(venceUtc, ahora)`; `armarRevision(decisiones) → { confirmados, retirados }` (cada administrador en una sola lista).

**Usuarios (FR-031)**: `aConsulta(texto, rol, estado, pagina)` → `ids=[uuid]` si el texto es un UUID; `busqueda` si tiene ≥ 3 caracteres; nada si es más corto.

## 6. Textos de categorías y acciones de auditoría

| `CategoriaAuditoria` | Texto |
|---|---|
| `Sesion` | Sesión |
| `Moderacion` | Moderación |
| `Usuarios` | Usuarios |
| `Exportacion` | Exportación |
| `CuentaDelTitular` | Cuenta del titular |
| `AccesoADatos` | Acceso a datos |

| `AccionAuditada` | Texto |
|---|---|
| `UsuarioCreado` | Invitó un usuario |
| `RolCambiado` | Cambió un rol |
| `CuentaDesactivada` | Desactivó una cuenta |
| `CuentaActivada` | Activó una cuenta |
| `CuentaEliminada` | Eliminó su cuenta |
| `InicioSesionFallido` | Inicio de sesión fallido |
| `CuentaBloqueada` | Bloqueo temporal |
| `InicioSesionAdministrador` | Inició sesión |
| `RecuperacionSolicitada` | Pidió recuperar su contraseña |
| `ContrasenaCambiada` | Cambió su contraseña |
| `RevisionPrivilegios` | Revisión mensual de privilegios |
| `AnomaliaModerada` | según `contexto.accion`: Marcó reparada · Descartó · Reabrió · Mantuvo descartada |
| `DatasetExportado` | Exportó datos |
| `AccesoAdministrativoDatos` | Consultó datos personales |

## 7. Estados de pantalla (carga, vacío, error)

| Pantalla | Cargando | Vacío | Error |
|---|---|---|---|
| Resumen · indicadores | esqueleto de 6 tarjetas | «Aún no hay indicadores calculados.» | §2 + «Reintentar» |
| Resumen · atención | esqueleto de 4 filas | «Nada requiere atención ahora.» | fila con el error |
| Resumen · franja | esqueleto de barras | «No hay anomalías activas en el corredor.» | §2 |
| Resumen · últimos cambios | esqueleto de 5 filas | «Sin cambios recientes.» | §2 |
| Mapa · lista | «Cargando anomalías…» | «Ninguna anomalía coincide con los filtros.» | §2 |
| Mapa · detalle | esqueleto | «Selecciona una anomalía en el mapa o en la lista.» | §2 / «Esta anomalía ya no existe.» |
| Mapa · mapa base | lienzo vacío | — | «No se pudo cargar el mapa base.» (la lista sigue) |
| Moderación · por revisar | esqueleto de tabla | «Nada por revisar. Aparecerá aquí un punto descartado que reciba reportes de 3 vehículos en 7 días.» | §2 |
| Moderación · pendientes | esqueleto de tabla | «Ninguna anomalía espera confirmación.» | §2 |
| Usuarios | esqueleto de tabla | «Ningún usuario coincide.» | §2 |
| Auditoría | esqueleto de tabla | «Ningún registro coincide con los filtros.» | §2 |
| Exportación · recientes | esqueleto | «Todavía no hay exportaciones.» | §2 |

## 8. Configuración (`src/config/`)

| Nombre | Valor | Fuente |
|---|---|---|
| `ESTADOS_INICIALES_MAPA` | `['Activa', 'PendienteConfirmacion']` | D6, RN-08, RF-17 |
| `PERIODO_INICIAL_MAPA` | `'7d'` | prototipo |
| `TRAMO_DEMO_KM` | `{ desde: geojson.properties.demo_km_inicio, hasta: geojson.properties.demo_km_fin }` | RN-08 |
| `SESION_MAXIMA_HORAS` | `8` | RF-16 |
| `VIDA_ACCESO_MIN` | `5` | ADR-009 |
| `MARGEN_RENOVACION_S` | `60` | R-006 |
| `REFRESCO_OPERATIVO_S` | `60` | R-027 |
| `CACHE_LECTURA_AUDITADA_MIN` | `10` | R-027 |
| `TRAMO_FRANJA_KM` / `LARGO_CORREDOR_KM` | `10` / `geojson.properties.longitud_km` | prototipo; glosario (335.073 km) |
| `PUEBLOS_CORREDOR` | `[[0,'Inicio PE-1S'],[268,'Inicio demo'],[LARGO_CORREDOR_KM,'Fin Ayacucho']]` | prototipo |
| `DIAS_GRAFICO` | `[7, 14, 30]`, inicial `14` | 007 contrato |
| `TAMANO_PAGINA` | `50` | `convenciones-api.md` §2 |

**Variables de entorno** (`.env.example`; se validan al arrancar y el *build* falla si falta alguna):

| Variable | Producción | Pruebas / simulado |
|---|---|---|
| `VITE_URL_API` | `https://api.killauru.pe` | URL del entorno de pruebas / `http://localhost:4173` (MSW) |
| `VITE_URL_ESTILO_MAPA` | URL del estilo en `cdn.killauru.pe` (008, D16) | ídem o estilo local de prueba |
| `VITE_URL_PMTILES` | URL del PMTiles en `cdn.killauru.pe` (008, D16) | ídem |
| `VITE_TURNSTILE_SITEKEY` | clave del sitio | clave pública de prueba de Cloudflare |
| `VITE_URL_ESTADO` | página pública de UptimeRobot | ídem |
| `VITE_ENTORNO` | `produccion` | `pruebas` / `simulado` (muestra «DEMO») |
