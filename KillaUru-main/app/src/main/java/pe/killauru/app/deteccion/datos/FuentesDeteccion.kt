package pe.killauru.app.deteccion.datos

import java.time.Instant
import javax.inject.Inject
import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.deteccion.dominio.Fuentes
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.seudonimo.dominio.ObtenerSeudonimoUseCase

internal class FuentesDeteccion @Inject constructor(
    private val prefs: RepositorioPreferencias,
    private val seudonimo: ObtenerSeudonimoUseCase,
    private val reloj: Reloj,
) : Fuentes {
    override suspend fun seudonimo(horaConfiable: Instant?) = seudonimo.obtener(horaConfiable)
    override suspend fun categoria() = prefs.obtenerCategoria()
    override fun horaConfiable() = reloj.ahoraConfiable()
    override fun km(): Double? = null
}
