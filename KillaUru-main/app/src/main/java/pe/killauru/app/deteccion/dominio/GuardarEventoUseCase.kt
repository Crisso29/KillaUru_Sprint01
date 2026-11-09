package pe.killauru.app.deteccion.dominio

import java.time.Instant
import pe.killauru.app.comun.dominio.UuidV7

/** FR-012, FR-013, FR-015, R-003, R-018: guarda un evento verificando GPS y confianza. */
internal class GuardarEventoUseCase(
    private val repositorio: RepositorioEventos,
    private val politica: PoliticaGuardado,
    private val fuentes: Fuentes,
    private val maxEdadFixMs: Long = 2_000L,
) {
    suspend fun guardar(
        clasificacion: Clasificacion,
        lat: Double,
        lon: Double,
        precisionM: Double,
        fixUtc: Instant,
        modeloVersion: String,
        ahoraUtc: Instant,
    ): Boolean {
        if (!politica.debeGuardar(clasificacion)) return false
        val tipo = clasificacion.tipo ?: return false
        val edadMs = java.time.Duration.between(fixUtc, ahoraUtc).toMillis()
        if (edadMs > maxEdadFixMs) return false
        val hora = fuentes.horaConfiable() ?: return false
        val seudonimo = fuentes.seudonimo(hora)
        val evento = crearEvento(clasificacion, lat, lon, precisionM, tipo, modeloVersion, seudonimo, hora)
        repositorio.guardar(evento)
        return true
    }
    private fun crearEvento(clasificacion: Clasificacion, lat: Double, lon: Double, precisionM: Double,
        tipo: TipoAnomalia, modeloVersion: String, seudonimo: String, hora: Instant): Evento {
        return Evento(
            eventoId = UuidV7.de(hora, java.security.SecureRandom()).toString(),
            detectadoUtc = hora,
            lat = lat,
            lon = lon,
            precisionM = precisionM,
            tipo = tipo,
            severidad = clasificacion.severidad,
            confidence = clasificacion.confidence.toDouble(),
            modeloVersion = modeloVersion,
            seudonimo = seudonimo,
            estado = EstadoLocal.PENDING,
            estadoDesde = hora,
        )
    }
}
