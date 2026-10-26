namespace KillaUru.Anomalias.Domain;
/// <summary>Tabla explícita de RN-07; acciones desconocidas o estados incompatibles no producen efectos.</summary>
internal static class TablaModeracion
{
    internal static EstadoAnomalia? Destino(EstadoAnomalia estado, AccionAdmin accion, bool porRevisar) => (accion, estado) switch
    {
        (AccionAdmin.MarcarReparada, EstadoAnomalia.Activa or EstadoAnomalia.PendienteConfirmacion) => EstadoAnomalia.Reparada,
        (AccionAdmin.Descartar, EstadoAnomalia.Activa or EstadoAnomalia.PendienteConfirmacion or EstadoAnomalia.Expirada) => EstadoAnomalia.Descartada,
        (AccionAdmin.Reabrir, EstadoAnomalia.Reparada or EstadoAnomalia.Descartada) => EstadoAnomalia.Activa,
        (AccionAdmin.MantenerDescartada, EstadoAnomalia.Descartada) when porRevisar => EstadoAnomalia.Descartada,
        _ => null,
    };
}
