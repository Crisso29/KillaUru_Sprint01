package pe.killauru.app.deteccion.dominio

/** R-032, FR-016: calcula el indicador de estado combinando todos los factores. */
internal class CalcularIndicadorUseCase(
    private val repositorio: RepositorioEventos,
) {
    data class Entrada(
        val servicioActivo: Boolean,
        val tienePermiso: Boolean,
        val tieneGps: Boolean,
        val enAhorro: Boolean,
        val soloVerMapa: Boolean,
        val velocidadOk: Boolean,
    )

    data class Resultado(
        val indicador: IndicadorEstado,
        val pendientes: Int,
    )

    suspend fun calcular(entrada: Entrada): Resultado {
        val indicador = IndicadorEstado.calcular(
            servicioActivo = entrada.servicioActivo,
            tienePermiso = entrada.tienePermiso,
            tieneGps = entrada.tieneGps,
            enAhorro = entrada.enAhorro,
            soloVerMapa = entrada.soloVerMapa,
            velocidadOk = entrada.velocidadOk,
        )
        val pendientes = if (entrada.servicioActivo) repositorio.contarPendientes() else 0
        return Resultado(indicador, pendientes)
    }
}
