# Backend de KillaUru

Plataforma ASP.NET Core 10 de un monolito con seis módulos. La implementación actual corresponde a [001-plataforma-backend](../specs/001-plataforma-backend/spec.md); los módulos de negocio son esqueletos. La [revisión de implementación](../specs/001-plataforma-backend/revision-implementacion.md) documenta las brechas pendientes.

## Fuentes de verdad

1. [Documentos de referencia](../referencia/docs/).
2. [Constitución](../.specify/memory/constitution.md).
3. [Plan](../specs/001-plataforma-backend/plan.md), [decisiones](../specs/001-plataforma-backend/research.md), [contratos](../specs/001-plataforma-backend/contracts/) y [tareas](../specs/001-plataforma-backend/tasks.md).

## Dónde leer y cambiar código

| Carpeta | Responsabilidad |
|---|---|
| `src/KillaUru.Api/Program.cs` | Componer servicios y ordenar el pipeline HTTP |
| `src/KillaUru.Api/Seguridad` | Autenticación, cuotas, IP del túnel, CORS y cabeceras |
| `src/KillaUru.Api/Errores` | Respuestas ProblemDetails y excepciones inesperadas |
| `src/KillaUru.Api/Documentacion` | OpenAPI y Scalar |
| `src/KillaUru.Api/Salud` | Liveness, BD y migraciones esperadas |
| `src/KillaUru.Api/Observabilidad` | Logs, trazas e implementación del puerto de métricas |
| `src/KillaUru.Shared` | Value objects, contratos de eventos/métricas, transacciones y cálculo de horarios |
| `src/Modules` | Registro de cada módulo y sus futuras capas |
| `db/migraciones` | SQL explícito; la API no migra al iniciar |
| `deploy` | Contenedores, scripts, runbooks y puerta de cobertura |
| `tests` | Kernel, arquitectura, integración y pruebas k6 |

Los workflows están en `.github/workflows` de la raíz del repositorio, fuera de `KillaUru-main`.

## Compilar y comprobar

Requisitos: SDK indicado en `global.json`; Docker disponible para integración. Ejecutar desde esta carpeta:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Shared.Tests --no-build -- --coverage --coverage-output-format cobertura
dotnet test --project tests/KillaUru.Arquitectura.Tests --no-build
dotnet test --project tests/KillaUru.Integracion.Tests --no-build
python deploy/ci/cobertura_dominio.py 90
```

La solución usa Microsoft.Testing.Platform. Los proyectos de pruebas de módulos todavía vacíos no acreditan reglas de negocio. El script de cobertura tiene limitaciones documentadas en R08 de la revisión.

Para desplegar, consultar [deploy/runbooks/desplegar.md](deploy/runbooks/desplegar.md) y R02/R11/R12 de la revisión antes de usar el flujo. Los scripts `.sh` requieren Bash en Linux o en sus contenedores; no son comandos nativos de PowerShell.

## Comentarios de código

- Usar XML en contratos públicos, DTOs y controladores: propósito, entradas, salidas, unidades y condiciones relevantes. Las implementaciones pueden usar `<inheritdoc />` para conservar la documentación del puerto.
- Explicar decisiones que no son evidentes: orden del pipeline, propietario de una transacción, efectos después del commit, cancelación y confianza en cabeceras.
- Diferenciar lo que hace el código de lo que exige el requisito. Una protección pendiente no se presenta como implementada.
- Citar FR/RN/ADR cuando expliquen una decisión; el identificador por sí solo no explica el comportamiento.
- En las pruebas, describir el escenario comprobado. Una prueba con objetos sintéticos no equivale a verificar toda la salida HTTP o la telemetría.
- Conservar archivos de menos de 200 líneas y métodos de hasta 20. Si una responsabilidad crece, extraer un colaborador; no recortar la explicación útil ni repartir una clase con `partial`.
