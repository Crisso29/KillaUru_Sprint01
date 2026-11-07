package pe.killauru.app.comun.datos.red

import okhttp3.Interceptor
import okhttp3.Response

/**
 * FR-040, RF-27: añade X-KillaUru-Seudonimo solo en peticiones sin Authorization.
 * Los usuarios registrados se identifican con su token; el anónimo con seudónimo.
 */
internal class InterceptorSeudonimo(
    private val obtenerSeudonimo: () -> String?,
) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        val tieneToken = request.header("Authorization") != null
        if (tieneToken) return chain.proceed(request)
        val seudonimo = obtenerSeudonimo() ?: return chain.proceed(request)
        return chain.proceed(
            request.newBuilder()
                .header("X-KillaUru-Seudonimo", seudonimo)
                .build()
        )
    }
}
