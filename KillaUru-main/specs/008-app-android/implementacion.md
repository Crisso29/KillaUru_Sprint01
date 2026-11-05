# Seguimiento de implementación 008

Fecha de inicio: 2026-11-03. Estado: en curso; no se certifica el cierre de la feature.
Fuente normativa: constitución 1.3.0, `referencia/docs/`, D18 y los artefactos de 008.

## Revisión previa del 007

Se revisaron dominio, casos de uso, integración, migraciones, README, plan,
tareas y trazabilidad. Sus pruebas usan Arrange / Act / Assert y referencias
a requisitos. La existencia del código o una casilla de trazabilidad no
demuestran que sus pruebas hayan pasado ni prueban un ciclo TDD histórico.

Ejecuciones locales:

- Dashboard: 23 pruebas unitarias correctas; cobertura de líneas del dominio
  100 %, obtenida con Microsoft.Testing.Extensions.CodeCoverage en Cobertura.
- Integración Dashboard: inicialmente 12 errores por Docker detenido; tras
  iniciarlo, error real de permisos en V0007. Tras las correcciones, 12/12 verdes.
- Regresión de la solución: 1073 casos, 1046 correctos, 24 errores y 3 omitidos.
  Los errores corresponden a arquitectura y legibilidad; no se declara CI verde.
  Registro local: `backend/TestResults/regresion-previa-008.log`.

Correcciones reproducidas con pruebas rojas antes de modificar producción:

| Hallazgo | Corrección | Trazabilidad |
|---|---|---|
| V0007 intenta crear el esquema sin permiso CREATE sobre la base | Usar el esquema que V0000 ya crea; no ampliar privilegios | 007 T001; 001 T-BD01 |
| Constructores internos no resolubles por DI | Constructores públicos en clases que siguen siendo internas | 007 T013; constitución II |
| Singleton captura lectores de base de datos scoped | Scope por consulta de salud; lectores de Mapa scoped, caché compartida | 005/006; constitución II |
| EF consulta `Dia`, `Indicador`, `Valor` en lugar de columnas SQL | Mapeo explícito a `dia`, `indicador`, `valor` | 007 T011 |
| Prueba de acceso usa un rol inexistente | Usar `Roles.ConductorRegistrado` para comprobar 403 | 007 T019 |
| Caso de consulta supera 20 líneas | Extraer `MapeadorResumen`, preservar consultas y fórmulas | 007 T017; constitución VIII |

La regresión completa obliga a conservar pendientes las puertas globales.
No se marcaron automáticamente todas las tareas de 007.

## Registro TDD del 008

| Tarea | Rojo observado | Verde observado | Refactorización / estado |
|---|---|---|---|
| T005 / FR-076 | `unittest`: falta `verificar_lineas` | 3/3 pruebas; 0 infracciones | Función pura de inspección separada de CLI; límites 199/200 |
| T008 / FR-013 | Pendiente de ejecución de JUnit | Pendiente | Pruebas AAA de versión, variante, orden y unicidad |

## Entorno y decisiones de integración

- El proyecto real vive en `KillaUru-main/app`, y `.github` está en la raíz
  Git exterior. CI usa esa ruta y también reacciona a cambios de contratos.
- Un módulo Gradle, JVM 17, minSdk 26, compileSdk/targetSdk 35.
- Gradle 8.13, AGP 8.11.1 y Kotlin 2.2.21 fijados. Compatibilidad consultada en
  [Android](https://developer.android.com/build/releases/about-agp) y
  [Kotlin](https://kotlinlang.org/docs/gradle-configure-project.html).
- Se generó el wrapper con Gradle. Configuración de tareas verificada con JDK 17.
  El equipo tenía JDK 24/25; se descargó Temurin 17 portable en TEMP para verificar.
- No hay evidencia de campo, publicación R2, APK funcional ni modelo validado de 009.
  Esos requisitos permanecen pendientes y no se sustituyen por resultados simulados.
