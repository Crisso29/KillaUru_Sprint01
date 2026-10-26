namespace KillaUru.Anomalias.Domain;

/// <summary>Estados persistidos del ciclo de vida RN-07; los números no cambian porque se guardan en la base.</summary>
internal enum EstadoAnomalia : short
{
    /// <summary>Publicada en el mapa y en las alertas.</summary>
    Activa = 1,

    /// <summary>Grave o creada por tope: espera a un segundo vehículo antes de publicarse.</summary>
    PendienteConfirmacion = 2,

    /// <summary>Nadie la volvió a ver antes de su vencimiento; renace si alguien la detecta después.</summary>
    Expirada = 3,

    /// <summary>El administrador la marcó como reparada; renace si alguien la detecta después.</summary>
    Reparada = 4,

    /// <summary>El administrador la descartó (falso positivo); nunca se reactiva sola.</summary>
    Descartada = 5,
}
