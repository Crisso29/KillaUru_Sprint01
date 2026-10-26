namespace KillaUru.Identidad.Contracts;

/// <summary>Consultas agregadas de identidad para Dashboard (RN-17), sin ids ni correos.</summary>
public interface IIdentidadConsultas
{
    /// <summary>Cuenta conductores con al menos una sesión iniciada o renovada en la ventana indicada.</summary>
    /// <param name="dias">Días de la ventana, mayor que cero.</param>
    /// <param name="ct">Cancelación.</param>
    /// <returns>Número de cuentas distintas.</returns>
    Task<int> ContarConductoresActivosAsync(int dias, CancellationToken ct);
}
