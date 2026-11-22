package pe.killauru.app.sesion.dominio

/** FR-060, FR-061, R-033: resultados de registrarse o iniciar sesión. */
internal sealed class ResultadoCuenta {
    data class Exitoso(val sesion: Sesion) : ResultadoCuenta()

    /** 401 `credenciales_invalidas`: mismo mensaje exista o no la cuenta (RN-19). */
    object CredencialesInvalidas : ResultadoCuenta()

    /** 409 `correo_en_uso`: sugerir iniciar sesión o recuperar la contraseña (FR-060). */
    object CorreoEnUso : ResultadoCuenta()

    /** R-033: la cuenta es de administración; se usa el panel y la sesión no se guarda. */
    object CuentaDeAdministracion : ResultadoCuenta()

    /** D13: fuera de 10–128 caracteres; se detecta en la app antes de llamar a la API. */
    object ContrasenaInvalida : ResultadoCuenta()

    /** HU-22, HU-27: falta aceptar la política o los términos. */
    object SinConsentimiento : ResultadoCuenta()

    /** 429: pedir que espere (FR-060). */
    object LimiteExcedido : ResultadoCuenta()

    data class Error(val causa: Throwable) : ResultadoCuenta()
}
