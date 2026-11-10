package pe.killauru.app.mapa.dominio

import pe.killauru.app.comun.dominio.Coordenada

/**
 * FR-049, FR-051, R-035: avisa una sola vez por viaje cuando el usuario anónimo
 * sale del tramo demo. Los usuarios registrados nunca reciben aviso.
 */
internal class AvisoFueraDemo(private val zona: ZonaDemo) {

    // Verdadero si ya se mostró el aviso en el viaje actual
    private var yaAvisado = false

    /** Llama esto al comenzar cada viaje para que el aviso pueda dispararse de nuevo. */
    fun iniciarViaje() {
        yaAvisado = false
    }

    /**
     * Devuelve `true` solo si se cumplen todas las condiciones:
     * - El [alcance] es [Alcance.Anonimo] (registrado → siempre `false`)
     * - El [punto] está fuera de la zona demo
     * - Todavía no se ha avisado en el viaje actual
     */
    fun debeAvisar(punto: Coordenada, alcance: Alcance): Boolean {
        if (alcance == Alcance.Registrado) return false
        if (zona.contiene(punto)) return false
        if (yaAvisado) return false
        yaAvisado = true
        return true
    }
}
