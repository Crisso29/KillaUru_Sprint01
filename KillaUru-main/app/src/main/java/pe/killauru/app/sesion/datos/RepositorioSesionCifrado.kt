package pe.killauru.app.sesion.datos

import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.longPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import kotlinx.coroutines.flow.first
import pe.killauru.app.comun.datos.almacen.SerializadorCifrado
import pe.killauru.app.sesion.dominio.PermisoLocal
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.Rol
import pe.killauru.app.sesion.dominio.Sesion
import javax.inject.Inject
import javax.inject.Named
import javax.inject.Singleton

/**
 * FR-062: guarda la sesión en DataStore; **ambos tokens** se cifran con la clave AES-GCM del Keystore.
 * Si falta cualquier campo (p. ej. una sesión del formato anterior) no hay sesión: obliga a iniciarla de nuevo.
 */
@Singleton
internal class RepositorioSesionCifrado @Inject constructor(
    @Named("sesion") private val ds: DataStore<Preferences>,
    private val cifrador: SerializadorCifrado,
) : RepositorioSesion {

    private companion object {
        val K_ACCESO = stringPreferencesKey("sesion_token_acceso")
        val K_EXPIRA_ACCESO = longPreferencesKey("sesion_expira_acceso")
        val K_RENOVACION = stringPreferencesKey("sesion_token_renovacion")
        val K_PERMISO = longPreferencesKey("sesion_permiso_vence")
        val K_ROL = stringPreferencesKey("sesion_rol")
        val CLAVES = listOf(K_ACCESO, K_EXPIRA_ACCESO, K_RENOVACION, K_PERMISO, K_ROL)
    }

    override suspend fun obtener(): Sesion? {
        val p = ds.data.first()
        return Sesion(
            tokenAcceso = cifrador.descifrar(p[K_ACCESO] ?: return null),
            expiraAccesoMs = p[K_EXPIRA_ACCESO] ?: return null,
            tokenRenovacion = cifrador.descifrar(p[K_RENOVACION] ?: return null),
            permiso = PermisoLocal(p[K_PERMISO] ?: return null),
            rol = Rol.valueOf(p[K_ROL] ?: return null),
        )
    }

    override suspend fun guardar(sesion: Sesion) {
        ds.edit { p ->
            p[K_ACCESO] = cifrador.cifrar(sesion.tokenAcceso)
            p[K_EXPIRA_ACCESO] = sesion.expiraAccesoMs
            p[K_RENOVACION] = cifrador.cifrar(sesion.tokenRenovacion)
            p[K_PERMISO] = sesion.permiso.venceMs
            p[K_ROL] = sesion.rol.name
        }
    }

    override suspend fun cerrar() {
        ds.edit { p -> CLAVES.forEach { p.remove(it) } }
    }
}
