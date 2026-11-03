namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>Genera UUIDs versión 7 basados en el timestamp Unix en milisegundos (D3, FR-025).</summary>
internal static class UUIDv7
{
    /// <summary>Crea un nuevo UUID v7 con el timestamp de ahora.</summary>
    internal static Guid NuevoGuid()
    {
        // .NET 9+ tiene Guid.CreateVersion7(); en .NET 10 ya está disponible.
        return Guid.CreateVersion7();
    }
}
