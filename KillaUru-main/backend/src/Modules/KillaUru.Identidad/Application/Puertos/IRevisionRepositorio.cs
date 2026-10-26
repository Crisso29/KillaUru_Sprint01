using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Constancias de revisión, independientes del historial de sesiones.</summary>
internal interface IRevisionRepositorio
{
    Task<DateTimeOffset?> UltimaAsync(CancellationToken ct);
    void Agregar(RevisionPrivilegios revision);
}
