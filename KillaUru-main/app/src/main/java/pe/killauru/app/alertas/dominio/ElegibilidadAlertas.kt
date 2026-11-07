package pe.killauru.app.alertas.dominio

internal data class EstadoElegibilidad(
    val registrado: Boolean,
    val permisoVigente: Boolean,
    val soloVerMapa: Boolean,
    val enAhorro: Boolean,
)

/** FR-054, FR-059, FR-063: solo usuarios registrados con permiso vigente. */
internal class ElegibilidadAlertas {
    fun esElegible(estado: EstadoElegibilidad): Boolean =
        estado.registrado && estado.permisoVigente && !estado.soloVerMapa && !estado.enAhorro
}
