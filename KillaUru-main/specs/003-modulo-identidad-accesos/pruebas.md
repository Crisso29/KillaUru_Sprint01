# Pruebas 003 — Identidad y accesos

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

Cuentas, roles, sesiones, revocación y privacidad.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Alcance | Ubicación |
|---|---|
| Reglas y casos de uso | [KillaUru.Identidad.Tests](../../backend/tests/KillaUru.Identidad.Tests/) |
| Arquitectura y legibilidad | [Suite compartida](../../backend/tests/KillaUru.Arquitectura.Tests/) |
| Integración por spec | [Carpetas por propósito](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/) |
| Contrato HTTP | [Contratos](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/contratos/) |
| Seguridad | [Seguridad](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/seguridad/) |
| Latencia individual | [Rendimiento](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/rendimiento/) |
| Carga y resiliencia | [Escenarios k6 compartidos](../../backend/tests/carga/README.md) |
| E2E entre módulos | [Recorrido compartido, pendiente](../../backend/tests/KillaUru.Integracion.Tests/sistema/e2e/FlujoPuntaAPuntaTests.cs) |

## Cómo ejecutar

Desde `KillaUru-main/backend/`:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Identidad.Tests --no-build
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

Citar `003/FR-...`, nombrar escenario y resultado, y separar Arrange / Act / Assert.
Asignar un propietario; enlazar desde otras specs cuando el flujo sea compartido.
Registrar comando, fecha, commit, entorno, resultado y omitidos.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.
Las suites de arquitectura, carga y E2E compartidas se detallan en [pruebas de plataforma](../001-plataforma-backend/pruebas.md#inventario-de-archivos-de-prueba).

<details>
<summary>Casos de uso (7 archivos)</summary>

- [ConsultasAdministrativasTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Application/ConsultasAdministrativasTests.cs)
- [GestionUsuariosTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Application/GestionUsuariosTests.cs)
- [InvitacionesTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Application/InvitacionesTests.cs)
- [ProteccionAdministradoresTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Application/ProteccionAdministradoresTests.cs)
- [PurgaIdentidadTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Application/PurgaIdentidadTests.cs)
- [RegistrarConductorTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Application/RegistrarConductorTests.cs)
- [RevisionPrivilegiosTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Application/RevisionPrivilegiosTests.cs)

</details>

<details>
<summary>Reglas de dominio (8 archivos)</summary>

- [ConstanciasTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Domain/ConstanciasTests.cs)
- [CorreoTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Domain/CorreoTests.cs)
- [HorasGuardadasTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Domain/HorasGuardadasTests.cs)
- [PoliticaSesionTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Domain/PoliticaSesionTests.cs)
- [SesionRenovacionTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Domain/SesionRenovacionTests.cs)
- [TokenCorreoTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Domain/TokenCorreoTests.cs)
- [UsuarioBloqueoTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Domain/UsuarioBloqueoTests.cs)
- [UsuarioTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Domain/UsuarioTests.cs)

</details>

<details>
<summary>Adaptadores con dobles o componentes locales (3 archivos)</summary>

- [LimitadorRecuperacionTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Infrastructure/LimitadorRecuperacionTests.cs)
- [SecretosTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Infrastructure/SecretosTests.cs)
- [ServiciosExternosTests.cs](../../backend/tests/KillaUru.Identidad.Tests/Infrastructure/ServiciosExternosTests.cs)

</details>

<details>
<summary>Contratos HTTP (1 archivo)</summary>

- [ContratoIdentidadTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/contratos/ContratoIdentidadTests.cs)

</details>

<details>
<summary>Integración (9 archivos)</summary>

- [ConsultasYPrivacidadTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/integracion/ConsultasYPrivacidadTests.cs)
- [CuentaApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/integracion/CuentaApiTests.cs)
- [DesactivacionRestauracionTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/integracion/DesactivacionRestauracionTests.cs)
- [HorasGuardadasTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/integracion/HorasGuardadasTests.cs)
- [PersistenciaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/integracion/PersistenciaTests.cs)
- [PurgaIdentidadTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/integracion/PurgaIdentidadTests.cs)
- [RespuestasComunesApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/integracion/RespuestasComunesApiTests.cs)
- [SesionApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/integracion/SesionApiTests.cs)
- [UsuariosApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/integracion/UsuariosApiTests.cs)

</details>

<details>
<summary>Latencia individual (1 archivo)</summary>

- [RendimientoIdentidadTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/rendimiento/RendimientoIdentidadTests.cs)

</details>

<details>
<summary>Seguridad (1 archivo)</summary>

- [ConsolaPrivacidadTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/seguridad/ConsolaPrivacidadTests.cs)

</details>
