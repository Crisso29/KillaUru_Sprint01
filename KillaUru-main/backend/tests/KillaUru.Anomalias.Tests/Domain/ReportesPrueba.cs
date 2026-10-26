using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
namespace KillaUru.Anomalias.Tests.Domain;
/// <summary>Datos sintéticos reproducibles; nunca representan una cuenta o trayecto real.</summary>
internal static class ReportesPrueba
{
    internal static readonly DateTimeOffset Ahora = new(2026, 10, 24, 15, 0, 0, TimeSpan.Zero);
    internal static Coordenada Punto(double lat = 0, double lon = 0) => Coordenada.Crear(lat, lon)!.Value;
    internal static ReporteAnomalia Crear(int severidad = 2, int segundos = 0, bool sesion = false) => new(
        Guid.NewGuid(), TipoAnomalia.Bache, Ahora.AddSeconds(segundos), Ahora, Punto(), 0,
        Severidad.Crear(severidad)!.Value, Confidence.Crear(0.9)!.Value, "modelo-prueba", Guid.NewGuid(), sesion);
    internal static ReporteEntrada Entrada() => new(Guid.NewGuid(), Ahora, 0, 0, 0, "Bache", 2, 0.9, "prueba", Guid.NewGuid());
    internal static CorredorVia Corredor() => new([Punto(0, -0.01), Punto(0, 0.01)], 50);
}
