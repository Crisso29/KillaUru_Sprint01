using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Application.Resultados;

namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Persistencia específica de cuentas y sus aceptaciones, sin repositorio genérico.</summary>
internal interface IUsuarioRepositorio
{
    Task<Usuario?> PorCorreoAsync(string correo, CancellationToken ct);
    Task<Usuario?> PorIdAsync(Guid id, CancellationToken ct);
    void Agregar(Usuario usuario);
    void Eliminar(Usuario usuario);
    Task AceptarAsync(Consentimiento consentimiento, CancellationToken ct);
    Task<IReadOnlyList<Consentimiento>> AceptacionesAsync(Guid usuario, CancellationToken ct);
    Task<IReadOnlyList<Usuario>> AdministradoresAsync(CancellationToken ct);
    Task<PaginaUsuarios> ListarAsync(FiltrosUsuarios filtros, CancellationToken ct);
    Task<int> ContarConductoresActivosAsync(DateTimeOffset desde, CancellationToken ct);
}
