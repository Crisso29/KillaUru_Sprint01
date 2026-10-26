using KillaUru.Anomalias.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Privacidad y restricciones se comprueban en el esquema migrado, no solo en el modelo EF.</summary>
[Collection("Anomalias")]
public sealed class EsquemaAnomaliasTests(BaseDatosPrueba bd)
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public async Task Check_rechaza_datos_invalidos(int caso)
    {
        using var e=new EscenarioAnomalias(bd); var evento=e.Evento(); await e.ProcesarAsync(evento);
        await using var scope=e.Api.Services.CreateAsyncScope(); var ct=TestContext.Current.CancellationToken;
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>();
        var id=(await db.Reportes.SingleAsync(r=>r.EventoId==evento.EventoId,ct)).AnomaliaId;
        FormattableString sql=caso switch
        {
            0=>$"UPDATE anomalias.anomalia SET persistencia=2 WHERE id={id}",
            1=>$"UPDATE anomalias.reporte_anomalia SET confidence=1.01 WHERE evento_id={evento.EventoId}",
            2=>$"UPDATE anomalias.reporte_anomalia SET precision_m=-1 WHERE evento_id={evento.EventoId}",
            3=>$"UPDATE anomalias.transicion SET desde=hacia WHERE anomalia_id={id}",
            _=>$"UPDATE anomalias.transicion SET hacia=6 WHERE anomalia_id={id}",
        };
        var error=await Assert.ThrowsAsync<PostgresException>(()=>db.Database.ExecuteSqlInterpolatedAsync(sql,ct));
        Assert.Equal(PostgresErrorCodes.CheckViolation,error.SqlState);
    }
    [Fact]
    public async Task No_hay_columnas_de_identidad_ni_enlaces_a_cuentas()
    {
        using var api=new FabricaApi(bd); await using var scope=api.Services.CreateAsyncScope();
        var db=scope.ServiceProvider.GetRequiredService<AnomaliasDbContext>(); var ct=TestContext.Current.CancellationToken;
        var columnas=await db.Database.SqlQuery<string>($"SELECT column_name AS \"Value\" FROM information_schema.columns WHERE table_schema='anomalias'").ToListAsync(ct);
        Assert.DoesNotContain(columnas,c=>c.Contains("correo",StringComparison.OrdinalIgnoreCase)||c.Contains("cuenta",StringComparison.OrdinalIgnoreCase)||c.Contains("token",StringComparison.OrdinalIgnoreCase));
        var enlaces=await db.Database.SqlQuery<string>($"SELECT ccu.table_schema AS \"Value\" FROM information_schema.constraint_column_usage ccu JOIN information_schema.table_constraints tc ON tc.constraint_name=ccu.constraint_name AND tc.constraint_schema=ccu.constraint_schema WHERE tc.table_schema='anomalias' AND tc.constraint_type='FOREIGN KEY'").ToListAsync(ct);
        Assert.All(enlaces,e=>Assert.Equal("anomalias",e));
    }
}
