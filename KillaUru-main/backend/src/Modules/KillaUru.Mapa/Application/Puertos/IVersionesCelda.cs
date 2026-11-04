namespace KillaUru.Mapa.Application.Puertos;

/// <summary>Contador de versiones por celda para el cálculo del ETag (ADR-006; diseño-interno §4).</summary>
internal interface IVersionesCelda
{
    /// <summary>Instante en que arrancó el proceso; garantiza que un ETag viejo nunca dé 304.</summary>
    DateTimeOffset InstanteArranque { get; }

    /// <summary>Versión actual de la celda (empieza en 0 y solo sube).</summary>
    long ObtenerVersion(long celda);

    /// <summary>Incrementa la versión de la celda cuando llega un <c>AnomaliaCambiada</c>.</summary>
    void Incrementar(long celda);
}
