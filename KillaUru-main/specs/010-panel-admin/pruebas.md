# Pruebas 010 — Panel administrativo

> Inventario regenerado (2026-11-24). Actualizado (2026-11-28): E2E ejecutados en Chromium y Edge (Firefox deshabilitado en CI por incompatibilidad WebGL/headless — ver nota al pie de «Cómo ejecutar»).

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Tipo | Ubicación |
|---|---|
| Unitarias | `*.unitaria.test.ts`: enlaces individuales en el inventario inferior |
| Componentes | `*.componente.test.tsx`: React con jsdom, router o MSW según el caso |
| Integración HTTP | [http](../../panel/src/api/http.integracion.test.ts) con MSW |
| Seguridad de plantillas | [cabeceras](../../panel/scripts/cabeceras.seguridad.test.ts) |
| Accesibilidad de estilos | [contraste](../../panel/src/compartido/estilos/contraste.accesibilidad.test.ts) |
| Contratos de datos simulados | [contrato](../../panel/pruebas/contrato/) |
| Límites de archivos | [arquitectura](../../panel/pruebas/arquitectura/) |
| E2E funcionales | [funcionales](../../panel/pruebas/e2e/funcionales/) |
| Privacidad en navegador | [seguridad](../../panel/pruebas/e2e/seguridad/) |
| Teclado y accesibilidad | [accesibilidad](../../panel/pruebas/e2e/accesibilidad/) |
| Tiempo de carga | [rendimiento](../../panel/pruebas/e2e/rendimiento/) |

## Cómo ejecutar

Desde `KillaUru-main/panel/`:

```powershell
npm test
npm run test:unitarias
npm run test:componentes
npm run test:integracion
npm run lint
npm run deps
npm run e2e -- --list
npm run e2e -- --project=chromium
npm run e2e -- --project=msedge
# Firefox deshabilitado — ver nota abajo
```

Playwright local arranca un build simulado. `e2e:pruebas` usa una configuración
separada sin servidor local, requiere `PANEL_URL` HTTPS y selecciona `*.pruebas.ts`
(T087: `mapa.pruebas.ts`). Los sufijos y comandos por categoría están en la
[guía del panel](../../panel/pruebas/README.md).
No confundir una prueba del panel con la autorización del backend: esta pertenece a 001/003/004.

> **Por qué Firefox está deshabilitado en CI**
> El componente `LienzoMapa` usa MapLibre GL JS, que requiere un contexto WebGL.
> Firefox headless en Linux (GitHub Actions) no inicializa WebGL correctamente:
> el contexto no llega a crearse o lo hace tan tarde que los tests de la página
> `/mapa` agotan el tiempo de espera (30 s) antes de que aparezcan los botones.
> Chromium y Edge (Chromium-based) cubren los mismos flujos sin este problema.
> Adicionalmente, MSW se inicializa con `serviceWorkers: 'block'` en Playwright
> (configurado en `playwright.config.ts`) para que `page.route()` intercepte todas
> las peticiones API sin que el Service Worker de MSW las capture antes. Esto hace
> que la intercepción sea uniforme en todos los navegadores.

## Estado de las tareas E2E (2026-11-28)

Todas las tareas T032–T036, T050, T058, T062, T068, T077, T080, T081, T085, T086 ejecutadas
con éxito en Chromium y Edge (62 casos E2E × 2 navegadores = 124 casos en CI).
Firefox deshabilitado — causa documentada en la nota de «Cómo ejecutar».
T087 parcial: `mapa.pruebas.ts` escrito, pendiente de ejecutar contra el entorno desplegado.
Ver evidencia en [2026-11-24-panel-parte6](../../docs/seguimiento/evidencias/pruebas/2026-11-24-panel-parte6/README.md).

**Nota sobre simulado vs. real:** todos los E2E corren contra el build simulado con `page.route()`
interceptando las llamadas API (MSW Service Worker bloqueado en Playwright mediante
`serviceWorkers: 'block'`). Los tokens, cookies HttpOnly y CORS reales se verifican en el
backend (003/004) y en el entorno de pruebas (`e2e:pruebas`). Ver `cierre.md` para los
criterios de cierre pendientes.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.

<details>
<summary>Unitarias (37 archivos)</summary>

