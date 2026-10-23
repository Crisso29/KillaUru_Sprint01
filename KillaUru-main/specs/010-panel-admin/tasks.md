---

description: "Tareas del panel de administración «KillaUru Ops»"
---

# Tasks: Panel de administración «KillaUru Ops»

**Input**: `specs/010-panel-admin/` · **Requiere**: los contratos de 001–007 (YAML en `specs/*/contracts/`); para el E2E real, 001 (entorno «killauru-pruebas»), 003, 004, 006 y 007 desplegados; para el mapa base, las rutas de R2 de 008 (D16, H-04)

**Tests**: obligatorias y **primero** (constitución VII adaptada): cada fase empieza por sus pruebas, que deben fallar.

**Formato**: `- [ ] T### [P?] [US?] Descripción — ruta · (requisito)`. Rutas relativas a `panel/`. Abreviaturas: `F/` = `src/funcionalidades/`, `C/` = `src/compartido/`, `E2E/` = `pruebas/e2e/`. Las pruebas unitarias y de componentes van junto a su archivo (`x.ts` → `x.test.ts`). **Ningún `.ts`/`.tsx` > 199 líneas** (salvo `src/api/generado/`).

> **Recortables** (`specs/README.md` §1): Fase 10 (HU-20, la 1.ª), Fase 8 (HU-13, la 4.ª, si se recorta 007) y Fase 7 (HU-19, la 5.ª, si se recorta la moderación de 004). **No se recortan**: Fase 9 (HU-15) ni Fase 6 (HU-25 / enlaces de 003).

## Phase 1: Setup

- [ ] T001 `package.json` con las dependencias de `plan.md` (versiones exactas) y los scripts `dev`, `build`, `build:simulado`, `tipos`, `test`, `cobertura`, `e2e`, `e2e:pruebas`, `lint`, `deps`, `size` — `package.json` · (FR-001; R-001)
- [ ] T002 [P] `tsconfig.json` (`strict`, `noUncheckedIndexedAccess`), `vite.config.ts` (rutas perezosas, bloque `mapa`, modo `simulado` que copia el worker de MSW), `index.html` (`lang="es"`), `src/main.tsx`, `src/vite-env.d.ts` — `tsconfig.json`, `vite.config.ts`, `index.html`, `src/main.tsx` · (FR-001, FR-044)
- [ ] T003 [P] `eslint.config.js`: prohíbe `localStorage`, `sessionStorage`, `indexedDB`, `document.cookie` y `dangerouslySetInnerHTML` en todo `src/`, y `Date.now()` / `new Date()` sin argumento en `**/modelo/**` y `src/sesion/**` — `eslint.config.js` · (FR-006, FR-047)
- [ ] T004 [P] `.dependency-cruiser.cjs` con la regla de `research.md` R-005 (capas, `index.ts` entre funcionalidades, sin ciclos, `src/compartido` sin funcionalidades) — `.dependency-cruiser.cjs` · (FR-047)
- [ ] T005 [P] `lineas.test.ts`: recorre `src/**/*.{ts,tsx}` y `pruebas/**/*.ts` (menos `src/api/generado/`) y falla con archivo y número de líneas si alguno pasa de 100 — `pruebas/arquitectura/lineas.test.ts` · (FR-047, SC-010)
- [ ] T006 `generar-tipos.mjs` (`openapi-typescript` sobre los 6 YAML de `../specs/*/contracts/`) y los tipos generados versionados — `scripts/generar-tipos.mjs`, `src/api/generado/*.d.ts` · (FR-048; R-002)
- [ ] T007 [P] Configuración de `data-model.md` §8 con su fuente en un comentario por valor; `entorno.ts` valida las 6 variables y falla al arrancar si falta una; `.env.example` — `src/config/constantes.ts`, `src/config/corredor.ts`, `src/config/entorno.ts`, `.env.example` · (FR-022, FR-025, FR-010; D6)
- [ ] T008 [P] Tokens del `:root` del prototipo (claro y oscuro por `prefers-color-scheme`), base, fuentes propias con `@fontsource` (Overpass, Source Sans 3, Overpass Mono) y hojas por zona — `src/compartido/estilos/*.css` · (FR-003, FR-043, FR-050; R-021)
- [ ] T009 [P] `vitest.config.ts` (jsdom, cobertura de `src/**/modelo/**` ≥ 90 %), configuración (jest-dom, `vitest-axe`, MSW) y `renderizar.tsx` con proveedores de prueba — `vitest.config.ts`, `pruebas/configuracion.ts`, `pruebas/renderizar.tsx` · (SC-010)
- [ ] T010 MSW: manejadores por contrato y datos de ejemplo tomados de los `example` de los YAML (o escritos a mano), servidor (Vitest) y navegador (modo simulado) — `pruebas/msw/servidor.ts`, `pruebas/msw/navegador.ts`, `pruebas/msw/manejadores/*.ts`, `pruebas/msw/datos/*.json`, `pruebas/msw/mockServiceWorker.js` · (FR-048)
- [ ] T011 Prueba de contrato: `esquemas.ts` carga los YAML (resolviendo los `$ref` a 001) y `datosSimulados.test.ts` valida con Ajv 2020 cada archivo de `pruebas/msw/datos/` contra el esquema de la respuesta de su operación — `pruebas/contrato/esquemas.ts`, `pruebas/contrato/datosSimulados.test.ts` · (FR-048, SC-011)
- [ ] T012 [P] `playwright.config.ts`: proyectos `chromium`, `firefox` y `msedge`; modo `simulado` (sirve `build:simulado`) y modo `pruebas` (URL de «killauru-pruebas», H-02); `ayudas.ts` (entrar con usuarios de prueba, borrar la cookie `ku_acceso` con `context.clearCookies({ name })`) — `playwright.config.ts`, `pruebas/e2e/ayudas.ts` · (FR-004, FR-007, SC-009)

