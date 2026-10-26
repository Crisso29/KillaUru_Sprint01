namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Cuotas atómicas por correo e IP, también para cuentas inexistentes.</summary>
internal interface ILimitadorRecuperacion
{
    bool Admitir(string correo, string ip);
}
