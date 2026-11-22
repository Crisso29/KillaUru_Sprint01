package pe.killauru.app.sesion.datos

import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response
import pe.killauru.app.sesion.dominio.RepositorioSesion
import javax.inject.Inject

/**
 * FR-062: añade `Authorization: Bearer <token de acceso>` si hay sesión. Respeta una cabecera ya puesta (p. ej. la
 * revocación pendiente) y no la añade a las rutas públicas de autenticación ([RutasAutenticacion]).
 */
internal class InterceptorToken @Inject constructor(
    private val repo: RepositorioSesion,
) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        if (request.header(AUTORIZACION) != null || RutasAutenticacion.esPublica(request.url)) return chain.proceed(request)
        // OkHttp llama a los interceptores en su hilo de red: bloquear aquí no detiene la interfaz.
        val sesion = runBlocking { repo.obtener() } ?: return chain.proceed(request)
        return chain.proceed(request.newBuilder().header(AUTORIZACION, "Bearer ${sesion.tokenAcceso}").build())
    }

    private companion object {
        const val AUTORIZACION = "Authorization"
    }
}