---

## Phase 2: Foundational — red, sesión, formato, marco y cabeceras

### Pruebas (primero; deben fallar)

- [ ] T013 [P] `problema.test.ts` (`Problema` y `ProblemaBloqueo` → `ErrorApi`; sin cuerpo → `sin_conexion`) y `errores.test.ts` (un mensaje por cada fila de `data-model.md` §2; 5xx con `correlacionId`) — `src/api/problema.test.ts`, `C/textos/errores.test.ts` · (FR-008)
- [ ] T014 [P] Formato: «287,4», «8,1 %», «+12», «−4», «+4 pts», «+1 pt»; 2026-10-21T04:59Z → 20 oct en Lima y 05:00Z → 21 oct; «hace 2 h», «ayer a las 15:30»; id corto de 8 caracteres — `C/formato/numeros.test.ts`, `C/formato/fechas.test.ts`, `C/formato/relativo.test.ts`, `C/formato/km.test.ts`, `C/formato/ids.test.ts` · (FR-003, FR-045; R-026, R-029)
- [ ] T015 [P] Sesión: transiciones de `data-model.md` §1; `planificador` (desfase con `VIDA_ACCESO_MIN`; renovar a 61/60/59 s del vencimiento → no/sí/sí; fracción restante acotada a [0, 1]); `renovador` (3 llamadas a la vez → 1 renovación; renovación 401 → sesión terminada) — `src/sesion/estadoSesion.test.ts`, `src/sesion/planificador.test.ts`, `src/sesion/renovador.test.ts` · (FR-007, FR-010)
- [ ] T016 [P] `middleware.test.ts` (MSW): toda petición con `credentials: 'include'` y sin cabecera `Authorization`; 401 `token_invalido` → 1 renovación y 1 reintento; segundo 401 → sesión terminada; 401 `credenciales_invalidas` no renueva; mutación con cuerpo → `Content-Type: application/json` — `src/api/middleware.test.ts` · (FR-006, FR-007, FR-012)
- [ ] T017 [P] `contraste.test.ts`: lee `tokens.css` y comprueba en los dos temas texto ≥ 4,5:1 (`--ink*` sobre `--surface`/`--bg`, `--on-s*` sobre `--s*`) y gráficos ≥ 3:1 (`--s1…--s5`, `--killa` sobre `--surface`) — `C/estilos/contraste.test.ts` · (FR-043)
- [ ] T018 [P] `Dialogo.test.tsx` y `Avisos.test.tsx`: foco al abrir, foco atrapado, `Esc` cierra y devuelve el foco; avisos en `aria-live`; 0 violaciones de axe — `C/ui/Dialogo.test.tsx`, `C/ui/Avisos.test.tsx` · (FR-043)
- [ ] T019 [P] `Marco.test.tsx`: menú «Operación» / «Administración», contador de «Por revisar», migas, «Actualizado HH:MM:SS», «DEMO» solo si `VITE_ENTORNO ≠ produccion`, «Sesión hasta HH:MM» y barra, «Salir» llama a `cerrarSesion` — `src/app/marco/Marco.test.tsx` · (FR-009, FR-010, FR-041)
- [ ] T020 [P] `cabeceras.test.ts`: la plantilla genera las 7 cabeceras de FR-011; CSP sin `unsafe-inline` ni `unsafe-eval`, con `frame-ancestors 'none'`, la API y el CDN en `connect-src` y Turnstile en `script-src`/`frame-src`; `index.html` `no-cache`; `/assets/*` inmutable — `scripts/cabeceras.test.ts` · (FR-011, FR-050, SC-003)

