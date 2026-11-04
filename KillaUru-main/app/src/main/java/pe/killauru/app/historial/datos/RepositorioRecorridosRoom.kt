package pe.killauru.app.historial.datos

import pe.killauru.app.historial.dominio.RepositorioRecorridos
import pe.killauru.app.historial.dominio.ViajeRecorrido
import javax.inject.Inject

internal class RepositorioRecorridosRoom @Inject constructor(
    private val dao: RecorridoDao,
) : RepositorioRecorridos {

    override suspend fun guardar(viaje: ViajeRecorrido) {
        dao.insertar(
            RecorridoEntity(
                inicioUtc = viaje.inicioMs,
                finUtc = viaje.finMs,
                kmDesde = viaje.kmDesde,
                kmHasta = viaje.kmHasta,
                kmRecorridos = viaje.kmRecorridos,
                eventos = viaje.eventos,
            )
        )
    }

    override suspend fun todos(): List<ViajeRecorrido> =
        dao.todos().map {
            ViajeRecorrido(
                id = it.id.toString(),
                inicioMs = it.inicioUtc,
                finMs = it.finUtc,
                kmDesde = it.kmDesde,
                kmHasta = it.kmHasta,
                eventos = it.eventos,
            )
        }

    override suspend fun purgar(limiteMs: Long) = dao.purgar(limiteMs)
}
