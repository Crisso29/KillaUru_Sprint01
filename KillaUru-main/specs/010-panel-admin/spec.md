# Feature Specification: Panel de administración «KillaUru Ops»

**Feature Branch**: `010-panel-admin`

**Created**: 2026-10-22

**Status**: Draft

**Input**: User description: "Panel web «KillaUru Ops» de KillaUru para el administrador: inicio de sesión con cookie HttpOnly y renovación silenciosa, Resumen con los indicadores de negocio y lo que requiere atención, Mapa del corredor con filtros (abre con «Activa + PendienteConfirmacion»), Moderación con las 4 acciones de RN-07, Usuarios con la revisión mensual de privilegios, Exportación CSV/GeoJSON y Auditoría de solo lectura; SPA React + MapLibre GL JS en Cloudflare Pages, sin reglas de negocio, fiel al prototipo y a los contratos de 001–007."

---

## 0. Ficha de la feature

| Campo | Valor |
|---|---|
| Qué es | La **interfaz web del administrador** (`panel.killauru.pe`): una SPA estática que muestra y ordena lo que la API decide. Además aloja las páginas públicas de los enlaces de correo (`/cuenta/…`, 003 R-006) |
| Qué **no** es | No decide permisos, estados, alcances ni umbrales (regla 8 de `estilo-arquitectonico.md`; constitución VI): si el panel habilita un botón que la API rechaza, gana la API. No guarda datos personales en el navegador. No calcula indicadores técnicos (están en Grafana, ADR-010) |
| Contenedor C2 | Panel Admin `[React · MapLibre GL JS · Cloudflare Pages]` (`Nivel2-diagrama-de-contenedores.md`) |
| Componentes (C3 del panel) | `componentes-arquitectonicos.md` §5.1: Inicio de sesión, Mapa con filtros, Moderación, Dashboard (Resumen), Usuarios, Exportación, Auditoría |
| Depende de | API por HTTPS/JSON con cookie: 003 (sesión, cuenta, usuarios), 006 (mapa), 004 (detalle, moderación, listas, transiciones, exportación), 007 (indicadores), 002 (auditoría); R2 `cdn.killauru.pe` (PMTiles y estilo, ADR-011; rutas que fija 008, D16); Cloudflare Turnstile (003 R-008); página de estado de UptimeRobot |
| Lo usan | Administradores (`01-actores.md`); conductores solo en `/cuenta/verificar` y `/cuenta/restablecer` |
| Pantallas del prototipo | `referencia/prototipo/killauru-diseno.html` → pestaña Panel («KillaUru Ops»): Inicio de sesión, Resumen, Mapa, Moderación, Usuarios, Auditoría, Exportaciones; sistema de diseño (colores, Overpass / Source Sans 3 / Overpass Mono, severidad 1–5, hito kilométrico, marca) |
| Prioridad | *Must*: HU-11, HU-12, HU-14 y HU-25 (parte panel) · *Should*: HU-13, HU-15, HU-19 · *Could*: HU-20. Se recorta lo que recorten 004 y 007 (`specs/README.md` §1) |
| Carpeta de código | `panel/` en la raíz del repositorio (junto a `backend/`) |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historias | HU-11, HU-12, HU-13, HU-14, HU-15, HU-19, HU-20, HU-25 (y HU-18b por el enlace de verificación) | `02-historias-de-usuario.md` |
| Requisitos funcionales | RF-15, RF-16, RF-17, RF-18, RF-19, RF-20, RF-24, RF-32, RF-38 (cierre de sesión del panel) | `03-requisitos-funcionales.md` |
| Atributos de calidad | RNF-08, RNF-09, RNF-10, RNF-13, RNF-14, RNF-26, RNF-29, RNF-30 | `04-atributos-de-calidad.md` |
| Reglas de negocio | RN-07 (acciones del administrador), RN-08 (filtro por defecto del panel), RN-19 (bloqueo) | `09-reglas-de-negocio.md` |
| Decisiones | ADR-006 (solo lo anónimo va al borde), ADR-009 (cookie `HttpOnly; Secure; SameSite=Strict`, sesión ≤ 8 h, renovación, anti-bot), ADR-010 (Pages, Grafana), ADR-011 (PMTiles en R2 por rangos, MapLibre GL JS) | `07-decisiones-arquitectonicas.md` |
| Escenarios | EQ-04 (acceso al panel sin token, expirado o por fuerza bruta) | `08-escenarios-y-atributos-de-calidad.md` |
| Restricciones | R-05 (`panel.killauru.pe`), R-06 (Ley 29733) | `05-restricciones.md` |
| Arquitectura | Panel = SPA sin reglas; pantallas y endpoints | `componentes-arquitectonicos.md` §5.1; `estilo-arquitectonico.md` (stack: React + MapLibre GL JS, Pages + R2); `arquitectura-inicial.md` (capa Navegador; zona Panel ↔ API: XSS/CSRF → cookie, CSP, HSTS; RN-08 del administrador) |
| Decisiones del autor | D6 (filtro por defecto), D13 («Eventos válidos (ayer)», `kmVia`), D14 («eventos válidos»), D15 (lecturas auditadas), D16 (rutas de R2 en 008), D17 (orden) | `specs/revision/decisiones.md` |
| Hallazgos | PO-09, PO-12, PO-19, PO-20, PO-21; SEG-07, SEG-21 | `specs/revision/revision-producto.md`, `revision-seguridad.md` |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Entrar al panel y mantener la sesión sin escribir la contraseña otra vez (Priority: P1)

Como administrador, quiero iniciar sesión en el panel con mi correo y contraseña y trabajar hasta 8 h sin volver a escribirla, para gestionar el sistema de forma segura (HU-11, RF-16, ADR-009).

**Why this priority**: *Must*; sin sesión no hay ninguna otra pantalla.

**Independent Test**: Con el servidor simulado (MSW) y contra el entorno de pruebas: entrar, borrar la cookie `ku_acceso` y seguir navegando; fallar 3 veces y ver el bloqueo; entrar con una cuenta de conductor.

