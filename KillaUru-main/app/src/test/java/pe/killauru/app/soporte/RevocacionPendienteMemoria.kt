package pe.killauru.app.soporte

import pe.killauru.app.sesion.dominio.RevocacionPendiente

/** Doble de [RevocacionPendiente]: recuerda el token que se dejó para revocar cuando haya red. */
internal class RevocacionPendienteMemoria : RevocacionPendiente {
    var programada: String? = null

    override suspend fun programar(tokenRenovacion: String) { programada = tokenRenovacion }
}
