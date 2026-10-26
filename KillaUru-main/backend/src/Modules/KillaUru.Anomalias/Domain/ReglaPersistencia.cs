namespace KillaUru.Anomalias.Domain;

/// <summary>RN-12 cuenta días UTC−5 dentro de la ventana de detección inclusiva.</summary>
internal sealed class ReglaPersistencia(AnomaliasOptions opciones)
{
    internal EstadoPersistencia Evaluar(IEnumerable<DateTimeOffset> detecciones, DateTimeOffset ultima)
    {
        var desde = ultima.AddDays(-opciones.VentanaDias);
        var desfase = TimeSpan.FromHours(opciones.DesfaseHorarioHoras);
        var dias = detecciones.Where(t => t >= desde && t <= ultima)
            .Select(t => DateOnly.FromDateTime(t.ToOffset(desfase).DateTime)).Distinct().Count();
        return dias >= opciones.DiasDistintosMinimos ? EstadoPersistencia.Persistente : EstadoPersistencia.Episodica;
    }
}
