package pe.killauru.app.sincronizacion.dominio

/**
 * FR-031: cuenta los rechazos del servidor por motivo, en `contador_local` (data-model §1).
 * Puerto del dominio: el dominio decide **qué** contar; el adaptador decide cómo guardarlo.
 */
internal interface ContadorRechazos {
    /** Suma 1 por cada motivo de la lista (un motivo repetido suma varias veces). */
    suspend fun sumar(motivos: List<String>)
}
