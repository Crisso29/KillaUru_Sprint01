# Cierre de spec 010 — Panel administrativo

**Revisado:** 2026-11-15. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 77/94 tareas marcadas; 17 pendientes en tasks.md.
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Depende de contratos 001–007 y del entorno real.

## Evidencia disponible

UI y 389 casos Vitest aprobados. Playwright descubre casos organizados por propósito; eso no demuestra que se ejecuten con éxito.

## Qué falta localmente

15 casos vacíos convertidos a fixme; varios flujos con skip. Faltan 11 tareas E2E/calidad, datos de prueba, CORS, CI completo y aceptación. Desde el 21 de noviembre, e2e:pruebas usa configuración separada sin servidor local y requiere PANEL_URL; todavía no tiene archivos *.pruebas.ts. Preparar entorno, datos y escenarios reales antes de ejecutar.

## Qué falta en el entorno desplegado

Login/renovación/revocación y roles reales; cookies/CSRF; moderación/auditoría/exportación; almacenamiento, teclado/NVDA, cabeceras/CSP, presupuesto y tiempos; despliegue y rollback por SHA.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T032 | Local | `sesion.spec.ts` (simulado): MSW responde 401 `token_invalido` una vez → la pantalla sigue sin ir al login; renovación rechazada → login con «Tu sesión terminó. Vuelve a entrar.»; «Salir» en una pestaña lleva a la otra al login |
| T033 | Local | `login.spec.ts` (simulado y pruebas): entrar, credenciales malas, bloqueo 423 tras 3 fallos (pruebas), conductor → «Esta cuenta no tiene acceso al panel.», en los 3 navegadores |
| T034 | Local | `almacenamiento.spec.ts`: al final de cada flujo, `localStorage`, `sessionStorage`, IndexedDB y `document.cookie` vacíos de datos personales y tokens |
| T050 | Local | `mapa.spec.ts`: intercepta la primera petición sin parámetros (SC-001); cambia filtros; la dirección copiada abre lo mismo; la lista es navegable con teclado |
| T058 | Local | `usuarios.spec.ts`: invitar, cambiar rol, desactivar con motivo, activar, reenviar y revisión mensual |
| T062 | Local | `cuenta.spec.ts`: las 3 páginas; el fragmento no queda en el historial ni en la barra |
| T068 | Local | `moderacion.spec.ts`: las 4 acciones y un 409 provocado por otra pestaña |
| T081 | Local | `auditoria.spec.ts`: filtra por categoría, por id y por correo; comprueba que las acciones de T058, T068 y T085 aparecen con su texto |
| T085 | Local | `exportacion.spec.ts`: descarga CSV y GeoJSON; la cabecera del CSV es exactamente `id,tipo,severidad,estado,persistencia,km,latitud,longitud,reportes,primer_reporte,ultimo_reporte`; aparece en «Recientes» |
| T086 | Local | `accesibilidad.spec.ts` (axe con `wcag2a` y `wcag2aa` en las 11 rutas, con datos y con diálogos abiertos) y `teclado.spec.ts` (login, filtro del mapa, lista → detalle → moderar, revisión mensual, solo con teclado) |
| T087 | Local | Presupuesto y medición: `.size-limit.json`, `lighthouserc.json` y `rendimiento.spec.ts` (10 cargas del mapa contra «killauru-pruebas», p95 de `mapa-listo`) |
| T088 | Local | Cuentas y datos de prueba que necesita el E2E real (2 administradores, 1 conductor, anomalías en los 5 estados, 1 descartada «por revisar») documentados para la semilla de «killauru-pruebas» |
| T089 | Local | CORS propuesto para el *bucket* de R2 (`GET`, `HEAD`, `Range`, origen `https://panel.killauru.pe`, expone `Content-Range`, `ETag`) |
| T090 | Local | CI del panel: tipos al día (`git diff --exit-code`), `tsc`, ESLint, `dependency-cruiser`, Vitest con cobertura y líneas, `build` + `build:simulado`, `size-limit`, Playwright simulado + axe en 3 navegadores, Lighthouse CI |
| T091 | Desplegado/campo | Despliegue: al unir a `main`, `build` de producción → `wrangler pages deploy` (proyecto `killauru-panel`, rama `main`) → `verificar-cabeceras.mjs` contra `https://panel.killauru.pe`; secretos `CLOUDFLARE_API_TOKEN` y `CLOUDFLARE_ACCOUNT_ID` (H-03) |
| T093 | Desplegado/campo | Antes de cada versión: `npm run e2e:pruebas` (flujos críticos contra «killauru-pruebas»), revisión manual con NVDA + Firefox y Mozilla HTTP Observatory (≥ A); anotar resultados en el calendario de simulacros de 001 |
| T094 | Local | Marcar `checklists/trazabilidad.md` y llevar al autor los pendientes de `research.md` y los huecos H-01…H-06 |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
