package pe.killauru.app.sesion.datos

import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.Sesion
import pe.killauru.app.sesion.dominio.casos.AplicarSesionInvalidaUseCase
import java.util.concurrent.locks.ReentrantLock
import javax.inject.Inject
import javax.inject.Singleton
import kotlin.concurrent.withLock

/**
 * FR-062: renueva JWT ante 401; un solo intento aunque lleguen peticiones paralelas.
 * Si el servidor indica cuenta desactivada, invalida la sesión local.
 */
@Singleton
internal class RenovadorToken @Inject constructor(
    private val repoSesion: RepositorioSesion,
    private val api: IdentidadApi,
    private val aplicarInvalida: AplicarSesionInvalidaUseCase,
) : Interceptor {

    private val bloqueo = ReentrantLock()

    override fun intercept(chain: Interceptor.Chain): Response {
        val respuesta = chain.proceed(chain.request())
        if (respuesta.code != 401) return respuesta

        return bloqueo.withLock {
            runBlocking {
                renovar(chain, respuesta)
            }
        }
    }
    /** ADR-009: se renueva con el token de **renovación** (el de acceso ya venció); la respuesta es otra `SesionApp`. */
    private suspend fun renovar(chain: Interceptor.Chain, respuesta: Response): Response {
        val sesion = repoSesion.obtener() ?: return respuesta
        val renovacion = try {
            api.renovar(RenovarSolicitudDto(sesion.tokenRenovacion))
        } catch (_: Exception) {
            return respuesta
        }
        val cuerpo = renovacion.body()
        if (!renovacion.isSuccessful || cuerpo == null) {
            if (renovacion.code() == 401 || renovacion.code() == 403) aplicarInvalida.ejecutar()
            return respuesta
        }
        return reintentar(chain, respuesta, MapeadorSesion.aDominio(cuerpo))
    }

    private suspend fun reintentar(chain: Interceptor.Chain, respuesta: Response, nueva: Sesion): Response {
        repoSesion.guardar(nueva)
        respuesta.close()
        return chain.proceed(chain.request().newBuilder().header("Authorization", "Bearer ${nueva.tokenAcceso}").build())
    }
}
