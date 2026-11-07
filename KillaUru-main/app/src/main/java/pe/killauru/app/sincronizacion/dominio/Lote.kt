package pe.killauru.app.sincronizacion.dominio

import pe.killauru.app.deteccion.dominio.Evento

internal data class Lote(
    val loteId: String,
    val seudonimo: String,
    val eventos: List<Evento>,
)
