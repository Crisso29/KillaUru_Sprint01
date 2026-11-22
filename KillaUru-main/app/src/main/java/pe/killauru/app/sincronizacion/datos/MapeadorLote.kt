package pe.killauru.app.sincronizacion.datos

import pe.killauru.app.deteccion.dominio.Evento
import pe.killauru.app.sincronizacion.dominio.Lote
import pe.killauru.app.sincronizacion.dominio.Rechazo
import pe.killauru.app.sincronizacion.dominio.RespuestaLote

/**
 * Traduce entre el dominio y el JSON del contrato 005. Es el único lugar donde viven las diferencias de nombres
 * y tipos (p. ej. `lat` del dominio → `latitud`, `Instant` → texto ISO-8601), así el dominio no conoce la API.
 */
internal object MapeadorLote {

    fun aDto(lote: Lote): LoteDto = LoteDto(eventos = lote.eventos.map(::eventoDto))

    fun aDominio(dto: RespuestaLoteDto): RespuestaLote = RespuestaLote(
        aceptados = dto.aceptados,
        duplicados = dto.duplicados,
        rechazados = dto.rechazados.map { Rechazo(id = it.id, indice = it.indice, motivo = it.motivo) },
    )

    private fun eventoDto(e: Evento) = EventoDto(
        eventoId = e.eventoId,
        detectadoUtc = e.detectadoUtc.toString(),
        latitud = e.lat,
        longitud = e.lon,
        precisionMetros = e.precisionM,
        tipo = e.tipo.texto,
        severidad = e.severidad,
        confidence = e.confidence,
        modeloVersion = e.modeloVersion,
        seudonimo = e.seudonimo,
    )
}
