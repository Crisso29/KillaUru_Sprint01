package pe.killauru.app.sesion.datos

import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response
import pe.killauru.app.sesion.dominio.PermisoLocal
import pe.killauru.app.sesion.dominio.RepositorioSesion
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
                val sesion = repoSesion.obtener() ?: return@runBlocking respuesta
                val renovacion = try {
                    api.renovarToken(mapOf("token" to sesion.token))
                } catch (_: Exception) {
                    return@runBlocking respuesta
                }
                if (renovacion.isSuccessful) {
                    val body = renovacion.body()!!
                    val permiso = body.permisoVenceMs?.let { PermisoLocal(it) }
                    val nueva = sesion.copy(token = body.token, tokenExpiraMs = body.expiraMs,
                        permiso = permiso)
                    repoSesion.guardar(nueva)
                    respuesta.close()
                    chain.proceed(chain.request().newBuilder()
                        .header("Authorization", "Bearer ${body.token}")
                        .build())
                } else {
                    if (renovacion.code == 401 || renovacion.code == 403) {
                        aplicarInvalida.ejecutar()
                    }
                    respuesta
                }
            }
        }
    }
}
