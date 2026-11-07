package pe.killauru.app.deteccion.dominio

import java.time.Instant

/** FR-018, R-019: datos de un viaje activo. */
internal data class Viaje(
    val id: String,
    val inicioUtc: Instant,
    val kmDesde: Double,
)
