# Research: Panel de administración «KillaUru Ops»

> Docs fija solo esto del panel: SPA **React + MapLibre GL JS**, estática en **Cloudflare Pages** (`panel.killauru.pe`), mapa base PMTiles leído **por rangos** desde R2, cookie `HttpOnly; Secure; SameSite=Strict`, sesión ≤ 8 h con renovación silenciosa, anti-bot en el login, HSTS/CSP/XFO/nosniff y **ninguna regla de negocio** (`estilo-arquitectonico.md`, `componentes-arquitectonicos.md` §5.1, ADR-009, ADR-011, RNF-09). Todo lo demás de este archivo es **Decisión de diseño SDD** (constitución I) y todas cuestan cero (constitución X): librerías MIT/Apache, Pages y GitHub Actions gratuitos.

## R-001 · Lenguaje, librería y empaquetador

- **Decision**: React 19 + TypeScript 5 en modo `strict` (con `noUncheckedIndexedAccess`) + Vite 6. Node 22 LTS en el CI.
- **Rationale**: React lo fija docs. TypeScript da tipos generados desde los contratos (R-002) y errores en compilación en vez de en producción. Vite produce estáticos con *code splitting* por ruta (necesario para cargar MapLibre solo en «Mapa», R-023) y su servidor de desarrollo es inmediato.
- **Alternatives**: Create React App (abandonado); Next.js (pide servidor o *export* estático con restricciones; no hay SSR que aprovechar en un panel privado); JavaScript sin tipos (los contratos no se podrían verificar al compilar).

## R-002 · Cliente HTTP: tipos generados desde los YAML

- **Decision**: `openapi-typescript` genera **solo tipos** (`.d.ts`, 0 bytes en el bundle) desde cada YAML de `specs/*/contracts/` (resuelve los `$ref` entre archivos hacia `plataforma.openapi.yaml`); `openapi-fetch` (~6 KB) hace las llamadas tipadas por ruta y método. Un *middleware* de `openapi-fetch` pone `credentials: 'include'`, convierte `application/problem+json` en `ErrorApi` y aplica la renovación (R-006). Los archivos generados van a `src/api/generado/` y se versionan; el CI los regenera y falla si hay diferencias (`git diff --exit-code`).
- **Rationale**: un campo mal escrito o un código de respuesta no previsto rompe la compilación (constitución IX). Sin generador de código en tiempo de ejecución ni clases que mantener.
- **Alternatives**: Orval o `openapi-generator` (generan hooks y clases: mucho código generado, acopla a una librería de estado); tipos escritos a mano (se desalinean de los YAML sin aviso); Axios (más peso y sin tipos por ruta).

## R-003 · Enrutador

- **Decision**: React Router 7 en modo librería (`createBrowserRouter`), rutas perezosas (`lazy`) por pantalla. Rutas: `/login`, `/login/recuperar`, `/resumen`, `/mapa`, `/moderacion`, `/usuarios`, `/auditoria`, `/exportaciones`, `/cuenta/verificar`, `/cuenta/restablecer`, `/cuenta/invitacion`; `/` → `/resumen`; cualquier otra → «No encontramos esta página». Los nombres siguen las direcciones del prototipo (`panel.killauru.pe/login`, `/resumen`…).
- **Rationale**: estándar conocido, carga diferida simple y estado en la dirección (`useSearchParams`) para los filtros del mapa (FR-026). Cloudflare Pages sirve `index.html` en cualquier ruta cuando no hay `404.html` (modo SPA).
- **Alternatives**: TanStack Router (tipado de búsqueda excelente, pero otra curva de aprendizaje para una sola persona); `wouter` (sin carga diferida integrada).

## R-004 · Estado del servidor

- **Decision**: TanStack Query 5. Cada pantalla usa *hooks* en `api/` con claves estables (`data-model.md` §3). El estado local de pantalla (diálogo abierto, pestaña) es estado de React; **no hay** almacén global. Al cerrar sesión o perderla: `queryClient.clear()`.
- **Rationale**: caché, reintentos, refresco al volver a la pestaña, `refetchInterval` y la invalidación tras moderar (FR-030) vienen hechos; evita Redux y su código repetitivo (KISS).
- **Alternatives**: SWR (sin mutaciones con invalidación tan clara); Redux Toolkit Query (trae Redux completo).