### Implementación

- [ ] T021 `reloj.ts`, `formato/*`, `textos/errores.ts`, `textos/estados.ts` — `C/reloj.ts`, `C/formato/`, `C/textos/` · (FR-003, FR-008, FR-045)
- [ ] T022 `problema.ts`, `clientes.ts`, `middleware.ts` y `consultas.ts` (tiempos de `data-model.md` §3) — `src/api/` · (FR-006, FR-007, FR-008, FR-012, FR-046)
- [ ] T023 `estadoSesion`, `planificador`, `renovador`, `canalSesion`, `ProveedorSesion` (arranque con renovar + `GET /me`, temporizador, `visibilitychange`) y `useSesion` — `src/sesion/` · (FR-007, FR-009, FR-010)
- [ ] T024 [P] Componentes comunes (`Boton`, `Dialogo`, `Avisos`, `InsigniaEstado`, `Severidad` con número, `Hito`, `MenuFiltro`, `EstadoVacio`, `EstadoError` con «Reintentar» y código de soporte, `Esqueleto`, `Ayuda` accesible con teclado, `Minigrafico`, `Icono`, `Paginador`, `Marca`, `TablaOculta`, `Pestanas`) — `C/ui/` · (FR-042, FR-043)
- [ ] T025 `App`, `rutas` (11 rutas perezosas), `RutaProtegida`, `PaginaNoEncontrada` y `TituloDeRuta` («Mapa · KillaUru Ops»), con enlace «Saltar al contenido» — `src/app/` · (FR-001, FR-043)
- [ ] T026 Marco: `Marco`, `MenuLateral`, `BarraSuperior`, `ResumenSesion`, `SelectorPeriodo` (opciones según la pantalla, R-019), `MenuMovil` (< 1024 px) — `src/app/marco/` · (FR-009, FR-010, FR-041, FR-043; R-030)
- [ ] T027 Plantilla de cabeceras y `generar-cabeceras.mjs` (escribe `dist/_headers` con `VITE_URL_API`) — `cabeceras/_headers.plantilla`, `scripts/generar-cabeceras.mjs` · (FR-011; R-008)

**Checkpoint**: `npm run lint && npm run deps && npm test` en verde (incluidas líneas y contrato).

---

## Phase 3: User Story 1 — Entrar y mantener la sesión (P1) 🎯 MVP

### Pruebas

- [ ] T028 [P] [US1] `bloqueo.test.ts`: `bloqueadoHastaUtc` a 4 min 01 s → «5 min»; hora en Lima; el formulario se reactiva exactamente a esa hora (reloj falso) — `F/login/modelo/bloqueo.test.ts` · (FR-005; R-020)
- [ ] T029 [P] [US1] `FormularioLogin.test.tsx` (MSW, Turnstile doble): envía `canal = panel` y `turnstileToken`; 200 → `/resumen`; 401, 403 ×2, 423 y 429 con sus textos; nuevo desafío tras cada error; botón desactivado mientras envía — `F/login/ui/FormularioLogin.test.tsx` · (FR-005)
- [ ] T030 [P] [US1] `PaginaRecuperar.test.tsx`: con 202 el mismo texto exista o no el correo; 429 con su aviso — `F/login/ui/PaginaRecuperar.test.tsx` · (FR-013)

### Implementación

