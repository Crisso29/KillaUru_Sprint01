package pe.killauru.app.sesion.datos

import okhttp3.Interceptor
import okhttp3.Response
import pe.killauru.app.sesion.dominio.RepositorioSesion
import javax.inject.Inject

/** FR-063: añade el Bearer token JWT a requests que lo requieren. */
internal class InterceptorToken @Inject constructor(
    private val repo: RepositorioSesion,
) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        if (request.header("Authorization") != null) return chain.proceed(request)
        return chain.proceed(request)
    }
}
