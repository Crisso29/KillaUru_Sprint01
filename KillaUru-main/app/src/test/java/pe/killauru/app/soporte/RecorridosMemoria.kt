package pe.killauru.app.soporte

import pe.killauru.app.historial.dominio.RepositorioRecorridos
import pe.killauru.app.historial.dominio.ViajeRecorrido

/** Doble de [RepositorioRecorridos] en memoria: Mis viajes sin Room. */
internal class RecorridosMemoria(vararg iniciales: ViajeRecorrido) : RepositorioRecorridos {
    val viajes = iniciales.toMutableList()

    override suspend fun guardar(viaje: ViajeRecorrido) { viajes.add(viaje) }
    override suspend fun todos(): List<ViajeRecorrido> = viajes.toList()
    override suspend fun purgar(limiteMs: Long) { viajes.removeAll { it.finMs < limiteMs } }
    override suspend fun borrarTodo() = viajes.clear()
}
