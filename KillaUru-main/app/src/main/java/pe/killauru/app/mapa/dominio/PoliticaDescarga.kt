package pe.killauru.app.mapa.dominio

import pe.killauru.app.sesion.dominio.RepositorioPreferencias

/** FR-043, FR-044: decide si se debe descargar una nueva versión del mapa. */
internal class PoliticaDescarga {
    fun debeDescargar(
        versionLocal: String?,
        versionRemota: String,
        usarDatosMoviles: Boolean,
        hayWifi: Boolean,
    ): Boolean {
        if (versionLocal == versionRemota) return false
        return hayWifi || usarDatosMoviles
    }
}
