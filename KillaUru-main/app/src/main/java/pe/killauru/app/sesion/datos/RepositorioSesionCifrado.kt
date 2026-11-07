package pe.killauru.app.sesion.datos

import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import kotlinx.coroutines.flow.first
import pe.killauru.app.sesion.dominio.PermisoLocal
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.Sesion
import javax.inject.Inject
import javax.inject.Named
import javax.inject.Singleton

/** FR-063, R-NF-10: repositorio de sesión usando DataStore cifrado. */
@Singleton
internal class RepositorioSesionCifrado @Inject constructor(
    @Named("sesion") private val ds: DataStore<Preferences>,
) : RepositorioSesion {

    companion object {
        val K_ID = stringPreferencesKey("sesion_id")
        val K_TOKEN = stringPreferencesKey("sesion_token")
        val K_EXPIRA = longPreferencesKey("sesion_expira")
        val K_PERMISO = longPreferencesKey("sesion_permiso")
    }

    override suspend fun obtener(): Sesion? {
        val prefs = ds.data.first()
        val id = prefs[K_ID] ?: return null
        val token = prefs[K_TOKEN] ?: return null
        val expira = prefs[K_EXPIRA] ?: return null
        val permiso = prefs[K_PERMISO]?.let { PermisoLocal(it) }
        return Sesion(id, token, expira, permiso)
    }

    override suspend fun guardar(sesion: Sesion) {
        ds.edit { p ->
            p[K_ID] = sesion.usuarioId
            p[K_TOKEN] = sesion.token
            p[K_EXPIRA] = sesion.tokenExpiraMs
            sesion.permiso?.let { p[K_PERMISO] = it.venceMs }
        }
    }

    override suspend fun cerrar() {
        ds.edit { p ->
            p.remove(K_ID)
            p.remove(K_TOKEN)
            p.remove(K_EXPIRA)
            p.remove(K_PERMISO)
        }
    }
}
