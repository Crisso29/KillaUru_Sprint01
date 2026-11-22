# Backend de KillaUru

Plataforma ASP.NET Core 10 de un monolito con seis módulos implementados en distintos grados. Consulta el [estado verificable](../docs/seguimiento/estado-implementacion.md) y el [índice de pruebas por spec](../docs/guias/pruebas.md) para distinguir código, pruebas y validaciones pendientes.

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
| `src/Modules` | Registro, dominio, casos de uso, adaptadores e infraestructura de cada módulo |
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

La solución usa Microsoft.Testing.Platform. Las suites de módulos contienen pruebas de reglas y casos de uso; la integración se organiza por spec y propósito. Consulta [tests/README.md](tests/README.md) para ejecutar cada alcance. La cobertura debe medirse en la revisión correspondiente; no se deduce de la cantidad de tests.

Para desplegar, consultar [deploy/runbooks/desplegar.md](deploy/runbooks/desplegar.md) y R02/R11/R12 de la revisión antes de usar el flujo. Los scripts `.sh` requieren Bash en Linux o en sus contenedores; no son comandos nativos de PowerShell.
Para pruebas de carga sin la VM existe un entorno local desechable: [deploy/local/README.md](deploy/local/README.md).

## Comentarios de código

- Usar XML en contratos públicos, DTOs y controladores: propósito, entradas, salidas, unidades y condiciones relevantes. Las implementaciones pueden usar `<inheritdoc />` para conservar la documentación del puerto.
- Explicar decisiones que no son evidentes: orden del pipeline, propietario de una transacción, efectos después del commit, cancelación y confianza en cabeceras.
- Diferenciar lo que hace el código de lo que exige el requisito. Una protección pendiente no se presenta como implementada.
- Citar FR/RN/ADR cuando expliquen una decisión; el identificador por sí solo no explica el comportamiento.
- En las pruebas, describir el escenario comprobado. Una prueba con objetos sintéticos no equivale a verificar toda la salida HTTP o la telemetría.
- Conservar archivos de menos de 200 líneas y métodos de hasta 20. Si una responsabilidad crece, extraer un colaborador; no recortar la explicación útil ni repartir una clase con `partial`.
