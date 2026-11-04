package pe.killauru.app.comun.datos

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable
import pe.killauru.app.comun.dominio.ParametrosApp

/** T012: traduccion del formato del APK; serializacion fuera del dominio. */
@Serializable
internal data class ParametrosDto(
    @SerialName("VelocidadMinimaKmh") val velocidadMinimaKmh: Int,
    @SerialName("DuracionSostenidaSegundos") val duracionSostenidaSegundos: Int,
    @SerialName("ConfidenceMinimo") val confidenceMinimo: Double,
    @SerialName("CategoriasQueDetectan") val categoriasQueDetectan: List<String>,
    @SerialName("TiempoAvisoSegundos") val tiempoAvisoSegundos: Int,
    @SerialName("DistanciaAvisoMinimaMetros") val distanciaAvisoMinimaMetros: Int,
    @SerialName("TiempoAvisoPesadoSegundos") val tiempoAvisoPesadoSegundos: Int,
    @SerialName("DistanciaAvisoMinimaPesadoMetros") val distanciaAvisoMinimaPesadoMetros: Int,
    @SerialName("DistanciaFinPasadaMetros") val distanciaFinPasadaMetros: Int,
    @SerialName("DistanciaMaximaViaMetros") val distanciaMaximaViaMetros: Int,
    @SerialName("BateriaMinimaPorcentaje") val bateriaMinimaPorcentaje: Int,
    @SerialName("DuracionPermisoLocalDias") val duracionPermisoLocalDias: Int,
    @SerialName("ParadaFinViajeMinutos") val paradaFinViajeMinutos: Int,
    @SerialName("RotacionSeudonimoDias") val rotacionSeudonimoDias: Int,
    @SerialName("AntiguedadMaximaEnvioHoras") val antiguedadMaximaEnvioHoras: Int,
    @SerialName("PurgaEstadosFinalesDias") val purgaEstadosFinalesDias: Int,
    @SerialName("MaxEventosPorLote") val maxEventosPorLote: Int,
    @SerialName("BackoffSegundos") val backoffSegundos: List<Long>,
    @SerialName("JitterPorcentaje") val jitterPorcentaje: Int,
    @SerialName("ReanudarSincronizacionSegundosMax") val reanudarSincronizacionSegundosMax: Int,
    @SerialName("RetencionHistorialMeses") val retencionHistorialMeses: Int,
    @SerialName("PurgaAnomaliasVencidasDias") val purgaAnomaliasVencidasDias: Int,
    @SerialName("FrecuenciaSensoresHz") val frecuenciaSensoresHz: Int,
    @SerialName("LatenciaLoteSensoresMs") val latenciaLoteSensoresMs: Int,
    @SerialName("AntiguedadMaximaFixMs") val antiguedadMaximaFixMs: Int,
    @SerialName("FusionVentanasMs") val fusionVentanasMs: Int,
    @SerialName("IntervaloActualizacionAnomaliasMin") val intervaloActualizacionAnomaliasMin: Int,
    @SerialName("ContrasenaMinima") val contrasenaMinima: Int,
) {
    fun aDominio(): ParametrosApp = ParametrosApp(
        velocidadMinimaKmh, duracionSostenidaSegundos, confidenceMinimo,
        categoriasQueDetectan, tiempoAvisoSegundos, distanciaAvisoMinimaMetros,
        tiempoAvisoPesadoSegundos, distanciaAvisoMinimaPesadoMetros, distanciaFinPasadaMetros,
        distanciaMaximaViaMetros, bateriaMinimaPorcentaje, duracionPermisoLocalDias,
        paradaFinViajeMinutos, rotacionSeudonimoDias, antiguedadMaximaEnvioHoras,
        purgaEstadosFinalesDias, maxEventosPorLote, backoffSegundos,
        jitterPorcentaje, reanudarSincronizacionSegundosMax, retencionHistorialMeses,
        purgaAnomaliasVencidasDias, frecuenciaSensoresHz, latenciaLoteSensoresMs,
        antiguedadMaximaFixMs, fusionVentanasMs, intervaloActualizacionAnomaliasMin,
        contrasenaMinima,
    )

    fun esValido(): Boolean = numerosValidos() && secuenciasValidas() &&
        confidenceMinimo <= 1 && maxEventosPorLote <= MAXIMO_LOTE_CONTRATO && jitterPorcentaje <= 100

    private fun numerosValidos(): Boolean = listOf(
        velocidadMinimaKmh.toDouble(), duracionSostenidaSegundos.toDouble(), confidenceMinimo.toDouble(),
        tiempoAvisoSegundos.toDouble(), distanciaAvisoMinimaMetros.toDouble(), tiempoAvisoPesadoSegundos.toDouble(),
        distanciaAvisoMinimaPesadoMetros.toDouble(), distanciaFinPasadaMetros.toDouble(), distanciaMaximaViaMetros.toDouble(),
        bateriaMinimaPorcentaje.toDouble(), duracionPermisoLocalDias.toDouble(), paradaFinViajeMinutos.toDouble(),
        rotacionSeudonimoDias.toDouble(), antiguedadMaximaEnvioHoras.toDouble(), purgaEstadosFinalesDias.toDouble(),
        maxEventosPorLote.toDouble(), jitterPorcentaje.toDouble(), reanudarSincronizacionSegundosMax.toDouble(),
        retencionHistorialMeses.toDouble(), purgaAnomaliasVencidasDias.toDouble(), frecuenciaSensoresHz.toDouble(),
        latenciaLoteSensoresMs.toDouble(), antiguedadMaximaFixMs.toDouble(), fusionVentanasMs.toDouble(),
        intervaloActualizacionAnomaliasMin.toDouble(), contrasenaMinima.toDouble(),
    ).all { it.isFinite() && it > 0 }

    private fun secuenciasValidas(): Boolean =
        backoffSegundos.isNotEmpty() && backoffSegundos.all { it > 0 } &&
            backoffSegundos.zipWithNext().all { (anterior, siguiente) -> anterior < siguiente } &&
            categoriasQueDetectan.isNotEmpty() &&
            categoriasQueDetectan.all { it in setOf("LIVIANO", "PESADO", "MOTO") }
}

// Limite estructural del contrato de ingesta, no valor operativo de respaldo.
private const val MAXIMO_LOTE_CONTRATO = 50