**Acceptance Scenarios**:

1. **Given** credenciales válidas de un administrador y el desafío de Turnstile resuelto, **When** pulso «Entrar», **Then** el panel envía `POST /api/v1/auth/inicio-sesion` con `canal = panel` y `turnstileToken`, recibe `SesionPanel` (las cookies las pone el navegador) y abre «Resumen».
2. **Given** una sesión abierta, **When** el token de acceso vence (5 min) o la API responde 401 `token_invalido`, **Then** el panel llama **una sola vez** a `POST /api/v1/auth/renovar` (cookie `ku_renovacion`) y repite la petición, sin que yo lo note (§5.1 «renovación silenciosa»).
3. **Given** que la renovación responde 401 (`sesion_invalida`, `cuenta_desactivada`) o pasaron las 8 h, **When** ocurre, **Then** vuelvo a «Iniciar sesión» con el aviso «Tu sesión terminó. Vuelve a entrar.» y la caché de datos se vacía.
4. **Given** correo o contraseña incorrectos, **When** entro, **Then** veo «Correo o contraseña incorrectos.» (401 `credenciales_invalidas`, igual exista o no la cuenta, RN-19).
5. **Given** una cuenta bloqueada, **When** la API responde 423 `cuenta_bloqueada` con `bloqueadoHastaUtc`, **Then** veo «Cuenta bloqueada N min por intentos fallidos. Vuelve a intentar a las HH:MM.» con la contraseña y «Entrar» desactivados hasta esa hora (RN-19; 003 R-005; R-020).
6. **Given** una cuenta de conductor, **When** intenta entrar, **Then** ve «Esta cuenta no tiene acceso al panel.» (403 `solo_administradores`); si falla Turnstile, «No pudimos comprobar que eres una persona. Vuelve a intentarlo.» (403 `antibot_fallido`).
7. **Given** una sesión abierta, **When** pulso «Salir», **Then** el panel llama a `POST /api/v1/auth/cerrar-sesion`, vacía la caché y las demás pestañas abiertas del panel vuelven también al inicio de sesión (RF-38).
8. **Given** cualquier momento, **When** reviso el navegador, **Then** no hay tokens ni datos personales en `localStorage`, `sessionStorage` ni IndexedDB: el token solo vive en cookies `HttpOnly` que el JavaScript no puede leer (ADR-009 punto 5, RNF-10).
9. **Given** el menú lateral, **When** tengo sesión, **Then** veo «Sesión hasta HH:MM» (hora de Lima, de `sesionVenceUtc`) y una barra con el tiempo que queda.

---

### User Story 2 - Ver el mapa completo del corredor y filtrarlo (Priority: P1)

Como administrador, quiero ver todas las anomalías del corredor, incluidas las no confirmadas, y filtrarlas por tipo, severidad, fecha y estado, para monitorear la vía (HU-12, RF-17).

**Why this priority**: *Must*.

**Independent Test**: Abrir `/mapa` sin parámetros y comprobar la petición `estado=Activa,PendienteConfirmacion`; cambiar cada filtro; abrir el detalle de una anomalía.

**Acceptance Scenarios**:

1. **Given** que abro «Mapa» sin filtros en la dirección, **When** carga, **Then** el filtro «Estado» aparece con «Activa» y «Pendiente de confirmar» marcados y la petición es `GET /api/v1/anomalies?estado=Activa,PendienteConfirmacion&desde=…` (RF-17, RN-08, D6; periodo inicial «7 d»).
2. **Given** el mapa, **When** se dibuja, **Then** el mapa base es el PMTiles del corredor leído **por rangos** desde `cdn.killauru.pe` con el mismo estilo que la app, con la atribución de OpenStreetMap, y cada anomalía tiene **forma por estado** y **número de severidad** (nunca solo color) (ADR-011).
3. **Given** filtros de tipo, severidad, estado y periodo, **When** los cambio, **Then** el panel pide de nuevo con esos parámetros y el contador dice «N de M» (M = total con el filtro de estado vigente) (RF-17).
4. **Given** que desmarco todos los estados (o todos los tipos o severidades), **When** se aplica, **Then** no se hace ninguna petición y la lista dice «Ninguna anomalía coincide con los filtros.».
5. **Given** la pestaña «Lista», **When** la abro, **Then** cada fila dice «Tipo · km 287,4» (de `kmVia`), el id corto y «hace 2 h» (D13, PO-09, RNF-14).
6. **Given** que elijo una anomalía, **When** se abre el detalle, **Then** veo lo que entrega `GET /api/v1/anomalies/{id}`: tipo y severidad, estado, km y coordenadas, «Dentro / Fuera del tramo demo», clasificación, días distintos (15 d), reportes, último reporte, vence (vacío en estados finales), visible a conductores, reportes por día de 15 días e historial con quién hizo cada cambio.
7. **Given** una anomalía seleccionada, **When** copio la dirección, **Then** incluye los filtros y `?anomalia=<id>`: al abrirla otra vez se ve lo mismo.

---

### User Story 3 - Moderar anomalías (Priority: P2)

Como administrador, quiero marcar una anomalía como reparada, descartarla, reabrirla o mantenerla descartada, para que el mapa refleje la realidad de la vía (HU-19, RF-24, RN-07).

**Why this priority**: *Should*; es la 5.ª en el orden de recorte (con 004).

**Independent Test**: Desde el detalle de cada estado, comprobar qué acciones están habilitadas; ejecutar las 4 acciones; forzar un 409.

**Acceptance Scenarios**:

