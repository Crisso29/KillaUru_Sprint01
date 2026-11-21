# Pruebas de KillaUru: empieza aquí

> Guía de trabajo. [Volver al índice de documentación](../README.md).

Una **spec** describe una funcionalidad y sus requisitos. Una **suite** es un conjunto de pruebas ejecutado por una herramienta.
Para buscar una prueba, abre primero la spec y su archivo `pruebas.md`.

**Navegación revisada el 21 de noviembre de 2026:** cada guía de spec tiene un
inventario desplegable por categoría con enlaces al archivo exacto. Para entender
el resto de documentos, consulta [qué archivo abrir](../../specs/README.md#qué-archivo-abrir).
La [revisión archivada de arquitectura](../historial/revision-arquitectura-2026-10-21/README.md) explica decisiones e informes
históricos; el estado actual está en `cierre.md`.

El [mapa del repositorio](../../README.md) distingue documentación normativa, código,
operación y evidencia. En el panel, el sufijo identifica la suite:
`.unitaria.test`, `.componente.test`, `.integracion.test`, `.contrato.test`,
`.arquitectura.test`, `.accesibilidad.test` o `.seguridad.test`.
`npm test` ejecuta todas; `npm run test:unitarias` selecciona solo unitarias.
Los componentes pueden integrar React, router y HTTP simulado; no prueban la API real.

Los E2E Playwright `*.spec.ts` prueban el frontend con MSW. Los escenarios
desplegados `*.pruebas.ts` usan otra configuración, requieren `PANEL_URL` y todavía
no están implementados. La ausencia de casos debe fallar, nunca contar como aprobación.

La [revisión de organización](../historial/revision-organizacion-tests.md) registra los cambios,
las pruebas ejecutadas y los bloqueos detectados el 21 de noviembre de 2026.

Para decidir qué falta implementar o validar, consulta `cierre.md` de la spec y el
[plan de cierre y despliegue](cierre-y-despliegue.md).

## 1. Encuentra la funcionalidad

| Spec | Qué comprueba | Guía |
|---|---|---|
| 001 | Plataforma, autenticación HTTP, cuotas y salud | [Pruebas](../../specs/001-plataforma-backend/pruebas.md) |
| 002 | Auditoría y transacciones | [Pruebas](../../specs/002-modulo-auditoria/pruebas.md) |
| 003 | Identidad, cuentas y sesiones | [Pruebas](../../specs/003-modulo-identidad-accesos/pruebas.md) |
| 004 | Anomalías y moderación | [Pruebas](../../specs/004-modulo-anomalias/pruebas.md) |
| 005 | Ingesta y procesamiento de lotes | [Pruebas](../../specs/005-modulo-ingesta/pruebas.md) |
| 006 | Consulta del mapa | [Pruebas](../../specs/006-modulo-consulta-mapa/pruebas.md) |
| 007 | Indicadores del dashboard | [Pruebas](../../specs/007-modulo-dashboard/pruebas.md) |
| 008 | Aplicación Android | [Pruebas](../../specs/008-app-android/pruebas.md) |
| 009 | Dataset y modelo | [Pruebas](../../specs/009-dataset-y-modelo/pruebas.md) |
| 010 | Panel web | [Pruebas](../../specs/010-panel-admin/pruebas.md) |

## 2. Distingue los tipos

| Tipo | Pregunta que responde | Dependencias habituales |
|---|---|---|
| Unitarias | ¿Esta regla o caso de uso produce el resultado esperado? | Objetos en memoria; dobles para puertos |
| Integración | ¿Colaboran correctamente varios componentes? | BD, archivos, HTTP de prueba o framework |
| Contratos | ¿La interfaz coincide con lo pactado? | OpenAPI, esquemas y respuestas |
| Seguridad | ¿Se rechaza lo prohibido y se protege información? | Puede requerir integración |
| E2E | ¿Se completa el recorrido desde la entrada hasta el resultado? | Navegador/app y servicios involucrados |
| Rendimiento | ¿Cuánto tarda una operación bajo condiciones declaradas? | Reloj de medición y entorno identificado |
| Carga | ¿Qué ocurre con múltiples usuarios concurrentes? | k6 y entorno de pruebas |
| Arquitectura | ¿Se respetan capas, dependencias y límites de código? | Inspección de fuentes/ensamblados |

Seguridad es un propósito; integración es un alcance. Una prueba puede tener ambos.
Se guarda una vez, según su propósito principal. No crear carpetas vacías para aparentar cobertura.
Una medición individual de latencia no sustituye una prueba de carga.

## 3. Organización física

- Backend: proyectos `KillaUru.<Modulo>.Tests` para reglas y casos de uso.
  Sus capas `Domain` y `Application` ayudan a ubicar la responsabilidad.
- Backend con host/BD: `KillaUru.Integracion.Tests/specs/<spec>/<tipo>`.
  Las fábricas comunes están en `soporte/`; los E2E compartidos, en `sistema/e2e/`.
- Carga combinada: `backend/tests/carga/`. Sus scripts comparten rutas de datos y ejecución.
- Android: `app/src/test` para JVM/Robolectric y `app/src/androidTest` para dispositivo.
  Dentro se conservan paquetes por funcionalidad. `src/test` también contiene integración local.
- Panel: unitarias/componentes junto a la funcionalidad en `src/`;
  `pruebas/contrato`, `pruebas/arquitectura` y `pruebas/e2e/<propósito>`.
- ML: `ml/tests/unitarias`, `integracion`, `e2e` y `arquitectura`.
  `tests/sinteticos.py` contiene datos reutilizables, no casos ejecutables.

Los proyectos, namespaces C# y paquetes Android conservan su identidad.
La ubicación de los archivos de integración C# expresa spec y propósito; el namespace conserva
el módulo original para mantener las referencias entre fixtures y la identidad de las pruebas.
No se cambian frameworks ni proyectos solo para ordenar carpetas.

## 4. Escribe una prueba que se entienda

1. Identifica requisito y spec: `003/FR-020`, no solo `FR-020`.
2. Nombra el escenario y resultado: `Conductor_sin_rol_admin_recibe_403`.
3. **Arrange (preparar):** datos mínimos, permisos, reloj y dependencias.
4. **Act (actuar):** una operación de negocio.
5. **Assert (comprobar):** resultado observable y efectos relevantes.
6. Incluye valores límite, entradas inválidas y el caso permitido junto al rechazado.
7. Mantén independencia: sin orden entre pruebas, esperas arbitrarias ni secretos reales.

Un doble sustituye una dependencia de forma controlada. Una fixture prepara recursos compartidos.
Extrae helpers con nombre de negocio cuando se repite preparación; conserva visible la aserción.
Los comentarios explican el requisito, las unidades, el límite o el motivo de un doble.
No repiten literalmente cada línea de código.

## 5. Registra evidencia, no solo casillas

| Campo | Qué guardar |
|---|---|
| Escenario | Spec/requisito, nombre del caso y ruta |
| Estado | Pendiente, implementado sin ejecutar, aprobado, fallido u omitido |
| Ejecución | Comando, fecha, commit y entorno |
| Resultado | Aprobados, fallidos, omitidos y enlace al informe |
| Alcance | Qué dependencia fue real o simulada y qué no se verificó |

`skip` y `fixme` cuentan como pendientes. Un archivo existente no demuestra cobertura.
Un E2E con MSW verifica el panel contra respuestas simuladas; no acredita la API real.
El pipeline ML sintético no acredita calidad del modelo en carretera.
Nunca cerrar una tarea por una aserción que solo comprueba que existe `page` o `browser`.

Consulta [calidad y seguridad](calidad-y-seguridad.md) y el [estado actual](../seguimiento/estado-implementacion.md).

