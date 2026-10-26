namespace KillaUru.Anomalias.Domain;

/// <summary>Códigos estables de RN-11; el último corresponde exclusivamente a la recepción de lotes.</summary>
internal enum MotivoDescarte : short
{
    /// <summary>Falta un campo obligatorio o un valor no es finito o está fuera de rango.</summary>
    EsquemaInvalido = 1,

    /// <summary>El modelo tuvo menos confianza que el mínimo (0,75).</summary>
    ConfidenceBajo = 2,

    /// <summary>El GPS tenía un error mayor que el máximo (20 m).</summary>
    PrecisionGpsInsuficiente = 3,

    /// <summary>Detectado hace más de 7 días o más de 5 min en el futuro respecto de la recepción.</summary>
    TimestampFueraDeRango = 4,

    /// <summary>El tipo no está en el catálogo (por ejemplo «Normal»).</summary>
    TipoDesconocido = 5,

    /// <summary>Está a más de 50 m de la PE-28A.</summary>
    FueraDelCorredor = 6,

    /// <summary>Respecto del evento anterior del mismo seudónimo, implica más de 150 km/h.</summary>
    VelocidadImposible = 7,

    /// <summary>El lote mezcla seudónimos; lo evalúa Ingesta, Anomalías nunca lo devuelve (FR-034).</summary>
    SeudonimoInconsistente = 8,
}
