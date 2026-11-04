package pe.killauru.app.sesion.datos

import androidx.datastore.preferences.core.stringPreferencesKey
import kotlinx.coroutines.flow.first
import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import pe.killauru.app.comun.datos.almacen.PreferenciasDataStore
import pe.killauru.app.comun.datos.almacen.SerializadorCifrado
import pe.killauru.app.sesion.dominio.RepositorioSesion
import pe.killauru.app.sesion.dominio.Sesion
import javax.inject.Inject
import javax.inject.Singleton

/** FR-062, R-022: sesión cifrada con AES-256-GCM vía Keystore. */
@Singleton
internal class RepositorioSesionCifrado @Inject constructor(
    private val prefs: PreferenciasDataStore,
    private val cifrador: SerializadorCifrado,
) : RepositorioSesion {

    private val CLAVE = stringPreferencesKey("sesion_cifrada")

    override suspend fun guardar(sesion: Sesion) {
        val json = Json.encodeToString(SesionDto.de(sesion))
        prefs.guardar(CLAVE, cifrador.cifrar(json))
    }

    override suspend fun obtener(): Sesion? {
        val cifrado = prefs.leer(CLAVE).first() ?: return null
        return try {
            SesionDto.aSesion(Json.decodeFromString(cifrador.descifrar(cifrado)))
        } catch (_: Exception) { null }
    }

    override suspend fun borrar() = prefs.borrar(CLAVE)

    override suspend fun actualizarTokens(acceso: String, refresco: String, venceUtc: Long) {
        val sesion = obtener() ?: return
        guardar(sesion.copy(tokenAcceso = acceso, tokenRefresco = refresco, venceUtc = venceUtc))
    }
}
