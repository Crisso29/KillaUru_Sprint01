package pe.killauru.app.deteccion.dominio

import pe.killauru.app.sesion.dominio.CategoriaVehiculo

/** RN-03, RN-18, RN-22, RF-29, FR-010: tabla de verdad completa. */
internal class PoliticaDeteccion(private val categoriasQueDetectan: List<String>) {
    data class EstadoDeteccion(
        val categoria: CategoriaVehiculo?,
        val soloVerMapa: Boolean,
        val enAhorro: Boolean,
        val tienePermiso: Boolean,
    )

    fun puedeDetectar(estado: EstadoDeteccion): Boolean {
        if (estado.soloVerMapa || estado.enAhorro || !estado.tienePermiso) return false
        val cat = estado.categoria ?: return false
        return cat.texto in categoriasQueDetectan
    }
}
