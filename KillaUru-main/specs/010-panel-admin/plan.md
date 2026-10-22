# Implementation Plan: Panel de administración «KillaUru Ops»

**Branch**: `010-panel-admin` | **Date**: 2026-10-22 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/010-panel-admin/spec.md`

## Summary

SPA **sin reglas de negocio** en `panel/`: React 19 + TypeScript + Vite, servida por Cloudflare Pages en `panel.killauru.pe` con cabeceras de seguridad en `_headers`. La sesión vive solo en las cookies `HttpOnly` de la API y se renueva en silencio (un solo renovador compartido + reintento tras 401). Los tipos del cliente se **generan desde los YAML** de 001–007 (`openapi-typescript` + `openapi-fetch`) y el estado del servidor lo maneja TanStack Query con una política de refresco que evita multiplicar las lecturas que la API audita (D15). El mapa usa MapLibre GL JS con el PMTiles del corredor leído por rangos desde `cdn.killauru.pe` y se carga solo en «Mapa». El código se organiza por funcionalidades (una por pantalla de `componentes-arquitectonicos.md` §5.1) con capas `ui → api → modelo` y archivos ≤ 100 líneas.

## Technical Context

**Language/Version**: TypeScript 5 (`strict`), React 19, Node 22 LTS (solo para construir y probar)

**Primary Dependencies**: Vite 6, React Router 7, TanStack Query 5, `openapi-fetch` + `openapi-typescript`, `maplibre-gl` (compilación CSP) + `pmtiles`, `@fontsource/overpass`, `@fontsource/source-sans-3`, `@fontsource/overpass-mono`. Desarrollo: Vitest, Testing Library, `vitest-axe`, MSW 2, Ajv 2020 + `yaml`, Playwright + `@axe-core/playwright`, ESLint, `dependency-cruiser`, `size-limit`, Lighthouse CI, `wrangler` (por la acción de GitHub)

**Storage**: ninguno (memoria del navegador; FR-006)

**Testing**: Vitest (unitarias y componentes con MSW), contrato (tipos generados + Ajv contra los YAML), Playwright (E2E simulado en el CI y real contra «killauru-pruebas»), axe, `size-limit`, Lighthouse CI (`research.md` R-024)

**Target Platform**: navegadores de escritorio Chrome, Firefox y Edge (2 últimas versiones, RNF-13), servidos desde Cloudflare Pages

**Project Type**: aplicación web de una sola página (cliente de la API de 001–007)

**Performance Goals**: JS inicial ≤ 120 KB gzip; bloque del mapa ≤ 300 KB gzip; mapa listo ≤ 3 s p95; LCP del Resumen ≤ 2,5 s (R-023)

**Constraints**: sin reglas de negocio; sin token ni datos personales en el navegador; CSP sin `unsafe-inline`/`unsafe-eval`; costo cero; archivos ≤ 100 líneas; solo español

**Scale/Scope**: pocos administradores (≤ 5 a la vez); 8 pantallas + 3 páginas de enlaces; pocos miles de anomalías en el mapa con el filtro inicial

## Constitution Check

La constitución 1.1.0 está pensada para el backend; aquí se aplica su intención al cliente. La adaptación formal se propone en `research.md` («Propuesta de enmienda a la constitución»).

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | Stack, cookie, renovación, anti-bot, cabeceras y pantallas como docs (§5.1, ADR-009, ADR-011, RNF-09); cada FR cita su fuente; lo del prototipo y lo SDD está en `research.md` | ✅ |
| II. Monolito modular | No aplica al cliente; equivalente: funcionalidades aisladas que se usan solo por su `index.ts`, sin ciclos, verificado con `dependency-cruiser` (R-005) | ✅ (adaptado) |
| III. Calibrable | El panel no tiene reglas; los pocos valores de docs que muestra viven en `src/config/` con su fuente; reloj inyectado en `modelo/` y `sesion/` | ✅ |
| IV. Datos confiables | No aplica (el panel no recibe eventos); las mutaciones no se reintentan solas | ✅ |
| V. Privacidad | Nada en `localStorage`/`sessionStorage`/IndexedDB; sin analítica ni fuentes de terceros; seudónimo tachado en la exportación; correos resueltos solo cuando se muestran (D15) | ✅ |
| VI. Acceso en el servidor | El panel nunca decide permisos (FR-002); sesión solo en cookie `HttpOnly`; `private, no-store` lo pone la API | ✅ |
| VII. Pruebas primero | Cada fase empieza por sus pruebas; `modelo/` con cobertura ≥ 90 %; E2E de los flujos críticos; CI en cada PR | ✅ |
| VIII. Código pequeño | Archivos `.ts`/`.tsx` ≤ 100 líneas (prueba), funciones cortas, nombres de negocio en español | ✅ (ampliado a TS) |
| IX. API por contrato | Tipos generados desde los YAML; datos simulados validados con Ajv; el panel no usa ningún endpoint fuera de los contratos | ✅ |
| X. Costo cero | Pages, GitHub Actions, librerías libres, Turnstile gratis; sin servidor propio | ✅ |

## Project Structure

### Documentation

```text
specs/010-panel-admin/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/contratos-internos.md          # lo que el panel consume (operación por operación) y lo que ofrece
└── checklists/requirements.md · checklists/trazabilidad.md
```

### Source Code — árbol exacto

```text
panel/
├── package.json                    # scripts: dev, build, build:simulado, tipos, test, cobertura, e2e, e2e:pruebas, lint, deps, size
├── tsconfig.json
├── vite.config.ts                  # rutas perezosas; bloque «mapa»; modo simulado copia el worker de MSW
├── vitest.config.ts                # jsdom; cobertura de src/**/modelo/** ≥ 90 %
├── playwright.config.ts            # proyectos chromium, firefox, msedge × modos simulado y pruebas
├── eslint.config.js                # prohíbe localStorage, sessionStorage, indexedDB, document.cookie,
│                                   #   dangerouslySetInnerHTML; Date.now()/new Date() sin argumento en modelo/ y sesion/
├── .dependency-cruiser.cjs         # regla de capas y de funcionalidades (R-005)
├── .size-limit.json                # 120 KB inicial · 300 KB mapa (gzip)
├── lighthouserc.json               # LCP del Resumen ≤ 2,5 s (modo simulado)
├── index.html                      # lang="es"
├── .env.example
├── README.md
├── cabeceras/_headers.plantilla    # FR-011 (R-008); {{URL_API}} se reemplaza al construir
├── infra/r2-cors.json              # CORS propuesto para el bucket de cdn.killauru.pe (H-04)
├── public/ favicon.svg · robots.txt               # robots: Disallow: /
├── scripts/
│   ├── generar-tipos.mjs           # openapi-typescript sobre ../specs/*/contracts/*.openapi.yaml
│   ├── generar-cabeceras.mjs       # dist/_headers desde la plantilla y VITE_URL_API
│   ├── verificar-cabeceras.mjs     # tras desplegar: las 7 cabeceras y la CSP
│   └── cabeceras.test.ts           # prueba de la plantilla (FR-011)
├── src/
│   ├── main.tsx · vite-env.d.ts
│   ├── app/
│   │   ├── App.tsx                 # proveedores (consultas, sesión, avisos) + enrutador
│   │   ├── rutas.tsx               # 11 rutas perezosas (R-003)
│   │   ├── RutaProtegida.tsx       # espera la sesión; sin sesión → /login
│   │   ├── PaginaNoEncontrada.tsx · TituloDeRuta.tsx
│   │   └── marco/ Marco.tsx · MenuLateral.tsx · BarraSuperior.tsx · ResumenSesion.tsx · SelectorPeriodo.tsx · BusquedaRapida.tsx · MenuMovil.tsx
│   ├── config/ constantes.ts · corredor.ts · entorno.ts     # data-model §8
│   ├── api/
│   │   ├── generado/ plataforma.d.ts · auditoria.d.ts · identidad.d.ts · anomalias.d.ts · mapa.d.ts · dashboard.d.ts
│   │   ├── clientes.ts             # un cliente openapi-fetch por contrato, misma base y middleware
│   │   ├── middleware.ts           # credentials: 'include', problema → ErrorApi, renovación + 1 reintento
│   │   ├── problema.ts             # Problema / ProblemaBloqueo → ErrorApi (data-model §2)
│   │   └── consultas.ts            # QueryClient y tiempos por defecto (data-model §3)
│   ├── sesion/
│   │   ├── estadoSesion.ts         # estados y transiciones puras (data-model §1)
│   │   ├── planificador.ts         # desfase, hora de renovación, fracción restante
│   │   ├── renovador.ts            # una sola renovación en curso (promesa compartida)
│   │   ├── canalSesion.ts          # BroadcastChannel('ku-sesion')
│   │   ├── ProveedorSesion.tsx · useSesion.ts
│   ├── compartido/
│   │   ├── reloj.ts                # ahora() inyectable
│   │   ├── formato/ numeros.ts · fechas.ts · relativo.ts · km.ts · ids.ts
│   │   ├── textos/ errores.ts · estados.ts
│   │   ├── estilos/ tokens.css · base.css · fuentes.css · marco.css · controles.css · tablas.css · mapa.css · resumen.css
│   │   └── ui/ Boton.tsx · Dialogo.tsx · Avisos.tsx · InsigniaEstado.tsx · Severidad.tsx · Hito.tsx · MenuFiltro.tsx ·
│   │           EstadoVacio.tsx · EstadoError.tsx · Esqueleto.tsx · Ayuda.tsx · Minigrafico.tsx · Icono.tsx ·
│   │           Paginador.tsx · Marca.tsx · TablaOculta.tsx · Pestanas.tsx
│   └── funcionalidades/
│       ├── login/        index.ts · textos.ts · modelo/bloqueo.ts · api/useIniciarSesion.ts · api/useRecuperar.ts ·
│       │                 ui/PaginaLogin.tsx · ui/FormularioLogin.tsx · ui/AvisoBloqueo.tsx · ui/Turnstile.tsx · ui/PaginaRecuperar.tsx
│       ├── cuenta/       index.ts · textos.ts · modelo/tokenDelFragmento.ts · modelo/contrasena.ts · api/useEnlaces.ts ·
│       │                 ui/PaginaVerificar.tsx · ui/PaginaFijarContrasena.tsx
│       ├── resumen/      index.ts · textos.ts · modelo/indicadores.ts · modelo/franja.ts · modelo/atencion.ts · modelo/transicion.ts ·
│       │                 api/useIndicadores.ts · api/useTransiciones.ts ·
│       │                 ui/PaginaResumen.tsx · ui/FranjaIndicadores.tsx · ui/TarjetaIndicador.tsx · ui/GraficoEventos.tsx ·
│       │                 ui/RequiereAtencion.tsx · ui/FranjaCorredor.tsx · ui/UltimosCambios.tsx
│       ├── mapa/         index.ts · textos.ts · modelo/filtros.ts · modelo/peticion.ts · modelo/tramoDemo.ts · modelo/simbolos.ts ·
│       │                 modelo/busqueda.ts · api/useMapaAdmin.ts · api/useDetalleAnomalia.ts ·
│       │                 ui/PaginaMapa.tsx · ui/BarraFiltros.tsx · ui/LienzoMapa.tsx · ui/capas.ts · ui/iconos.ts · ui/Leyenda.tsx ·
│       │                 ui/PanelLateral.tsx · ui/ListaAnomalias.tsx · ui/DetalleAnomalia.tsx · ui/DatosAnomalia.tsx ·
│       │                 ui/ReportesPorDia.tsx · ui/Historial.tsx
│       ├── moderacion/   index.ts · textos.ts · modelo/acciones.ts · api/useListasModeracion.ts · api/useModerar.ts ·
│       │                 ui/PaginaModeracion.tsx · ui/TablaPorRevisar.tsx · ui/TablaPendientes.tsx ·
│       │                 ui/AccionesModeracion.tsx · ui/DialogoModeracion.tsx
│       ├── usuarios/     index.ts · textos.ts · modelo/consulta.ts · modelo/revision.ts ·
│       │                 api/useUsuarios.ts · api/useAccionesUsuario.ts · api/useRevision.ts · api/useActores.ts ·
│       │                 ui/PaginaUsuarios.tsx · ui/FiltrosUsuarios.tsx · ui/TablaUsuarios.tsx · ui/MenuUsuario.tsx ·
│       │                 ui/DialogoInvitar.tsx · ui/DialogoRol.tsx · ui/DialogoDesactivar.tsx · ui/AvisoRevision.tsx · ui/DialogoRevision.tsx
│       ├── auditoria/    index.ts · textos.ts · modelo/actor.ts · modelo/textoAccion.ts · api/useAuditoria.ts ·
│       │                 ui/PaginaAuditoria.tsx · ui/FiltrosAuditoria.tsx · ui/TablaAuditoria.tsx · ui/FilaAuditoria.tsx
│       └── exportacion/  index.ts · textos.ts · modelo/solicitud.ts · modelo/descarga.ts · api/useExportar.ts · api/useRecientes.ts ·
│                         ui/PaginaExportacion.tsx · ui/FormularioExportacion.tsx · ui/ExportacionesRecientes.tsx
└── pruebas/
    ├── configuracion.ts · renderizar.tsx            # setup de Vitest (jest-dom, axe, MSW) y render con proveedores
    ├── msw/ servidor.ts · navegador.ts · mockServiceWorker.js (generado por `msw init`)
    │        manejadores/ identidad.ts · mapa.ts · anomalias.ts · dashboard.ts · auditoria.ts
    │        datos/ *.json                            # respuestas de ejemplo (salen de los `example` de los YAML)
    ├── contrato/ esquemas.ts · datosSimulados.test.ts
    ├── arquitectura/ lineas.test.ts
    └── e2e/ ayudas.ts · login.spec.ts · sesion.spec.ts · almacenamiento.spec.ts · mapa.spec.ts · usuarios.spec.ts ·
             cuenta.spec.ts · moderacion.spec.ts · resumen.spec.ts · auditoria.spec.ts · exportacion.spec.ts ·
             accesibilidad.spec.ts · teclado.spec.ts · rendimiento.spec.ts

