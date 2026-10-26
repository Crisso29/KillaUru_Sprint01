using KillaUru.Anomalias.Application.Procesamiento;
using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
namespace KillaUru.Anomalias.Infrastructure;
/// <summary>Carga una instantánea desde la BD; la API permanece no lista si falta la traza.</summary>
internal sealed class CargadorCorredor(IServiceScopeFactory scopes, EstadoCorredor estado,
    IOptions<AnomaliasOptions> opciones) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var fila = await db.Corredores.AsNoTracking().SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);
        if (fila is null || fila.Traza.NumPoints < 2) return;
        var puntos = fila.Traza.Coordinates.Select(p => Coordenada.Crear(p.Y, p.X)!.Value).ToArray();
        estado.Cargar(new(puntos, opciones.Value.DistanciaMaximaViaMetros));
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
