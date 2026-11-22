package pe.killauru.app.sesion.dominio

/**
 * HU-25: resultado de pedir el enlace de recuperación. El servidor responde 202 exista o no la cuenta, así que
 * `Enviado` no dice si el correo está registrado (evita adivinar cuentas).
 */
internal enum class ResultadoRecuperacion { Enviado, CorreoInvalido, LimiteExcedido, Error }
