# Pruebas 010 — Panel administrativo

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Tipo | Ubicación |
|---|---|
| Unitarias | `*.unitaria.test.ts`: enlaces individuales en el inventario inferior |
| Componentes | `*.componente.test.tsx`: React con jsdom, router o MSW según el caso |
| Integración HTTP | [middleware](../../panel/src/api/middleware.integracion.test.ts) con MSW |
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
```

Playwright local arranca un build simulado con MSW. `e2e:pruebas` usa una configuración
separada sin servidor local, requiere `PANEL_URL` HTTPS y selecciona `*.pruebas.ts`,
todavía sin casos. Los sufijos y comandos por categoría están en la
[guía del panel](../../panel/pruebas/README.md).
No confundir una prueba del panel con la autorización del backend: esta pertenece a 001/003/004.

## Pendientes detectados al organizar (2026-11-15)

- T032, T033, T034 y T050: sesión, login, almacenamiento y mapa tenían 15 aserciones
  que solo comprobaban que existía page/browser. Ahora figuran como `fixme`.
- T058, T062, T068, T081 y T085: usuarios, cuenta, moderación, auditoría y exportación
  contienen escenarios omitidos con `skip`.
- T086 y T087: accesibilidad/teclado y medición de carga del mapa tienen escenarios omitidos.
- T077: resumen contiene interacciones y aserciones; necesita ejecución para acreditar el resultado.

Se reabrieron esas 11 tareas. El antiguo “100 % completado” de la checklist no acredita E2E.
Para habilitar cada caso, implementar interacción y aserciones, ejecutarlo y registrar evidencia.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.

<details>
<summary>Integración HTTP con MSW (1 archivo)</summary>

- [middleware.integracion.test.ts](../../panel/src/api/middleware.integracion.test.ts)

</details>

<details>
<summary>Unitarias (29 archivos)</summary>

- [problema.unitaria.test.ts](../../panel/src/api/problema.unitaria.test.ts)
- [fechas.unitaria.test.ts](../../panel/src/compartido/formato/fechas.unitaria.test.ts)
- [ids.unitaria.test.ts](../../panel/src/compartido/formato/ids.unitaria.test.ts)
- [km.unitaria.test.ts](../../panel/src/compartido/formato/km.unitaria.test.ts)
- [numeros.unitaria.test.ts](../../panel/src/compartido/formato/numeros.unitaria.test.ts)
- [relativo.unitaria.test.ts](../../panel/src/compartido/formato/relativo.unitaria.test.ts)
- [errores.unitaria.test.ts](../../panel/src/compartido/textos/errores.unitaria.test.ts)
- [actor.unitaria.test.ts](../../panel/src/funcionalidades/auditoria/modelo/actor.unitaria.test.ts)
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
- [franja.unitaria.test.ts](../../panel/src/funcionalidades/resumen/modelo/franja.unitaria.test.ts)
- [indicadores.unitaria.test.ts](../../panel/src/funcionalidades/resumen/modelo/indicadores.unitaria.test.ts)
- [transicion.unitaria.test.ts](../../panel/src/funcionalidades/resumen/modelo/transicion.unitaria.test.ts)
- [consulta.unitaria.test.ts](../../panel/src/funcionalidades/usuarios/modelo/consulta.unitaria.test.ts)
- [revision.unitaria.test.ts](../../panel/src/funcionalidades/usuarios/modelo/revision.unitaria.test.ts)
- [estadoSesion.unitaria.test.ts](../../panel/src/sesion/estadoSesion.unitaria.test.ts)
- [planificador.unitaria.test.ts](../../panel/src/sesion/planificador.unitaria.test.ts)
- [renovador.unitaria.test.ts](../../panel/src/sesion/renovador.unitaria.test.ts)

</details>

<details>
<summary>Componentes con jsdom (18 archivos)</summary>

- [BusquedaRapida.componente.test.tsx](../../panel/src/app/marco/BusquedaRapida.componente.test.tsx)
- [Marco.componente.test.tsx](../../panel/src/app/marco/Marco.componente.test.tsx)
- [Avisos.componente.test.tsx](../../panel/src/compartido/ui/Avisos.componente.test.tsx)
- [Dialogo.componente.test.tsx](../../panel/src/compartido/ui/Dialogo.componente.test.tsx)
- [PaginaAuditoria.componente.test.tsx](../../panel/src/funcionalidades/auditoria/ui/PaginaAuditoria.componente.test.tsx)
- [PaginaFijarContrasena.componente.test.tsx](../../panel/src/funcionalidades/cuenta/ui/PaginaFijarContrasena.componente.test.tsx)
- [PaginaVerificar.componente.test.tsx](../../panel/src/funcionalidades/cuenta/ui/PaginaVerificar.componente.test.tsx)
- [PaginaExportacion.componente.test.tsx](../../panel/src/funcionalidades/exportacion/ui/PaginaExportacion.componente.test.tsx)
- [FormularioLogin.componente.test.tsx](../../panel/src/funcionalidades/login/ui/FormularioLogin.componente.test.tsx)
- [PaginaRecuperar.componente.test.tsx](../../panel/src/funcionalidades/login/ui/PaginaRecuperar.componente.test.tsx)
- [DetalleAnomalia.componente.test.tsx](../../panel/src/funcionalidades/mapa/ui/DetalleAnomalia.componente.test.tsx)
- [PaginaMapa.componente.test.tsx](../../panel/src/funcionalidades/mapa/ui/PaginaMapa.componente.test.tsx)
- [AccionesModeracion.componente.test.tsx](../../panel/src/funcionalidades/moderacion/ui/AccionesModeracion.componente.test.tsx)
- [PaginaModeracion.componente.test.tsx](../../panel/src/funcionalidades/moderacion/ui/PaginaModeracion.componente.test.tsx)
- [PaginaResumen.componente.test.tsx](../../panel/src/funcionalidades/resumen/ui/PaginaResumen.componente.test.tsx)
- [DialogoRevision.componente.test.tsx](../../panel/src/funcionalidades/usuarios/ui/DialogoRevision.componente.test.tsx)
- [MenuUsuario.componente.test.tsx](../../panel/src/funcionalidades/usuarios/ui/MenuUsuario.componente.test.tsx)
- [PaginaUsuarios.componente.test.tsx](../../panel/src/funcionalidades/usuarios/ui/PaginaUsuarios.componente.test.tsx)

</details>

<details>
<summary>Accesibilidad de estilos (1 archivo)</summary>

- [contraste.accesibilidad.test.ts](../../panel/src/compartido/estilos/contraste.accesibilidad.test.ts)

</details>

<details>
<summary>Arquitectura (1 archivo)</summary>

- [lineas.arquitectura.test.ts](../../panel/pruebas/arquitectura/lineas.arquitectura.test.ts)

</details>

<details>
<summary>Contratos de datos simulados (1 archivo)</summary>

- [datosSimulados.contrato.test.ts](../../panel/pruebas/contrato/datosSimulados.contrato.test.ts)

</details>

<details>
<summary>Seguridad de plantillas (1 archivo)</summary>

- [cabeceras.seguridad.test.ts](../../panel/scripts/cabeceras.seguridad.test.ts)

</details>

<details>
<summary>E2E frontend simulado — accesibilidad (2 archivos)</summary>

- [accesibilidad.spec.ts](../../panel/pruebas/e2e/accesibilidad/accesibilidad.spec.ts)
- [teclado.spec.ts](../../panel/pruebas/e2e/accesibilidad/teclado.spec.ts)

</details>

<details>
<summary>E2E frontend simulado — funcionales (9 archivos)</summary>

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
<summary>E2E frontend simulado — rendimiento (1 archivo)</summary>

- [rendimiento.spec.ts](../../panel/pruebas/e2e/rendimiento/rendimiento.spec.ts)

</details>

<details>
<summary>E2E frontend simulado — seguridad (1 archivo)</summary>

- [almacenamiento.spec.ts](../../panel/pruebas/e2e/seguridad/almacenamiento.spec.ts)

</details>
