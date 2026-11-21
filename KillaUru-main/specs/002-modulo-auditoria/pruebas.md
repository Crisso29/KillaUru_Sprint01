# Pruebas 002 — Auditoría

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

Registro, consulta, inmutabilidad y transacción compartida.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Alcance | Ubicación |
|---|---|
| Reglas y casos de uso | [KillaUru.Auditoria.Tests](../../backend/tests/KillaUru.Auditoria.Tests/) |
| Arquitectura y legibilidad | [Suite compartida](../../backend/tests/KillaUru.Arquitectura.Tests/) |
| Integración por spec | [Carpetas por propósito](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/) |
| Contrato HTTP | [Contratos](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/contratos/) |
| Seguridad | [Seguridad](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/seguridad/) |
| Latencia individual | [Rendimiento](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/rendimiento/) |
| Carga y resiliencia | [Escenarios k6 compartidos](../../backend/tests/carga/README.md) |
| E2E entre módulos | [Recorrido compartido, pendiente](../../backend/tests/KillaUru.Integracion.Tests/sistema/e2e/FlujoPuntaAPuntaTests.cs) |

## Cómo ejecutar

Desde `KillaUru-main/backend/`:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Auditoria.Tests --no-build
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

Citar `002/FR-...`, nombrar escenario y resultado, y separar Arrange / Act / Assert.
Asignar un propietario; enlazar desde otras specs cuando el flujo sea compartido.
Registrar comando, fecha, commit, entorno, resultado y omitidos.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.
Las suites de arquitectura, carga y E2E compartidas se detallan en [pruebas de plataforma](../001-plataforma-backend/pruebas.md#inventario-de-archivos-de-prueba).

<details>
<summary>Casos de uso (3 archivos)</summary>

- [ConsultarAuditoriaUseCaseTests.cs](../../backend/tests/KillaUru.Auditoria.Tests/Application/ConsultarAuditoriaUseCaseTests.cs)
- [RegistrarAuditoriaUseCaseTests.cs](../../backend/tests/KillaUru.Auditoria.Tests/Application/RegistrarAuditoriaUseCaseTests.cs)
- [RegistrarLecturaUseCaseTests.cs](../../backend/tests/KillaUru.Auditoria.Tests/Application/RegistrarLecturaUseCaseTests.cs)

</details>

<details>
<summary>Reglas de dominio (2 archivos)</summary>

- [CatalogoAccionesTests.cs](../../backend/tests/KillaUru.Auditoria.Tests/Domain/CatalogoAccionesTests.cs)
- [RegistroAuditoriaTests.cs](../../backend/tests/KillaUru.Auditoria.Tests/Domain/RegistroAuditoriaTests.cs)

</details>

<details>
<summary>Contratos HTTP (1 archivo)</summary>

- [ContratoAuditoriaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/contratos/ContratoAuditoriaTests.cs)

</details>

<details>
<summary>Integración (3 archivos)</summary>

- [AuditoriaApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/integracion/AuditoriaApiTests.cs)
- [ConsultaAuditoriaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/integracion/ConsultaAuditoriaTests.cs)
- [TransaccionCompartidaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/integracion/TransaccionCompartidaTests.cs)

</details>

<details>
<summary>Latencia individual (1 archivo)</summary>

- [RendimientoConsultaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/rendimiento/RendimientoConsultaTests.cs)

</details>

<details>
<summary>Seguridad (1 archivo)</summary>

- [InmutabilidadTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/002-modulo-auditoria/seguridad/InmutabilidadTests.cs)

</details>
