package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.ResultadoRecuperacion
import javax.inject.Inject

/**
 * HU-25: pide el enlace para recuperar la contraseña. Valida el correo antes de ir a la red: un correo sin «@» no gasta
 * una petición ni el límite de intentos del servidor.
 */
internal class RecuperarContrasenaUseCase @Inject constructor(private val api: ApiCuenta) {
    suspend fun ejecutar(correo: String): ResultadoRecuperacion {
        val limpio = correo.trim()
        if ('@' !in limpio || limpio.length < MINIMO) return ResultadoRecuperacion.CorreoInvalido
        return api.recuperarContrasena(limpio)
    }

    private companion object {
        const val MINIMO = 3 // «a@b»
    }
}