## R-005 · Arquitectura del código y regla de dependencia

- **Decision**: por **funcionalidades** con capas internas:

  ```text
  src/app            → marco, rutas, proveedores           (puede usar todo)
  src/funcionalidades/<f>/ui     → componentes React        (usa api, modelo de su f; index de otras f; compartido)
  src/funcionalidades/<f>/api    → hooks de TanStack Query  (usa modelo de su f; src/api; compartido sin ui)
  src/funcionalidades/<f>/modelo → funciones puras          (solo compartido/formato y config; sin React, red ni reloj)
  src/compartido, src/sesion, src/api, src/config           (nunca usan funcionalidades)
  ```

  Una funcionalidad usa a otra **solo** por su `index.ts` (igual que `Contracts/` en el backend). Sin ciclos. Lo verifica `dependency-cruiser` en el CI (`.dependency-cruiser.cjs`). Ningún `.ts`/`.tsx` supera **100 líneas** (prueba `pruebas/arquitectura/lineas.test.ts`; excluye `src/api/generado/`). En `modelo/` y `sesion/` está prohibido `Date.now()`/`new Date()` sin argumento (regla de ESLint `no-restricted-syntax`): el reloj se inyecta (`compartido/reloj.ts`), como `TimeProvider` en el backend.
- **Rationale**: cada pantalla de §5.1 es una carpeta con todo lo suyo; la lógica derivada (filtros por defecto, franja, textos de transición, acciones permitidas) queda en funciones puras fáciles de probar con valores límite (constitución VII).
- **Alternatives**: por capas globales (`components/`, `hooks/`, `services/`: una pantalla queda repartida en 4 carpetas); *Feature-Sliced Design* completo (6 capas: demasiado para 8 pantallas).

## R-006 · Sesión: cookies, renovación silenciosa y varias pestañas

- **Decision**:
  1. **Arranque**: el panel no sabe si hay sesión (no puede leer la cookie). Llama a `POST /auth/renovar`; 200 → guarda `SesionPanel` en memoria y pide `GET /me` (correo para el menú); 401 → `/login`. En las rutas `/cuenta/*` y `/login` no se intenta.
  2. **Hora del servidor**: el navegador no ve la cabecera `Date` (no es de las expuestas por CORS). Como el acceso dura 5 min (ADR-009), `horaServidor ≈ expiraAccesoUtc − 5 min` al recibir la respuesta; `desfase = horaServidor − ahoraLocal`. Solo sirve para los temporizadores y la barra de sesión.
  3. **Renovación planificada**: a `expiraAccesoUtc − 60 s` (hora corregida). Al volver la pestaña al frente (`visibilitychange`), si ya pasó, renueva antes de pedir datos.
  4. **Renovación por 401**: el *middleware* ve 401 con `codigo = token_invalido`, espera la **única** renovación en curso (promesa compartida) y reintenta **una vez**. Si la renovación da 401 (`sesion_invalida`, `cuenta_desactivada`) o el reintento vuelve a dar 401 → sesión terminada: `queryClient.clear()`, aviso y `/login?motivo=sesion`.
  5. **Pestañas**: cada una renueva por su cuenta con la cookie compartida (la API devuelve el mismo par dentro de 60 s, ADR-009). «Salir» publica `cerrada` en `BroadcastChannel('ku-sesion')`; las demás vacían su caché y van a `/login`. El mensaje no lleva datos.
- **Rationale**: cumple «renovación silenciosa» (§5.1) sin leer el token; la corrección la da el paso 4, el temporizador solo evita esperas. Nada se guarda en el navegador (FR-006).
- **Alternatives**: guardar `sesionVenceUtc` en `sessionStorage` para no renovar al recargar (evita una rotación, pero deja estado de sesión en el navegador); token en memoria con cabecera `Authorization` (lo prohíbe ADR-009 punto 5: expuesto a XSS).

## R-007 · CSRF y CORS

