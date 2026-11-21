using KillaUru.Identidad.Application.Puertos;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>Doble explícito; nunca se registra en la aplicación real.</summary>
internal sealed class AntiBotDePrueba : IVerificadorAntiBot
{
    public Task<bool> VerificarAsync(string? token, CancellationToken ct) => Task.FromResult(token == "antibot-prueba");
}
