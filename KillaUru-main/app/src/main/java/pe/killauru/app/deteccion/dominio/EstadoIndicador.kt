package pe.killauru.app.deteccion.dominio

internal data class EstadoIndicador(
    val servicioActivo: Boolean,
    val tieneGps: Boolean,
    val pendientes: Int,
    val enAhorro: Boolean,
    val soloVerMapa: Boolean,
    val sinPermiso: Boolean,
)
