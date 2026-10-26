using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;

namespace KillaUru.Identidad.Infrastructure.Repositorios;

/// <summary>Consultas acotadas y agregados sin revelar identidades a Dashboard.</summary>
internal sealed class UsuarioRepositorio(IdentidadDbContext db) : IUsuarioRepositorio
{
    public async Task<Usuario?> PorCorreoAsync(string correo, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            return await db.Usuarios.SingleOrDefaultAsync(x => x.Correo == new Domain.Correo(correo), ct);
        // También protege el correo aún inexistente frente a registros e invitaciones simultáneos.
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"correo:" + correo}, 0))", ct);
        return await db.Usuarios.FromSqlInterpolated($"SELECT * FROM identidad.usuario WHERE correo = {correo} FOR UPDATE").SingleOrDefaultAsync(ct);
    }

    /// <summary>Dentro de una transacción bloquea la fila (FOR UPDATE) para serializar cambios sobre la cuenta.</summary>
    public Task<Usuario?> PorIdAsync(Guid id, CancellationToken ct) => db.Database.CurrentTransaction is null
        ? db.Usuarios.SingleOrDefaultAsync(x => x.Id == id, ct)
        : db.Usuarios.FromSqlInterpolated($"SELECT * FROM identidad.usuario WHERE id = {id} FOR UPDATE").SingleOrDefaultAsync(ct);
    public void Agregar(Usuario usuario) => db.Usuarios.Add(usuario);
    public void Eliminar(Usuario usuario) => db.Usuarios.Remove(usuario);

    public async Task AceptarAsync(Consentimiento consentimiento, CancellationToken ct)
    {
        var existe = await db.Consentimientos.AnyAsync(x => x.UsuarioId == consentimiento.UsuarioId
            && x.Documento == consentimiento.Documento && x.Version == consentimiento.Version, ct);
        if (!existe) db.Consentimientos.Add(consentimiento);
    }

    public async Task<IReadOnlyList<Consentimiento>> AceptacionesAsync(Guid usuario, CancellationToken ct) =>
        await db.Consentimientos.AsNoTracking().Where(x => x.UsuarioId == usuario).ToArrayAsync(ct);
    public async Task<IReadOnlyList<Usuario>> AdministradoresAsync(CancellationToken ct) =>
        await db.Usuarios.Where(x => x.Rol == RolUsuario.Administrador && x.Activo).OrderBy(x => x.Id).ToArrayAsync(ct);

    public async Task<PaginaUsuarios> ListarAsync(FiltrosUsuarios filtros, CancellationToken ct)
    {
        var consulta = Filtrar(filtros);
        var total = await consulta.LongCountAsync(ct);
        var filas = await consulta.OrderBy(x => x.Id).Skip((filtros.Pagina - 1) * filtros.Tamano)
            .Take(filtros.Tamano).ToArrayAsync(ct);
        var ultimaActividad = await UltimaActividadAsync(filas.Select(x => x.Id).ToArray(), ct);
        var vistas = filas.Select(x => new UsuarioVista(x.Id, x.Correo.Valor, x.Rol.ToString(), x.Estado,
            x.CorreoVerificado, x.CreadoUtc, ultimaActividad.TryGetValue(x.Id, out var ultima) ? ultima : null)).ToArray();
        return new(vistas, filtros.Pagina, filtros.Tamano, total);
    }

    /// <summary>Una sola consulta agrupada para la página, en vez de una por usuario (evita N+1).</summary>
    private Task<Dictionary<Guid, DateTimeOffset>> UltimaActividadAsync(Guid[] ids, CancellationToken ct) =>
        db.Sesiones.Where(x => ids.Contains(x.UsuarioId)).GroupBy(x => x.UsuarioId)
            .Select(g => new { Id = g.Key, Ultima = g.Max(x => x.CreadoUtc) })
            .ToDictionaryAsync(x => x.Id, x => x.Ultima, ct);

    private IQueryable<Usuario> Filtrar(FiltrosUsuarios filtros)
    {
        // Correo se mapea con un conversor de valor y LINQ no puede aplicarle ILIKE: va en SQL
        // parametrizado (sin inyección) y con los comodines % y _ escapados.
        var consulta = filtros.Busqueda is { Length: > 0 } busqueda
            ? db.Usuarios.FromSqlInterpolated($"SELECT * FROM identidad.usuario WHERE correo ILIKE {"%" + Escapar(busqueda) + "%"}").AsNoTracking()
            : db.Usuarios.AsNoTracking();
        if (filtros.Rol is not null)
        {
            var rol = Enum.Parse<RolUsuario>(filtros.Rol);
            consulta = consulta.Where(x => x.Rol == rol);
        }
        if (filtros.Ids is not null) consulta = consulta.Where(x => filtros.Ids.Contains(x.Id));
        return PorEstado(consulta, filtros.Estado);
    }

    /// <summary>Misma definición de estado que <see cref="Usuario.Estado"/>, traducida a SQL.</summary>
    private static IQueryable<Usuario> PorEstado(IQueryable<Usuario> consulta, string? estado) => estado switch
    {
        "InvitacionPendiente" => consulta.Where(x => x.HashContrasena == null),
        "Activa" => consulta.Where(x => x.Activo && x.HashContrasena != null),
        "Desactivada" => consulta.Where(x => !x.Activo && x.HashContrasena != null),
        _ => consulta,
    };

    /// <summary>Solo el número: Dashboard nunca recibe quiénes son (FR-036).</summary>
    public Task<int> ContarConductoresActivosAsync(DateTimeOffset desde, CancellationToken ct) =>
        db.Usuarios.Where(x => x.Rol == RolUsuario.ConductorRegistrado)
            .Where(x => db.Sesiones.Any(s => s.UsuarioId == x.Id && s.CreadoUtc >= desde))
            .CountAsync(ct);

    private static string Escapar(string valor) => valor.Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal);
}