1. **Given** el detalle de una anomalía, **When** lo miro, **Then** «Marcar reparada» está habilitada solo desde `Activa` y `PendienteConfirmacion`, «Descartar» desde `Activa`, `PendienteConfirmacion` y `Expirada`, y «Reabrir» desde `Reparada` y `Descartada`; las deshabilitadas explican «No disponible desde X. Solo desde: …» (RN-07; es solo ayuda: decide la API).
2. **Given** una acción, **When** la elijo, **Then** se abre la confirmación con los textos del prototipo («Marcar … como reparada», «Descartar …», «Reabrir …») y «Queda en la auditoría con tu usuario»; al confirmar se envía `POST /api/v1/anomalies/{id}/moderacion` con `accion` y el detalle se actualiza con la respuesta.
3. **Given** «Moderación → Por revisar», **When** pulso «Mantener descartada» o «Reabrir», **Then** se envía `MantenerDescartada` o `Reabrir` y la fila sale de la lista (RF-24).
4. **Given** «Moderación → Esperan confirmación», **When** la abro, **Then** es solo lectura (reportes, primer reporte, expira), ordenada por vencimiento, con «Ver en el mapa» (RN-06, RN-07).
5. **Given** que otra persona o el sistema ya cambió la anomalía, **When** confirmo, **Then** la API responde 409 `transicion_no_permitida`, veo «Ya no se puede: la anomalía cambió de estado. Actualizamos el detalle.» y el detalle se recarga.
6. **Given** una moderación correcta, **When** termina, **Then** se refrescan el mapa, las listas, el contador del menú y «Últimos cambios de estado».

---

### User Story 4 - Ver el resumen del sistema (Priority: P2)

Como administrador, quiero ver los indicadores de negocio, lo que requiere atención y dónde se concentran los baches, para tomar decisiones sobre la plataforma (HU-13, RF-18).

**Why this priority**: *Should*; 4.ª en el orden de recorte (con 007). Si se recorta 007, el Resumen queda solo con «Requiere atención», la franja y «Últimos cambios».

**Independent Test**: Con las respuestas de ejemplo de 007, 004 y 003: comprobar textos, unidades, nulos y enlaces.

**Acceptance Scenarios**:

1. **Given** `GET /api/v1/dashboard/indicadores`, **When** se muestra, **Then** veo 6 indicadores con los rótulos del prototipo: «Eventos válidos (ayer)», «Conductores activos», «Dispositivos 24 h», «Cobertura», «Confirmación» y «Reaparición», cada uno con valor, variación «vs. 7 d previos» en su unidad (% relativo, absoluta o puntos), mini serie de 7 días y su ayuda (D13, D14).
2. **Given** un indicador o un día con valor `null`, **When** se muestra, **Then** aparece «—» con la ayuda «Sin calcular», nunca 0.
3. **Given** el gráfico «Eventos válidos por día», **When** cambio el periodo (7 d, 14 d, 30 d), **Then** se pide `dias` y se dibujan las barras con la línea punteada del periodo anterior.
4. **Given** «Paneles técnicos», **When** lo pulso, **Then** se abre `urlPanelesTecnicos` (Grafana) en otra pestaña (HU-13).
5. **Given** «Requiere atención», **When** se muestra, **Then** tiene: puntos descartados con reportes nuevos (cantidad de `por-revisar`), esperan confirmación (cantidad y «<id> expira en N h» del primero), revisión mensual de privilegios («Vence el 1 nov · 2 administradores» y los días que faltan) y «Estado del servicio», que abre la página pública de UptimeRobot **sin mostrar números** (PO-21).
6. **Given** la franja «Anomalías activas a lo largo del corredor», **When** se dibuja, **Then** sale del mapa del administrador con `estado=Activa`, agrupada cada 10 km por `kmVia`, con el color de la severidad más alta del tramo y los pueblos de referencia (007 «fuera de alcance», D13).
7. **Given** «Últimos cambios de estado», **When** se muestra, **Then** lista `GET /api/v1/anomalies/transiciones?limite=5` con hora, anomalía, «Qué pasó» (texto derivado de `desde`/`hacia`/actor), «Quién» (correo o «sistema») y el estado nuevo; cada fila abre la anomalía en el mapa.

---

### User Story 5 - Gestionar usuarios y revisar privilegios cada mes (Priority: P1)

Como administrador, quiero invitar usuarios, cambiar su rol, desactivarlos o activarlos y registrar la revisión mensual de privilegios, para mantener el control de acceso (HU-14, RF-19, RNF-30).

**Why this priority**: *Must* (HU-14); la revisión mensual es obligación legal (R-06).

**Independent Test**: Con dos administradores y varios conductores: filtrar, invitar, cambiar un rol, desactivar con motivo, activar, reenviar, hacer la revisión.

**Acceptance Scenarios**:

1. **Given** «Usuarios», **When** filtro por correo o id, rol y estado, **Then** veo correo, id, rol, estado (Activa, Desactivada, Invitación pendiente), correo verificado, alta y última sesión, paginados de 50 (`GET /api/v1/usuarios`); con un id completo se usa `ids=` y con texto, `busqueda=`.
2. **Given** «Invitar usuario», **When** envío correo y rol, **Then** se llama a `POST /api/v1/usuarios` y veo «Invitación enviada a …»; con 409 `correo_en_uso`, «Ese correo ya tiene una cuenta.».
3. **Given** el menú de una fila, **When** cambio el rol o desactivo (con motivo), **Then** se llama a `PUT …/rol` o `POST …/desactivar` y el aviso dice «Se aplica en máximo 5 minutos» / «pierde el acceso en máximo 5 minutos» (RNF-26); 409 `accion_sobre_si_mismo` → «No puedes cambiar tu propio rol ni desactivar tu propia cuenta.»; 409 `ultimo_administrador` → «Debe quedar al menos un administrador activo.».
4. **Given** una cuenta desactivada o con correo sin verificar, **When** abro su menú, **Then** veo «Activar cuenta» o «Reenviar verificación» («Reenviar invitación» si está pendiente).
5. **Given** el aviso «Revisión mensual de privilegios · vence el …», **When** pulso «Hacer revisión», **Then** veo cada administrador con «Confirmar» o «Retirar acceso», y al registrar se envía `POST /api/v1/usuarios/revision-privilegios` con `confirmados` y `retirados`; 400 `revision_incompleta` → «La lista de administradores cambió. La cargamos de nuevo.» (RF-19, RNF-30).
6. **Given** mi propia fila (mismo correo que `GET /api/v1/me`), **When** abro el menú, **Then** no aparecen «Cambiar rol» ni «Desactivar cuenta» (ayuda; la API igual responde 409).