- [ ] T031 [US1] `textos`, `modelo/bloqueo`, `useIniciarSesion`, `useRecuperar`, `Turnstile` (script cargado solo aquí, `render=explicit`), `FormularioLogin`, `AvisoBloqueo`, `PaginaLogin` (textos del prototipo y versión «KillaUru Ops vX · CRISSO DEV S.A.C.»), `PaginaRecuperar`, `index` — `F/login/` · (FR-005, FR-013, FR-050; R-009)
- [ ] T032 [US1] `sesion.spec.ts` (simulado): MSW responde 401 `token_invalido` una vez → la pantalla sigue sin ir al login; renovación rechazada → login con «Tu sesión terminó. Vuelve a entrar.»; «Salir» en una pestaña lleva a la otra al login — `E2E/sesion.spec.ts` · (FR-007, FR-009, SC-007)
- [ ] T033 [P] [US1] `login.spec.ts` (simulado y pruebas): entrar, credenciales malas, bloqueo 423 tras 3 fallos (pruebas), conductor → «Esta cuenta no tiene acceso al panel.», en los 3 navegadores — `E2E/login.spec.ts` · (FR-005, SC-009; EQ-04)
- [ ] T034 [P] [US1] `almacenamiento.spec.ts`: al final de cada flujo, `localStorage`, `sessionStorage`, IndexedDB y `document.cookie` vacíos de datos personales y tokens — `E2E/almacenamiento.spec.ts` · (FR-006, SC-002)

**Checkpoint**: se entra, se navega por el marco vacío y la sesión se renueva sola.

---

## Phase 4: User Story 2 — Mapa con filtros (P1)

### Pruebas

- [ ] T035 [P] [US2] `filtros.test.ts`: sin parámetros → `Activa,PendienteConfirmacion`, todos los tipos, 1–5, `7d`; ida y vuelta con la dirección; valores desconocidos descartados; grupo vacío → sin petición — `F/mapa/modelo/filtros.test.ts` · (FR-022, FR-026, SC-001; D6)
- [ ] T036 [P] [US2] `peticion.test.ts`: `estado` siempre; `tipo` solo con uno; `severidad` omitida con las 5; `desde` = ahora − 24 h / 7 d / 30 d con reloj fijo; nunca `bbox` ni `updatedSince` — `F/mapa/modelo/peticion.test.ts` · (FR-022; R-011)
- [ ] T037 [P] [US2] `tramoDemo.test.ts`: 267,9 → fuera; 268 y 333 → dentro; 333,1 → fuera — `F/mapa/modelo/tramoDemo.test.ts` · (FR-025; R-012)
- [ ] T038 [P] [US2] `simbolos.test.ts`: forma por cada estado; anillo solo en `Activa` 4–5; las 25 combinaciones llevan su número — `F/mapa/modelo/simbolos.test.ts` · (FR-023)
- [ ] T039 [P] [US2] `busqueda.test.ts`: UUID completo, prefijo de 8, «287,4» → la más cercana, correo → usuarios, texto suelto → sin resultados — `F/mapa/modelo/busqueda.test.ts` · (FR-040; R-028)
- [ ] T040 [US2] `PaginaMapa.test.tsx` (MSW, `LienzoMapa` doble): la primera petición lleva `estado=Activa,PendienteConfirmacion`; lista «Bache · km 287,4», id corto y «hace 2 h»; «N de M»; vacío; error con «Reintentar»; elegir una fila abre el detalle y pone `?anomalia=` — `F/mapa/ui/PaginaMapa.test.tsx` · (FR-022, FR-024, FR-026, FR-042)
- [ ] T041 [US2] `DetalleAnomalia.test.tsx`: los 17 campos; `venceUtc: null` → «—»; dentro/fuera del demo; historial con «sistema» y correo (un solo `?ids=`); 404 → «Esta anomalía ya no existe.» — `F/mapa/ui/DetalleAnomalia.test.tsx` · (FR-025, FR-039)
- [ ] T042 [P] [US2] `BusquedaRapida.test.tsx`: `Ctrl K` enfoca; Enter con id, km y correo navega; sin resultados avisa — `src/app/marco/BusquedaRapida.test.tsx` · (FR-040)

### Implementación

