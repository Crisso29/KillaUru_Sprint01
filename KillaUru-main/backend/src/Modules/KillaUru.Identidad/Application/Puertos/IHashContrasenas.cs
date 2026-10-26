namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Verificación constante para cuentas inexistentes y actualización de hashes antiguos.</summary>
internal interface IHashContrasenas
{
    string Hash(string contrasena);
    bool Verificar(string? hash, string contrasena, out bool requiereRehash);
}
