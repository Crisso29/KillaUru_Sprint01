package pe.killauru.app.seudonimo.datos

import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response
import pe.killauru.app.seudonimo.dominio.RepositorioSeudonimo
import javax.inject.Inject

/**
 * FR-040, RF-27: añade `X-KillaUru-Seudonimo` a las peticiones anónimas (sin `Authorization`); el registrado se
 * identifica con su token. No pisa una cabecera ya puesta: el lote lleva el seudónimo con el que se detectó (D2).
 * Solo lee el seudónimo vigente; rotarlo es tarea de la detección, no de la red.
 */
internal class InterceptorSeudonimo @Inject constructor(
    private val seudonimos: RepositorioSeudonimo,
) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val request = chain.request()
        if (request.header("Authorization") != null || request.header(CABECERA) != null) return chain.proceed(request)
        // OkHttp llama a los interceptores en su propio hilo de red: bloquear aquí no detiene la interfaz.
        val seudonimo = runBlocking { seudonimos.obtener() } ?: return chain.proceed(request)
        return chain.proceed(request.newBuilder().header(CABECERA, seudonimo).build())
    }

    private companion object {
        const val CABECERA = "X-KillaUru-Seudonimo"
    }
}
