package pe.killauru.app.sincronizacion.dominio

/** Evento que el servidor no guardó: su `id` (null si no tenía uno válido), su posición en el lote y el motivo (D5). */
internal data class Rechazo(val id: String?, val indice: Int?, val motivo: String)
