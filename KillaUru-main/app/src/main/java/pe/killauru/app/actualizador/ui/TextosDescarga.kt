package pe.killauru.app.actualizador.ui

import pe.killauru.app.comun.dominio.FormatoNumero
import pe.killauru.app.mapa.dominio.EstadoDescarga

/** Textos de la descarga del mapa (prototipo «Mapa sin conexión» y «Aún no tienes el mapa»). Funciones puras. */
internal object TextosDescarga {
    private const val MB = 1024.0 * 1024.0
    private const val UN_DECIMAL_HASTA_MB = 10

    /** «2,3 MB», «38 MB»; vacío si aún no se sabe. MB binarios, como el ejemplo del contrato (39 845 120 B = 38 MB). */
    fun peso(bytes: Long): String {
        if (bytes <= 0) return ""
        val mb = bytes / MB
        return "${FormatoNumero.decimal(mb, if (mb < UN_DECIMAL_HASTA_MB) 1 else 0)} MB"
    }

    /** Lo que pasa, con palabras: la barra sola no basta (WCAG 1.4.1). */
    fun nota(estado: EstadoDescarga): String = when (estado) {
        EstadoDescarga.SinEmpezar -> "Preparando la descarga…"
        is EstadoDescarga.Descargando -> "Si se corta, sigue desde donde quedó."
        is EstadoDescarga.EsperandoWifi -> "Esperando Wi-Fi. Activa los datos móviles si quieres descargarlo ya."
        EstadoDescarga.Listo -> "Listo: el mapa ya está en tu celular."
        EstadoDescarga.Reintentando -> "Sin conexión. Seguirá solo cuando vuelva la señal."
    }

    fun porcentaje(estado: EstadoDescarga): Int = when (estado) {
        is EstadoDescarga.Descargando -> estado.porcentaje
        EstadoDescarga.Listo -> 100
        else -> 0
    }

    fun bytes(estado: EstadoDescarga): Long = when (estado) {
        is EstadoDescarga.Descargando -> estado.bytes
        is EstadoDescarga.EsperandoWifi -> estado.bytes
        else -> 0
    }
}
