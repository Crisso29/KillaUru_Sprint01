using System.Collections.Concurrent;
using KillaUru.Identidad.Application.Puertos;
namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>Buzón solo de pruebas: permite canjear enlaces sin contactar un proveedor real.</summary>
internal sealed class CorreoDePrueba : IEnviadorCorreo
{
    internal ConcurrentDictionary<string, string> Enlaces { get; } = new();
    public Task VerificacionAsync(string correo, string enlace, CancellationToken ct) => Guardar(correo, enlace);
    public Task RecuperacionAsync(string correo, string enlace, CancellationToken ct) => Guardar(correo, enlace);
    public Task InvitacionAsync(string correo, string enlace, CancellationToken ct) => Guardar(correo, enlace);
    internal string Token(string correo) => Enlaces[correo].Split("#token=")[1];
    private Task Guardar(string correo, string enlace) { Enlaces[correo] = enlace; return Task.CompletedTask; }
}
