using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Metricas;
namespace KillaUru.Anomalias.Application.Tareas;
/// <summary>La invalidación solo se publica tras confirmar el cambio de estado.</summary>
internal sealed class CaducarAnomaliasUseCase(ICaducidadRepository repo, ITransaccionAnomalias tx,
    IPublicadorEventos eventos, IMetricas metricas, TimeProvider reloj)
{
    internal async Task EjecutarAsync(CancellationToken ct)
    {
        int cantidad;
        do
        {
            cantidad = await tx.EjecutarAsync([], null, null, async () =>
            {
                var cambios = await repo.CaducarAsync(reloj.GetUtcNow(), ct);
                foreach (var cambio in cambios) tx.AlConfirmar(() => eventos.PublicarAsync(cambio, CancellationToken.None));
                return cambios.Count;
            }, ct);
        } while (cantidad == 5000);
        metricas.RegistrarTareaCorrecta("caducidad");
    }
}
