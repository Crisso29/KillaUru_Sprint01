package pe.killauru.app.soporte

import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.Sesion

internal class SesionMemoria(var actual: Sesion? = null) : RepositorioSesion {
    override suspend fun obtener() = actual
    override suspend fun guardar(sesion: Sesion) { actual = sesion }
    override suspend fun cerrar() { actual = null }
}
