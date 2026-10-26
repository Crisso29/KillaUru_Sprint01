using KillaUru.Anomalias.Application.Puertos;
using KillaUru.Anomalias.Domain;
using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>PostGIS elige el vecino del mismo tipo y conserva el radio incluso con varios candidatos.</summary>
[Collection("Anomalias")]
public sealed class RepositorioEspacialTests(BaseDatosPrueba bd)
{
    [Fact]
    public async Task Vecino_mas_cercano_tipo_y_radio_se_aplican_en_la_base()
    {
        using var api=new FabricaApi(bd);await using var scope=api.Services.CreateAsyncScope();var ct=TestContext.Current.CancellationToken;
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();var repo=scope.ServiceProvider.GetRequiredService<IAnomaliaRepository>();
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var a=Crear(-70,"Bache");var b=Crear(-69.9985,"Bache");var c=Crear(-70,"Hundimiento");
        await repo.GuardarAsync(a,true,ct);await repo.GuardarAsync(b,true,ct);await repo.GuardarAsync(c,true,ct);
        var punto=Coordenada.Crear(-10,-69.9994)!.Value;
        Assert.Equal(a.Id,(await repo.MasCercanaAsync(punto,TipoAnomalia.Bache,100,ct))!.Id);
        Assert.Equal(c.Id,(await repo.MasCercanaAsync(punto,TipoAnomalia.Hundimiento,100,ct))!.Id);
        Assert.Null(await repo.MasCercanaAsync(Coordenada.Crear(-10,-69.99)!.Value,TipoAnomalia.Bache,100,ct));
        Assert.Equal(a.Id,(await repo.ObtenerParaActualizarAsync(a.Id,ct))!.Id);
        await tx.RollbackAsync(ct);
    }
    private static Anomalia Crear(double lon,string tipo)
    {
        var ahora=TimeProvider.System.GetUtcNow();
        var r=ReporteAnomalia.Desde(new(Guid.NewGuid(),ahora,-10,lon,1,tipo,2,.9,"prueba",Guid.NewGuid()),new(false,1,ahora)).Reporte!;
        var a=Anomalia.Crear(r,false,ahora);a.RegistrarReporte(r,[r],new(),ahora);return a;
    }
}
