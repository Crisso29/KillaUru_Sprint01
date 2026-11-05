package pe.killauru.app.historial.dominio

internal interface RepositorioRecorridos {
    suspend fun guardar(viaje: ViajeRecorrido)
    suspend fun todos(): List<ViajeRecorrido>
    suspend fun purgar(limiteMs: Long)
}
