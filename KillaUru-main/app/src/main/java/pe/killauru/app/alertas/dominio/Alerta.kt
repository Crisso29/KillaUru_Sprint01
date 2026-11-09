package pe.killauru.app.alertas.dominio

import pe.killauru.app.mapa.dominio.AnomaliaLocal

/** FR-054: alerta de proximidad a una anomalía en la vía. */
internal data class Alerta(
    val anomalia: AnomaliaLocal,
    val distanciaM: Double,
    val urgente: Boolean,
)
