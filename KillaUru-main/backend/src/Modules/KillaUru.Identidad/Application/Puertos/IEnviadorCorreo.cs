namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Única salida de correo del monolito; ninguna operación registra destinatarios o tokens.</summary>
internal interface IEnviadorCorreo
{
    Task VerificacionAsync(string correo, string enlace, CancellationToken ct);
    Task RecuperacionAsync(string correo, string enlace, CancellationToken ct);
    Task InvitacionAsync(string correo, string enlace, CancellationToken ct);
}
