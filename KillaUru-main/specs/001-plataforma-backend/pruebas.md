# Pruebas 001 — Plataforma backend

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

Infraestructura HTTP, salud, migraciones, cuotas y privacidad de logs.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Alcance | Ubicación |
|---|---|
| Reglas y casos de uso | [KillaUru.Shared.Tests](../../backend/tests/KillaUru.Shared.Tests/) |
| Arquitectura y legibilidad | [Suite compartida](../../backend/tests/KillaUru.Arquitectura.Tests/) |
| Integración por spec | [Carpetas por propósito](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/) |
| Contrato HTTP | [Contratos](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/contratos/) |
| Seguridad | [Seguridad](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/seguridad/) |
| Carga y resiliencia | [Escenarios k6 compartidos](../../backend/tests/carga/README.md) |
| E2E entre módulos | [Recorrido compartido, pendiente](../../backend/tests/KillaUru.Integracion.Tests/sistema/e2e/FlujoPuntaAPuntaTests.cs) |

## Cómo ejecutar

Desde `KillaUru-main/backend/`:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Shared.Tests --no-build
dotnet test --project tests/KillaUru.Arquitectura.Tests --no-build
dotnet test --project tests/KillaUru.Integracion.Tests --no-build
python -m unittest discover -s deploy/ci -p 'test_*.py'
```

La suite de integración requiere Docker y ejecuta también las otras specs.
Las pruebas con dobles no certifican PostgreSQL real. El rendimiento local no acredita carga.

## Estado y límites

Hay casos implementados; su existencia no certifica una ejecución aprobada.
El E2E compartido conserva dos casos omitidos con pasos sin implementar.
La ejecución de k6 y las validaciones operacionales requieren evidencia del entorno de pruebas.
Consulta [la revisión actual](../../docs/seguimiento/estado-implementacion.md) antes de cerrar tareas.

## Al añadir un caso

Citar `001/FR-...`, nombrar escenario y resultado, y separar Arrange / Act / Assert.
Asignar un propietario; enlazar desde otras specs cuando el flujo sea compartido.
Registrar comando, fecha, commit, entorno, resultado y omitidos.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.

<details>
<summary>Herramientas de CI — comprobación del cálculo de cobertura (1 archivo)</summary>

- [test_cobertura_dominio.py](../../backend/deploy/ci/test_cobertura_dominio.py)

</details>

<details>
<summary>Reglas de dominio (3 archivos)</summary>

- [CeldaTests.cs](../../backend/tests/KillaUru.Shared.Tests/Domain/CeldaTests.cs)
- [SeveridadTests.cs](../../backend/tests/KillaUru.Shared.Tests/Domain/SeveridadTests.cs)
- [ValoresSimplesTests.cs](../../backend/tests/KillaUru.Shared.Tests/Domain/ValoresSimplesTests.cs)

</details>

<details>
<summary>Componentes compartidos / Eventos (1 archivo)</summary>

- [PublicadorEnMemoriaTests.cs](../../backend/tests/KillaUru.Shared.Tests/Eventos/PublicadorEnMemoriaTests.cs)

</details>

<details>
<summary>Componentes compartidos / Persistencia (1 archivo)</summary>

- [ContextoTransaccionTests.cs](../../backend/tests/KillaUru.Shared.Tests/Persistencia/ContextoTransaccionTests.cs)

</details>

<details>
<summary>Componentes compartidos / Tareas (1 archivo)</summary>

- [ProgramaRelojTests.cs](../../backend/tests/KillaUru.Shared.Tests/Tareas/ProgramaRelojTests.cs)

</details>

<details>
<summary>Contratos HTTP (1 archivo)</summary>

- [ContratoOpenApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/contratos/ContratoOpenApiTests.cs)

</details>

<details>
<summary>Integración (5 archivos)</summary>

- [DocumentacionTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/integracion/DocumentacionTests.cs)
- [MetricasOpenTelemetryTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/integracion/MetricasOpenTelemetryTests.cs)
- [MigracionesTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/integracion/MigracionesTests.cs)
- [SaludTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/integracion/SaludTests.cs)
- [SinFkEntreEsquemasTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/integracion/SinFkEntreEsquemasTests.cs)

</details>

<details>
<summary>Seguridad (6 archivos)</summary>

- [ErroresYCabecerasTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/seguridad/ErroresYCabecerasTests.cs)
- [FiltroDatosPersonalesTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/seguridad/FiltroDatosPersonalesTests.cs)
- [LimiteTasaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/seguridad/LimiteTasaTests.cs)
- [PendientesPlataformaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/seguridad/PendientesPlataformaTests.cs)
- [SeguridadTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/seguridad/SeguridadTests.cs)
- [TechoPorIpTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/001-plataforma-backend/seguridad/TechoPorIpTests.cs)

</details>

<details>
<summary>Arquitectura compartida (10 archivos)</summary>

- [ApisBanidasTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/ApisBanidasTests.cs)
- [ChequeoDeSaludPostgresTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/ChequeoDeSaludPostgresTests.cs) — todo compose chequea PostgreSQL por TCP (DT-03, T083)
- [LegibilidadTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/LegibilidadTests.cs)
- [LimiteDeLineasTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/LimiteDeLineasTests.cs)
- [PoolDeConexionesTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/PoolDeConexionesTests.cs) — tope de 30 conexiones en cada cadena (§3.7)
- [ReglasAnomaliasTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/ReglasAnomaliasTests.cs)
- [ReglasDeCapasTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/ReglasDeCapasTests.cs)
- [ReglasDeDominioTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/ReglasDeDominioTests.cs)
- [ReglasDeModulosTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/ReglasDeModulosTests.cs)
- [ReglasIdentidadTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/ReglasIdentidadTests.cs)

</details>

<details>
<summary>Carga compartida — requiere entorno de pruebas (6 archivos)</summary>

- [caos.k6.js](../../backend/tests/carga/caos.k6.js)
- [carga-combinada.k6.js](../../backend/tests/carga/carga-combinada.k6.js)
- [estres.k6.js](../../backend/tests/carga/estres.k6.js)
- [identidad.k6.js](../../backend/tests/carga/identidad.k6.js)
- [picos.k6.js](../../backend/tests/carga/picos.k6.js)
- [resistencia.k6.js](../../backend/tests/carga/resistencia.k6.js)

</details>

<details>
<summary>E2E entre módulos — omitido, pendiente (1 archivo)</summary>

- [FlujoPuntaAPuntaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/sistema/e2e/FlujoPuntaAPuntaTests.cs)

</details>
