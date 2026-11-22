package pe.killauru.app.soporte

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.ResultadoEliminacion
import pe.killauru.app.sesion.dominio.SolicitudRegistro

/** Doble de [ApiCuenta]: devuelve lo configurado y recuerda qué se le pidió, para comprobarlo en las pruebas. */
internal class ApiCuentaDoble(
    var resultado: ResultadoCuenta = ResultadoCuenta.CredencialesInvalidas,
    var resultadoEliminacion: ResultadoEliminacion = ResultadoEliminacion.Eliminada,
) : ApiCuenta {
    var llamadas = 0
    var ultimaSolicitudRegistro: SolicitudRegistro? = null
    var ultimaContrasenaEliminacion: String? = null

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
}
