package pe.killauru.app.sincronizacion.dominio

/** FR-022, FR-023: puerto para dejar programados el envío periódico de eventos y la purga diaria. */
internal interface ProgramadorSincronizacion {
    fun programar()
}
