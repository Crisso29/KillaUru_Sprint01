package pe.killauru.app.sesion.datos

import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response
import pe.killauru.app.sesion.dominio.RepositorioSesion
import javax.inject.Inject

/** FR-062: añade Authorization: Bearer <token> a peticiones autenticadas. */
internal class InterceptorToken @Inject constructor(
    private val repoSesion: RepositorioSesion,
) : Interceptor {

    override fun intercept(chain: Interceptor.Chain): Response {
        val token = runBlocking { repoSesion.obtener()?.tokenAcceso }
        val req = if (token != null) {
            chain.request().newBuilder()
                .header("Authorization", "Bearer $token")
                .build()
        } else chain.request()
        return chain.proceed(req)
    }
}
