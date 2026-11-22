using KillaUru.Anomalias.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace KillaUru.Mapa.Adapters.Dtos;

/// <summary>Parámetros de la petición GET /api/v1/anomalies (data-model §3).</summary>
internal sealed class ParametrosMapaDto
{
    [FromQuery(Name = "bbox")]
    public string? Bbox { get; init; }

    [FromQuery(Name = "tipo")]
    public TipoAnomalia? Tipo { get; init; }

    /// <summary>Severidades separadas por comas, p. ej. `4,5` (contrato `mapa.openapi.yaml`).</summary>
    [FromQuery(Name = "severidad")]
    public string? Severidad { get; init; }

    [FromQuery(Name = "desde")]
    public DateTimeOffset? Desde { get; init; }

    [FromQuery(Name = "hasta")]
    public DateTimeOffset? Hasta { get; init; }

    /// <summary>Estados separados por comas, p. ej. `Activa,PendienteConfirmacion`; solo administrador.</summary>
    [FromQuery(Name = "estado")]
    public string? Estado { get; init; }

    [FromQuery(Name = "updatedSince")]
    public DateTimeOffset? UpdatedSince { get; init; }

    /// <summary>Convierte las listas de texto; null si alguna está mal formada (→ 400 `datos_invalidos`).</summary>
    internal ListasMapa? LeerListas()
    {
        int[]? severidades = null;
        EstadoAnomalia[]? estados = null;
        if (Severidad is not null && !TryLeer(Severidad, TryLeerSeveridad, out severidades)) return null;
        if (Estado is not null && !TryLeer(Estado, TryLeerEstado, out estados)) return null;
        return new ListasMapa(severidades, estados);
    }

    private delegate bool Lector<T>(string texto, out T valor);

    private static bool TryLeer<T>(string lista, Lector<T> leer, out T[]? valores)
    {
        var partes = lista.Split(',');
        valores = new T[partes.Length];
        for (var i = 0; i < partes.Length; i++)
        {
            if (!leer(partes[i].Trim(), out valores[i])) return false;
        }
        return true;
    }

    private static bool TryLeerSeveridad(string texto, out int severidad) => int.TryParse(texto, out severidad);

    // Solo nombres del catálogo: Enum.TryParse aceptaría también "1" o "99".
    private static bool TryLeerEstado(string texto, out EstadoAnomalia estado)
    {
        estado = default;
        return Enum.GetNames<EstadoAnomalia>().Contains(texto, StringComparer.OrdinalIgnoreCase)
            && Enum.TryParse(texto, ignoreCase: true, out estado);
    }
}
