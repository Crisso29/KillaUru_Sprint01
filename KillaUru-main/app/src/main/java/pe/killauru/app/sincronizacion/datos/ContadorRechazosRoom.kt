package pe.killauru.app.sincronizacion.datos

import pe.killauru.app.comun.datos.bd.ContadorDao
import pe.killauru.app.sincronizacion.dominio.ContadorRechazos
import javax.inject.Inject

/** FR-031: guarda los rechazos en `contador_local` con la clave `rechazo.<Motivo>` (data-model §1). */
internal class ContadorRechazosRoom @Inject constructor(
    private val dao: ContadorDao,
) : ContadorRechazos {

    /** Agrupa antes de escribir: un lote con 3 rechazos del mismo motivo hace una sola suma de 3. */
    override suspend fun sumar(motivos: List<String>) {
        motivos.groupingBy { it }.eachCount().forEach { (motivo, cantidad) ->
            dao.sumar(clave = "$PREFIJO$motivo", cantidad = cantidad)
        }
    }

    private companion object {
        const val PREFIJO = "rechazo."
    }
}
