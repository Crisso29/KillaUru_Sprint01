# Estado verificable de KillaUru

> Seguimiento actual. [Volver al índice de documentación](../README.md).

**Revisión base:** 2026-11-15. **Commit base:** `5e5e86b`, con cambios de organización sin commit.
Actualización de organización 2026-11-21: suites del panel identificadas por sufijo,
configuración E2E desplegada separada y T017 de Android reabierta por validación de ejemplos pendiente.
Los resultados de noviembre 15 conservan su fecha; no representan una repetición de todas las suites.
La [revisión de organización](../historial/revision-organizacion-tests.md) registra las comprobaciones
del 21 de noviembre y los tres errores de compilación que impiden ejecutar Android.
Este informe sustituye el resumen desactualizado del 9 de noviembre.
[Índice de pruebas](../guias/pruebas.md) · [Calidad y seguridad](../guias/calidad-y-seguridad.md) · [Cierre y despliegue](../guias/cierre-y-despliegue.md)

## Seguimiento actual

Conteos de casillas en `tasks.md`; no son cobertura ni certificación de producción.

| Spec | Área | Marcadas / total | Pendientes |
|---|---|---:|---:|
| 001 | Plataforma | 71/84 | 13 |
| 002 | Auditoría | 35/38 | 3 |
| 003 | Identidad | 65/65 | 0 |
| 004 | Anomalías | 56/58 | 2 |
| 005 | Ingesta | 29/41 | 12 |
| 006 | Consulta de mapa | 20/33 | 13 |
| 007 | Dashboard | 24/26 | 2 |
| 008 | Android | 103/128 | 25 |
| 009 | Dataset/modelo | 46/74 | 28 |
| 010 | Panel | 77/94 | 17 |
| **Total** | | **526/641** | **115** |

Panel tenía 88 tareas marcadas. Se reabrieron 11 por E2E vacíos u omitidos:
T032, T033, T034, T050, T058, T062, T068, T081, T085, T086 y T087.
No se retiró funcionalidad: se corrigió el seguimiento para reflejar la evidencia.

## Actualización documental y verificación de pendientes

Se cerraron solo cinco entregables documentales: 001/T055, 005/T033, 006/T028, 007/T021 y 009/T072. Se reabrieron 001/T042 (contratos incompletos) y 001/T067 (captura real de logs/trazas no cubierta por el filtro sintético). Las checklists se conciliaron con sus tareas abiertas; detalle en [revisión documental](../historial/revision-documental.md). Los resultados de tests de abajo proceden de la revisión local anterior de esta sesión; no se repitieron por estos cambios exclusivamente documentales.

## Organización aplicada

- 76 archivos C# de integración por spec y propósito; contenido idéntico verificado con SHA-256.
  Se conservan proyectos, namespaces y fixtures.
- ML separado en unitarias, integración, E2E sintético y arquitectura.
- E2E del panel separado en funcionales, seguridad, accesibilidad y rendimiento.
- Una guía `pruebas.md` por spec; guía común, criterios de calidad y reglas de mantenimiento.
- 15 pruebas vacías del panel figuran como `fixme`, sin falsas aserciones de éxito.
- Corregidas la raíz del test de líneas ML y la escritura UTF-8 de sus fixtures CSV.
- Actualizadas referencias a archivos trasladados y documentación de estado.

## Evidencia local de esta revisión

Windows/PowerShell, SDK .NET del repositorio, Node local y Python 3.11 de `ml/.venv`.
Los resultados corresponden al árbol de trabajo, no al estado remoto de CI.

| Comprobación | Resultado |
|---|---|
| `dotnet build KillaUru.sln --no-restore` | 0 errores; 2 advertencias de permisos de metadatos de la extensión VS Code |
| Anomalías / Auditoría / Identidad | 100 / 22 / 89 aprobados |
| Ingesta / Mapa / Dashboard / Shared | 46 / 44 / 23 / 62 aprobados |
| Arquitectura backend | 485 aprobados |
| **Backend sin Docker** | **871 aprobados, 0 fallidos, 0 omitidos** |
| Panel: `npm test` | 389 casos en 52 archivos aprobados; avisos de navegación no implementada en jsdom |
| Panel: `npm run e2e -- --list` | 159 casos en 13 archivos, incluidos pendientes y repetición por navegador; solo descubrimiento |
| ML: `python -m pytest` | Primera ejecución: 127 aprobados, 1 fallo de codificación de fixture en Windows |
| ML tras corregir UTF-8 | `python -m pytest tests/integracion/test_estado.py tests/arquitectura`: 6/6 aprobados |
| Integración backend | Compila y xUnit lista los casos; no ejecutada contra PostgreSQL |

