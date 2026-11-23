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

    fun aDominio(dto: ManifiestoMapaDto): ManifiestoMapa {
        require(dto.esquema == ESQUEMA) { "Esquema de manifiesto ${dto.esquema} no soportado" }
        val archivos = dto.archivos.mapNotNull { a ->
            TIPOS[a.tipo]?.let { ArchivoMapa(it, a.version, a.url, a.bytes, a.sha256) }
        }
        require(archivos.map { it.tipo }.toSet() == TipoArchivoMapa.entries.toSet()) { "Al manifiesto le falta un archivo" }
        return ManifiestoMapa(dto.version, Instant.parse(dto.publicadoUtc).toEpochMilli(), dto.atribucion, archivos)
    }

    /** Al revés, para guardar el manifiesto de la versión activa con el mismo formato del contrato. */
    fun aDto(m: ManifiestoMapa) = ManifiestoMapaDto(
        esquema = ESQUEMA,
        version = m.version,
        publicadoUtc = Instant.ofEpochMilli(m.publicadoUtcMs).toString(),
        atribucion = m.atribucion,
        archivos = m.archivos.map { a ->
            ArchivoMapaDto(TIPOS.entries.first { it.value == a.tipo }.key, a.version, a.url, a.bytes, a.sha256)
        },
    )
}
