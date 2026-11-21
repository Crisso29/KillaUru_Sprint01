package pe.killauru.app.mapa.dominio

/** Doble local para aislar la sincronización del mapa. */
internal class FuenteAnomaliasMemoria(
    var respuesta: FuenteAnomalias.Respuesta,
) : FuenteAnomalias {
    var llamadas = 0
    var ultimoUpdatedSince: Long? = null
    var ultimoEtag: String? = null

    override suspend fun obtener(
        alcance: Alcance,
        updatedSince: Long?,
        etag: String?,
    ): FuenteAnomalias.Respuesta {
        llamadas++
        ultimoUpdatedSince = updatedSince
        ultimoEtag = etag
        return respuesta
    }
}
