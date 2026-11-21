# Pruebas 007 — Dashboard

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

Cálculo, persistencia y consulta de indicadores.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Alcance | Ubicación |
|---|---|
| Reglas y casos de uso | [KillaUru.Dashboard.Tests](../../backend/tests/KillaUru.Dashboard.Tests/) |
| Arquitectura y legibilidad | [Suite compartida](../../backend/tests/KillaUru.Arquitectura.Tests/) |
| Integración por spec | [Carpetas por propósito](../../backend/tests/KillaUru.Integracion.Tests/specs/007-modulo-dashboard/) |
| Seguridad | [Seguridad](../../backend/tests/KillaUru.Integracion.Tests/specs/007-modulo-dashboard/seguridad/) |
| Carga y resiliencia | [Escenarios k6 compartidos](../../backend/tests/carga/README.md) |
| E2E entre módulos | [Recorrido compartido, pendiente](../../backend/tests/KillaUru.Integracion.Tests/sistema/e2e/FlujoPuntaAPuntaTests.cs) |

## Cómo ejecutar

Desde `KillaUru-main/backend/`:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Dashboard.Tests --no-build
dotnet test --project tests/KillaUru.Arquitectura.Tests --no-build
dotnet test --project tests/KillaUru.Integracion.Tests --no-build
```

La suite de integración requiere Docker y ejecuta también las otras specs.
Las pruebas con dobles no certifican PostgreSQL real. El rendimiento local no acredita carga.

## Estado y límites

Hay casos implementados; su existencia no certifica una ejecución aprobada.
El E2E compartido conserva dos casos omitidos con pasos sin implementar.
La ejecución de k6 y las validaciones operacionales requieren evidencia del entorno de pruebas.
Consulta [la revisión actual](../../docs/seguimiento/estado-implementacion.md) antes de cerrar tareas.

## Al añadir un caso

Citar `007/FR-...`, nombrar escenario y resultado, y separar Arrange / Act / Assert.
Asignar un propietario; enlazar desde otras specs cuando el flujo sea compartido.
Registrar comando, fecha, commit, entorno, resultado y omitidos.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.
Las suites de arquitectura, carga y E2E compartidas se detallan en [pruebas de plataforma](../001-plataforma-backend/pruebas.md#inventario-de-archivos-de-prueba).

<details>
<summary>Casos de uso (3 archivos)</summary>

- [CalcularIndicadoresUseCaseTests.cs](../../backend/tests/KillaUru.Dashboard.Tests/Application/CalcularIndicadoresUseCaseTests.cs)
- [ConsultarResumenUseCaseTests.cs](../../backend/tests/KillaUru.Dashboard.Tests/Application/ConsultarResumenUseCaseTests.cs)
- [MapeoResumenDiaTests.cs](../../backend/tests/KillaUru.Dashboard.Tests/Application/MapeoResumenDiaTests.cs)

</details>

<details>
<summary>Reglas de dominio (2 archivos)</summary>

- [CalculadoraIndicadoresTests.cs](../../backend/tests/KillaUru.Dashboard.Tests/Domain/CalculadoraIndicadoresTests.cs)
- [DiaLocalTests.cs](../../backend/tests/KillaUru.Dashboard.Tests/Domain/DiaLocalTests.cs)

</details>

<details>
<summary>Adaptadores con dobles o componentes locales (1 archivo)</summary>

- [TareaIndicadoresDiariosTests.cs](../../backend/tests/KillaUru.Dashboard.Tests/Infrastructure/TareaIndicadoresDiariosTests.cs)

</details>

<details>
<summary>Integración (4 archivos)</summary>

- [DashboardApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/007-modulo-dashboard/integracion/DashboardApiTests.cs)
- [DesfaseHorarioTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/007-modulo-dashboard/integracion/DesfaseHorarioTests.cs)
- [IndicadoresRepositorioTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/007-modulo-dashboard/integracion/IndicadoresRepositorioTests.cs)
- [TareaIndicadoresTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/007-modulo-dashboard/integracion/TareaIndicadoresTests.cs)

</details>

<details>
<summary>Seguridad (1 archivo)</summary>

- [EsquemaPrivacidadTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/007-modulo-dashboard/seguridad/EsquemaPrivacidadTests.cs)

</details>
