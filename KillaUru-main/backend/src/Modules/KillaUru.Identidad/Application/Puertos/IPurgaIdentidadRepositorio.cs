namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Retención por lotes, separada de la persistencia interactiva.</summary>
internal interface IPurgaIdentidadRepositorio
{
    Task PurgarAsync(DateTimeOffset ahora, int dias, int minutos, CancellationToken ct);
}
