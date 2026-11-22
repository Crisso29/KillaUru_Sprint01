package pe.killauru.app.comun.ui

/**
 * FR-008: rutas de navegación de NavGrafo. Los pasos del primer uso (bienvenida, términos, vehículo, permisos, cuenta)
 * no son rutas: son estados de `PrimerUsoViewModel`, que decide el paso según lo ya guardado.
 */
internal sealed class Rutas(val ruta: String) {
    data object PrimerUso : Rutas("primer_uso")
    data object Mapa : Rutas("mapa")
    data object Detalle : Rutas("detalle/{$ARG_ANOMALIA}") {
        fun con(id: String) = "detalle/$id"
    }
    data object Historial : Rutas("historial")
    data object Ajustes : Rutas("ajustes")

    companion object {
        const val ARG_ANOMALIA = "anomaliaId"
    }
}
