package pe.killauru.app.sesion.dominio

/** FR-063, RN-21: permiso local para alertas, ligado a la sesión vigente. */
internal data class PermisoLocal(val venceMs: Long) {
    private val graciaPorViajeMs = 30 * 60 * 1000L

    fun vigente(ahoraMs: Long, viajeActivo: Boolean = false): Boolean {
        if (ahoraMs <= venceMs) return true
        if (!viajeActivo) return false
        return (ahoraMs - venceMs) <= graciaPorViajeMs
    }
}
