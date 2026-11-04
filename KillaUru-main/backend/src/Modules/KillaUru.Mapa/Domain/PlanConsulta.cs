using KillaUru.Anomalias.Contracts;

namespace KillaUru.Mapa.Domain;

/// <summary>Ruta de consulta elegida por <see cref="PoliticaAlcance"/> (data-model §1).</summary>
internal abstract record PlanConsulta
{
    private PlanConsulta() { }

    /// <summary>Consulta con caché por celdas y ETag (anónimo o conductor sin filtros).</summary>
    internal sealed record PorCeldas(Alcance Alcance) : PlanConsulta;

    /// <summary>Consulta directa a Anomalías con filtros opcionales; sin caché ni ETag.</summary>
    internal sealed record Filtrada(EstadoAnomalia[] Estados, bool RecortarDemo) : PlanConsulta;

    /// <summary>Cambios desde un instante; sin caché ni ETag.</summary>
    internal sealed record Cambios(bool RecortarDemo) : PlanConsulta;

    /// <summary>Rol sin permiso para usar el parámetro solicitado → 403 (T-SEG20).</summary>
    internal sealed record NoPermitido : PlanConsulta;
}
