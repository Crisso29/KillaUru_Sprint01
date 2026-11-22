package pe.killauru.app.sincronizacion.dominio

/** Doble de [ContadorRechazos]: recuerda los motivos sumados, en orden, para comprobarlos en las pruebas. */
internal class ContadorRechazosEnMemoria : ContadorRechazos {
    val motivos = mutableListOf<String>()

    override suspend fun sumar(motivos: List<String>) {
        this.motivos += motivos
    }
}
