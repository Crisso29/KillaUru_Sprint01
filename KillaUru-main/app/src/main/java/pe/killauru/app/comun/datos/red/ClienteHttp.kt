package pe.killauru.app.comun.datos.red

import okhttp3.OkHttpClient
import java.util.concurrent.TimeUnit

/** FR-030: cliente base; TLS por defecto con OkHttp, timeouts de 15/30 s. */
internal object ClienteHttp {
    fun construir(interceptorSeudonimo: InterceptorSeudonimo): OkHttpClient =
        OkHttpClient.Builder()
            .connectTimeout(15, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
            .addInterceptor(interceptorSeudonimo)
            .build()
}
