package pe.killauru.app.soporte

import com.networknt.schema.JsonSchemaFactory
import com.networknt.schema.SpecVersion
import io.swagger.parser.OpenAPIParser
import okhttp3.mockwebserver.RecordedRequest
import com.fasterxml.jackson.databind.ObjectMapper

/**
 * T017 / FR-078: valida peticiones y respuestas de MockWebServer contra los YAML de contratos.
 * Los YAML se leen desde 'build/generated/contratos/' (tarea copiarContratos del build.gradle.kts).
 */
internal object ValidadorContrato {
    private val mapper = ObjectMapper()
    private val factory = JsonSchemaFactory.getInstance(SpecVersion.VersionFlag.V7)

    fun cargarYaml(nombre: String): Any {
        val ruta = "build/generated/contratos/$nombre"
        return OpenAPIParser().readLocation(ruta, null, null).openAPI
            ?: error("No se pudo cargar el YAML: $ruta")
    }

    fun validarJson(json: String, schema: String): List<String> {
        val esquema = factory.getSchema(schema)
        val nodo = mapper.readTree(json)
        return esquema.validate(nodo).map { it.message }
    }

    fun bodyDe(request: RecordedRequest): String =
        request.body.clone().readUtf8()

    fun estaPresente(nombreYaml: String): Boolean {
        val archivo = java.io.File("build/generated/contratos/$nombreYaml")
        return archivo.exists()
    }
}
