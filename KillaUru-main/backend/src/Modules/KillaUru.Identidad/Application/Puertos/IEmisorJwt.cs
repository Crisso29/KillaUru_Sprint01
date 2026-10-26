using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Emite claims mínimos con vigencia acotada por el reloj recibido.</summary>
internal interface IEmisorJwt
{
    string Emitir(Usuario usuario, Guid familia, DateTimeOffset ahora, DateTimeOffset vence);
}