- [anomaliasSimuladas.unitaria.test.ts](../../panel/pruebas/msw/simulador/anomaliasSimuladas.unitaria.test.ts): simulador con la tabla RN-07 del backend (TablaModeracion.cs) y filtros del contrato 006
- [problema.unitaria.test.ts](../../panel/src/api/problema.unitaria.test.ts)
- [rutaBusqueda.unitaria.test.ts](../../panel/src/app/marco/rutaBusqueda.unitaria.test.ts): a qué pantalla lleva cada búsqueda rápida
- [paginasRutas.unitaria.test.ts](../../panel/src/app/paginasRutas.unitaria.test.ts): ninguna página de las rutas es un `lazy` dentro de otro (el Mapa no abría)
- [fechas.unitaria.test.ts](../../panel/src/compartido/formato/fechas.unitaria.test.ts)
- [ids.unitaria.test.ts](../../panel/src/compartido/formato/ids.unitaria.test.ts)
- [km.unitaria.test.ts](../../panel/src/compartido/formato/km.unitaria.test.ts)
- [numeros.unitaria.test.ts](../../panel/src/compartido/formato/numeros.unitaria.test.ts)
- [relativo.unitaria.test.ts](../../panel/src/compartido/formato/relativo.unitaria.test.ts)
- [errores.unitaria.test.ts](../../panel/src/compartido/textos/errores.unitaria.test.ts)
- [transicion.unitaria.test.ts](../../panel/src/compartido/textos/transicion.unitaria.test.ts): tabla R-014 única para el Resumen y el Historial (antes dos copias que ya diferían)
- [actor.unitaria.test.ts](../../panel/src/funcionalidades/auditoria/modelo/actor.unitaria.test.ts)
- [nombreActor.unitaria.test.ts](../../panel/src/funcionalidades/auditoria/modelo/nombreActor.unitaria.test.ts): columna «Actor»: sistema, correo, cuenta borrada o id mientras llega
- [textoAccion.unitaria.test.ts](../../panel/src/funcionalidades/auditoria/modelo/textoAccion.unitaria.test.ts)
- [contrasena.unitaria.test.ts](../../panel/src/funcionalidades/cuenta/modelo/contrasena.unitaria.test.ts)
- [tokenDelFragmento.unitaria.test.ts](../../panel/src/funcionalidades/cuenta/modelo/tokenDelFragmento.unitaria.test.ts)
- [descarga.unitaria.test.ts](../../panel/src/funcionalidades/exportacion/modelo/descarga.unitaria.test.ts)
- [solicitud.unitaria.test.ts](../../panel/src/funcionalidades/exportacion/modelo/solicitud.unitaria.test.ts)
- [bloqueo.unitaria.test.ts](../../panel/src/funcionalidades/login/modelo/bloqueo.unitaria.test.ts)
- [busqueda.unitaria.test.ts](../../panel/src/funcionalidades/mapa/modelo/busqueda.unitaria.test.ts)
- [filtros.unitaria.test.ts](../../panel/src/funcionalidades/mapa/modelo/filtros.unitaria.test.ts)
- [peticion.unitaria.test.ts](../../panel/src/funcionalidades/mapa/modelo/peticion.unitaria.test.ts)
- [simbolos.unitaria.test.ts](../../panel/src/funcionalidades/mapa/modelo/simbolos.unitaria.test.ts)
- [tramoDemo.unitaria.test.ts](../../panel/src/funcionalidades/mapa/modelo/tramoDemo.unitaria.test.ts)
- [acciones.unitaria.test.ts](../../panel/src/funcionalidades/moderacion/modelo/acciones.unitaria.test.ts)
- [atencion.unitaria.test.ts](../../panel/src/funcionalidades/resumen/modelo/atencion.unitaria.test.ts)
- [escalaGrafico.unitaria.test.ts](../../panel/src/funcionalidades/resumen/modelo/escalaGrafico.unitaria.test.ts): escala, guías y trazo del gráfico de eventos
- [franja.unitaria.test.ts](../../panel/src/funcionalidades/resumen/modelo/franja.unitaria.test.ts)
- [indicadores.unitaria.test.ts](../../panel/src/funcionalidades/resumen/modelo/indicadores.unitaria.test.ts)
- [consulta.unitaria.test.ts](../../panel/src/funcionalidades/usuarios/modelo/consulta.unitaria.test.ts)
- [erroresAccion.unitaria.test.ts](../../panel/src/funcionalidades/usuarios/modelo/erroresAccion.unitaria.test.ts)
- [revision.unitaria.test.ts](../../panel/src/funcionalidades/usuarios/modelo/revision.unitaria.test.ts)
- [useBusquedaDiferida.unitaria.test.ts](../../panel/src/funcionalidades/usuarios/ui/useBusquedaDiferida.unitaria.test.ts): FR-031: espera 400 ms y cada tecla la reinicia (validada con mutaciones)
- [estadoSesion.unitaria.test.ts](../../panel/src/sesion/estadoSesion.unitaria.test.ts)
- [planificador.unitaria.test.ts](../../panel/src/sesion/planificador.unitaria.test.ts)
- [renovador.unitaria.test.ts](../../panel/src/sesion/renovador.unitaria.test.ts)
- [sesionPanel.unitaria.test.ts](../../panel/src/sesion/sesionPanel.unitaria.test.ts): lee `SesionPanel` del contrato 003 y rechaza el formato viejo

