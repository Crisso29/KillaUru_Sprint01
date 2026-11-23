namespace KillaUru.Identidad.Infrastructure.Correo;

/// <summary>Cómo terminó un intento de envío al proveedor; decide si vale la pena reintentar (R-007).</summary>
internal enum ResultadoEnvioCorreo
{
    /// <summary>2xx: el proveedor aceptó el correo.</summary>
    Enviado,

    /// <summary>Red, 408, 429 o 5xx: el proveedor falló y puede responder bien en unos milisegundos.</summary>
    Pasajero,

    /// <summary>Otro 4xx (clave inválida, dirección rechazada): reintentar no lo arregla.</summary>
    Definitivo,
}
