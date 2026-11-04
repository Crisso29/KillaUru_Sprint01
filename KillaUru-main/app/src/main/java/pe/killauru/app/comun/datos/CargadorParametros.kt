package pe.killauru.app.comun.datos

import kotlinx.serialization.SerializationException
import kotlinx.serialization.json.Json
import pe.killauru.app.comun.dominio.ParametrosApp

/** FR-010, FR-029: configuración ilegible o inválida impide arrancar la detección. */
internal object CargadorParametros {
    fun cargar(texto: String): ParametrosApp? = try {
        Json.decodeFromString<ParametrosDto>(texto).takeIf { it.esValido() }?.aDominio()
    } catch (_: SerializationException) {
        // Error de formato externo: no sustituirlo por parámetros inventados.
        null
    }
}