- **Decision**: (a) la API ya pone cookies `SameSite=Strict` y CORS con credenciales solo para `https://panel.killauru.pe` (001 FR-014); (b) el panel cambia estado solo con `POST`/`PUT`/`DELETE` y, cuando hay cuerpo, con `Content-Type: application/json` (no es un tipo «simple»: fuerza la verificación previa de CORS desde cualquier otro origen); (c) la CSP impide inyectar scripts (R-008). Falta la verificación de `Origin` en el servidor para las operaciones con cookie (SEG-21, no aplicado en 001): **hueco H-01**.
- **Rationale**: `panel`, `api` y `cdn` son el mismo *site*: `SameSite` no protege de un subdominio comprometido; la comprobación de `Origin` sí, y solo la puede hacer la API.
- **Alternatives**: *token* anti-CSRF propio (pide endpoint nuevo: no se inventan endpoints); cabecera personalizada (pide cambiar la lista de cabeceras permitidas en el CORS de 001).

## R-008 · Cabeceras de seguridad (`public/_headers`)

- **Decision** (Cloudflare Pages aplica `_headers` a cada respuesta):

  ```text
  /*
    Strict-Transport-Security: max-age=31536000; includeSubDomains
    Content-Security-Policy: default-src 'none'; script-src 'self' https://challenges.cloudflare.com; style-src 'self'; img-src 'self' data: blob: https://cdn.killauru.pe; font-src 'self'; connect-src 'self' https://api.killauru.pe https://cdn.killauru.pe; worker-src 'self'; frame-src https://challenges.cloudflare.com; manifest-src 'self'; base-uri 'none'; form-action 'self'; frame-ancestors 'none'; object-src 'none'; upgrade-insecure-requests
    X-Frame-Options: DENY
    X-Content-Type-Options: nosniff
    Referrer-Policy: strict-origin-when-cross-origin
    Permissions-Policy: camera=(), microphone=(), geolocation=(), payment=(), usb=()
    Cross-Origin-Opener-Policy: same-origin
  /index.html
    Cache-Control: no-cache
  /assets/*
    Cache-Control: public, max-age=31536000, immutable
  ```

  Para que la CSP no necesite `unsafe-inline`/`unsafe-eval`: CSS en archivos (Vite los extrae), estilos dinámicos solo por CSSOM (`element.style`, que la CSP no bloquea), MapLibre en su versión **CSP** con el *worker* servido como archivo propio (`setWorkerUrl`), fuentes propias (R-021). La URL de la API cambia en el entorno de pruebas: `_headers` se genera en el *build* desde `VITE_URL_API` (`scripts/generar-cabeceras.mjs`).
- **Rationale**: RNF-09 pide HSTS, CSP, XFO y nosniff; `Referrer-Policy` solo envía el origen a otros sitios (nunca ruta ni fragmento); `frame-ancestors 'none'` + XFO evitan el *clickjacking*; HSTS de toda la zona `killauru.pe` lo configura Cloudflare (001, SEG-07; hueco H-06).
- **Alternatives**: `Referrer-Policy: no-referrer` (más estricto, pero Turnstile y R2 no tienen garantía documentada de funcionar sin el origen); `<meta http-equiv>` (no admite `frame-ancestors` ni HSTS); `'unsafe-inline'` en estilos (rechazado: abre la puerta a inyección de CSS).

## R-009 · Anti-bot del login: Cloudflare Turnstile

- **Decision**: el script `https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit` se carga **solo** en `/login` (al montar el formulario), con `turnstile.render` en modo administrado; el token va en `turnstileToken` y se pide uno nuevo tras cada intento. En el entorno de pruebas y en el modo simulado se usan las claves de prueba públicas de Cloudflare (siempre aprueban). «Olvidé mi contraseña» no lleva Turnstile (el contrato no lo pide).
- **Fuente**: 003 R-008, FR-012 (Turnstile solo en el panel).

## R-010 · Mapa: MapLibre GL JS + PMTiles por rangos

- **Decision**: `maplibre-gl` (compilación CSP) + `pmtiles` (`Protocol` registrado con `maplibregl.addProtocol`), que pide al archivo de R2 solo los rangos de bytes de las teselas visibles. El estilo, las fuentes (glifos) y el *sprite* son los mismos que usa la app y los publica 008 en `cdn.killauru.pe` (D16); el panel recibe sus URL por configuración (`VITE_URL_ESTILO_MAPA`, `VITE_URL_PMTILES`). Las anomalías son una fuente GeoJSON aparte con la respuesta de 006 tal cual; sus íconos (5 formas × 5 severidades) se dibujan una vez en un `canvas` y se registran con `map.addImage`, así no dependen de glifos. Sin agrupamiento: el administrador ve pocos miles de puntos con el filtro inicial. Todo el código del mapa vive en un bloque que se carga al entrar a «Mapa» (R-023).
- **Rationale**: ADR-011 (mismo mapa base y estilo que la app; el panel no descarga el archivo entero). Los íconos propios permiten el borde punteado de «Pendiente», que una capa `circle` no dibuja.
- **Alternatives**: Leaflet con teselas públicas de OSM (rechazado por ADR-011); capas `circle` + `symbol` con texto (necesitan glifos para el número y no hacen bordes punteados).

