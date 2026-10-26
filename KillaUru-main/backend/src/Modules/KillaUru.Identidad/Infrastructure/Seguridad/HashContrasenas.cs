using KillaUru.Identidad.Application.Puertos;
using Microsoft.AspNetCore.Identity;

namespace KillaUru.Identidad.Infrastructure.Seguridad;

/// <summary>PBKDF2 con sal y verificación ficticia de igual coste cuando no existe la cuenta.</summary>
internal sealed class HashContrasenas : IHashContrasenas
{
    private readonly PasswordHasher<object> _hasher = new();
    private readonly object _contexto = new();
    private readonly string _ficticio;

    public HashContrasenas() => _ficticio = _hasher.HashPassword(_contexto, "credencial-ficticia-no-utilizable");
    public string Hash(string contrasena) => _hasher.HashPassword(_contexto, contrasena);

    public bool Verificar(string? hash, string contrasena, out bool requiereRehash)
    {
        var resultado = _hasher.VerifyHashedPassword(_contexto, hash ?? _ficticio, contrasena);
        requiereRehash = hash is not null && resultado == PasswordVerificationResult.SuccessRehashNeeded;
        return hash is not null && resultado != PasswordVerificationResult.Failed;
    }
}
