package pe.killauru.app.deteccion.dominio

/** RN-22 / FR-017: cargar prevalece sobre el porcentaje; el umbral es configuración. */
internal class PoliticaBateria(private val minimoPorcentaje: Int) {
    fun enAhorro(porcentaje: Int, cargando: Boolean): Boolean =
        !cargando && porcentaje < minimoPorcentaje
}