## R-011 · Filtro inicial del mapa (D6) y parámetros

- **Decision**: sin parámetros en la dirección: `estado = [Activa, PendienteConfirmacion]`, todos los tipos, severidades 1–5 y periodo «7 d» (como el prototipo). Traducción a `GET /api/v1/anomalies`: `estado` siempre explícito (lista); `tipo` solo si hay **uno** marcado (el contrato admite un valor; con los dos se omite); `severidad` solo si no están las 5; `desde = ahora − periodo` (24 h, 7 d, 30 d) sobre la última detección; sin `bbox` (todo el corredor; el administrador no usa la caché por celdas, 006). Si un grupo queda vacío, no se pide nada. El contador «N de M» usa como M la cantidad con el filtro de estado y periodo, sin tipo ni severidad, que sale de la misma respuesta cuando tipo y severidad están completos; si no, se muestra solo N.
- **Fuente**: D6, RN-08, RF-17; 006 R-004 «nota para 010»; prototipo (`O.f`, `O.range = '7 d'`).

## R-012 · «Dentro / Fuera del tramo demo» y resaltado del tramo

- **Decision**: se deriva de `kmVia`: dentro si 268 ≤ `kmVia` ≤ 333 (`TRAMO_DEMO_KM`, RN-08). El resaltado del mapa usa el mismo intervalo sobre la polilínea si 008 la publica; mientras tanto se resalta con la capa `tramo_demo` del estilo compartido o, si no existe, se omite (la leyenda lo dice).
- **Rationale**: toda anomalía nace a ≤ 50 m de la vía (RN-13) y el polígono tiene 200 m de margen (RN-08): con `kmVia` en el intervalo, la posición está dentro. Evita bajar el polígono solo para una etiqueta.
- **Alternatives**: descargar `tramo_demo.geojson` desde R2 (su ruta la fija 008, D16); pedir el dato a la API (no existe y no se inventa).

## R-013 · Franja «Anomalías activas a lo largo del corredor»

- **Decision**: `GET /api/v1/anomalies?estado=Activa` (sin otros filtros); 34 tramos de 10 km (`[0,10)`, …, `[330,333]`); altura = cantidad, color = `--s{max severidad}`; tramo vacío = sin barra (el prototipo inventa valores de relleno: no se copian). Etiquetas «Pisco · 0», «Huaytará · 140», «Rumichaca · 196», «Vinchos · 268», «Ayacucho · 333» (del prototipo). Accesible: `role="img"` con resumen («N anomalías activas; más en km X–Y») y tabla oculta para lectores de pantalla.
- **Fuente**: 007 «fuera de alcance» («el panel la arma con el mapa del administrador»); D13 (`kmVia`); PO-09.

## R-014 · «Qué pasó» en «Últimos cambios» y en el historial

- **Decision**: texto derivado de `desde → hacia` y de si hay actor (`data-model.md` §5): p. ej. `PendienteConfirmacion → Activa` sin actor = «Confirmada por 2.º vehículo» (la única transición del sistema entre esos estados, RN-06). «Llegó a N vehículos: por revisar» no es una transición: se **omite** (PO-19); el mismo dato está en «Requiere atención» y en «Moderación».
- **Alternatives**: pedir un `motivo` a 004 (PO-19 lo propone; no imprescindible: la derivación es exacta para todas las transiciones de RN-07).

## R-015 · Correo de los actores (D15)

