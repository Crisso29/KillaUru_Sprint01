package pe.killauru.app.mapa.datos

import pe.killauru.app.mapa.dominio.ArchivoMapa
import pe.killauru.app.mapa.dominio.ManifiestoMapa
import pe.killauru.app.mapa.dominio.TipoArchivoMapa
import java.time.Instant

/**
 * `ManifiestoMapaDto` → [ManifiestoMapa]. Un manifiesto de otro esquema o al que le falte uno de los 4 archivos se
 * rechaza (IllegalArgumentException): la app no instala un mapa incompleto. Un tipo nuevo que no conoce se ignora.
 */
internal object MapeadorManifiesto {
    private const val ESQUEMA = 1
    private val TIPOS = mapOf(
        "pmtiles" to TipoArchivoMapa.PMTILES,
        "corredor" to TipoArchivoMapa.CORREDOR,
        "tramoDemo" to TipoArchivoMapa.TRAMO_DEMO,
        "estilo" to TipoArchivoMapa.ESTILO,
    )
    private val NOMBRE_DE_TIPO = TIPOS.entries.associate { (nombre, tipo) -> tipo to nombre }

    fun aDominio(dto: ManifiestoMapaDto): ManifiestoMapa {
        require(dto.esquema == ESQUEMA) { "Esquema de manifiesto ${dto.esquema} no soportado" }
        val archivos = dto.archivos.mapNotNull { archivo ->
            TIPOS[archivo.tipo]?.let { tipo -> ArchivoMapa(tipo, archivo.version, archivo.url, archivo.bytes, archivo.sha256) }
        }
        require(archivos.map { it.tipo }.toSet() == TipoArchivoMapa.entries.toSet()) { "Al manifiesto le falta un archivo" }
        return ManifiestoMapa(dto.version, Instant.parse(dto.publicadoUtc).toEpochMilli(), dto.atribucion, archivos)
    }

    /** Al revés, para guardar el manifiesto de la versión activa con el mismo formato del contrato. */
    fun aDto(manifiesto: ManifiestoMapa) = ManifiestoMapaDto(
        esquema = ESQUEMA,
        version = manifiesto.version,
        publicadoUtc = Instant.ofEpochMilli(manifiesto.publicadoUtcMs).toString(),
        atribucion = manifiesto.atribucion,
        archivos = manifiesto.archivos.map { archivo ->
            ArchivoMapaDto(NOMBRE_DE_TIPO.getValue(archivo.tipo), archivo.version, archivo.url, archivo.bytes, archivo.sha256)
        },
    )
}
