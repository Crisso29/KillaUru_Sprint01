package pe.killauru.app.sesion.dominio

/** FR-060, FR-061: resultados de operaciones de cuenta. */
internal sealed class ResultadoCuenta {
    data object Exito : ResultadoCuenta()
    data class Error(val mensaje: String) : ResultadoCuenta()
    data object Conflicto : ResultadoCuenta()          // 409
    data object LimiteAlcanzado : ResultadoCuenta()    // 429
    data object RolNoPermitido : ResultadoCuenta()     // R-033
    data object SesionInvalida : ResultadoCuenta()
}
