package pe.killauru.app.historial.dominio

internal data class ViajeRecorrido(
    val id: String,
    val inicioMs: Long,
    val finMs: Long,
    val kmDesde: Double,
    val kmHasta: Double,
    val eventos: Int,
) {
    val kmRecorridos: Double get() = kotlin.math.abs(kmHasta - kmDesde)
}