---

### User Story 6 - Consultar la auditoría (Priority: P2)

Como administrador, quiero consultar el registro de auditoría completo en modo lectura, para tener trazabilidad de todas las acciones sensibles (HU-15, RF-15, RNF-29).

**Why this priority**: *Should*, **no recortable** (002).

**Independent Test**: Con registros de cada categoría: filtrar por categoría, fechas, actor por id y por correo; abrir una fila.

**Acceptance Scenarios**:

1. **Given** «Auditoría», **When** se carga, **Then** veo «Solo lectura», la tabla «Fecha y hora (UTC)», «Actor», «Categoría», «Acción», del más reciente al más antiguo, paginada, y «N registros · retención permanente» (`GET /api/v1/auditoria`).
2. **Given** una fila, **When** la abro, **Then** veo el objetivo y el `contexto` en JSON, tal como lo guarda el sistema.
3. **Given** el filtro «Actor (correo o id)», **When** escribo un id, **Then** se usa `actorId`; **When** escribo un correo, **Then** el panel lo busca con `GET /api/v1/usuarios?busqueda=` y usa el id de la cuenta cuyo correo coincide exactamente; si no hay ninguna, «No hay ninguna cuenta con ese correo. Las cuentas eliminadas solo se buscan por su id.» (PO-20; R-016).
4. **Given** las filas de una página, **When** se muestran, **Then** los correos de los actores salen de **un** `GET /api/v1/usuarios?ids=…`; `actorId = null` → «sistema»; un id sin cuenta (eliminada) → el id en monoespaciado (002, RF-30).

---

### User Story 7 - Exportar el dataset (Priority: P3)

Como administrador, quiero exportar las anomalías en CSV o GeoJSON con filtros, para analizarlas fuera de la plataforma, sin seudónimos (HU-20, RF-20).

**Why this priority**: *Could*; es lo primero que se recorta (con 004).

**Independent Test**: Exportar en ambos formatos con filtros; revisar el archivo y la auditoría.

**Acceptance Scenarios**:

1. **Given** el formulario «Nueva exportación», **When** elijo fechas (días de Lima), tipo, estado y formato y pulso «Generar archivo», **Then** se envía `POST /api/v1/exportaciones` con `formato`, `desdeUtc`, `hastaUtc`, `tipo` y `estados`, y el navegador descarga `killauru-anomalias-AAAAMMDD-AAAAMMDD.csv` (o `.geojson`).
2. **Given** el formulario, **When** lo miro, **Then** veo las columnas `id, tipo, severidad, estado, persistencia, km, latitud, longitud, reportes, primer_reporte, ultimo_reporte` y `seudonimo` tachado, y «Nunca incluye seudónimos. Queda en la auditoría con tus filtros.».
3. **Given** «Recientes», **When** se muestra, **Then** sale de `GET /api/v1/auditoria?accion=DatasetExportado` con fecha, formato, filtros y filas (del `contexto`), y «tú» cuando la hizo mi cuenta.

---

### User Story 8 - Abrir los enlaces de correo (Priority: P1)

Como conductor o administrador, quiero que el enlace de verificación, de recuperación o de invitación abra una página que termine la operación, para no perder mi cuenta (HU-18b, HU-25, RF-32; 003 R-006).

**Why this priority**: *Must*; sin estas páginas los enlaces de 003 no funcionan.

**Independent Test**: Abrir `/cuenta/verificar#token=…`, `/cuenta/restablecer#token=…` y `/cuenta/invitacion#token=…` con tokens válidos, vencidos y usados.

**Acceptance Scenarios**:

1. **Given** `/cuenta/verificar#token=X`, **When** abre, **Then** el panel quita el fragmento de la barra de direcciones, envía `POST /api/v1/auth/verificar-correo` y muestra «Listo, tu correo está verificado.» o, con 400 `enlace_invalido`, «El enlace no es válido o ya venció. Pide uno nuevo desde la app.».
2. **Given** `/cuenta/restablecer#token=X` o `/cuenta/invitacion#token=X`, **When** escribo una contraseña de 10 a 128 caracteres dos veces iguales, **Then** se envía `POST /api/v1/auth/fijar-contrasena` y veo «Tu contraseña cambió. Cerramos todas tus sesiones: vuelve a entrar.» (restablecer) o «Tu cuenta está lista. Ya puedes iniciar sesión.» (invitación).
3. **Given** «Olvidé mi contraseña» en el login, **When** envío un correo, **Then** se llama a `POST /api/v1/auth/recuperar` y veo siempre «Si el correo está registrado, te llegará un enlace que vence en 30 min.» (HU-25, RF-32).

### Edge Cases

- **Sesión de 8 h agotada** mientras se edita un formulario: la renovación falla, el panel vuelve al login y avisa; lo escrito se pierde (no se guarda en el navegador, FR-006).
- **Varias pestañas**: renuevan cada una con la cookie compartida; si dos renuevan a la vez, la API devuelve el mismo par dentro de 60 s (ADR-009). «Salir» en una pestaña cierra todas (FR-009).
- **Portátil suspendido**: al volver la pestaña al frente, si el acceso venció, se renueva antes de pedir datos.
- **Reloj del equipo adelantado o atrasado**: los avisos de tiempo usan la diferencia con la hora del servidor (R-006); la corrección real la da el reintento tras 401.
- **API caída o sin red**: cada pantalla muestra «No pudimos conectar con el servidor. Revisa tu conexión y vuelve a intentar.» con «Reintentar»; el mapa base sigue visible si ya cargó.
- **R2 no responde**: el mapa muestra la lista y el aviso «No se pudo cargar el mapa base.»; las anomalías siguen en la pestaña «Lista».
- **429 `limite_excedido`** (3 000/h del administrador, RNF-11): «Hiciste muchas consultas seguidas. Espera un momento y vuelve a intentar.».
- **403 `sin_permiso`** en una sesión abierta (por ejemplo, el rol cambió a conductor): «No tienes permiso para esto.»; en la siguiente renovación la API cierra la sesión.
- **Anomalía que ya no existe** (404 al abrir el detalle): «Esta anomalía ya no existe.» y se quita `?anomalia` de la dirección.
- **Indicadores sin calcular** (`ultimoDiaCerrado = null`): la franja muestra «—» y el aviso «Aún no hay indicadores calculados.».
- **Correo de un actor eliminado**: solo el id (RF-30).
- **Enlace de correo abierto dos veces**: la segunda vez la API responde 400 `enlace_invalido`; el panel nunca reenvía el token.
- **Dirección con filtros inválidos** (`?estado=Foo`): se ignoran los valores desconocidos y se vuelve al valor por defecto de ese filtro.

