using KillaUru.Anomalias.Contracts;
using KillaUru.Shared.Metricas;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>Clasifica eventos antes de persistir: esquema, duplicados y reglas intrínsecas.</summary>
internal sealed class EvaluacionLote(IAnomaliasService anomalias, IMetricas metricas)
{
    internal List<EventoReportado> Aceptados { get; } = [];
    internal List<Guid> Duplicados { get; } = [];
    internal List<Rechazo> Rechazados { get; } = [];
    private readonly HashSet<Guid> _idsVistos = [];

    internal void Evaluar(LoteEntrante lote, DateTimeOffset recibidoUtc)
    {
        var eventos = lote.Eventos.Select((e, i) => LectorEvento.Leer(e, i)).ToList();
        var seudonimo = SeudonimoDelLote.Resolver(lote.SeudonimoCabecera,
            eventos.Where(x => x.Evento != null).Select(x => x.Evento!));
        for (int i = 0; i < eventos.Count; i++)
            EvaluarEvento(eventos[i].Id, eventos[i].Evento, i, seudonimo, recibidoUtc);
    }

    private void EvaluarEvento(Guid? id, EventoReportado? evento, int indice,
        Guid? seudonimo, DateTimeOffset recibidoUtc)
    {
        if (id is null) { Rechazar(null, indice, MotivoDescarte.EsquemaInvalido); return; }
        if (!_idsVistos.Add(id.Value)) { Duplicados.Add(id.Value); return; }
        var motivo = MotivoRechazo(evento, seudonimo, recibidoUtc);
        if (motivo.HasValue) { Rechazar(id, indice, motivo.Value); return; }
        Aceptados.Add(evento!);
    }

    private MotivoDescarte? MotivoRechazo(EventoReportado? evento, Guid? seudonimo,
        DateTimeOffset recibidoUtc)
    {
        if (seudonimo.HasValue && evento != null && SeudonimoDelLote.EsInconsistente(seudonimo.Value, evento))
            return MotivoDescarte.SeudonimoInconsistente;
        return anomalias.EvaluarIntrinseca(evento!, recibidoUtc);
    }

    private void Rechazar(Guid? id, int indice, MotivoDescarte motivo)
    {
        Rechazados.Add(new Rechazo(id, indice, motivo));
        metricas.RegistrarRechazo(motivo.ToString());
    }
}
