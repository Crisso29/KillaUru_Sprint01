package pe.killauru.app.sesion.dominio

/** FR-063, FR-065, FR-067: resultados posibles de operaciones de cuenta. */
internal sealed class ResultadoCuenta {
    data class Exitoso(val sesion: Sesion) : ResultadoCuenta()
    object CorreoNoVerificado : ResultadoCuenta()
    object CredencialesInvalidas : ResultadoCuenta()
    object CorreoYaRegistrado : ResultadoCuenta()
    data class Error(val causa: Throwable) : ResultadoCuenta()
}
