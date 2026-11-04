using KillaUru.Anomalias.Contracts;

namespace KillaUru.Ingesta.Application.Procesamiento;

/// <summary>Fila Recibido leída del repositorio para procesarla (IPendientesRepositorio).</summary>
/// <param name="Evento">Datos del evento con todos sus campos opcionales.</param>
/// <param name="ConSesion">Si el conductor tenía correo verificado al enviar (RN-14).</param>
/// <param name="RecibidoUtc">Hora de recepción original del lote (antes de redondear).</param>
internal sealed record EventoPendiente(
    EventoReportado Evento,
    bool ConSesion,
    DateTimeOffset RecibidoUtc);
