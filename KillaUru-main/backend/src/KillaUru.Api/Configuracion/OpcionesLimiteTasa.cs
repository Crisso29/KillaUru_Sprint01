namespace KillaUru.Api.Configuracion;

/// <summary>Límites de tasa por hora (RNF-11). Los valores iniciales son los de docs.</summary>
internal sealed class OpcionesLimiteTasa
{
    /// <summary>Consultas por hora de un anónimo (clave: seudónimo + IP).</summary>
    public int AnonimoConsultasPorHora { get; init; } = 60;

    /// <summary>Lotes por hora de un anónimo, cuota aparte de las consultas (clave: seudónimo + IP).</summary>
    public int AnonimoLotesPorHora { get; init; } = 120;

    /// <summary>Peticiones por hora de un conductor registrado (clave: cuenta).</summary>
    public int RegistradoPorHora { get; init; } = 1_000;

    /// <summary>Peticiones por hora de un administrador (clave: cuenta).</summary>
    public int AdministradorPorHora { get; init; } = 3_000;

    /// <summary>Techo de lotes anónimos por IP, aunque cambie el seudónimo (D2).</summary>
    public int TechoIpLotesPorHora { get; init; } = 600;

    /// <summary>Techo de consultas anónimas por IP, aunque cambie el seudónimo (D2).</summary>
    public int TechoIpConsultasPorHora { get; init; } = 300;

    /// <summary>Registros de cuenta por IP (D8).</summary>
    public int RegistrosPorHoraPorIp { get; init; } = 5;
}