## Requirements *(mandatory)*

### Functional Requirements

**A. Base del panel**

- **FR-001**: El panel MUST ser una SPA en React + TypeScript servida como archivos estáticos desde Cloudflare Pages en `https://panel.killauru.pe`, solo por HTTPS. *(`estilo-arquitectonico.md`; ADR-010; R-05)*
- **FR-002**: El panel MUST NOT decidir permisos, estados ni alcances: muestra lo que entrega la API; las ayudas (botones deshabilitados, filas ocultas) nunca reemplazan la respuesta de la API, que siempre gana. *(`componentes-arquitectonicos.md` §5.1; Nivel2; constitución VI)*
- **FR-003**: Todo texto MUST estar en español con la voz del prototipo (de tú, frases cortas); números con coma decimal; horas en hora de Lima (UTC−5), salvo la auditoría, que se muestra en UTC. *(prototipo; constitución III «día local UTC−5»; R-026)*
- **FR-004**: El panel MUST funcionar en Chrome, Firefox y Edge, en sus 2 últimas versiones. *(RNF-13)*

**B. Sesión y seguridad (ADR-009, RNF-08, RNF-09)**

- **FR-005**: El inicio de sesión MUST usar `POST /api/v1/auth/inicio-sesion` con `canal = panel` y `turnstileToken` (Cloudflare Turnstile), y tratar 200, 401 `credenciales_invalidas`, 403 `solo_administradores` / `antibot_fallido`, 423 `cuenta_bloqueada` (con `bloqueadoHastaUtc`) y 429. *(HU-11, RF-16, RN-19; ADR-009 puntos 5 y 6; 003 FR-020, R-005, R-008)*
- **FR-006**: El panel MUST NOT leer, guardar ni enviar el token desde JavaScript: toda llamada a la API va con `credentials: 'include'` y la sesión vive solo en las cookies `HttpOnly` `ku_acceso` y `ku_renovacion`. MUST NOT escribir datos personales ni de sesión en `localStorage`, `sessionStorage`, IndexedDB ni cookies propias: el correo y los tiempos de sesión viven solo en memoria. *(ADR-009 punto 5; RNF-10; `arquitectura-inicial.md` zona Panel ↔ API)*
- **FR-007**: El panel MUST renovar la sesión en silencio con `POST /api/v1/auth/renovar` (cookie `ku_renovacion`, sin cuerpo): antes de que venza el acceso, al volver la pestaña al frente si ya venció, y ante cualquier 401 `token_invalido`; con **una sola** renovación en curso y **un** reintento de la petición original. Si la renovación responde 401, MUST volver a «Iniciar sesión» con aviso y vaciar la caché. *(§5.1; ADR-009 punto 4; RF-16; RNF-26)*
- **FR-008**: Los errores MUST tratarse de forma uniforme según `Problema.codigo`: 401 → renovación o login (FR-007); 403 `sin_permiso` → mensaje sin salir de la pantalla; 404 → «ya no existe»; 409 → mensaje por código; 423 → bloqueo; 429 → espera; 5xx y red → error con «Código de soporte: `correlacionId`». *(`convenciones-api.md` §3 y §4; T-SEG20)*
- **FR-009**: «Salir» MUST llamar a `POST /api/v1/auth/cerrar-sesion` (si el acceso venció, renueva y reintenta una vez), vaciar la caché y avisar a las otras pestañas del panel (`BroadcastChannel`), que vuelven al login. *(RF-38 parte panel; `specs/README.md` §2)*
- **FR-010**: El menú MUST mostrar «Sesión hasta HH:MM» (hora de Lima, de `sesionVenceUtc`) y una barra con la fracción que queda de la sesión máxima de 8 h. *(prototipo; RF-16)*
- **FR-011**: El sitio MUST servir, con el archivo `_headers` de Cloudflare Pages: `Strict-Transport-Security` (1 año, `includeSubDomains`), una `Content-Security-Policy` estricta que solo permite el propio origen, `https://api.killauru.pe`, `https://cdn.killauru.pe` y Turnstile, `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `Permissions-Policy` sin cámara, micrófono ni ubicación y `Cross-Origin-Opener-Policy: same-origin`; `index.html` sin caché y los recursos con huella, inmutables. *(RNF-08, RNF-09; SEG-07; R-008)*
- **FR-012**: Contra CSRF, el panel MUST cambiar estado solo con `POST`/`PUT`/`DELETE`, enviar cuerpo JSON con `Content-Type: application/json` cuando la operación tiene cuerpo, y depender de las cookies `SameSite=Strict` y del CORS con credenciales solo desde `https://panel.killauru.pe` (001 FR-014). La verificación de `Origin` en el servidor queda como hueco H-01 de 001. *(`arquitectura-inicial.md` zona Panel ↔ API; SEG-21; R-007)*
- **FR-013**: «Olvidé mi contraseña» MUST llamar a `POST /api/v1/auth/recuperar` y mostrar siempre el mismo texto, exista o no la cuenta. *(HU-25, RF-32)*
- **FR-014**: El panel MUST ofrecer las páginas públicas `/cuenta/verificar`, `/cuenta/restablecer` y `/cuenta/invitacion`, que leen el token del fragmento `#token=`, lo quitan de la barra de direcciones antes de llamar a la API (`verificar-correo` o `fijar-contrasena`) y nunca lo guardan ni lo reenvían. *(HU-18b, HU-25, RF-19 invitación; 003 R-006, FR-024)*

