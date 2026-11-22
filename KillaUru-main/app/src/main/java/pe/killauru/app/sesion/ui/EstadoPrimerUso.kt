package pe.killauru.app.sesion.ui

/** Pasos del primer uso (FR-001, FR-004, FR-006). `NavGrafo` sale del flujo al llegar a [Completado]. */
internal sealed interface EstadoPrimerUso {
    data object Cargando : EstadoPrimerUso
    data object Bienvenida : EstadoPrimerUso
    data class MostrarTerminos(
        val terminosTexto: String,
        val politicaTexto: String,
        val terminosVersion: String,
        val politicaVersion: String,
    ) : EstadoPrimerUso
    data object SeleccionarVehiculo : EstadoPrimerUso
    data object SolicitarPermisos : EstadoPrimerUso
    data object CuentaONo : EstadoPrimerUso
    data object Completado : EstadoPrimerUso
}
