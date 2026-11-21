# Pruebas 004 — Anomalías

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

Confirmación, moderación, consulta espacial, exportación y retención.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Alcance | Ubicación |
|---|---|
| Reglas y casos de uso | [KillaUru.Anomalias.Tests](../../backend/tests/KillaUru.Anomalias.Tests/) |
| Arquitectura y legibilidad | [Suite compartida](../../backend/tests/KillaUru.Arquitectura.Tests/) |
| Integración por spec | [Carpetas por propósito](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/) |
| Contrato HTTP | [Contratos](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/contratos/) |
| Seguridad | [Seguridad](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/seguridad/) |
| Latencia individual | [Rendimiento](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/rendimiento/) |
| Carga y resiliencia | [Escenarios k6 compartidos](../../backend/tests/carga/README.md) |
| E2E entre módulos | [Recorrido compartido, pendiente](../../backend/tests/KillaUru.Integracion.Tests/sistema/e2e/FlujoPuntaAPuntaTests.cs) |

## Cómo ejecutar

Desde `KillaUru-main/backend/`:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Anomalias.Tests --no-build
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

Citar `004/FR-...`, nombrar escenario y resultado, y separar Arrange / Act / Assert.
Asignar un propietario; enlazar desde otras specs cuando el flujo sea compartido.
Registrar comando, fecha, commit, entorno, resultado y omitidos.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.
Las suites de arquitectura, carga y E2E compartidas se detallan en [pruebas de plataforma](../001-plataforma-backend/pruebas.md#inventario-de-archivos-de-prueba).

<details>
<summary>Casos de uso (4 archivos)</summary>

- [CaducidadTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Application/CaducidadTests.cs)
- [ExportacionTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Application/ExportacionTests.cs)
- [ProcesamientoUseCaseTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Application/ProcesamientoUseCaseTests.cs)
- [TareasProgramadasTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Application/TareasProgramadasTests.cs)

</details>

<details>
<summary>Reglas de dominio (7 archivos)</summary>

- [AnomaliaTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Domain/AnomaliaTests.cs)
- [CalidadTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Domain/CalidadTests.cs)
- [CicloVidaTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Domain/CicloVidaTests.cs)
- [CorredorRealTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Domain/CorredorRealTests.cs)
- [CorredorViaTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Domain/CorredorViaTests.cs)
- [ModeracionTablaTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Domain/ModeracionTablaTests.cs)
- [PoliticasTests.cs](../../backend/tests/KillaUru.Anomalias.Tests/Domain/PoliticasTests.cs)

</details>

<details>
<summary>Contratos HTTP (1 archivo)</summary>

- [ContratoAnomaliasTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/contratos/ContratoAnomaliasTests.cs)

</details>

<details>
<summary>Integración (15 archivos)</summary>

- [AtomicidadAuditoriaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/AtomicidadAuditoriaTests.cs)
- [ConcurrenciaAdicionalTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/ConcurrenciaAdicionalTests.cs)
- [CorredorMigracionTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/CorredorMigracionTests.cs)
- [DisponibilidadCorredorTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/DisponibilidadCorredorTests.cs)
- [EsquemaAnomaliasTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/EsquemaAnomaliasTests.cs)
- [ExportacionApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/ExportacionApiTests.cs)
- [InstantaneaExportacionTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/InstantaneaExportacionTests.cs)
- [IntegridadDatoTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/IntegridadDatoTests.cs)
- [LecturasTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/LecturasTests.cs)
- [ListasAdministracionTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/ListasAdministracionTests.cs)
- [MantenimientoTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/MantenimientoTests.cs)
- [ModeracionApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/ModeracionApiTests.cs)
- [ProcesamientoTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/ProcesamientoTests.cs)
- [RepositorioEspacialTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/RepositorioEspacialTests.cs)
- [RetencionYBloqueosTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/integracion/RetencionYBloqueosTests.cs)

</details>

<details>
<summary>Latencia individual (1 archivo)</summary>

- [RendimientoAnomaliasTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/rendimiento/RendimientoAnomaliasTests.cs)

</details>

<details>
<summary>Seguridad (1 archivo)</summary>

- [SeguridadAnomaliasTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/004-modulo-anomalias/seguridad/SeguridadAnomaliasTests.cs)

</details>