- **Decision**: el panel resuelve `actorId → correo` con **un** `GET /api/v1/usuarios?ids=` por pantalla (Auditoría: por página; Resumen: las de «Últimos cambios»; detalle: las del historial), con caché de 10 min por id. Cada llamada la audita la API como `AccesoAdministrativoDatos` (D15): por eso se agrupan los ids, no se repite en cada refresco y solo se pide cuando hay ids no nulos.
- **Rationale**: el prototipo muestra correos (admin.operaciones) y 002 dice que el panel los resuelve con `?ids=`. Una lectura auditada por pantalla es el precio de mostrar un dato personal: es lo que RF-15 quiere dejar registrado.
- **Alternatives**: mostrar solo el id corto en Resumen y Mapa (cero lecturas auditadas, menos claro; queda como alternativa en «Pendientes de confirmar»).

## R-016 · Filtro de auditoría por correo (PO-20)

- **Decision**: si el texto es un UUID → `actorId`. Si tiene `@` → `GET /api/v1/usuarios?busqueda=<correo>&tamano=20`; se toma la cuenta cuyo correo es **igual** (sin distinguir mayúsculas) y se usa su id; 0 coincidencias → aviso «No hay ninguna cuenta con ese correo. Las cuentas eliminadas solo se buscan por su id.» (RF-30 borra el correo). Otro texto → «Escribe un correo completo o un id.».
- **Rationale**: el contrato de 002 solo filtra por `actorId` y 003 ya ofrece la búsqueda; no hace falta tocar la API (PO-20 «No» requiere decisión). Coincidencia exacta para no filtrar por la persona equivocada.
- **Alternatives**: limitar el filtro a ids (más simple pero menos útil); aceptar el correo en 002 (rompería la regla «Auditoría no depende de Identidad»).

## R-017 · Exportación: descarga, fechas y «Recientes»

- **Decision**: `POST /api/v1/exportaciones` con `fetch` → `Blob` → `URL.createObjectURL` → `<a download>` y `revokeObjectURL`. Nombre propio `killauru-anomalias-AAAAMMDD-AAAAMMDD.csv|geojson` (no se depende de `Content-Disposition`, que CORS no expone). Fechas: los campos son días de Lima → `desdeUtc = día 00:00 −05:00`, `hastaUtc = día 23:59:59 −05:00`; vacías → `null`. Estado «Todos los estados» → `estados: []`. «Recientes»: `GET /api/v1/auditoria?accion=DatasetExportado&tamano=5` (lectura auditada; caché 10 min, se invalida al exportar); muestra fecha, formato, filtros y `filas` del `contexto` (002 `data-model` §4) y «tú» si el correo del actor coincide con el de `/me`. El prototipo dice «por ti»: como el panel no conoce su propio id, se muestran las de todas las cuentas con «Quién».
- **Fuente**: RF-20, 004 FR-028 y contrato (columnas, «las recientes salen de la auditoría»); PO-12.

## R-018 · Disponibilidad del servicio (PO-21)

- **Decision**: «Requiere atención» muestra «Estado del servicio · Página pública de UptimeRobot» con un ícono de enlace externo y **sin** «99,9 %» ni «último chequeo» (ningún contrato los entrega). El menú lateral repite el enlace «Estado del servicio» (como el prototipo). URL en `VITE_URL_ESTADO`.
- **Fuente**: `01-actores.md` (página de estado «enlazada desde la app y el panel»); EQ-07; PO-21.

## R-019 · Selector de periodo por pantalla

- **Decision**: el selector de la barra superior cambia según la pantalla: en «Mapa», 24 h · 7 d · 30 d (`desde`); en «Resumen», 7 d · 14 d · 30 d (`dias` de 007, que solo admite esos valores; 14 por defecto, como el gráfico del prototipo, que muestra 14 días); en las demás no aparece.
- **Rationale**: el prototipo muestra 24 h / 7 d / 30 d en todas las pantallas, pero 007 no acepta 24 h: no se inventa.

## R-020 · Mensaje de bloqueo

- **Decision**: «Cuenta bloqueada N min por intentos fallidos. Vuelve a intentar a las HH:MM.»: N = minutos entre la respuesta y `bloqueadoHastaUtc` (redondeado hacia arriba); HH:MM en hora de Lima. Contraseña y «Entrar» se reactivan solos a esa hora. Se quita el «3» del prototipo («por 3 intentos fallidos») porque el número es un parámetro de RN-19 (`IntentosMaximos`) y el panel no lo recibe (constitución I: no se copian valores).

## R-021 · Tipografías y sistema de diseño

