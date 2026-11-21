using KillaUru.Auditoria;
using KillaUru.Auditoria.Contracts;
using KillaUru.Shared.Metricas;
using KillaUru.Shared.Persistencia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
namespace KillaUru.Integracion.Tests.Auditoria;

/// <summary>Composición real del módulo; solo reemplaza el destino externo de métricas.</summary>
internal sealed class EscenarioAuditoria : IDisposable
{
    private readonly ServiceProvider _servicios;
    private readonly IServiceScope _scope;
    internal IMetricas Metricas { get; } = Substitute.For<IMetricas>();
    internal IAuditoriaService Servicio => _scope.ServiceProvider.GetRequiredService<IAuditoriaService>();
    internal IContextoTransaccion Transaccion => _scope.ServiceProvider.GetRequiredService<IContextoTransaccion>();

    internal EscenarioAuditoria(string cadena)
    {
        var configuracion = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:KillaUru"] = cadena }).Build();
        _servicios = new ServiceCollection().AddSingleton(TimeProvider.System).AddSingleton(Metricas)
            .AddScoped<IContextoTransaccion, ContextoTransaccion>().AddAuditoriaModule(configuracion).BuildServiceProvider();
        _scope = _servicios.CreateScope();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _servicios.Dispose();
    }
}