</details>

<details>
<summary>Componentes con jsdom (24 archivos)</summary>

- [BusquedaRapida.componente.test.tsx](../../panel/src/app/marco/BusquedaRapida.componente.test.tsx)
- [Marco.componente.test.tsx](../../panel/src/app/marco/Marco.componente.test.tsx)
- [armazon.componente.test.tsx](../../panel/src/app/marco/armazon.componente.test.tsx): menú con página actual, migas, contador y marca DEMO
- [rutasPublicas.componente.test.tsx](../../panel/src/app/rutasPublicas.componente.test.tsx): los enlaces `/cuenta/verificar|restablecer|invitacion` abren su página y su mensaje
- [Avisos.componente.test.tsx](../../panel/src/compartido/ui/Avisos.componente.test.tsx)
- [Dialogo.componente.test.tsx](../../panel/src/compartido/ui/Dialogo.componente.test.tsx)
- [PaginaAuditoria.componente.test.tsx](../../panel/src/funcionalidades/auditoria/ui/PaginaAuditoria.componente.test.tsx)
- [PaginaFijarContrasena.componente.test.tsx](../../panel/src/funcionalidades/cuenta/ui/PaginaFijarContrasena.componente.test.tsx)
- [PaginaVerificar.componente.test.tsx](../../panel/src/funcionalidades/cuenta/ui/PaginaVerificar.componente.test.tsx)
- [PaginaExportacion.componente.test.tsx](../../panel/src/funcionalidades/exportacion/ui/PaginaExportacion.componente.test.tsx)
- [FormularioLogin.componente.test.tsx](../../panel/src/funcionalidades/login/ui/FormularioLogin.componente.test.tsx)
- [PaginaLogin.componente.test.tsx](../../panel/src/funcionalidades/login/ui/PaginaLogin.componente.test.tsx): `?motivo=sesion` muestra «Tu sesión terminó. Vuelve a entrar.»
- [PaginaRecuperar.componente.test.tsx](../../panel/src/funcionalidades/login/ui/PaginaRecuperar.componente.test.tsx)
- [DetalleAnomalia.componente.test.tsx](../../panel/src/funcionalidades/mapa/ui/DetalleAnomalia.componente.test.tsx)
- [PaginaMapa.componente.test.tsx](../../panel/src/funcionalidades/mapa/ui/PaginaMapa.componente.test.tsx)
- [AccionesModeracion.componente.test.tsx](../../panel/src/funcionalidades/moderacion/ui/AccionesModeracion.componente.test.tsx)
- [PaginaModeracion.componente.test.tsx](../../panel/src/funcionalidades/moderacion/ui/PaginaModeracion.componente.test.tsx)
- [PestanaEnDireccion.componente.test.tsx](../../panel/src/funcionalidades/moderacion/ui/PestanaEnDireccion.componente.test.tsx): `?pestana=pendientes` abre esa pestaña
- [PaginaResumen.componente.test.tsx](../../panel/src/funcionalidades/resumen/ui/PaginaResumen.componente.test.tsx)
- [UltimosCambios.componente.test.tsx](../../panel/src/funcionalidades/resumen/ui/UltimosCambios.componente.test.tsx): «Quién» con correo y km con coma, como el resto del panel
- [DialogoRevision.componente.test.tsx](../../panel/src/funcionalidades/usuarios/ui/DialogoRevision.componente.test.tsx)
- [MenuUsuario.componente.test.tsx](../../panel/src/funcionalidades/usuarios/ui/MenuUsuario.componente.test.tsx)
- [PaginaUsuarios.componente.test.tsx](../../panel/src/funcionalidades/usuarios/ui/PaginaUsuarios.componente.test.tsx)
- [ProveedorSesion.componente.test.tsx](../../panel/src/sesion/ProveedorSesion.componente.test.tsx): renueva en cada vencimiento; al salir avisa a otras pestañas, deja de renovar y vacía la caché

