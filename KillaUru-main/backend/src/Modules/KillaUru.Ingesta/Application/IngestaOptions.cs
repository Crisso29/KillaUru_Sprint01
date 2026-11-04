using System.ComponentModel.DataAnnotations;

namespace KillaUru.Ingesta.Application;

/// <summary>Parámetros calibrables del módulo Ingesta (sección «Ingesta» en appsettings.json).</summary>
internal sealed class IngestaOptions
{
    /// <summary>Máximo de eventos por lote; la app siempre envía ≤ 50 (RF-11).</summary>
    [Range(1, 50)]
    public int MaxEventosPorLote { get; init; } = 50;

    /// <summary>Capacidad máxima de la cola en memoria (DA-05).</summary>
    [Range(1, int.MaxValue)]
    public int CapacidadCola { get; init; } = 2000;

    /// <summary>Número de trabajadores en paralelo; 0 = número de núcleos de la VM (RNF-23).</summary>
    [Range(0, int.MaxValue)]
    public int Trabajadores { get; init; } = 0;

    /// <summary>Segundos que la app debe esperar antes de reintentar cuando el servidor responde 503 (DA-05).</summary>
    [Range(1, int.MaxValue)]
    public int ReintentarEnSegundos { get; init; } = 30;

    /// <summary>Segundos entre pasadas del barredor de pendientes (EQ-09).</summary>
    [Range(1, int.MaxValue)]
    public int IntervaloBarredorSegundos { get; init; } = 60;

    /// <summary>Segundos mínimos de antigüedad para que el barredor encole una fila (EQ-09).</summary>
    [Range(1, int.MaxValue)]
    public int AntiguedadMinimaBarredorSegundos { get; init; } = 60;

    /// <summary>Minutos máximos de pendiente antes de que /health/ready falle (RF-28).</summary>
    [Range(1, int.MaxValue)]
    public int AntiguedadMaximaColaMinutos { get; init; } = 5;

    /// <summary>Horas hasta purgar el seudónimo de la copia de recepción (RN-09).</summary>
    [Range(1, int.MaxValue)]
    public int PurgaSeudonimoHoras { get; init; } = 72;

    /// <summary>Días máximos de retención de filas Procesado/Descartado (RN-10).</summary>
    [Range(1, int.MaxValue)]
    public int RetencionRecepcionDias { get; init; } = 30;

    /// <summary>Tamaño máximo del cuerpo de la petición HTTP en kilobytes (R-002).</summary>
    [Range(1, int.MaxValue)]
    public int TamanoMaximoCuerpoKb { get; init; } = 256;
}
