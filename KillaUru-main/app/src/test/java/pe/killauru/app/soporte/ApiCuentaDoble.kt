package pe.killauru.app.soporte

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.MisDatos
import pe.killauru.app.sesion.dominio.ResultadoRecuperacion
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.ResultadoEliminacion
import pe.killauru.app.sesion.dominio.SolicitudRegistro

/** Doble de [ApiCuenta]: devuelve lo configurado y recuerda qué se le pidió, para comprobarlo en las pruebas. */
internal class ApiCuentaDoble(
    var resultado: ResultadoCuenta = ResultadoCuenta.CredencialesInvalidas,
    var resultadoEliminacion: ResultadoEliminacion = ResultadoEliminacion.Eliminada,
    var resultadoRecuperacion: ResultadoRecuperacion = ResultadoRecuperacion.Enviado,
    var datos: MisDatos? = null,
) : ApiCuenta {
    var llamadas = 0
    var ultimaSolicitudRegistro: SolicitudRegistro? = null
    var ultimaContrasenaEliminacion: String? = null
    var revocacionExitosa = true
    var ultimaRevocacion: String? = null

    override suspend fun registrar(solicitud: SolicitudRegistro): ResultadoCuenta {
        llamadas++
        ultimaSolicitudRegistro = solicitud
        return resultado
    }

    override suspend fun iniciarSesion(correo: String, contrasena: String): ResultadoCuenta {
        llamadas++
        return resultado
    }

    override suspend fun reenviarVerificacion(): Boolean = true

    override suspend fun eliminarCuenta(contrasena: String): ResultadoEliminacion {
        llamadas++
        ultimaContrasenaEliminacion = contrasena
        return resultadoEliminacion
    }

    override suspend fun recuperarContrasena(correo: String): ResultadoRecuperacion {
        llamadas++
        return resultadoRecuperacion
    }

    override suspend fun misDatos(): MisDatos? = datos

    override suspend fun revocarSesion(tokenAcceso: String, tokenRenovacion: String): Boolean {
        llamadas++
        ultimaRevocacion = tokenRenovacion
        return revocacionExitosa
    }
}
