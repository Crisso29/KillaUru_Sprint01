package pe.killauru.app.deteccion.dominio

import java.time.Instant

/**
 * FR-011, R-018: ventana de 500 ms (25 muestras a 50 Hz) con sus señales descompuestas y el
 * contexto de 2 s necesario para la característica v_energia_rel (contrato p1, 009 §4).
 *
 * [aV]        aceleración vertical sin gravedad, 25 muestras (m/s²).
 * [aH]        magnitud horizontal sin gravedad, 25 muestras (m/s²).
 * [w]         norma del giroscopio, 25 muestras (rad/s).
 * [contextoAv] los últimos 2 s de aV (100 muestras) que terminan en esta ventana.
 * [velocidadKmH] velocidad del último fix GPS anterior al fin de la ventana (km/h).
 */
internal class Ventana(
    val aV: FloatArray,
    val aH: FloatArray,
    val w: FloatArray,
    val contextoAv: FloatArray,
    val velocidadKmH: Float,
    val inicioUtc: Instant,
)
