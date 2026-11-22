package pe.killauru.app.mapa.dominio

import javax.inject.Inject

/**
 * FR-044, FR-045, FR-052: actualiza la copia local de anomalías desde el servidor.
 *
 * Flujo:
 * 1. Lee el [EstadoCopia] para saber si hay delta previo.
 * 2. Pide a [FuenteAnomalias] con los parámetros correctos según el rol y el estado.
 * 3. 200 → aplica altas/bajas y guarda el nuevo `generadoUtc`.
 * 4. 304 → solo actualiza `verificadoUtc`; las anomalías no cambian.
 * 5. 400 → descarta el `generadoUtc` y repite la descarga completa en la misma llamada.
 *
 * Invariante: `venceUtc: null` en la respuesta se trata como baja (R-011).
 *
 * No depende del reloj del celular: `verificadoUtc` («Actualizado hace…») es siempre la hora del **servidor**
 * (`generadoUtc` en un 200, la de la fuente en un 304), como pide data-model §estado_copia y FR-026.
 */
internal class ActualizarAnomaliasUseCase @Inject constructor(
    private val repositorio: RepositorioAnomalias,
    private val fuente: FuenteAnomalias,
) {
    sealed class Resultado {
        object Exito : Resultado()
        object SinCambios : Resultado()
        data class Error(val causa: Throwable) : Resultado()
    }

    /** Actualiza la copia con el [alcance] del usuario actual. */
    suspend fun actualizar(alcance: Alcance): Resultado {
        val estado = repositorio.obtenerEstado()
        return when (val r = pedirAlServidor(alcance, estado)) {
            is FuenteAnomalias.Respuesta.Datos -> aplicarDatos(r, alcance)
            is FuenteAnomalias.Respuesta.SinCambios -> guardarVerificado(estado, r.verificadoUtc)
            is FuenteAnomalias.Respuesta.SolicitudInvalida -> {
                // 400 → descarta delta y pide descarga completa sin updatedSince
                val completo = fuente.obtener(alcance, null, null)
                if (completo is FuenteAnomalias.Respuesta.Datos) aplicarDatos(completo, alcance)
                else Resultado.SinCambios
            }
            is FuenteAnomalias.Respuesta.Error -> Resultado.Error(r.causa)
        }
    }

    /**
     * Vacía la copia local al cambiar de rol (ej. cerrar sesión).
     * Los mapas de cada rol son distintos; no se mezclan (FR-052).
     */
    suspend fun limpiarAlCambiarRol() {
        repositorio.limpiarCopia()
    }

    // ── privado ──────────────────────────────────────────────────────────────

    private suspend fun pedirAlServidor(
        alcance: Alcance,
        estado: EstadoCopia?,
    ): FuenteAnomalias.Respuesta {
        // El rol coincide con el estado guardado → pedimos delta si existe
        val mismoRol = estado?.alcance == alcance
        return when {
            alcance == Alcance.Anonimo ->
                // Anónimo siempre pide todo con ETag para revalidar (If-None-Match)
                fuente.obtener(alcance, null, estado?.etag?.takeIf { mismoRol })

            // mismoRol ⇒ estado no es null (se comparó su alcance con uno no nulo)
            mismoRol && estado.generadoUtc != null ->
                // Registrado con copia existente → delta con updatedSince
                fuente.obtener(alcance, estado.generadoUtc, null)

            else ->
                // Registrado sin copia o rol distinto → descarga completa
                fuente.obtener(alcance, null, null)
        }
    }

    private suspend fun aplicarDatos(
        r: FuenteAnomalias.Respuesta.Datos,
        alcance: Alcance,
    ): Resultado {
        val nuevoEstado = EstadoCopia(
            alcance = alcance,
            generadoUtc = r.generadoUtc,
            etag = r.etag,
            verificadoUtc = r.generadoUtc,
        )
        repositorio.actualizarLote(r.altas, r.bajas, nuevoEstado)
        return Resultado.Exito
    }

    private suspend fun guardarVerificado(
        estado: EstadoCopia?,
        verificadoUtc: Long,
    ): Resultado {
        // 304: mantiene anomalías, actualiza la marca de verificación
        val actualizado = (estado ?: EstadoCopia(Alcance.Anonimo, null, null, null))
            .copy(verificadoUtc = verificadoUtc)
        repositorio.actualizarLote(emptyList(), emptyList(), actualizado)
        return Resultado.SinCambios
    }
}
