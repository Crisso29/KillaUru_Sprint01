package pe.killauru.app.historial.dominio

/** FR-069: acumula viajes de usuarios registrados, purga a 6 meses. */
internal class AcumuladorRecorrido {
    private val recorridos = mutableListOf<ViajeRecorrido>()
    private val retencionMs = (6 * 30).toLong() * 24 * 60 * 60 * 1000L

    fun registrar(viaje: ViajeRecorrido, registrado: Boolean) {
        if (registrado) recorridos.add(viaje)
    }

    fun todos(): List<ViajeRecorrido> = recorridos.toList()

    fun purgar(ahoraMs: Long) {
        val limite = ahoraMs - retencionMs
        recorridos.removeAll { it.finMs < limite }
    }
}
