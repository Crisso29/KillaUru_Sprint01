package pe.killauru.app.alertas.dominio

import pe.killauru.app.sesion.dominio.CategoriaVehiculo

/** FR-056, RN-16: parámetros de aviso según categoría de vehículo. */
internal class ParametrosAviso(
    private val tiempoAvisoLivianoS: Double = 8.0,
    private val tiempoAvisoPesadoS: Double = 12.0,
    private val distMinLivianoM: Double = 100.0,
    private val distMinPesadoM: Double = 150.0,
    private val finPasadaM: Double = 500.0,
) {
    fun distanciaAviso(categoria: CategoriaVehiculo, velocidadKmh: Double): Double {
        val velocidadMs = velocidadKmh * 1000.0 / 3600.0
        return when (categoria) {
            CategoriaVehiculo.PESADO -> maxOf(distMinPesadoM, velocidadMs * tiempoAvisoPesadoS)
            else -> maxOf(distMinLivianoM, velocidadMs * tiempoAvisoLivianoS)
        }
    }

    fun finPasadaM(): Double = finPasadaM
}
