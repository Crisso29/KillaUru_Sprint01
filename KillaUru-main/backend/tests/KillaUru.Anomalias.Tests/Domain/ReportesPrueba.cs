using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;

namespace KillaUru.Anomalias.Tests.Domain;

/// <summary>
/// Datos sintéticos reproducibles; nunca representan una cuenta o trayecto real. Cada reporte usa un
/// seudónimo nuevo (otro vehículo) salvo que la prueba lo fije.
/// </summary>
internal static class ReportesPrueba
{
    internal static readonly DateTimeOffset Ahora = new(2026, 10, 24, 15, 0, 0, TimeSpan.Zero);

    /// <summary>Radio medio de la Tierra (IUGG), el mismo que usa <see cref="GeodesiaLocal"/>.</summary>
    internal const double RadioTerrestreMetros = 6371008.8;

    internal static Coordenada Punto(double lat = 0, double lon = 0) => Coordenada.Crear(lat, lon)!.Value;

    /// <summary>Grados de arco que equivalen a <paramref name="metros"/> sobre un meridiano.</summary>
    internal static double GradosDe(double metros) => metros / RadioTerrestreMetros * 180 / Math.PI;

    internal static ReporteAnomalia Crear(int severidad = 2, int segundos = 0, bool sesion = false) => new(
        Guid.NewGuid(), TipoAnomalia.Bache, Ahora.AddSeconds(segundos), Ahora, Punto(), 0,
        Severidad.Crear(severidad)!.Value, Confidence.Crear(0.9)!.Value, "modelo-prueba", Guid.NewGuid(), sesion);

    internal static ReporteEntrada Entrada() => new(Guid.NewGuid(), Ahora, 0, 0, 0, "Bache", 2, 0.9, "prueba", Guid.NewGuid());

    internal static ContextoDeteccion Contexto(bool conSesion = false) => new(conSesion, 1, Ahora);

    /// <summary>Recta de unos 2,2 km sobre el ecuador con franja de 50 m.</summary>
    internal static CorredorVia Corredor() => new([Punto(0, -0.01), Punto(0, 0.01)], 50);

    /// <summary>Anomalía recién nacida de <paramref name="primero"/>, como la deja el procesamiento.</summary>
    internal static Anomalia Nacida(ReporteAnomalia primero, AnomaliasOptions? opciones = null, bool porTope = false)
    {
        var anomalia = Anomalia.Crear(primero, porTope, Ahora);
        anomalia.RegistrarReporte(primero, [primero], opciones ?? new(), Ahora);
        return anomalia;
    }

    /// <summary>Tres vehículos distintos, separados 3 min, reportan después de <paramref name="inicial"/>.</summary>
    internal static List<ReporteAnomalia> TresVehiculosMas(Anomalia anomalia, ReporteAnomalia inicial)
    {
        var ventana = new List<ReporteAnomalia> { inicial };
        for (var vehiculo = 1; vehiculo <= 3; vehiculo++)
        {
            var reporte = Crear(segundos: vehiculo * 180);
            ventana.Add(reporte);
            anomalia.RegistrarReporte(reporte, ventana, new(), reporte.DetectadoUtc);
        }
        return ventana;
    }
}
