using KillaUru.Shared.Eventos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace KillaUru.Shared.Tests.Eventos;

public sealed class PublicadorEnMemoriaTests
{
    private sealed record Algo(int Valor) : IEventoDominio;

    private sealed class Anotador(List<string> registro, string nombre) : IManejador<Algo>
    {
        public Task ManejarAsync(Algo evento, CancellationToken ct)
        {
            registro.Add($"{nombre}:{evento.Valor}");
            return Task.CompletedTask;
        }
    }

    private sealed class QueFalla : IManejador<Algo>
    {
        public Task ManejarAsync(Algo evento, CancellationToken ct) => throw new InvalidOperationException("falla a propósito");
    }

    [Fact]
    public async Task Entrega_a_todos_los_manejadores_en_orden_de_registro()
    {
        var registro = new List<string>();
        var publicador = Publicador(s => s
            .AddSingleton<IManejador<Algo>>(new Anotador(registro, "a"))
            .AddSingleton<IManejador<Algo>>(new Anotador(registro, "b")));

        await publicador.PublicarAsync(new Algo(7), TestContext.Current.CancellationToken);

        Assert.Equal(["a:7", "b:7"], registro);
    }

    [Fact]
    public async Task La_falla_de_un_manejador_no_impide_a_los_demas()
    {
        var registro = new List<string>();
        var publicador = Publicador(s => s
            .AddSingleton<IManejador<Algo>, QueFalla>()
            .AddSingleton<IManejador<Algo>>(new Anotador(registro, "b")));

        await publicador.PublicarAsync(new Algo(1), TestContext.Current.CancellationToken);

        Assert.Equal(["b:1"], registro);
    }

    [Fact]
    public async Task Sin_manejadores_no_hace_nada() =>
        await Publicador(_ => { }).PublicarAsync(new Algo(1), TestContext.Current.CancellationToken);

    private static PublicadorEnMemoria Publicador(Action<IServiceCollection> registrar)
    {
        var servicios = new ServiceCollection();
        registrar(servicios);
        var proveedor = servicios.BuildServiceProvider();
        return new PublicadorEnMemoria(
            proveedor.GetRequiredService<IServiceScopeFactory>(), NullLogger<PublicadorEnMemoria>.Instance);
    }
}