**C. Resumen (HU-13, RF-18)**

- **FR-015**: El Resumen MUST mostrar los 6 indicadores de `GET /api/v1/dashboard/indicadores` con los rótulos y ayudas del prototipo (el primero, «Eventos válidos (ayer)»), la variación en su unidad (eventos: % relativo; cuentas y dispositivos: diferencia absoluta; porcentajes: puntos), la mini serie de 7 días y «—» para los `null`; en «Reaparición», subir se pinta como malo. *(HU-13, RF-18; D13, D14; 007 FR-013)*
- **FR-016**: El gráfico «Eventos válidos por día» MUST dibujar `eventosPorDia` (barras) y `eventosPeriodoAnterior` (línea punteada) con el selector 7 d / 14 d / 30 d (`dias`, 14 por defecto). *(007 FR-010; prototipo; R-019)*
- **FR-017**: «Paneles técnicos» MUST abrir `urlPanelesTecnicos` en otra pestaña (`rel="noopener noreferrer"`). *(HU-13, RF-18; ADR-010)*
- **FR-018**: «Requiere atención» MUST armarse con `GET /api/v1/moderacion/por-revisar`, `GET /api/v1/moderacion/pendientes`, `GET /api/v1/usuarios/revision-privilegios` y el enlace a la página pública de estado de UptimeRobot, **sin números de disponibilidad** (ningún contrato los entrega). *(RF-24, RNF-30; `01-actores.md` UptimeRobot; 007 «fuera de alcance»; PO-21; R-018)*
- **FR-019**: La franja «Anomalías activas a lo largo del corredor» MUST armarse con `GET /api/v1/anomalies?estado=Activa`: cada 10 km de `kmVia`, altura = cantidad y color = severidad más alta del tramo, con los pueblos de referencia. *(007 «fuera de alcance»; D13; PO-09; R-013)*
- **FR-020**: «Últimos cambios de estado» MUST mostrar `GET /api/v1/anomalies/transiciones?limite=5` con un texto «Qué pasó» derivado de `desde`, `hacia` y `actorId` (tabla de `data-model.md` §5); los cambios que no son transiciones («llegó a N vehículos») no se muestran. *(004 `listarTransicionesRecientes`; PO-19; R-014)*

**D. Mapa (HU-12, RF-17)**

- **FR-021**: El mapa MUST usar MapLibre GL JS con el PMTiles del corredor leído por rangos HTTP desde `https://cdn.killauru.pe` y el mismo estilo que la app (rutas que publica 008, D16), y mostrar la atribución de OpenStreetMap. *(ADR-011 puntos 2–4; `estilo-arquitectonico.md`)*
- **FR-022**: Al abrir, el filtro «Estado» MUST traer marcados `Activa` y `PendienteConfirmacion` y enviarlos en `estado`; los filtros tipo, severidad, estado y periodo (24 h / 7 d / 30 d → `desde`) se envían a `GET /api/v1/anomalies` sin `bbox` (todo el corredor). *(RF-17, RN-08, HU-12; D6; R-011)*
- **FR-023**: Cada anomalía MUST dibujarse con forma por estado (Activa: círculo lleno, con anillo en 4 y 5; Pendiente: círculo con borde punteado; Expirada: anillo gris; Reparada: círculo con ✓; Descartada: círculo con ✕) y su número de severidad; el tramo demo (km 268–333) se resalta y hay leyenda. *(prototipo «Nunca solo color»; RN-08)*
- **FR-024**: La pestaña «Lista» MUST mostrar cada anomalía filtrada con «Tipo · km X» (`kmVia`), id corto, tiempo relativo y estado, y servir de alternativa accesible al mapa. *(D13; PO-09; RNF-14)*
- **FR-025**: El detalle MUST mostrar `DetalleAnomalia` completo (incluido `historial` con el actor) y derivar «Dentro / Fuera del tramo demo» de `kmVia` (268 ≤ km ≤ 333). *(004 `obtenerDetalleAnomalia`; prototipo; RN-08; R-012)*
- **FR-026**: Los filtros y la anomalía elegida MUST vivir en la dirección (`/mapa?estado=…&tipo=…&severidad=…&periodo=…&anomalia=…`); sin parámetros rige FR-022. *(Decisión SDD, R-011)*

**E. Moderación (HU-19, RF-24, RN-07)**

- **FR-027**: El detalle MUST ofrecer «Marcar reparada», «Descartar» y «Reabrir», habilitadas según la tabla de RN-07 y con la confirmación del prototipo; al confirmar, `POST /api/v1/anomalies/{id}/moderacion` con `MarcarReparada`, `Descartar` o `Reabrir`. *(HU-19, RF-24, RN-07)*
- **FR-028**: «Por revisar» MUST listar `GET /api/v1/moderacion/por-revisar` (severidad, tipo, id, km, vehículos en 7 d, reportes, último reporte) con «Mantener descartada» (`MantenerDescartada`) y «Reabrir» (con confirmación). *(RF-24, RN-07; 004 T-PO19)*
- **FR-029**: «Esperan confirmación» MUST listar `GET /api/v1/moderacion/pendientes` en solo lectura con «Ver en el mapa». *(RN-06, RN-07)*
- **FR-030**: Tras moderar, el panel MUST refrescar mapa, detalle, listas, contador del menú y transiciones; con 409 `transicion_no_permitida`, avisar y recargar el detalle. *(004 contrato; T-BD02)*

**F. Usuarios (HU-14, RF-19, RNF-30)**

