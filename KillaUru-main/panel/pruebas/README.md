# Pruebas del panel — spec 010

| Ubicación | Propósito |
|---|---|
| `../src/**/*.test.ts(x)` | Reglas y componentes junto a su funcionalidad |
| `contrato` | Datos simulados frente a esquemas |
| `arquitectura` | Límites de archivos |
| `e2e/funcionales` | Login, sesión, mapa y operaciones del panel |
| `e2e/seguridad` | Privacidad del almacenamiento del navegador |
| `e2e/accesibilidad` | Navegación por teclado y accesibilidad |
| `e2e/rendimiento` | Medición del tiempo de carga |

Desde `panel/`:

```powershell
npm test
npm run test:unitarias
npm run test:componentes
npm run test:integracion
npm run test:contratos
npm run test:arquitectura
npm run test:accesibilidad
npm run test:seguridad
npm run e2e -- --list
npm run e2e -- --project=chromium
npm run e2e -- --project=msedge
```

Playwright inicia `build:simulado` y usa `page.route()` para interceptar todas las llamadas
API. El Service Worker de MSW está bloqueado (`serviceWorkers: 'block'` en `playwright.config.ts`)
para que `page.route()` capture las peticiones antes que el SW y los overrides de
`simularRespuesta()` funcionen de forma fiable en todos los navegadores.
Firefox está deshabilitado en CI: MapLibre GL JS requiere WebGL y Firefox headless en Linux
no inicializa el contexto correctamente. Ver nota completa en
[specs/010-panel-admin/pruebas.md](../../specs/010-panel-admin/pruebas.md).
No acredita integración con la API real.
`npm run e2e:pruebas` utiliza `playwright.pruebas.config.ts`, exige `PANEL_URL`
HTTPS y no inicia servidor local. Selecciona `*.pruebas.ts`, todavía sin casos;
por tanto no acredita validación desplegada y debe fallar si se intenta ejecutar vacío.

## Por qué cada prueba está ahí

Los tests conservan la cercanía con su funcionalidad; el sufijo permite filtrar el alcance.
`npm test` incluye todas las categorías, también seguridad en `scripts/`.

| Sufijo | Responsabilidad y límites |
|---|---|
| `.unitaria.test.ts` | Regla o colaborador aislado, con dobles cuando corresponde; no usa la API desplegada |
| `.componente.test.tsx` | UI en jsdom; puede integrar router, proveedores y MSW; no es E2E |
| `.integracion.test.ts` | Cliente HTTP y middleware trabajando juntos con respuestas MSW |
| `.contrato.test.ts` | Datos simulados contrastados con esquemas; no certifica el servidor |
| `.arquitectura.test.ts` | Restricciones estructurales del código |
| `.accesibilidad.test.ts` | Contraste de estilos; complementar con teclado y navegador |
| `.seguridad.test.ts` | Plantilla de cabeceras; no certifica las cabeceras desplegadas |

Playwright usa `.spec.ts` para recorridos simulados y `.pruebas.ts` para los futuros
recorridos desplegados. `soporte`, configuración y MSW preparan escenarios, no cuentan
como casos aprobados. No duplicar una prueba en varias carpetas por tener varios propósitos.

Login, sesión, mapa y almacenamiento contenían 15 comprobaciones de existencia de
`page/browser`, sin acciones ni aserciones de negocio. Se registran como `fixme`.
Otros flujos ya tienen `skip`. Resumen contiene aserciones reales, cuya aprobación
depende de ejecutarlas. Revisar [pendientes de la spec](../../specs/010-panel-admin/pruebas.md).

Para habilitar un pendiente, implementar preparación, interacción y comprobación del resultado.
Retirar `fixme/skip` solo tras ejecutar el escenario. No contabilizar omitidos como aprobados.

