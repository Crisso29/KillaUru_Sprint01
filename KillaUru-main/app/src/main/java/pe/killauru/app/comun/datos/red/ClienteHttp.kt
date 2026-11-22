package pe.killauru.app.comun.datos.red

import okhttp3.Interceptor
import okhttp3.OkHttpClient
import java.util.concurrent.TimeUnit

/**
 * FR-030, FR-062: cliente HTTP único de la app; TLS por defecto con OkHttp y timeouts de 15/30 s.
 * El orden de los interceptores importa: primero se pone el token, luego el seudónimo mira si la petición quedó
 * anónima, y al final el renovador atiende el 401 (y reintenta solo lo que viene después de él).
 */
internal object ClienteHttp {
    fun construir(token: Interceptor, seudonimo: Interceptor, renovador: Interceptor): OkHttpClient =
        OkHttpClient.Builder()
            .connectTimeout(15, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
            .addInterceptor(token)
            .addInterceptor(seudonimo)
            .addInterceptor(renovador)
            .build()
}