- **FR-031**: «Usuarios» MUST listar `GET /api/v1/usuarios` con búsqueda por correo (`busqueda`) o id (`ids`), filtros de rol y estado (incluida «Invitación pendiente») y paginación; la búsqueda espera 400 ms sin teclear (o Enter) y al menos 3 caracteres, para no multiplicar lecturas auditadas. *(HU-14, RF-19; D15; R-027)*
- **FR-032**: «Invitar usuario» MUST llamar a `POST /api/v1/usuarios` con correo y rol. *(RF-19; 003 R-009)*
- **FR-033**: El menú de cada fila MUST ofrecer «Cambiar rol» (`PUT /rol`), «Desactivar cuenta» con motivo obligatorio de hasta 200 caracteres (`POST /desactivar`), «Activar cuenta» (`POST /activar`) y «Reenviar verificación» o «Reenviar invitación» (`POST /reenviar-correo`), y tratar 409 `accion_sobre_si_mismo` y `ultimo_administrador`. *(HU-14, RF-19, RNF-26)*
- **FR-034**: El aviso y el diálogo de la revisión mensual MUST usar `GET` y `POST /api/v1/usuarios/revision-privilegios`: fecha de vencimiento, días que faltan (o «vencida»), última revisión y una decisión por administrador; con 400 `revision_incompleta`, recargar la lista. *(RF-19, RNF-30)*

**G. Exportación (HU-20, RF-20)**

- **FR-035**: «Nueva exportación» MUST enviar `POST /api/v1/exportaciones` con `formato`, `desdeUtc` (00:00 de Lima), `hastaUtc` (23:59:59 de Lima), `tipo` (`null` = todos) y `estados` (vacío = todos), descargar el archivo con un nombre propio y mostrar las 11 columnas con `seudonimo` tachado. *(RF-20, HU-20; 004 FR-028, T-PO11)*
- **FR-036**: «Recientes» MUST salir de `GET /api/v1/auditoria?accion=DatasetExportado&tamano=5` (fecha, formato, filtros y `filas` del `contexto`), marcando «tú» las de mi cuenta. *(004 contrato; PO-12; R-017)*

**H. Auditoría (HU-15, RF-15, RNF-29)**

- **FR-037**: «Auditoría» MUST ser de solo lectura: `GET /api/v1/auditoria` paginado con filtros de categoría, acción, desde y hasta (UTC) y filas desplegables con `objetivo` y `contexto` en JSON. *(HU-15, RF-15)*
- **FR-038**: El filtro «Actor (correo o id)» MUST usar `actorId` con un id, y con un correo resolverlo por `GET /api/v1/usuarios?busqueda=` exigiendo coincidencia exacta (sin distinguir mayúsculas). *(PO-20; 002, 003 contratos; R-016)*
- **FR-039**: El correo de los actores MUST resolverse con **un** `GET /api/v1/usuarios?ids=` por página (máx. 200 ids), con caché de 10 min; `actorId = null` → «sistema»; sin cuenta → el id. *(002 spec «fuera de alcance»; 003 `listarUsuarios`; D15; R-015)*

**I. Transversal**

- **FR-040**: La búsqueda rápida («Ir a anomalía, km o correo…», `Ctrl K`) MUST abrir una anomalía por id (completo o los 8 primeros caracteres de las cargadas), la más cercana a un km, o «Usuarios» filtrado por un correo. *(prototipo; R-028)*
- **FR-041**: El marco MUST tener el menú lateral («Operación»: Resumen, Mapa, Moderación; «Administración»: Usuarios, Auditoría, Exportaciones), el contador de «Por revisar» en «Moderación», migas, «Actualizado HH:MM:SS» y la insignia «DEMO» solo fuera de producción. *(prototipo)*
- **FR-042**: Cada pantalla MUST tener estados de carga, vacío y error con los textos de `data-model.md` §7. *(prototipo; Decisión SDD)*
- **FR-043**: El panel MUST cumplir WCAG 2.1 AA: contraste, teclado completo con foco visible, lector de pantalla (nombres, roles, regiones `aria-live`), gráficos con texto alternativo y tabla equivalente, diálogos con foco atrapado y reflujo a 320 px sin desplazamiento en dos ejes (salvo mapa y tablas). *(prototipo «Nunca solo color»; Decisión SDD, R-022)*
- **FR-044**: El JavaScript inicial MUST ser ≤ 120 KB gzip y el del mapa (MapLibre + PMTiles) se carga solo al entrar a «Mapa» (≤ 300 KB gzip). *(Decisión SDD, R-023)*
- **FR-045**: Los tiempos MUST mostrarse en forma relativa legible («hace 2 h», «ayer a las 15:30») donde el prototipo lo hace. *(RNF-14)*
- **FR-046**: Los datos operativos (mapa, listas, transiciones) MUST refrescarse cada 60 s y al volver a la pestaña; las lecturas que la API audita (usuarios, auditoría, revisión) MUST tener caché de 10 min y no refrescarse solas. *(D15; RNF-11; R-027)*
- **FR-047**: El código MUST organizarse por funcionalidades con capas `ui → api → modelo` y una regla de dependencia verificada en CI; ningún `.ts`/`.tsx` (salvo los generados) supera 100 líneas; `modelo/` es puro (sin React, red ni reloj directo). *(constitución VIII; propuesta de enmienda en `research.md`; R-005)*
- **FR-048**: Los tipos del cliente HTTP MUST generarse desde los YAML de `specs/*/contracts/` y el CI MUST fallar si no están al día o si un dato simulado no cumple su esquema. *(constitución IX; R-002, R-024)*
- **FR-049**: Cada PR que toque `panel/` MUST pasar el CI del panel; al unir a `main`, se despliega solo a Cloudflare Pages y se comprueban las cabeceras; volver atrás = reactivar el despliegue anterior. *(D10; RNF-17, RNF-18 por analogía; ADR-010; R-025)*
- **FR-050**: El panel MUST NOT cargar analítica, fuentes ni scripts de terceros, salvo el de Turnstile en la página de inicio de sesión. *(RNF-10, ADR-007; constitución X; R-021)*

### Reglas y parámetros (configuración del panel, `src/config/`)

