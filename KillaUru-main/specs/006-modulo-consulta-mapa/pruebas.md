# Pruebas 006 — Consulta de mapa

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

Filtros, celdas, cambios y alcance según rol.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Alcance | Ubicación |
|---|---|
| Reglas y casos de uso | [KillaUru.Mapa.Tests](../../backend/tests/KillaUru.Mapa.Tests/) |
| Arquitectura y legibilidad | [Suite compartida](../../backend/tests/KillaUru.Arquitectura.Tests/) |
| Integración por spec | [Carpetas por propósito](../../backend/tests/KillaUru.Integracion.Tests/specs/006-modulo-consulta-mapa/) |
| Contrato HTTP | [Contratos](../../backend/tests/KillaUru.Integracion.Tests/specs/006-modulo-consulta-mapa/contratos/) |
| Carga y resiliencia | [Escenarios k6 compartidos](../../backend/tests/carga/README.md) |
| Polígono del tramo demo (T002) | [test_construir_tramo_demo.py](../../backend/db/datos/test_construir_tramo_demo.py): km 268 interpolado, ≤ 120 vértices, antihorario, toda la vía dentro, km 258 fuera, `appsettings.json` y `tramo_demo.geojson` iguales a lo generado |
| E2E entre módulos | [Recorrido compartido, pendiente](../../backend/tests/KillaUru.Integracion.Tests/sistema/e2e/FlujoPuntaAPuntaTests.cs) |

## Cómo ejecutar

Desde `KillaUru-main/backend/`:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Mapa.Tests --no-build
dotnet test --project tests/KillaUru.Arquitectura.Tests --no-build
dotnet test --project tests/KillaUru.Integracion.Tests --no-build -- --filter-class "*ContratoMapaTests"
```

El polígono demo (requiere `pip install shapely`):

```powershell
python -m unittest discover -s db/datos -p "test_*.py"
```

La última línea requiere Docker Desktop encendido (Testcontainers levanta PostgreSQL).
Las pruebas con dobles no certifican PostgreSQL real. El rendimiento local no acredita carga.

## Estado y límites

Contrato HTTP (T029): 5/5 aprobados el 2026-11-21. Comprueba `operationId`, los códigos
(200, 304, 400, 401, 403, 429), el 200 en `application/geo+json`, los esquemas `ColeccionAnomalias`,
`AnomaliaFeature` y `BajaFeature`, y el tipo de cada parámetro de consulta.
Compara el OpenAPI **publicado** con el contrato; no consulta anomalías reales.
Al escribirla se halló que `?severidad=4,5` respondía 400; se corrigió y lo cubre `ParametrosMapaDtoTests` (8 casos).
Siguen pendientes las pruebas HTTP de recorte, ETag, filtros y cambios (T018, T019, T021, T024, T027).
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
<summary>Adaptadores sin host (1 archivo)</summary>

- [ParametrosMapaDtoTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Adapters/ParametrosMapaDtoTests.cs)

</details>

<details>
<summary>Contratos HTTP (1 archivo)</summary>

- [ContratoMapaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/006-modulo-consulta-mapa/contratos/ContratoMapaTests.cs)

</details>

<details>
<summary>Reglas de dominio (3 archivos)</summary>

- [BboxTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Domain/BboxTests.cs)
- [PoliticaAlcanceTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Domain/PoliticaAlcanceTests.cs)
- [ZonaDemoTests.cs](../../backend/tests/KillaUru.Mapa.Tests/Domain/ZonaDemoTests.cs)

</details>
