package pe.killauru.app.mapa.dominio

import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia

internal data class AnomaliaLocal(
    val id: String,
    val tipo: TipoAnomalia,
    val severidad: Int,
    val persistente: Boolean,
    val coordenada: Coordenada,
    val km: Double,
    val venceUtc: Long?,
    val ultimaDeteccionUtc: Long,
    val enDemo: Boolean,
)
