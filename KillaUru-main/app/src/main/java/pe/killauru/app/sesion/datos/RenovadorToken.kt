package pe.killauru.app.sesion.datos

import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Request
import okhttp3.Response
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.Sesion
import pe.killauru.app.sesion.dominio.casos.AplicarSesionInvalidaUseCase
import java.util.concurrent.locks.ReentrantLock
import javax.inject.Inject
import javax.inject.Singleton
import kotlin.concurrent.withLock

/**
 * FR-062, ADR-009, RN-21 c: ante un 401 de una petición con token, renueva con el token de **renovación** y la reintenta.
 * Una renovación a la vez: si otro hilo ya renovó mientras este esperaba, reintenta con el token nuevo sin renovar otra
 * vez (reusar el mismo token de renovación pasados 60 s revoca la sesión). Si el servidor rechaza la renovación, la
 * sesión local se invalida.
 */
@Singleton
internal class RenovadorToken @Inject constructor(
    private val repoSesion: RepositorioSesion,
    // Lazy: IdentidadApi usa este mismo cliente HTTP, así que pedirlo directo formaría un ciclo de dependencias.
    private val api: dagger.Lazy<IdentidadApi>,
    private val aplicarInvalida: AplicarSesionInvalidaUseCase,
) : Interceptor {

    private val bloqueo = ReentrantLock()

    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        val respuesta = chain.proceed(request)
        val tokenUsado = request.header(AUTORIZACION)?.removePrefix(BEARER)
        if (respuesta.code != NO_AUTORIZADO || tokenUsado == null || RutasAutenticacion.esPublica(request.url)) return respuesta
        val vigente = bloqueo.withLock { runBlocking { sesionVigente(tokenUsado) } } ?: return respuesta
        respuesta.close()
        return chain.proceed(conToken(request, vigente.tokenAcceso))
    }

    /** La sesión con la que reintentar, o null si no la hay (no había sesión o el servidor rechazó la renovación). */
    private suspend fun sesionVigente(tokenUsado: String): Sesion? {
        val actual = repoSesion.obtener() ?: return null
        if (actual.tokenAcceso != tokenUsado) return actual // otro hilo ya renovó
        val renovacion = runCatching { api.get().renovar(RenovarSolicitudDto(actual.tokenRenovacion)) }.getOrNull() ?: return null
        val cuerpo = renovacion.body()
        if (renovacion.isSuccessful && cuerpo != null) return MapeadorSesion.aDominio(cuerpo).also { repoSesion.guardar(it) }
        if (renovacion.code() == NO_AUTORIZADO || renovacion.code() == PROHIBIDO) aplicarInvalida.ejecutar()
        return null
    }

    private fun conToken(request: Request, token: String) = request.newBuilder().header(AUTORIZACION, "$BEARER$token").build()

    private companion object {
        const val AUTORIZACION = "Authorization"
        const val BEARER = "Bearer "
        const val NO_AUTORIZADO = 401
        const val PROHIBIDO = 403
    }
}
