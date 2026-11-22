package pe.killauru.app.sesion.dominio

/**
 * FR-062, FR-063: sesión de la app con la forma de `SesionApp` (contrato 003, ADR-009).
 * El token de acceso dura 5 min y se renueva con [tokenRenovacion]; [permiso] vence en `sesionVenceUtc` (RN-21 a).
 * No guarda el id del usuario: el servidor lo lee del propio token (`sub`).
 */
internal data class Sesion(
    val tokenAcceso: String,
    val expiraAccesoMs: Long,
    val tokenRenovacion: String,
    val permiso: PermisoLocal,
    val rol: Rol,
)
