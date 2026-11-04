package pe.killauru.app.alertas.dominio

/** FR-057: contrato de reproducción de alerta sonora. */
internal interface AlertaSonora {
    fun reproducir(alerta: Alerta)
    fun detener()
}
