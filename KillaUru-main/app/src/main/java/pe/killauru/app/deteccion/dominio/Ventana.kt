package pe.killauru.app.deteccion.dominio

import java.time.Instant

/** FR-011, R-018: ventana de acelerómetro de 2 s con muestras [ax, ay, az]. */
internal data class Ventana(
    val muestras: List<FloatArray>,
    val inicioUtc: Instant,
)