</details>

<details>
<summary>Integración HTTP con MSW (1 archivo)</summary>

- [http.integracion.test.ts](../../panel/src/api/http.integracion.test.ts): única puerta a la API: cookie, renovación ante 401 `token_invalido`, reintento y fin de sesión

</details>

<details>
<summary>Contratos (4 archivos)</summary>

- [consultaMapa.contrato.test.ts](../../panel/pruebas/contrato/consultaMapa.contrato.test.ts): cada parámetro que el panel manda a `GET /anomalies` está en el contrato 006 (detectó `severidad_min`)
- [datosSimulados.contrato.test.ts](../../panel/pruebas/contrato/datosSimulados.contrato.test.ts)
- [enlacesCorreo.contrato.test.ts](../../panel/pruebas/contrato/enlacesCorreo.contrato.test.ts): las rutas que el backend pone en los correos existen en el panel (antes abrían 404)
- [rutas.contrato.test.ts](../../panel/pruebas/contrato/rutas.contrato.test.ts): toda ruta del panel y de MSW existe en `specs/*/contracts/*.yaml` (detectó `auth/iniciar` y `auth/cerrar`)

</details>

<details>
<summary>Arquitectura (4 archivos)</summary>

- [aaa.arquitectura.test.ts](../../panel/pruebas/arquitectura/aaa.arquitectura.test.ts): toda prueba (Vitest y Playwright) marca Arrange, Act y Assert en orden
- [lineas.arquitectura.test.ts](../../panel/pruebas/arquitectura/lineas.arquitectura.test.ts)
- [llamadasHttp.arquitectura.test.ts](../../panel/pruebas/arquitectura/llamadasHttp.arquitectura.test.ts): solo `src/api/http.ts` llama a `fetch`
- [variablesCss.arquitectura.test.ts](../../panel/pruebas/arquitectura/variablesCss.arquitectura.test.ts): toda `var(--x)` usada existe en las hojas (había 10 inexistentes)

</details>

<details>
<summary>Accesibilidad de estilos (1 archivo)</summary>

- [contraste.accesibilidad.test.ts](../../panel/src/compartido/estilos/contraste.accesibilidad.test.ts)

</details>

<details>
<summary>Seguridad de plantillas (1 archivo)</summary>

- [cabeceras.seguridad.test.ts](../../panel/scripts/cabeceras.seguridad.test.ts)

</details>

<details>
<summary>E2E simulado — funcionales (9 archivos)</summary>

- [auditoria.spec.ts](../../panel/pruebas/e2e/funcionales/auditoria.spec.ts)
- [cuenta.spec.ts](../../panel/pruebas/e2e/funcionales/cuenta.spec.ts)
- [exportacion.spec.ts](../../panel/pruebas/e2e/funcionales/exportacion.spec.ts)
- [login.spec.ts](../../panel/pruebas/e2e/funcionales/login.spec.ts)
- [mapa.spec.ts](../../panel/pruebas/e2e/funcionales/mapa.spec.ts)
- [moderacion.spec.ts](../../panel/pruebas/e2e/funcionales/moderacion.spec.ts)
- [resumen.spec.ts](../../panel/pruebas/e2e/funcionales/resumen.spec.ts)
- [sesion.spec.ts](../../panel/pruebas/e2e/funcionales/sesion.spec.ts)
- [usuarios.spec.ts](../../panel/pruebas/e2e/funcionales/usuarios.spec.ts)

</details>

<details>
<summary>E2E simulado — accesibilidad y teclado (2 archivos)</summary>

- [accesibilidad.spec.ts](../../panel/pruebas/e2e/accesibilidad/accesibilidad.spec.ts)
- [teclado.spec.ts](../../panel/pruebas/e2e/accesibilidad/teclado.spec.ts)

</details>

<details>
<summary>E2E simulado — seguridad del navegador (1 archivo)</summary>

- [almacenamiento.spec.ts](../../panel/pruebas/e2e/seguridad/almacenamiento.spec.ts)

</details>

<details>
<summary>E2E contra el entorno de pruebas (1 archivo)</summary>

- [mapa.pruebas.ts](../../panel/pruebas/e2e/rendimiento/mapa.pruebas.ts): SC-006 contra el entorno de pruebas (p95 de `mapa-listo` en 10 cargas); requiere PANEL_URL y credenciales

</details>