- [ ] T043 [US2] `textos`, `modelo/filtros`, `modelo/peticion`, `modelo/tramoDemo`, `modelo/simbolos`, `modelo/busqueda` — `F/mapa/modelo/`, `F/mapa/textos.ts` · (FR-022…FR-026, FR-040)
- [ ] T044 [US2] `useMapaAdmin` (también la variante de la franja, `estado=Activa`) y `useDetalleAnomalia` con los tiempos de `data-model.md` §3 — `F/mapa/api/` · (FR-022, FR-046)
- [ ] T045 [US2] `LienzoMapa`, `capas` e `iconos`: MapLibre CSP con `setWorkerUrl`, protocolo `pmtiles`, estilo y PMTiles de `entorno.ts`, atribución OSM, 25 íconos con `addImage`, resaltado del tramo demo, `performance.mark('mapa-listo')`, aviso si falla el mapa base — `F/mapa/ui/LienzoMapa.tsx`, `F/mapa/ui/capas.ts`, `F/mapa/ui/iconos.ts` · (FR-021, FR-023, FR-044; R-010)
- [ ] T046 [US2] `BarraFiltros` (menús con casillas, «Todos», «Listo»), `Leyenda`, `PanelLateral` (pestañas «Detalle» / «Lista · N») y `ListaAnomalias` — `F/mapa/ui/` · (FR-022, FR-023, FR-024, FR-043)
- [ ] T047 [US2] `DetalleAnomalia`, `DatosAnomalia`, `ReportesPorDia` (con tabla oculta) e `Historial` (textos de `data-model.md` §5, actores con `useActores`) — `F/mapa/ui/` · (FR-025, FR-039, FR-045)
- [ ] T048 [US2] `PaginaMapa` (filtros y selección en la dirección) e `index` — `F/mapa/ui/PaginaMapa.tsx`, `F/mapa/index.ts` · (FR-026)
- [ ] T049 [US2] `BusquedaRapida` en la barra superior — `src/app/marco/BusquedaRapida.tsx` · (FR-040)
- [ ] T050 [US2] `mapa.spec.ts`: intercepta la primera petición sin parámetros (SC-001); cambia filtros; la dirección copiada abre lo mismo; la lista es navegable con teclado — `E2E/mapa.spec.ts` · (FR-022, FR-026, SC-001)

---

## Phase 5: User Story 5 — Usuarios y revisión mensual (P1)

### Pruebas

- [ ] T051 [P] [US5] `consulta.test.ts` (UUID → `ids`; < 3 caracteres → sin filtro; ≥ 3 → `busqueda`) y `revision.test.ts` (días que faltan, vencida, cada administrador en una sola lista) — `F/usuarios/modelo/consulta.test.ts`, `F/usuarios/modelo/revision.test.ts` · (FR-031, FR-034)
- [ ] T052 [US5] `PaginaUsuarios.test.tsx`: filtros (incluida «Invitación pendiente»), paginación «N de total usuarios», espera de 400 ms o Enter (temporizadores falsos), vacío y error — `F/usuarios/ui/PaginaUsuarios.test.tsx` · (FR-031, FR-042)
- [ ] T053 [US5] `MenuUsuario.test.tsx` y diálogos: acciones según estado y verificación; en la fila propia no hay «Cambiar rol» ni «Desactivar»; motivo obligatorio ≤ 200; 409 `accion_sobre_si_mismo`, `ultimo_administrador` y `correo_en_uso` con sus textos aunque el botón estuviera visible — `F/usuarios/ui/MenuUsuario.test.tsx` · (FR-002, FR-032, FR-033)
- [ ] T054 [US5] `DialogoRevision.test.tsx`: una decisión por administrador; cuerpo `{confirmados, retirados}`; 400 `revision_incompleta` recarga la lista — `F/usuarios/ui/DialogoRevision.test.tsx` · (FR-034)

### Implementación

- [ ] T055 [US5] `textos`, `modelo/consulta`, `modelo/revision`, `useUsuarios`, `useAccionesUsuario` (invalidaciones de `data-model.md` §3), `useRevision` y `useActores` (un `?ids=` por lote, caché 10 min) — `F/usuarios/modelo/`, `F/usuarios/api/`, `F/usuarios/textos.ts` · (FR-031, FR-033, FR-034, FR-039, FR-046)
- [ ] T056 [US5] `PaginaUsuarios`, `FiltrosUsuarios`, `TablaUsuarios` (pie «Ninguna cuenta está unida a viajes ni detecciones (RNF-10)») y `MenuUsuario` — `F/usuarios/ui/` · (FR-031, FR-033)
- [ ] T057 [US5] `DialogoInvitar`, `DialogoRol`, `DialogoDesactivar`, `AvisoRevision`, `DialogoRevision` e `index` — `F/usuarios/ui/`, `F/usuarios/index.ts` · (FR-032, FR-033, FR-034)
- [ ] T058 [US5] `usuarios.spec.ts`: invitar, cambiar rol, desactivar con motivo, activar, reenviar y revisión mensual — `E2E/usuarios.spec.ts` · (FR-031…FR-034, SC-008)

