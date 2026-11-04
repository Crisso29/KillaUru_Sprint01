namespace KillaUru.Dashboard.Domain;

internal sealed class DiaIndicadores(
    DateOnly dia,
    long eventosRecibidos,
    long conductoresActivos,
    long dispositivosDistintos,
    int tramosConReportes7d,
    int tramosTotales,
    long confirmaciones,
    long pendientesExpiradas,
    long expiraciones,
    long renacimientos)
{
    internal DateOnly Dia                 { get; } = dia;
    internal long     EventosRecibidos    { get; } = eventosRecibidos;
    internal long     ConductoresActivos  { get; } = conductoresActivos;
    internal long     DispositivosDistintos { get; } = dispositivosDistintos;
    internal int      TramosConReportes7d { get; } = tramosConReportes7d;
    internal int      TramosTotales       { get; } = tramosTotales;
    internal long     Confirmaciones      { get; } = confirmaciones;
    internal long     PendientesExpiradas { get; } = pendientesExpiradas;
    internal long     Expiraciones        { get; } = expiraciones;
    internal long     Renacimientos       { get; } = renacimientos;

    internal IEnumerable<(Indicador Ind, decimal Val)> ComoFilas()
    {
        yield return (Indicador.EventosRecibidos,      EventosRecibidos);
        yield return (Indicador.ConductoresActivos,    ConductoresActivos);
        yield return (Indicador.DispositivosDistintos, DispositivosDistintos);
        yield return (Indicador.TramosConReportes7d,   TramosConReportes7d);
        yield return (Indicador.TramosTotales,         TramosTotales);
        yield return (Indicador.Confirmaciones,        Confirmaciones);
        yield return (Indicador.PendientesExpiradas,   PendientesExpiradas);
        yield return (Indicador.Expiraciones,          Expiraciones);
        yield return (Indicador.Renacimientos,         Renacimientos);
    }
}
