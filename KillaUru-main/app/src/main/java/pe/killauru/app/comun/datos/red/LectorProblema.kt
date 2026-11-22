package pe.killauru.app.comun.datos.red

import kotlinx.serialization.json.Json
import retrofit2.Response

/** Lee el `codigo` estable de un error ProblemDetails de la API (RFC 9457, convenciones-api §3). */
internal object LectorProblema {
    private val json = Json { ignoreUnknownKeys = true }

    /** null si el cuerpo no es un ProblemDetails, por ejemplo un error de un proxy intermedio. */
    fun codigo(resp: Response<*>): String? = resp.errorBody()?.string()?.let { cuerpo ->
        runCatching { json.decodeFromString<ProblemaDto>(cuerpo).codigo }.getOrNull()
    }
}