- **Decision**: las del prototipo: **Overpass** (títulos y cifras), **Source Sans 3** (texto) y **Overpass Mono** (códigos y horas), servidas desde el propio origen con paquetes `@fontsource/*` (solo los pesos usados: 600–900, 400–700 y 400/600). Tokens de color, espaciado Fibonacci y escala tipográfica copiados del `:root` del prototipo en `src/compartido/estilos/tokens.css`, con tema claro y oscuro por `prefers-color-scheme`.
- **Nota**: el encargo menciona «Atkinson Hyperlegible»; el prototipo (fuente de la interfaz) usa Source Sans 3 para el texto. Se sigue el prototipo (ver «Pendientes de confirmar»).
- **Rationale**: Google Fonts obligaría a abrir la CSP a `fonts.googleapis.com`/`fonts.gstatic.com` y enviaría la IP del administrador a un tercero (FR-050).

## R-022 · Accesibilidad (WCAG 2.1 AA)

- **Decision**: (1) la pestaña «Lista» es la alternativa equivalente al mapa (cada anomalía es un botón); el lienzo tiene nombre y el teclado de MapLibre activo; (2) diálogos con `role="dialog"`, `aria-modal`, foco atrapado, `Esc` y retorno del foco; (3) avisos en una región `aria-live="polite"` (y `assertive` para errores de login); (4) gráficos con `role="img"`, resumen y tabla oculta; (5) severidad siempre con número y estado siempre con forma o texto; (6) prueba unitaria de contraste sobre los pares de tokens (texto ≥ 4,5:1, elementos gráficos ≥ 3:1) en los dos temas; (7) enlace «Saltar al contenido», `lang="es"`, título por ruta («Mapa · KillaUru Ops»); (8) `prefers-reduced-motion`; (9) reflujo: bajo 1024 px el menú lateral pasa a un menú desplegable y mapa/detalle se apilan; las tablas se desplazan dentro de su tarjeta (excepción permitida de 1.4.10).
- **Verificación**: axe (en componentes con `vitest-axe` y por ruta con `@axe-core/playwright`), pruebas de teclado en Playwright y revisión manual con NVDA + Firefox antes de cada versión.

## R-023 · Rendimiento

- **Decision**: presupuesto en `size-limit`: bloque inicial (marco + login + resumen) ≤ 120 KB gzip; bloque del mapa ≤ 300 KB gzip (MapLibre ~220 KB). Rutas perezosas; el mapa no se carga hasta entrar a «Mapa». Medición: marca `performance.mark('mapa-listo')` cuando la fuente de anomalías termina de dibujarse (`idle`), leída por Playwright en 10 cargas contra el entorno de pruebas (p95 ≤ 3 s); LCP del Resumen con Lighthouse CI (≤ 2,5 s) en el CI con el modo simulado.
- **Rationale**: docs no fija metas para el panel; se eligen metas habituales de *Core Web Vitals* y un tope que el mapa base por rangos permite (ADR-011).

## R-024 · Estrategia de pruebas

- **Decision** (primero las pruebas, constitución VII):

  | Nivel | Herramienta | Qué |
  |---|---|---|
  | Unitarias | Vitest | todo `modelo/`, `sesion/`, `compartido/formato` y `api/problema` (valores límite: 267,9/268/333/333,1 km; 59/60/61 s antes del vencimiento; `null` en indicadores) |
  | Componentes | Vitest + Testing Library + MSW | cada pantalla en carga, vacío, error y con datos; acciones y mensajes por código |
  | Contrato | `openapi-typescript` + `tsc`; Ajv 2020 | tipos al día (CI); cada dato simulado de `pruebas/msw/datos/` validado contra el esquema de su operación en el YAML |
  | E2E simulado | Playwright + compilación `simulado` (MSW en el navegador) | todos los flujos, en el CI, en Chromium, Firefox y Edge |
  | E2E real | Playwright contra «killauru-pruebas» | flujos críticos con la API real (login, renovación, 423, mapa D6, moderación + auditoría, usuarios, exportación, cuenta); antes de cada versión, desde la PC del autor |
  | Accesibilidad | `vitest-axe`, `@axe-core/playwright` | componentes y 11 rutas |
  | Seguridad | prueba de `_headers`; E2E de almacenamiento; comprobación tras desplegar; Mozilla Observatory | FR-006, FR-011 |
  | Arquitectura | `dependency-cruiser`; `lineas.test.ts` | FR-047 |

