namespace KillaUru.Anomalias.Domain;
/// <summary>Códigos estables de RN-11; el último corresponde exclusivamente a la recepción de lotes.</summary>
internal enum MotivoDescarte : short
{
    
    /// <summary>Código estable: EsquemaInvalido.</summary>
    EsquemaInvalido = 1,
    
    /// <summary>Código estable: ConfidenceBajo.</summary>
    ConfidenceBajo = 2,
    
    /// <summary>Código estable: PrecisionGpsInsuficiente.</summary>
    PrecisionGpsInsuficiente = 3,
    
    /// <summary>Código estable: TimestampFueraDeRango.</summary>
    TimestampFueraDeRango = 4,
    
    /// <summary>Código estable: TipoDesconocido.</summary>
    TipoDesconocido = 5,
    
    /// <summary>Código estable: FueraDelCorredor.</summary>
    FueraDelCorredor = 6,
    
    /// <summary>Código estable: VelocidadImposible.</summary>
    VelocidadImposible = 7,
    
    /// <summary>Código estable: SeudonimoInconsistente.</summary>
    SeudonimoInconsistente = 8,
}
