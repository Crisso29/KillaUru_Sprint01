namespace KillaUru.Auditoria.Contracts;

/// <summary>Códigos persistidos de RF-15: agregar con migración, nunca renumerar.</summary>
public enum AccionAuditada : short
{
    /// <summary>UsuarioCreado: hecho auditable del catálogo RF-15.</summary>
    UsuarioCreado = 1,
    /// <summary>RolCambiado: hecho auditable del catálogo RF-15.</summary>
    RolCambiado = 2,
    /// <summary>CuentaDesactivada: hecho auditable del catálogo RF-15.</summary>
    CuentaDesactivada = 3,
    /// <summary>CuentaActivada: hecho auditable del catálogo RF-15.</summary>
    CuentaActivada = 4,
    /// <summary>CuentaEliminada: hecho auditable del catálogo RF-15.</summary>
    CuentaEliminada = 5,
    /// <summary>InicioSesionFallido: hecho auditable del catálogo RF-15.</summary>
    InicioSesionFallido = 6,
    /// <summary>CuentaBloqueada: hecho auditable del catálogo RF-15.</summary>
    CuentaBloqueada = 7,
    /// <summary>InicioSesionAdministrador: hecho auditable del catálogo RF-15.</summary>
    InicioSesionAdministrador = 8,
    /// <summary>RecuperacionSolicitada: hecho auditable del catálogo RF-15.</summary>
    RecuperacionSolicitada = 9,
    /// <summary>ContrasenaCambiada: hecho auditable del catálogo RF-15.</summary>
    ContrasenaCambiada = 10,
    /// <summary>RevisionPrivilegios: hecho auditable del catálogo RF-15.</summary>
    RevisionPrivilegios = 11,
    /// <summary>AnomaliaModerada: hecho auditable del catálogo RF-15.</summary>
    AnomaliaModerada = 12,
    /// <summary>DatasetExportado: hecho auditable del catálogo RF-15.</summary>
    DatasetExportado = 13,
    /// <summary>AccesoAdministrativoDatos: hecho auditable del catálogo RF-15.</summary>
    AccesoAdministrativoDatos = 14,
}