- **Datos simulados**: salen de los `example` de los YAML cuando existen y, si no, se escriben a mano y se validan con Ajv (así nunca se desalinean).

## R-025 · CI y despliegue

- **Decision**: `.github/workflows/panel-ci.yml` (en PR que toquen `panel/**` o `specs/*/contracts/**`): `npm ci` → tipos generados al día → `tsc` → ESLint → `dependency-cruiser` → Vitest con cobertura (`modelo/` ≥ 90 %) y líneas → `build` y `build:simulado` → `size-limit` → Playwright simulado + axe (3 navegadores) → Lighthouse CI. `panel-deploy.yml` (al unir a `main` con cambios en `panel/**`): `build` con las variables de producción → `wrangler pages deploy` (acción oficial `cloudflare/wrangler-action`) al proyecto `killauru-panel`, rama `main` → `scripts/verificar-cabeceras.mjs` contra `https://panel.killauru.pe`. Sin despliegues de vista previa (sus dominios `*.pages.dev` no son del *site* `killauru.pe`: no tendrían sesión) y sin integración Git de Pages. Volver atrás: «Rollback» al despliegue anterior en Pages o volver a desplegar el *commit* anterior (< 5 min). Secretos: `CLOUDFLARE_API_TOKEN` (permiso solo «Pages: editar») y `CLOUDFLARE_ACCOUNT_ID` (hueco H-03).
- **Fuente**: D10 (GitHub Actions, despliegue automático al unir a `main`); ADR-010 (Pages).

## R-026 · Formatos de número, fecha y tiempo

- **Decision**: `Intl.NumberFormat('es')` (coma decimal: «287,4», «8,1 %»); fechas y horas con `timeZone: 'America/Lima'` (sin horario de verano desde 1994: igual a UTC−5 fijo); auditoría en UTC con el formato del prototipo «2026-10-20 14:32:05»; tiempo relativo con `Intl.RelativeTimeFormat('es', { numeric: 'auto' })` («hace 2 h», «ayer a las 15:30»). Variación: «+8,1 %», «+12», «−4», «+4 pts», «+1 pt» (signo menos tipográfico).
- **Fuente**: prototipo; RNF-14; constitución III (día local UTC−5).

## R-027 · Refresco y lecturas auditadas

- **Decision**: mapa, listas de moderación y transiciones: `staleTime` 30 s, `refetchInterval` 60 s y al volver a la pestaña. Indicadores: `staleTime` 10 min (cambian una vez al día). Lecturas auditadas por la API (`/usuarios`, `/usuarios/revision-privilegios`, `/auditoria`): `staleTime` 10 min, sin `refetchInterval` ni refresco al enfocar; solo se vuelven a pedir por acción del usuario o tras una mutación propia. «Actualizado HH:MM:SS» = hora de la última respuesta correcta de la consulta principal de la pantalla.
- **Rationale**: 3 000 peticiones/h del administrador (RNF-11) sobran (≈ 4/min por pantalla abierta); D15 registra cada lectura de datos personales, y no conviene llenar la auditoría con refrescos automáticos.

## R-028 · Búsqueda rápida (`Ctrl K`)

- **Decision**: (1) UUID completo → `/mapa?anomalia=<id>` (el detalle se pide por id aunque no esté en los filtros); (2) 8 a 35 caracteres hexadecimales con o sin guiones → anomalía cargada cuyo id empieza así; (3) número («287» o «287,4») → la anomalía cargada con `kmVia` más cercano; (4) texto con `@` → `/usuarios?busqueda=<texto>`; (5) otro → «Sin resultados para «…»». Si el mapa aún no cargó, (2) y (3) piden primero el mapa con los filtros vigentes.
- **Fuente**: prototipo (`opsQ`).

## R-029 · Identificadores en pantalla

- **Decision**: las anomalías se muestran con los **8 primeros caracteres** del UUID en monoespaciado (p. ej. `0192f1c0`) y el completo en la ayuda y al copiar. El prototipo usa «A-1042», que no existe en los contratos (ids `uuid`, `convenciones-api.md` §2).

## R-030 · Tamaños de pantalla

- **Decision**: diseño de escritorio del prototipo (menú 220 px + contenido; mapa 61,8 % / detalle 38,2 %) desde 1280 px; de 1024 a 1279 px el detalle se estrecha; bajo 1024 px se apila (R-022). No se diseña para teléfono (el administrador trabaja en escritorio), pero nada se rompe a 320 px.

