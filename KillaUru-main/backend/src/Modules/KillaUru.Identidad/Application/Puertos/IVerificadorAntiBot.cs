namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Comprueba el desafío antes de verificar contraseñas del panel.</summary>
internal interface IVerificadorAntiBot
{
    Task<bool> VerificarAsync(string? token, CancellationToken ct);
}
