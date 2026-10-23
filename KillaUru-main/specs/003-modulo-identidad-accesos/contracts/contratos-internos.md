# Contratos internos del módulo Identidad y Accesos

## Lo que el módulo **ofrece** (`KillaUru.Identidad/Contracts/`)

```csharp
public interface IIdentidadConsultas
{
    /// Número de cuentas ConductorRegistrado que iniciaron o renovaron su sesión en los últimos `dias` días
    /// (RN-17). Devuelve solo un número: nunca ids ni correos.
    Task<int> ContarConductoresActivosAsync(int dias, CancellationToken ct);
}
```

Consumidor: Dashboard (007). Es la única dependencia entrante de otro módulo.

## Lo que el módulo **usa**

| Contrato | De | Para qué |
|---|---|---|
| `IAuditoriaService.RegistrarAsync` | Auditoría (002) | Acciones 1–11 del catálogo, **dentro** de la transacción del caso de uso (`ITransaccionIdentidad`); sin transacción unida, falla (D15) |
| `IAuditoriaService.RegistrarLecturaAsync` | Auditoría (002) | Acción 14 `AccesoAdministrativoDatos`: lista de usuarios, `GET /usuarios?ids=` y estado de la revisión de privilegios, con transacción propia (D15) |
| `IContextoTransaccion` | Shared (001) | Unir su transacción para que la auditoría entre en ella (ADR-013) |
| `IMetricas.RegistrarTareaCorrecta("purga-identidad")` | Shared (001) | Tarea diaria de purga (D3, T-OPS05; alerta a las 26 h, RNF-22) |
| `ProgramaReloj` | Shared (001) | Próximo instante de la tarea: al arrancar y a las 03:45 hora local (T-OPS05) |
| `Roles` | Shared (001) | Nombres de rol en el JWT y en `[Authorize]` |
| Política de límite de tasa del registro (`LimiteTasa:RegistrosPorHoraPorIp = 5`, `ApiOptions`) | Host (001) | Tope de registros por IP (D8) |

**No usa** `IMetricas.RegistrarAccionRiesgo`: la métrica de acciones de riesgo la emite solo Auditoría, después del commit (D15, OPS-16); una prueba lo verifica.

## Lo que el módulo **no** ofrece a propósito

- Ningún contrato devuelve correos ni ids de cuentas a otros módulos (RNF-10). Los correos de actores para la auditoría se piden por HTTP desde el panel (`GET /api/v1/usuarios?ids=`), con rol administrador y auditado.
- La validación de sesión **no** es un contrato: es el middleware del host (001, ADR-009 punto 2), con `ClockSkew = TimeSpan.Zero` (T-SEG09).
- El *claim* `verificado` del token de acceso (D8) tampoco es un contrato de código: Ingesta (005) lo lee del `ClaimsPrincipal` para `con_sesion` (RN-14).