## Huecos para las specs del backend (sin endpoints nuevos)

| # | Feature | Hueco | Por qué lo necesita el panel |
|---|---|---|---|
| H-01 | 001 | Verificar `Origin = Api:OrigenPanel` en las peticiones autenticadas por cookie que cambian estado (SEG-21) | Defensa CSRF que el panel no puede hacer (R-007) |
| H-02 | 001 | `appsettings.Pruebas.json`: `Api:OrigenPanel` del panel de pruebas (p. ej. `http://localhost:4173`) y claves de prueba de Turnstile | E2E real contra «killauru-pruebas» (R-024) |
| H-03 | 001 / docs RNF-34 | Agregar `CLOUDFLARE_API_TOKEN` y `CLOUDFLARE_ACCOUNT_ID` (secretos de GitHub) al inventario de secretos | Despliegue del panel (R-025) |
| H-04 | 008 (D16) | Publicar en `cdn.killauru.pe` el estilo, glifos y *sprite* además del PMTiles, y CORS del *bucket* con `GET`, `HEAD` y `Range` desde `https://panel.killauru.pe` (propuesta en `panel/infra/r2-cors.json`) | Mapa base por rangos (R-010) |
| H-06 | 001 (SEG-07) | HSTS `includeSubDomains` y TLS mínimo 1.2 en la zona de Cloudflare; escaneo SSL Labs de `panel` | RNF-08 para el subdominio del panel |

(H-05 no se abrió: el `motivo` de las transiciones, PO-19, se deriva en el panel, R-014.)

## Propuesta de enmienda a la constitución

La constitución 1.1.0 está escrita para el backend .NET. **No se edita aquí**; se propone al autor una versión **1.2.0 (MENOR)**:

1. **II** — agregar: «El panel (`panel/`) se organiza por funcionalidades con capas `ui → api → modelo`; una funcionalidad usa a otra solo por su `index.ts`; sin ciclos; se verifica con `dependency-cruiser` en el CI del panel.»
2. **III** — agregar: «El panel no tiene reglas de negocio; los pocos valores de docs que usa para mostrar (8 h, km 268–333, estados iniciales de RN-08) viven en `panel/src/config/` citando su fuente.»
3. **VII** — agregar: «En el panel: Vitest para `modelo/` con cobertura ≥ 90 %, componentes con Testing Library + MSW, contrato con tipos generados y Ajv, E2E con Playwright y axe; CI en cada PR que toque `panel/`.»
4. **VIII** — ampliar la regla de 100 líneas a `.ts`/`.tsx` (excepto `panel/src/api/generado/`) y prohibir `Date.now()`/`new Date()` sin argumento en `modelo/` y `sesion/` (ESLint), equivalente a `BannedApiAnalyzers`.
5. **IX** — agregar: «Los clientes (app y panel) generan sus tipos desde los mismos YAML y el CI falla si están desalineados.»
6. **Restricciones** — nueva fila «Panel web: React + TypeScript + Vite, MapLibre GL JS con PMTiles por rangos, TanStack Query, Cloudflare Pages; despliegue con `wrangler` al unir a `main`» (fuente: `estilo-arquitectonico.md`, ADR-010, ADR-011).
7. **Flujo SDD** — punto 1: el orden incluye 008–010 después de 001–007 (D17).

## Pendientes de confirmar por el autor

1. **R-015** — ¿mostrar el correo del actor en Resumen y Mapa (1 lectura auditada por pantalla) o solo el id corto (0 lecturas)? Propuesta: correo.
2. **R-017** — «Recientes» de exportación muestra las de **todas** las cuentas con «Quién» (el prototipo dice «por ti»). Alternativa: agregar `id` a `MisDatos` en 003.
3. **R-019** — selector de periodo distinto por pantalla (Resumen 7/14/30 d por el contrato de 007).
4. **R-020** — quitar el «3» del mensaje de bloqueo del prototipo.
5. **R-021** — tipografía de texto: Source Sans 3 (prototipo) frente a Atkinson Hyperlegible (mencionada en el encargo).
6. **H-02** — origen local del panel en la configuración del entorno de pruebas (necesario para el E2E real).