---

## Phase 6: User Story 8 — Páginas de los enlaces de correo (P1)

### Pruebas

- [ ] T059 [P] [US8] `tokenDelFragmento.test.ts` (lee `#token=`, llama a `history.replaceState` sin el fragmento, sin token → error) y `contrasena.test.ts` (9/10/128/129 caracteres; las dos iguales) — `F/cuenta/modelo/tokenDelFragmento.test.ts`, `F/cuenta/modelo/contrasena.test.ts` · (FR-014)
- [ ] T060 [US8] `PaginaVerificar.test.tsx` y `PaginaFijarContrasena.test.tsx` (MSW): 204 y 400 `enlace_invalido` con sus textos; restablecer e invitación con su mensaje final; el token no se envía dos veces — `F/cuenta/ui/PaginaVerificar.test.tsx`, `F/cuenta/ui/PaginaFijarContrasena.test.tsx` · (FR-014)

### Implementación

- [ ] T061 [US8] `textos`, modelos, `useEnlaces`, `PaginaVerificar`, `PaginaFijarContrasena` (modo restablecer / invitación) e `index` — `F/cuenta/` · (FR-014)
- [ ] T062 [US8] `cuenta.spec.ts`: las 3 páginas; el fragmento no queda en el historial ni en la barra — `E2E/cuenta.spec.ts` · (FR-014)

---

## Phase 7: User Story 3 — Moderación (P2) · recortable con HU-19

### Pruebas

- [ ] T063 [P] [US3] `acciones.test.ts`: tabla de RN-07 completa (5 estados × 4 acciones); `MantenerDescartada` solo con `porRevisar`; texto «No disponible desde X. Solo desde: …» — `F/moderacion/modelo/acciones.test.ts` · (FR-027, FR-028)
- [ ] T064 [US3] `AccionesModeracion.test.tsx`: textos de confirmación del prototipo; `POST` con `accion`; el detalle se reemplaza con la respuesta; 409 `transicion_no_permitida` → aviso y recarga aunque el botón estaba habilitado; invalidaciones de `data-model.md` §3 — `F/moderacion/ui/AccionesModeracion.test.tsx` · (FR-002, FR-027, FR-030)
- [ ] T065 [US3] `PaginaModeracion.test.tsx`: pestañas «Por revisar N» / «Esperan confirmación N»; «Mantener descartada» sin diálogo y «Reabrir» con diálogo; pendientes de solo lectura con «Ver en el mapa»; textos de vacío y del aviso del prototipo — `F/moderacion/ui/PaginaModeracion.test.tsx` · (FR-028, FR-029, FR-042)

### Implementación

- [ ] T066 [US3] `textos`, `modelo/acciones`, `useListasModeracion`, `useModerar`, `AccionesModeracion` y `DialogoModeracion` (con «Queda en la auditoría con tu usuario») — `F/moderacion/` · (FR-027, FR-030)
- [ ] T067 [US3] `PaginaModeracion`, `TablaPorRevisar`, `TablaPendientes` e `index`; el detalle del mapa usa `AccionesModeracion` desde `moderacion/index.ts` y el menú lateral, el contador — `F/moderacion/ui/`, `F/moderacion/index.ts`, `F/mapa/ui/DetalleAnomalia.tsx` · (FR-028, FR-029, FR-041)
- [ ] T068 [US3] `moderacion.spec.ts`: las 4 acciones y un 409 provocado por otra pestaña — `E2E/moderacion.spec.ts` · (FR-027…FR-030, SC-008)

---

## Phase 8: User Story 4 — Resumen (P2) · recortable con HU-13

### Pruebas

