using System.Net.Http.Headers;
using KillaUru.Anomalias.Contracts;
using KillaUru.Integracion.Tests.Anomalias;
using KillaUru.Integracion.Tests.Infraestructura;
using KillaUru.Mapa.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Mapa;

/// <summary>
/// La API real con PostgreSQL y un tramo demo de prueba sobre la vía sintética de <see cref="FabricaApi"/> (latitud
/// −13): el polígono real está en Ayacucho y la vía sintética no pasa por él. El demo de prueba cubre las longitudes
/// −75,6025 a −75,4025. Los bordes caen a MITAD de una celda (rejilla de 0,005°): así hay celdas de borde, con parte
/// dentro y parte fuera, y se prueba el filtro punto por punto. Con bordes alineados a la rejilla no las habría.
/// La base es compartida: las pruebas solo afirman sobre SUS anomalías (por id) o sobre propiedades de toda la
/// respuesta (p. ej. «todo lo que ve el anónimo está dentro del demo»).
/// </summary>
internal sealed class EscenarioMapa : IDisposable
{
    internal const double OesteDemo = -75.6025, EsteDemo = -75.4025, SurDemo = -13.01, NorteDemo = -12.99;

    // 0,002° ≈ 216 m: más que el radio de agrupación (100 m), así cada anomalía de prueba queda sola
    private const double Separacion = 0.002;
    private static int secuencia;

    internal EscenarioAnomalias Anomalias { get; }

    internal EscenarioMapa(BaseDatosPrueba bd) =>
        Anomalias = new EscenarioAnomalias(bd, servicios => servicios.AddSingleton(new ZonaDemo(new PoligonoGeo(
            [[OesteDemo, SurDemo], [EsteDemo, SurDemo], [EsteDemo, NorteDemo], [OesteDemo, NorteDemo], [OesteDemo, SurDemo]]))));

    internal static double LongitudDentro() => OesteDemo + Separacion * (1 + Interlocked.Increment(ref secuencia) % 95);

    internal static double LongitudFuera() => EsteDemo + Separacion * (1 + Interlocked.Increment(ref secuencia) % 95);

    /// <summary>
    /// Una anomalía real (procesada por Anomalías) llevada al estado pedido, como lo harían la confirmación, el tiempo o
    /// la moderación. Siempre se fija el estado: una grave (4–5) nace PendienteConfirmacion hasta que la confirman dos
    /// vehículos, uno con sesión (004 FR-011), y la prueba no trata de eso.
    /// </summary>
    internal async Task<Guid> CrearAsync(double longitud, EstadoAnomalia estado = EstadoAnomalia.Activa, int severidad = 2,
        string tipo = "Bache")
    {
        var evento = Anomalias.Evento(severidad) with { Longitud = longitud, Tipo = tipo };
        var id = await Anomalias.ProcesarYObtenerAnomaliaAsync(evento);
        await Anomalias.EjecutarSqlAsync($"UPDATE anomalias.anomalia SET estado={(short)estado} WHERE id={id}");
        return id;
    }

    internal HttpClient Anonimo() => Anomalias.Api.CreateClient();

    internal HttpClient Conductor() => Anomalias.ClienteComo("ConductorRegistrado");

    internal HttpClient Administrador() => Anomalias.ClienteComo("Administrador");

    /// <summary>Token de conductor vencido hace 1 s según el reloj del escenario.</summary>
    internal HttpClient ConductorConTokenVencido()
    {
        var cliente = Anomalias.Api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            Tokens.Emitir("ConductorRegistrado", venceEn: TimeSpan.FromSeconds(-1), reloj: Anomalias.Reloj));
        return cliente;
    }

    public void Dispose() => Anomalias.Dispose();
}
