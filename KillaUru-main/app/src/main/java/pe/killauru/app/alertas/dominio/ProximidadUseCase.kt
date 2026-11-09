package pe.killauru.app.alertas.dominio

import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.comun.dominio.Geodesia
import pe.killauru.app.mapa.dominio.AnomaliaLocal
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import javax.inject.Inject

/** FR-054, FR-056: detecta anomalías en zona de aviso para la posición actual. */
internal class ProximidadUseCase @Inject constructor(
    private val parametros: ParametrosAviso,
) {
    fun evaluar(
        posicion: Coordenada,
        velocidadKmh: Double,
        categoria: CategoriaVehiculo,
        anomalias: List<AnomaliaLocal>,
    ): List<Alerta> {
        val distAviso = parametros.distanciaAviso(categoria, velocidadKmh)
        return anomalias.mapNotNull { anomalia ->
            val dist = Geodesia.distanciaMetros(posicion, anomalia.coordenada)
            if (dist <= distAviso) Alerta(anomalia, dist, dist < distAviso / 2) else null
        }.sortedBy { it.distanciaM }
    }
}
