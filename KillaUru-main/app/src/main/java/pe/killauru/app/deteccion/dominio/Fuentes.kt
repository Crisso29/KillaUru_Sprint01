package pe.killauru.app.deteccion.dominio

import java.time.Instant
import pe.killauru.app.sesion.dominio.CategoriaVehiculo

/** FR-013, R-003, R-010: proporciona datos contextuales al detector. */
internal interface Fuentes {
    suspend fun seudonimo(horaConfiable: Instant?): String
    suspend fun categoria(): CategoriaVehiculo?
    fun horaConfiable(): Instant?
    fun km(): Double?
}
