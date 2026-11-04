using KillaUru.Anomalias.Contracts;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>Resultado de intentar recibir un lote; el controlador mapea cada caso al código HTTP (FR-006).</summary>
internal abstract record ResultadoLote
{
    /// <summary>Lote guardado: cada evento en exactamente una de las tres listas.</summary>
    internal sealed record Aceptado(
        Guid LoteId,
        IReadOnlyList<Guid> Aceptados,
        IReadOnlyList<Guid> Duplicados,
        IReadOnlyList<Rechazo> Rechazados) : ResultadoLote;

    /// <summary>El cuerpo no era válido como lote (vacío, >MaxEventos, mal JSON).</summary>
    internal sealed record Invalido : ResultadoLote;

    /// <summary>La cola estaba llena; la app debe reintentar tras ReintentarEn (DA-05).</summary>
    internal sealed record Saturado(TimeSpan ReintentarEn) : ResultadoLote;
}

/// <summary>Evento que no se guardó; Id es null cuando faltó el eventoId en el cuerpo (D5).</summary>
internal sealed record Rechazo(Guid? Id, int Indice, MotivoDescarte Motivo);
