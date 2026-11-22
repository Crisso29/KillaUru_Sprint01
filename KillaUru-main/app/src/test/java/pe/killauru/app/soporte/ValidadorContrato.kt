package pe.killauru.app.soporte

import com.networknt.schema.JsonSchemaFactory
import com.networknt.schema.SpecVersion
import io.swagger.parser.OpenAPIParser
import io.swagger.v3.core.util.Json31
import io.swagger.v3.oas.models.OpenAPI
import io.swagger.v3.oas.models.PathItem
import io.swagger.v3.parser.core.models.ParseOptions
import okhttp3.mockwebserver.RecordedRequest
import com.fasterxml.jackson.databind.JsonNode
import com.fasterxml.jackson.databind.ObjectMapper
import com.fasterxml.jackson.databind.node.ObjectNode

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

    /** Esquema [componente] del contrato con sus `$ref` resueltos, como JSON listo para [validarJson]. */
    fun esquema(nombreYaml: String, componente: String): String {
        val arbol = Json31.mapper().valueToTree<JsonNode>(abrir(nombreYaml).components.schemas.getValue(componente))
        quitarTipoJuntoAConst(arbol)
        return arbol.toString()
    }

    /**
     * El parser de Swagger añade `"type": "object"` a una propiedad llamada `type` cuyo esquema es `{ const: … }`
     * (p. ej. `type: { const: FeatureCollection }` en el contrato 006), y eso rechazaría un JSON correcto.
     * Con `const` el valor ya queda fijado del todo, así que quitar ese `type` no afloja ninguna regla del YAML.
     */
    private fun quitarTipoJuntoAConst(nodo: JsonNode) {
        if (nodo is ObjectNode && nodo.has("const")) nodo.remove("type")
        nodo.forEach(::quitarTipoJuntoAConst)
    }

    /**
     * Nombres de las propiedades del esquema [componente]. Sirve cuando el esquema es permisivo a propósito
     * (p. ej. `Evento`, D5): validar no detectaría un campo mal nombrado, pero compararlo con esta lista sí.
     */
    fun propiedades(nombreYaml: String, componente: String): Set<String> =
        abrir(nombreYaml).components.schemas.getValue(componente).properties.keys

    /** Ejemplo que el contrato declara para la respuesta [codigo] de [metodo] [ruta], como JSON. */
    fun ejemploRespuesta(nombreYaml: String, ruta: String, metodo: PathItem.HttpMethod, codigo: String): String {
        val operacion = abrir(nombreYaml).paths.getValue(ruta).readOperationsMap().getValue(metodo)
        val ejemplo = operacion.responses.getValue(codigo).content.getValue("application/json").example
        return Json31.mapper().writeValueAsString(ejemplo ?: error("$ruta $codigo no tiene ejemplo en $nombreYaml"))
    }

    private fun abrir(nombreYaml: String): OpenAPI {
        val opciones = ParseOptions().apply { isResolve = true; isResolveFully = true }
        return OpenAPIParser().readLocation("build/generated/contratos/$nombreYaml", null, opciones).openAPI
            ?: error("No se pudo cargar el YAML: $nombreYaml")
    }
}
