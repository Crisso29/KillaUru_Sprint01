# Contratos internos del módulo Auditoría (`KillaUru.Auditoria/Contracts/`)

Es lo **único** público del módulo. Lo usan Identidad y Accesos (003), Anomalías (004) y el propio módulo. El host no audita: cada módulo dueño registra las lecturas sensibles de sus datos (D15).

## `IAuditoriaService`

```csharp
public interface IAuditoriaService
{
    /// Registra una acción sensible que ESCRIBE (RF-15) dentro de la transacción del llamador,
    /// unida en IContextoTransaccion; se confirma o se revierte junto con la acción (ADR-013, RNF-31).
    /// Sin transacción unida lanza InvalidOperationException (error de programación, D15).
    /// Si la acción es de riesgo, la métrica se emite con IContextoTransaccion.AlConfirmar (después del commit).
    /// El contexto no puede tener las claves correo, email, contrasena, password ni seudonimo.
    Task RegistrarAsync(
        AccionAuditada accion,
        ActorAuditoria actor,
        string? objetivo,
        IReadOnlyDictionary<string, object?> contexto,
        CancellationToken ct);

    /// Registra una LECTURA sensible (lista y detalle de usuarios, GET /usuarios?ids=, consulta de la
    /// auditoría, exportación) en una transacción propia que confirma él mismo (D15).
    /// Mapa y Dashboard no se registran.
    Task RegistrarLecturaAsync(
        AccionAuditada accion,
        ActorAuditoria actor,
        string? objetivo,
        IReadOnlyDictionary<string, object?> contexto,
        CancellationToken ct);
}
```

## `AccionAuditada`

`enum AccionAuditada : short` con los 14 valores y códigos de `data-model.md` §3. Los códigos **nunca** se renumeran.

## `ActorAuditoria`

```csharp
public sealed record ActorAuditoria(Guid? Id, string? Rol)
{
    public static readonly ActorAuditoria Sistema = new(null, null);   // en la consulta se muestra "Sistema"
}
```

`Rol` usa los nombres de `Shared/Domain/Roles` (`ConductorRegistrado`, `Administrador`).

## Forma de uso esperada (en el módulo que actúa)

**Acción que escribe** (`RegistrarAsync`):

1. Abrir la transacción propia y llamar `IContextoTransaccion.Unir(conexion, transaccion)`.
2. Hacer la acción (por ejemplo, cambiar el rol).
3. Llamar `IAuditoriaService.RegistrarAsync(...)`.
4. Confirmar con `IContextoTransaccion.ConfirmarAsync(ct)` (ejecuta las acciones de `AlConfirmar`, como la métrica de riesgo, y libera). Si el paso 2 o 3 falla, `RevertirAsync(ct)`.
5. Publicar eventos de dominio, si los hay, **después** del commit.

**Lectura sensible** (`RegistrarLecturaAsync`): llamarla una vez por consulta, sin transacción unida. La exportación cuenta las filas primero, audita `DatasetExportado` con `filas` y luego escribe el archivo (PO-12).
