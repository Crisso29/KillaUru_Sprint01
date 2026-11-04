namespace KillaUru.Mapa.Application;

/// <summary>Resultado de una consulta al mapa (spec 006 §Application).</summary>
internal abstract record ResultadoMapa
{
    private ResultadoMapa() { }

    /// <summary>200 OK con cuerpo GeoJSON; ETag presente solo en el camino por celdas.</summary>
    internal sealed record Contenido(byte[] Cuerpo, string? ETag, bool Publico) : ResultadoMapa;

    /// <summary>304 Not Modified; sin cuerpo.</summary>
    internal sealed record NoModificado(string ETag) : ResultadoMapa;

    /// <summary>400 — parámetro inválido o combinación incompatible.</summary>
    internal sealed record Invalido(string Codigo) : ResultadoMapa;

    /// <summary>403 — parámetro reservado usado por un rol sin permiso (T-SEG20).</summary>
    internal sealed record Prohibido : ResultadoMapa;
}
