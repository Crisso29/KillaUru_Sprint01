# Pruebas del backend

Empieza por el [índice por spec](../../docs/guias/pruebas.md).
Los proyectos de reglas conservan sus nombres `KillaUru.<Modulo>.Tests`.
La integración se organiza en `KillaUru.Integracion.Tests/specs/<spec>/<tipo>`.

| Ubicación | Responsabilidad |
|---|---|
| `KillaUru.<Modulo>.Tests/Domain` | Reglas de negocio y límites |
| `KillaUru.<Modulo>.Tests/Application` | Casos de uso con dobles |
| `KillaUru.<Modulo>.Tests/Adapters` | Lectura de parámetros y cuerpos HTTP sin levantar el host (Mapa e Ingesta) |
| `KillaUru.Integracion.Tests/specs` | HTTP, BD, contratos, seguridad y rendimiento por spec |
| `KillaUru.Integracion.Tests/soporte` | Host, PostgreSQL, tokens y lectura de contratos compartidos |
| `KillaUru.Integracion.Tests/sistema/e2e` | Recorridos compartidos entre specs; actualmente pendientes |
| `KillaUru.Arquitectura.Tests` | Capas, dependencias y legibilidad |
| `carga` | k6; ejecución separada en entorno de pruebas |

Dentro de cada spec, `soporte` contiene preparación específica, no casos de prueba.
Las carpetas `Infrastructure` de proyectos de módulo pueden probar adaptadores con dobles:
leer las dependencias del caso antes de asumir que usa una BD real.

Desde `backend/`:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Identidad.Tests --no-build
dotnet test --project tests/KillaUru.Arquitectura.Tests --no-build
dotnet test --project tests/KillaUru.Integracion.Tests --no-build
```

La última suite requiere Docker para Testcontainers.

**Resultados y cobertura de todo el backend** (lo mismo que CI, un proyecto por carpeta):
`pwsh tests/generar-resultados.ps1` borra y regenera `TestResults/` con `RESUMEN.md`, `cobertura/index.html`
y la puerta del 90 % por dominio. Las ejecuciones que sirven de evidencia se copian a
[docs/seguimiento/evidencias/pruebas](../../docs/seguimiento/evidencias/README.md).
Para toda la solución: `dotnet test --solution KillaUru.sln --no-build`.
El traslado conserva namespaces y contenidos C#: los fixtures siguen compartiendo la misma BD.
No crear un contenedor por spec únicamente para reflejar la estructura de carpetas.

Revisión 2026-11-21: los escenarios pendientes de `sistema/e2e` describen las rutas
vigentes (`/api/v1/anomalies/batch`, `/api/v1/anomalies`, `/api/v1/usuarios`).
La ingesta admite anónimos; no existe polling por lote ni auditoría `LoteEnviado`.
Conservan `Skip` y una aserción que falla si se habilitan sin implementación.

