namespace KillaUru.Identidad.Application.Puertos;

/// <summary>Operaciones criptográficas sin exponer detalles de infraestructura al caso de uso.</summary>
internal interface IGeneradorSecretos
{
    string Aleatorio32();
    string Derivar(string anterior);
    byte[] Hash(string secreto);
}