| Regla | Parámetro | Valor | Fuente |
|---|---|---|---|
| D6, RN-08 | `ESTADOS_INICIALES_MAPA` | `Activa`, `PendienteConfirmacion` | RF-17, RN-08 |
| — | `PERIODO_INICIAL_MAPA` | `7 d` | prototipo |
| RN-08 | `TRAMO_DEMO_KM` | 268 → 333 | RN-08 (`PoligonoDemo`) |
| RF-16 | `SESION_MAXIMA_HORAS` | 8 | RF-16, ADR-009 (solo para la barra; la API decide) |
| ADR-009 | `VIDA_ACCESO_MIN` | 5 | ADR-009 (para estimar la hora del servidor, R-006) |
| — | `MARGEN_RENOVACION_S` | 60 | Decisión SDD (R-006) |
| — | `REFRESCO_OPERATIVO_S` | 60 | Decisión SDD (R-027) |
| D15 | `CACHE_LECTURA_AUDITADA_MIN` | 10 | Decisión SDD (R-027) |
| — | `TRAMO_FRANJA_KM` | 10 | prototipo |
| — | `PUEBLOS_CORREDOR` | Pisco 0 · Huaytará 140 · Rumichaca 196 · Vinchos 268 · Ayacucho 333 | prototipo |
| 007 | `DIAS_GRAFICO` | 7, 14 (inicial), 30 | 007 contrato |
| — | Variables de entorno | `VITE_URL_API`, `VITE_URL_ESTILO_MAPA`, `VITE_URL_PMTILES`, `VITE_TURNSTILE_SITEKEY`, `VITE_URL_ESTADO`, `VITE_ENTORNO` | R-025 |

### Key Entities (modelo del cliente; detalle en `data-model.md`)

- **Sesión del panel**: `expiraAccesoUtc`, `sesionVenceUtc`, `rol`, desfase con el servidor, correo (de `/me`); solo en memoria.
- **Filtros del mapa**: tipos, severidades, estados, periodo, anomalía elegida; viven en la dirección.
- **Anomalía del mapa / Detalle**: `AnomaliaFeature` (006) y `DetalleAnomalia` (004), sin transformar.
- **Indicador del resumen**: `Indicador` (007) + rótulo, ayuda, formato de variación y sentido (subir = bueno o malo).
- **Tramo de la franja**: km inicial, cantidad de activas, severidad máxima.
- **Actor resuelto**: id → correo | «sistema» | id sin cuenta.
- **Problema**: `codigo`, `status`, `correlacionId` → mensaje en pantalla.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de las primeras peticiones del mapa sin parámetros en la dirección lleva `estado=Activa,PendienteConfirmacion` (prueba unitaria de filtros + E2E que intercepta la petición) (RF-17, D6).
- **SC-002**: 0 tokens y 0 datos personales en `localStorage`, `sessionStorage`, IndexedDB y `document.cookie` después de recorrer todos los flujos E2E (FR-006).
- **SC-003**: Producción responde con las 7 cabeceras de FR-011 (comprobación tras cada despliegue) y obtiene **A** o más en Mozilla HTTP Observatory; la CSP no tiene `unsafe-inline` ni `unsafe-eval` (RNF-09).
- **SC-004**: 0 violaciones de axe con las reglas WCAG 2.1 A y AA en las 11 rutas, y los flujos de login, filtro del mapa, moderación y revisión se completan solo con teclado (FR-043).
- **SC-005**: JS inicial ≤ 120 KB gzip y bloque del mapa ≤ 300 KB gzip, comprobado por `size-limit` en cada PR (FR-044).
- **SC-006**: Contra el entorno de pruebas, el mapa con sus anomalías está listo (marca `mapa-listo`) en ≤ 3 s p95 en 10 cargas, y el Resumen tiene LCP ≤ 2,5 s (FR-021, FR-044).
- **SC-007**: Con el acceso vencido o la cookie `ku_acceso` borrada, el 100 % de las peticiones se recupera con una sola renovación y sin ver el login; con la renovación rechazada, el 100 % termina en el login con aviso (FR-007).
- **SC-008**: Cada moderación, acción sobre usuarios, revisión y exportación hecha en E2E aparece en «Auditoría» con la acción correcta (RF-15, RNF-29).
- **SC-009**: Los E2E pasan en Chromium, Firefox y Edge (RNF-13).
- **SC-010**: 0 archivos `.ts`/`.tsx` escritos a mano con más de 100 líneas y 0 violaciones de la regla de dependencia (FR-047); cobertura de `modelo/` ≥ 90 % (constitución VII adaptada).
- **SC-011**: 0 diferencias entre los tipos generados y los YAML, y 100 % de los datos simulados válidos contra sus esquemas (FR-048).

## Fuera de alcance

- Reglas, umbrales, estados y alcance por rol: los decide la API (001–007).
- Indicadores técnicos (p95, errores, descartes, CPU, memoria, disco): Grafana Cloud (ADR-010, RNF-22); el panel solo enlaza.
- Cifras de disponibilidad (99,9 %, «último chequeo»): las muestra la página de UptimeRobot (PO-21).
- Publicación en R2 del PMTiles, el estilo, las fuentes del mapa y la polilínea: 008 (D16).
- Panel para empresas suscriptoras o autoridades (v1.5+, `01-actores.md`).
- Tema oscuro/claro elegido a mano: el panel sigue al sistema (`prefers-color-scheme`), sin guardar preferencia.
- Inglés u otros idiomas.

## Assumptions

- `panel.killauru.pe` y `api.killauru.pe` son el mismo *site* (`killauru.pe`): las cookies `SameSite=Strict` viajan (001 R-002).
- La API expone CORS con credenciales solo para `https://panel.killauru.pe` (001 FR-014) y en el entorno de pruebas para el origen local del panel (hueco H-02).
- 008 publica en `cdn.killauru.pe` el PMTiles, el estilo, las fuentes (glifos) y el *sprite* que usa la app, con CORS que permite `GET` con `Range` desde `panel.killauru.pe` (D16, hueco H-04).
- La URL de la página pública de UptimeRobot existe al desplegar (001 T050).
- Pantallas de escritorio (≥ 1280 px) son el uso principal; el panel refluye a 320 px por accesibilidad (R-030).
