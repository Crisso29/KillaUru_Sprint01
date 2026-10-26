using KillaUru.Identidad.Application.Puertos;
namespace KillaUru.Identidad.Infrastructure.Correo;

/// <summary>Adaptador local sin envío: no imprime enlaces ni destinatarios en los logs de Docker.</summary>
internal sealed class EnviadorCorreoConsola : IEnviadorCorreo
{
    public Task VerificacionAsync(string correo, string enlace, CancellationToken ct) => AvisarAsync(ct);
    public Task RecuperacionAsync(string correo, string enlace, CancellationToken ct) => AvisarAsync(ct);
    public Task InvitacionAsync(string correo, string enlace, CancellationToken ct) => AvisarAsync(ct);
    private static Task AvisarAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Console.WriteLine("Correo simulado de Identidad. Contenido y destinatario omitidos por privacidad.");
        return Task.CompletedTask;
    }
}