.github/workflows/ panel-ci.yml · panel-deploy.yml   # en la raíz del repositorio (../ desde panel/)
```

Las pruebas unitarias y de componentes van **junto a su archivo** (`filtros.ts` → `filtros.test.ts`, `PaginaMapa.tsx` → `PaginaMapa.test.tsx`). También cumplen las 100 líneas.

**Structure Decision**: una funcionalidad por pantalla de §5.1 (más `cuenta/` por 003 R-006). Las dependencias entre funcionalidades permitidas (solo por `index.ts`) son: `mapa → moderacion` (acciones en el detalle), `mapa → usuarios` (correo de los actores del historial), `resumen → mapa, moderacion, usuarios`, `auditoria → usuarios`, `exportacion → auditoria, usuarios`. `app/marco` usa `mapa` (búsqueda) y `moderacion` (contador). No hay ciclos.

## Dependencias

| De | Qué necesita | Estado |
|---|---|---|
| 001 | CORS con credenciales para `https://panel.killauru.pe`; cookie por `ku_acceso`; `/docs` en pruebas; entorno «killauru-pruebas» | especificado (FR-009, FR-014, R-012); H-01, H-02, H-06 |
| 002 | `GET /api/v1/auditoria` | especificado |
| 003 | sesión del panel, cuenta, `/me`, usuarios y revisión; Turnstile; páginas `/cuenta/…` | especificado (R-006, R-008) |
| 004 | detalle, moderación, listas, transiciones, exportación | especificado (US de moderación recortable) |
| 006 | `GET /api/v1/anomalies` con `estado` y `kmVia` | especificado |
| 007 | `GET /api/v1/dashboard/indicadores` | especificado (recortable) |
| 008 | PMTiles, estilo, glifos y *sprite* en `cdn.killauru.pe` con CORS (D16) | por escribir: H-04 |

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|---|---|---|
| Renovación al arrancar (una rotación del token de actualización por recarga) | El panel no puede saber si hay sesión sin preguntar (cookie `HttpOnly`) y no guarda nada en el navegador | Guardar `sesionVenceUtc` en `sessionStorage` contradice FR-006 |
| Lecturas auditadas para mostrar correos (R-015) | El prototipo y 002 piden ver quién hizo cada cosa | Mostrar solo ids: posible, queda como pendiente del autor |
| Dos compilaciones (`build` y `build:simulado`) | E2E en el CI sin backend; la de producción no lleva MSW | Levantar el backend .NET + PostGIS en cada PR del panel: lento y acopla los CI |
| `_headers` generado al construir | La CSP debe nombrar la API del entorno (producción o pruebas) | Un `_headers` fijo obligaría a desplegar la CSP de producción en pruebas |
