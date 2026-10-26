namespace KillaUru.Anomalias.Domain;
/// <summary>Estados persistidos del ciclo de vida RN-07.</summary>
internal enum EstadoAnomalia : short { 
    /// <summary>Código estable: Activa.</summary>
    Activa = 1,
    
    /// <summary>Código estable: PendienteConfirmacion.</summary>
    PendienteConfirmacion = 2,
    
    /// <summary>Código estable: Expirada.</summary>
    Expirada = 3,
    
    /// <summary>Código estable: Reparada.</summary>
    Reparada = 4,
    
    /// <summary>Código estable: Descartada.</summary>
    Descartada = 5 }
