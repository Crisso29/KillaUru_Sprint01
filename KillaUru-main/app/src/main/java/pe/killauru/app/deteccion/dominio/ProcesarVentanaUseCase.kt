package pe.killauru.app.deteccion.dominio

import java.time.Instant

/** FR-011, FR-012, FR-014: orquesta prefiltro → clasificador → fusión → guardar. */
internal class ProcesarVentanaUseCase(
    private val prefiltro: Prefiltro,
    private val clasificador: Clasificador,
    private val fusionVentanas: FusionVentanas,
    private val guardarEvento: GuardarEventoUseCase,
) {
    private val buffer = mutableListOf<Pair<Ventana, Clasificacion>>()

    suspend fun procesar(
        ventana: Ventana,
        lat: Double,
        lon: Double,
        precisionM: Double,
        fixUtc: Instant,
        ahoraUtc: Instant,
    ) {
        if (!prefiltro.pasar(ventana)) return
        val clasificacion = clasificador.clasificar(ventana)
        if (clasificacion.tipo == null) return
        buffer.add(ventana to clasificacion)
        val fusionados = fusionVentanas.fusionar(buffer.toList())
        buffer.clear()
        for ((v, c) in fusionados) {
            guardarEvento.guardar(
                clasificacion = c,
                lat = lat,
                lon = lon,
                precisionM = precisionM,
                fixUtc = fixUtc,
                modeloVersion = clasificador.version,
                ahoraUtc = ahoraUtc,
            )
        }
    }
}
