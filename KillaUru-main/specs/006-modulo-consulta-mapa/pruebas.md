# Pruebas 006 — Consulta de mapa

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

Filtros, celdas, cambios y alcance según rol.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Alcance | Ubicación |
|---|---|
| Reglas y casos de uso | [KillaUru.Mapa.Tests](../../backend/tests/KillaUru.Mapa.Tests/) |
| Arquitectura y legibilidad | [Suite compartida](../../backend/tests/KillaUru.Arquitectura.Tests/) |
| Carga y resiliencia | [Escenarios k6 compartidos](../../backend/tests/carga/README.md) |
| E2E entre módulos | [Recorrido compartido, pendiente](../../backend/tests/KillaUru.Integracion.Tests/sistema/e2e/FlujoPuntaAPuntaTests.cs) |

## Cómo ejecutar

Desde `KillaUru-main/backend/`:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Mapa.Tests --no-build
dotnet test --project tests/KillaUru.Arquitectura.Tests --no-build
```

Esta spec todavía no tiene suite de integración dedicada. La suite compartida del backend requiere Docker; no sustituye esos casos pendientes.
Las pruebas con dobles no certifican PostgreSQL real. El rendimiento local no acredita carga.

## Estado y límites

No se encontró una suite de integración dedicada a esta spec en el proyecto compartido.
Las pruebas de otros módulos y el código implementado no sustituyen esa evidencia.
El E2E compartido conserva dos casos omitidos con pasos sin implementar.
La ejecución de k6 y las validaciones operacionales requieren evidencia del entorno de pruebas.
Consulta [la revisión actual](../../docs/seguimiento/estado-implementacion.md) antes de cerrar tareas.

## Al añadir un caso

Citar `006/FR-...`, nombrar escenario y resultado, y separar Arrange / Act / Assert.
Asignar un propietario; enlazar desde otras specs cuando el flujo sea compartido.
Registrar comando, fecha, commit, entorno, resultado y omitidos.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.
Las suites de arquitectura, carga y E2E compartidas se detallan en [pruebas de plataforma](../001-plataforma-backend/pruebas.md#inventario-de-archivos-de-prueba).

<details>
<summary>Casos de uso (3 archivos)</summary>

- [ConsultarCambiosUseCaseTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Application/ConsultarCambiosUseCaseTests.cs)
- [ConsultarFiltradoUseCaseTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Application/ConsultarFiltradoUseCaseTests.cs)
- [ConsultarPorCeldasUseCaseTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Application/ConsultarPorCeldasUseCaseTests.cs)

</details>

<details>
<summary>Reglas de dominio (3 archivos)</summary>

- [BboxTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Domain/BboxTests.cs)
- [PoliticaAlcanceTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Domain/PoliticaAlcanceTests.cs)
- [ZonaDemoTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Domain/ZonaDemoTests.cs)

</details>