- [ ] T069 [P] [US4] `indicadores.test.ts`: rótulo, unidad y formato de variación por indicador; «Reaparición» al subir = malo; `null` → «—» — `F/resumen/modelo/indicadores.test.ts` · (FR-015)
- [ ] T070 [P] [US4] `franja.test.ts`: km 0, 9,99, 10, 332,9 y 333 → tramos 0, 0, 1, 33 y 33; severidad máxima; tramo vacío — `F/resumen/modelo/franja.test.ts` · (FR-019; R-013)
- [ ] T071 [P] [US4] `atencion.test.ts`: «N temas»; revisión a 16/15 días → no/sí suma; vencida; «<id> expira en N h» — `F/resumen/modelo/atencion.test.ts` · (FR-018)
- [ ] T072 [P] [US4] `transicion.test.ts`: cada fila de la tabla «Qué pasó» de `data-model.md` §5 — `F/resumen/modelo/transicion.test.ts` · (FR-020; R-014)
- [ ] T073 [US4] `PaginaResumen.test.tsx` (MSW con el `example` de 007): 6 rótulos, «—», selector 7/14/30 envía `dias`, «Paneles técnicos» con `target="_blank"` y `rel="noopener noreferrer"`, «Estado del servicio» sin cifras, últimos cambios con «sistema», una tarjeta con error no rompe las demás — `F/resumen/ui/PaginaResumen.test.tsx` · (FR-015…FR-020, FR-042)

### Implementación

- [ ] T074 [US4] `textos`, `modelo/indicadores`, `modelo/franja`, `modelo/atencion`, `modelo/transicion`, `useIndicadores` y `useTransiciones` — `F/resumen/modelo/`, `F/resumen/api/`, `F/resumen/textos.ts` · (FR-015, FR-018…FR-020)
- [ ] T075 [US4] `FranjaIndicadores`, `TarjetaIndicador` (ayuda accesible), `GraficoEventos` (barras + línea punteada + tabla oculta) — `F/resumen/ui/` · (FR-015, FR-016, FR-043)
- [ ] T076 [US4] `RequiereAtencion`, `FranjaCorredor` (tabla oculta), `UltimosCambios`, `PaginaResumen` (botón «Paneles técnicos») e `index` — `F/resumen/ui/`, `F/resumen/index.ts` · (FR-017…FR-020)
- [ ] T077 [US4] `resumen.spec.ts`: carga, cambio de periodo y paso a «Moderación» desde «Requiere atención» — `E2E/resumen.spec.ts` · (FR-015…FR-018)

---

## Phase 9: User Story 6 — Auditoría (P2) · no recortable

### Pruebas

- [ ] T078 [P] [US6] `actor.test.ts` (UUID, correo, inválido; `elegirCuenta` exacta sin mayúsculas; 0 y varias parciales → `null`) y `textoAccion.test.ts` (las 14 acciones; `AnomaliaModerada` según `contexto.accion`; 6 categorías) — `F/auditoria/modelo/actor.test.ts`, `F/auditoria/modelo/textoAccion.test.ts` · (FR-037, FR-038)
- [ ] T079 [US6] `PaginaAuditoria.test.tsx`: filtros → parámetros; actor por correo = 1 `?busqueda` + consulta con `actorId`; sin cuenta → aviso; 1 solo `?ids=` por página; `null` → «sistema»; fila con `aria-expanded` y JSON; «N registros · retención permanente» — `F/auditoria/ui/PaginaAuditoria.test.tsx` · (FR-037…FR-039, FR-042)

### Implementación

- [ ] T080 [US6] `textos`, modelos, `useAuditoria`, `PaginaAuditoria` (insignia «Solo lectura»), `FiltrosAuditoria` («Hora en UTC»), `TablaAuditoria`, `FilaAuditoria` e `index` — `F/auditoria/` · (FR-037…FR-039)
- [ ] T081 [US6] `auditoria.spec.ts`: filtra por categoría, por id y por correo; comprueba que las acciones de T058, T068 y T085 aparecen con su texto — `E2E/auditoria.spec.ts` · (FR-037, FR-038, SC-008)

---

## Phase 10: User Story 7 — Exportación (P3) · recortable primero (HU-20)

### Pruebas

- [ ] T082 [P] [US7] `solicitud.test.ts` (13 oct → `2026-10-13T05:00:00Z`; hasta 20 oct → `2026-10-21T04:59:59Z`; vacías → `null`; «Todos» → `tipo: null` y `estados: []`) y `descarga.test.ts` (nombre del archivo por formato) — `F/exportacion/modelo/solicitud.test.ts`, `F/exportacion/modelo/descarga.test.ts` · (FR-035; R-017)
- [ ] T083 [US7] `PaginaExportacion.test.tsx`: 11 columnas y `seudonimo` tachado; cuerpo del `POST`; descarga con `createObjectURL` espiado y liberado; «Recientes» con filas y «tú» — `F/exportacion/ui/PaginaExportacion.test.tsx` · (FR-035, FR-036)

### Implementación

