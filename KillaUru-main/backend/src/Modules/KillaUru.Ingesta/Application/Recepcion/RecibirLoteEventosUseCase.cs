using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;

namespace KillaUru.Ingesta.Application.Recepcion;

/// <summary>
/// Orquesta la recepción de un lote siguiendo el algoritmo de enfoque §6:
/// valida el tamaño, verifica capacidad, genera loteId, evalúa cada evento, inserta y avisa.
/// </summary>
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
            return new ResultadoLote.Invalido();

        if (!cola.HayCapacidad())
            return new ResultadoLote.Saturado(TimeSpan.FromSeconds(opts.ReintentarEnSegundos));

        var loteId = UUIDv7.NuevoGuid();
        var recibidoUtc = reloj.GetUtcNow();

        var aceptados = new List<EventoReportado>();
        var duplicados = new List<Guid>();
        var rechazados = new List<Rechazo>();
        var idsVistos = new HashSet<Guid>();

        // Lee el seudónimo del lote primero para detectar SeudonimoInconsistente
        var eventosLeidos = LeerTodos(lote.Eventos);
        var seudonimoLote = SeudonimoDelLote.Resolver(lote.SeudonimoCabecera,
            eventosLeidos.Where(x => x.Evento != null).Select(x => x.Evento!));

        for (int i = 0; i < eventosLeidos.Count; i++)
        {
            var (id, evento) = eventosLeidos[i];

            // Sin eventoId → EsquemaInvalido (D5, FR-023)
            if (id == null)
            {
                rechazados.Add(new Rechazo(null, i, MotivoDescarte.EsquemaInvalido));
                metricas.RegistrarRechazo(nameof(MotivoDescarte.EsquemaInvalido));
                continue;
            }

            // Repetido dentro del lote (R-003)
            if (!idsVistos.Add(id.Value))
            {
                duplicados.Add(id.Value);
                continue;
            }

            // Seudónimo inconsistente (FR-024)
            if (seudonimoLote.HasValue && evento != null && SeudonimoDelLote.EsInconsistente(seudonimoLote.Value, evento))
            {
                rechazados.Add(new Rechazo(id, i, MotivoDescarte.SeudonimoInconsistente));
                metricas.RegistrarRechazo(nameof(MotivoDescarte.SeudonimoInconsistente));
                continue;
            }

            // Reglas de recepción de Anomalías (RN-11, FR-004)
            var motivo = anomalias.EvaluarIntrinseca(evento!, recibidoUtc);
            if (motivo != null)
            {
                rechazados.Add(new Rechazo(id, i, motivo.Value));
                metricas.RegistrarRechazo(motivo.Value.ToString());
                continue;
            }

            aceptados.Add(evento!);
        }

        // Insertar aceptados (ADR-005)
        var loteAceptado = LoteAceptado.De(loteId, recibidoUtc, lote.ConSesion, aceptados, rechazados);
        var insertados = await recepcion.InsertarAsync(loteAceptado, ct);

        // Separar duplicados de BD de los ya conocidos
        var duplicadosBd = aceptados.Select(e => e.EventoId).Except(insertados).ToList();
        duplicados.AddRange(duplicadosBd);

        metricas.RegistrarLote(insertados.Count, duplicados.Count, rechazados.Count);

        // Avisar a la cola (ADR-005 punto 3)
        cola.TryEscribir(new LoteRecibido(loteId));

        return new ResultadoLote.Aceptado(loteId, insertados.ToList(), duplicados, rechazados);
    }

    private static List<(Guid? Id, EventoReportado? Evento)> LeerTodos(IReadOnlyList<System.Text.Json.JsonElement> elementos)
    {
        var resultado = new List<(Guid?, EventoReportado?)>(elementos.Count);
        for (int i = 0; i < elementos.Count; i++)
            resultado.Add(LectorEvento.Leer(elementos[i], i));
        return resultado;
    }
}
