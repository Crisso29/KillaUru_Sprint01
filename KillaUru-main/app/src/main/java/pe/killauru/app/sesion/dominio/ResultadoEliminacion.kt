package pe.killauru.app.sesion.dominio

/** FR-067: resultado de eliminar la cuenta. Va aparte de [ResultadoCuenta] porque no produce una sesión. */
internal sealed class ResultadoEliminacion {
    /** 204: se hace la misma limpieza local que al cerrar sesión. */
    object Eliminada : ResultadoEliminacion()

    /** 403 `confirmacion_invalida`: contraseña incorrecta; no se borra nada. */
    object ConfirmacionInvalida : ResultadoEliminacion()

    data class Error(val causa: Throwable) : ResultadoEliminacion()
}