- [ ] T084 [US7] `textos`, modelos, `useExportar` (con `Accept` por formato), `useRecientes`, `PaginaExportacion`, `FormularioExportacion`, `ExportacionesRecientes` e `index` — `F/exportacion/` · (FR-035, FR-036)
- [ ] T085 [US7] `exportacion.spec.ts`: descarga CSV y GeoJSON; la cabecera del CSV es exactamente `id,tipo,severidad,estado,persistencia,km,latitud,longitud,reportes,primer_reporte,ultimo_reporte`; aparece en «Recientes» — `E2E/exportacion.spec.ts` · (FR-035, FR-036; RF-20)

---

## Phase 11: Polish — calidad, CI y despliegue

- [ ] T086 [P] `accesibilidad.spec.ts` (axe con `wcag2a` y `wcag2aa` en las 11 rutas, con datos y con diálogos abiertos) y `teclado.spec.ts` (login, filtro del mapa, lista → detalle → moderar, revisión mensual, solo con teclado) — `E2E/accesibilidad.spec.ts`, `E2E/teclado.spec.ts` · (FR-043, SC-004)
- [ ] T087 [P] Presupuesto y medición: `.size-limit.json`, `lighthouserc.json` y `rendimiento.spec.ts` (10 cargas del mapa contra «killauru-pruebas», p95 de `mapa-listo`) — `.size-limit.json`, `lighthouserc.json`, `E2E/rendimiento.spec.ts` · (FR-044, SC-005, SC-006)
- [ ] T088 [P] Cuentas y datos de prueba que necesita el E2E real (2 administradores, 1 conductor, anomalías en los 5 estados, 1 descartada «por revisar») documentados para la semilla de «killauru-pruebas» — `README.md` · (SC-007, SC-008)
- [ ] T089 [P] CORS propuesto para el *bucket* de R2 (`GET`, `HEAD`, `Range`, origen `https://panel.killauru.pe`, expone `Content-Range`, `ETag`) — `infra/r2-cors.json` · (FR-021; H-04)
- [ ] T090 CI del panel: tipos al día (`git diff --exit-code`), `tsc`, ESLint, `dependency-cruiser`, Vitest con cobertura y líneas, `build` + `build:simulado`, `size-limit`, Playwright simulado + axe en 3 navegadores, Lighthouse CI — `../.github/workflows/panel-ci.yml` · (FR-047, FR-048, FR-049)
- [ ] T091 Despliegue: al unir a `main`, `build` de producción → `wrangler pages deploy` (proyecto `killauru-panel`, rama `main`) → `verificar-cabeceras.mjs` contra `https://panel.killauru.pe`; secretos `CLOUDFLARE_API_TOKEN` y `CLOUDFLARE_ACCOUNT_ID` (H-03) — `../.github/workflows/panel-deploy.yml`, `scripts/verificar-cabeceras.mjs` · (FR-011, FR-049, SC-003)
- [ ] T092 `README.md` del panel: propósito, arranque (`dev`, `build:simulado`), variables, modos de prueba, despliegue, volver atrás (Pages «Rollback» o volver a desplegar el *commit* anterior) — `README.md` · (FR-049; RNF-19 por analogía)
- [ ] T093 Antes de cada versión: `npm run e2e:pruebas` (flujos críticos contra «killauru-pruebas»), revisión manual con NVDA + Firefox y Mozilla HTTP Observatory (≥ A); anotar resultados en el calendario de simulacros de 001 — `../deploy/runbooks/simulacros.md` · (SC-003, SC-004, SC-006, SC-009)
- [ ] T094 Marcar `checklists/trazabilidad.md` y llevar al autor los pendientes de `research.md` y los huecos H-01…H-06 · (constitución I)

## Dependencies & Execution Order

Phase 1 → Phase 2 → US1 (login) → US2 (mapa) → US5 (usuarios: aporta `useActores`, que usan el detalle del mapa, el Resumen y la Auditoría) → US8 → (US3, US4, US6 en paralelo) → US7 → Polish. US3 necesita el detalle de US2; US4 necesita `useMapaAdmin` (US2), las listas de US3 y `useRevision` (US5); US6 necesita `useActores` (US5); US7 necesita `consultarAuditoria` de US6 para «Recientes». El E2E real (T093) necesita 001, 003, 004, 006 y 007 desplegados en «killauru-pruebas» con H-02 resuelto; el mapa base necesita H-04 (008). Sin H-04, el mapa funciona con la lista y un aviso.
