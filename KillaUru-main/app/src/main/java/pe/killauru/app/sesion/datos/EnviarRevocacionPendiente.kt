package pe.killauru.app.sesion.datos

import kotlinx.coroutines.CancellationException
import javax.inject.Inject

/**
 * FR-066, R-023: envía la revocación pendiente. El token de acceso dura 5 min, así que horas después ya venció:
 * primero renueva con el token guardado y luego cierra esa sesión con los tokens nuevos (el servidor revoca toda la
 * familia). Devuelve `true` si ya no queda nada por hacer y `false` si hay que reintentar (sin red o error del servidor).
 */
internal class EnviarRevocacionPendiente @Inject constructor(
    private val api: IdentidadApi,
    private val almacen: AlmacenRevocacion,
) {
    suspend fun ejecutar(): Boolean {
        val token = almacen.obtener() ?: return true
        val terminado = try {
            revocar(token)
        } catch (e: CancellationException) {
            throw e
        } catch (_: Exception) {
            false
        }
        if (terminado) almacen.borrar()
        return terminado
    }

    private suspend fun revocar(tokenRenovacion: String): Boolean {
        val renovacion = api.renovar(RenovarSolicitudDto(tokenRenovacion))
        if (renovacion.code() in SESION_YA_INVALIDA) return true
        val nueva = renovacion.body()?.takeIf { renovacion.isSuccessful } ?: return false
        val cierre = api.cerrarSesion("Bearer ${nueva.tokenAcceso}", RenovarSolicitudDto(nueva.tokenRenovacion))
        return cierre.isSuccessful || cierre.code() in SESION_YA_INVALIDA
    }

    private companion object {
        /** 401/403: el servidor ya no reconoce esa sesión (vencida, revocada o cuenta desactivada). */
        val SESION_YA_INVALIDA = setOf(401, 403)
    }
}
