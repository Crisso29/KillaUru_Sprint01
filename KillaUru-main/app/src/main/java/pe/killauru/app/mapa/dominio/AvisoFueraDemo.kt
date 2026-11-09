package pe.killauru.app.mapa.dominio

import pe.killauru.app.comun.dominio.Coordenada

/** FR-051: avisa al usuario cuando está fuera de la zona demo (US8 offline). */
internal class AvisoFueraDemo(private val zona: ZonaDemo) {
    fun estaFuera(punto: Coordenada): Boolean = !zona.contiene(punto)
}
