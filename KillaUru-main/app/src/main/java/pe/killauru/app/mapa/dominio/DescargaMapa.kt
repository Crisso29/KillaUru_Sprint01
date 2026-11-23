package pe.killauru.app.mapa.dominio

import kotlinx.coroutines.flow.Flow

/** Lo que ve el conductor de la descarga del mapa (paso 4 del primer uso y «Aún no tienes el mapa»). */
internal sealed interface EstadoDescarga {
    data object SinEmpezar : EstadoDescarga
    /** [bytes]: lo que pesa la versión (0 mientras aún no se leyó el manifiesto). */
    data class Descargando(val porcentaje: Int, val bytes: Long = 0) : EstadoDescarga

    /** Hay versión, pero espera Wi-Fi (sin permiso para datos móviles). [bytes] es lo que pesa. */
    data class EsperandoWifi(val bytes: Long) : EstadoDescarga
    data object Listo : EstadoDescarga

    /** Se reintentará solo; mientras tanto la detección sigue (prototipo «Si se corta, sigue desde donde quedó»). */
    data object Reintentando : EstadoDescarga
}

/** FR-041, FR-043: puerto para pedir la descarga en segundo plano y seguir su estado. */
internal interface DescargaMapa {
    val estado: Flow<EstadoDescarga>

    /** Pide comprobar y descargar. [reemplazar] = true al cambiar «Usar datos móviles» (la descarga sigue del parcial). */
    fun programar(usarDatosMoviles: Boolean, reemplazar: Boolean)
}
