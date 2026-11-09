using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>Valida, persiste el lote y avisa a la cola solo después del commit (ADR-005).</summary>
internal sealed class RecibirLoteEventosUseCase(
    IAnomaliasService anomalias,
    IRecepcionEventos recepcion,
    IColaLotes cola,
    IMetricas metricas,
    IOptions<IngestaOptions> opciones,
    TimeProvider reloj)
{
    internal async Task<ResultadoLote> ProcesarAsync(LoteEntrante lote, CancellationToken ct)
    {
        var opts = opciones.Value;
        if (lote.Eventos.Count == 0 || lote.Eventos.Count > opts.MaxEventosPorLote)
            return new ResultadoLoteInvalido();
        if (!cola.HayCapacidad())
            return new ResultadoLoteSaturado(TimeSpan.FromSeconds(opts.ReintentarEnSegundos));
        var loteId = UUIDv7.NuevoGuid();
        var recibidoUtc = reloj.GetUtcNow();
        var evaluacion = new EvaluacionLote(anomalias, metricas);
        evaluacion.Evaluar(lote, recibidoUtc);
        var aceptado = LoteAceptado.De(loteId, recibidoUtc, lote.ConSesion,
            evaluacion.Aceptados, evaluacion.Rechazados);
        var insertados = await recepcion.InsertarAsync(aceptado, ct);
        return Confirmar(loteId, evaluacion, insertados);
    }

    private ResultadoLoteAceptado Confirmar(Guid loteId, EvaluacionLote evaluacion,
        IReadOnlySet<Guid> insertados)
    {
        evaluacion.Duplicados.AddRange(evaluacion.Aceptados.Select(e => e.EventoId).Except(insertados));
        metricas.RegistrarLote(insertados.Count, evaluacion.Duplicados.Count, evaluacion.Rechazados.Count);
        cola.TryEscribir(new LoteRecibido(loteId));
        return new ResultadoLoteAceptado(loteId, insertados.ToList(), evaluacion.Duplicados, evaluacion.Rechazados);
    }
}