Desde `backend/`: `dotnet test --project tests/KillaUru.<Modulo>.Tests --no-build`.
Arquitectura usa `KillaUru.Arquitectura.Tests`. Panel y ML se ejecutan desde sus carpetas.
Vitest necesitó permiso fuera del sandbox: esbuild no podía leer directorios ascendentes.

No se repitió todo ML después del cambio de codificación: se verificaron los cinco casos
del archivo afectado y la regla de líneas. Los demás 122 casos no se modificaron después
de su ejecución aprobada. No se ocultaron advertencias de TensorFlow/sklearn.

## Pendientes para certificar el cierre

La deuda técnica ya demostrada (procesamiento, pools, *healthcheck*, prueba intermitente) está en
[deuda-tecnica.md](deuda-tecnica.md), con su ubicación en el código y la tarea que la sigue (001/T082–T084, 005/T041).

- **Integración:** no se encontró Docker en PATH; falta ejecutar Testcontainers.
- **E2E backend:** el recorrido compartido conserva dos casos con `Skip` y pasos sin implementar.
- **005/006:** ambas tienen contrato HTTP aprobado el 2026-11-21 con Testcontainers (005/T034 4/4, 006/T029 5/5).
  005/T012 (recepción HTTP con PostgreSQL real) está cerrada con 17/17 escenarios. Faltan el resto de pruebas
  con PostgreSQL de 005 (T011, T020, T024, T027, T030, T032) y las de API de 006. 006/T029 corrigió un 400 real con `?severidad=4,5` y `?estado=a,b`.
- **Prueba intermitente:** `003/ConsolaPrivacidadTests` falló 1 vez en 2 ejecuciones de la suite completa
  (`ArgumentOutOfRangeException` en `StringWriter`). Sola pasó 3/3. El sink de prueba no es seguro entre hilos.
  No reapareció después de quitar las purgas de Ingesta de `FabricaApi` (2 ejecuciones); no está demostrado que fuera esa la causa.
- **005/T012 bloque C:** halló un 500 real con cuerpos de más de 256 KB en Kestrel, `datos_invalidos` en vez de
  `lote_invalido` y errores sin `problem+json`. Corregido; detalle en `specs/005-modulo-ingesta/cierre.md`.
- **Carga k6 (2026-11-21, local, misma máquina):** [progresiva hasta 2 000 usuarios](evidencias/carga/2026-11-21-local-progresiva-2000/README.md).
  Metas de 300 (RNF-01, RNF-02) cumplidas, 0 5xx y 503 de defensa desde 1 000. **El procesamiento (77 eventos/s) es
  más lento que la llegada desde 300 usuarios: riesgo para RNF-23, pendiente de medir y diagnosticar.** Repetir en la VM.
- **Diagnóstico (2026-11-22):** [trabajadores y pool](evidencias/carga/2026-11-22-local-trabajadores-y-pool/README.md). Más
  trabajadores empeoran; el procesamiento se frena al crecer los datos. Faltaba `Maximum Pool Size=30` (§3.7, ARQ-22):
  bajo ráfaga daba 500 por error 53300. Ya está configurado y vigilado por `PoolDeConexionesTests`. Quedan 3 pools (techo real 90).
- **Pruebas y cobertura del backend (2026-11-22):** [1 153 pruebas, 0 fallidas, 3 pendientes; dominio ≥ 97 %, backend 78,8 %](evidencias/pruebas/2026-11-22-backend/README.md),
  generado con `backend/tests/generar-resultados.ps1`.
- **Pruebas y cobertura del panel (2026-11-22):** [389 Vitest aprobadas; 75,1 % de líneas; E2E: solo `resumen.spec.ts` implementado, 147 casos `skip/fixme`](evidencias/pruebas/2026-11-22-panel/README.md).
- **Suite backend completa (2026-11-21, Docker, 2 ejecuciones seguidas):** 1141 casos: 1138 aprobados, 0 fallidos y 3 omitidos (E2E pendientes).
- **Panel:** escenarios `skip/fixme`; E2E locales con MSW y configuración desplegada separada, aún sin archivos `*.pruebas.ts`
  para acreditar el backend real.
- **Android:** aún no existen `src/androidTest/` ni `src/test/resources/`; quedan pruebas
  instrumentadas, de campo, sesión y sincronización. No se ejecutó Gradle en este cambio.
- **ML:** campaña de campo, benchmark real y publicación final según 009.
- **Operación:** carga, caos, restauración y controles externos requieren evidencia del entorno de pruebas.
- **Seguridad:** esta reorganización no es una auditoría de vulnerabilidades ni certifica resistencia
  a SQL injection, DDoS, XSS o CSRF. La guía identifica controles y evidencia pendiente.
- **Cobertura/CI:** no se midió cobertura ni se certificó CI remoto en esta revisión.

Una spec se cierra cuando sus criterios de aceptación tienen evidencia suficiente.
Una casilla marcada, un test omitido o un archivo existente no reemplaza esa comprobación.
