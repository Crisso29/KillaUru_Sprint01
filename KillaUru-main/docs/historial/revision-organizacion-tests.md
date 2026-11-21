# Revisión de la organización de pruebas

> Registro histórico; conserva los resultados de su fecha. [Volver al índice de documentación](../README.md).

## Alcance del 21 de noviembre de 2026

Se corrigió la clasificación sin cambiar frameworks: xUnit, Vitest, JUnit y pytest.
Los resultados corresponden al árbol de trabajo local sin commit; no certifican
el despliegue ni sustituyen el cierre de cada spec.

## Cambios y motivos

| Área | Cambio | Motivo |
|---|---|---|
| Panel | 52 archivos con sufijo de categoría y comandos `test:*` | Encontrar y ejecutar cada suite manteniendo proximidad con producción |
| Panel E2E | Configuración simulada y desplegada separadas | Evitar ejecutar MSW y presentar el resultado como backend real |
| Backend E2E | Rutas y permisos corregidos en escenarios omitidos | Eliminar supuestos obsoletos; siguen pendientes y fallan si se habilitan vacíos |
| Android | Test del lector trasladado de soporte a contratos | Separar colaboradores de comprobaciones ejecutables |
| Android | Contratos esperados obligatorios | Evitar validar solamente los archivos presentes |
| Android mapa | Dobles en archivos propios; reloj conforme al puerto actual | Reducir tamaño por responsabilidad, conservando escenarios |
| ML | Caso partición + verificador trasladado a integración | Separar regla aislada de colaboración entre componentes |
| Repositorio | README técnico y enlaces desde la presentación | Distinguir referencia normativa, implementación, operación y evidencias |

Distribución Vitest: 29 archivos unitarios, 18 de componentes, uno de integración,
uno de contratos, uno de arquitectura, uno de accesibilidad y uno de seguridad.
Componentes con MSW integran partes del frontend; no ejecutan el backend.

## Verificación y límites

| Comprobación | Resultado |
|---|---|
| `npm test` en panel | 389 pruebas aprobadas, 52 archivos; advertencia preexistente de navegación jsdom |
| `npm run test:seguridad` tras renombrar el último archivo | 12 pruebas aprobadas |
| `npx tsc -b` | Correcto |
| `npm run e2e -- --list` | 159 casos descubiertos en 13 archivos; no ejecutados |
| `e2e:pruebas -- --list` con URL HTTPS ficticia | Falla por ausencia de tests, como corresponde; sin conectar a servicios |
| pytest no-fuga, partición integrada y arquitectura | 6 pruebas aprobadas |
| Build del proyecto .NET de integración | Correcto, cero advertencias y errores; no ejecuta PostgreSQL ni E2E |
| Tests Android seleccionados | Bloqueados antes de ejecutarse por errores de compilación en producción |
| Inspección de fuentes Kotlin de prueba | Todos por debajo de 200 líneas; soporte sin clases Test |

Android falla en `DescargaMapaPantalla.kt:28` por opt-in experimental, en
`ActualizarAnomaliasWorker.kt:26` por `obtenerSesion` y en
`ActualizarAnomaliasUseCase.kt:80` por `ahoraMs`. Deben corregirse y repetir Gradle.

T017 de la spec 008 vuelve a pendiente: comprobar presencia y parseo no cubre
la validación de ejemplos contra esquemas. Seguimiento conciliado: **523/637 tareas
marcadas, 114 pendientes**. Este conteo no expresa cobertura.

Consulta el [mapa del repositorio](../../README.md), la [guía de pruebas](../guias/pruebas.md)
y los archivos `pruebas.md` y `cierre.md` de cada spec para continuar.
