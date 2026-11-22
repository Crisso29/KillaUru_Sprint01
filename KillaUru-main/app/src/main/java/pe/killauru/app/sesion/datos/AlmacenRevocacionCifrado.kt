package pe.killauru.app.sesion.datos

import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import kotlinx.coroutines.flow.first
import pe.killauru.app.comun.datos.almacen.SerializadorCifrado
import javax.inject.Inject
import javax.inject.Named
import javax.inject.Singleton

/**
 * El token se guarda **cifrado** (AES-GCM del Keystore) en el DataStore de sesión, no en los datos de WorkManager,
 * que quedan en claro en su base. Una clave aparte: cerrar la sesión local no la borra.
 */
@Singleton
internal class AlmacenRevocacionCifrado @Inject constructor(
    @Named("sesion") private val ds: DataStore<Preferences>,
    private val cifrador: SerializadorCifrado,
) : AlmacenRevocacion {

    override suspend fun guardar(tokenRenovacion: String) {
        ds.edit { it[CLAVE] = cifrador.cifrar(tokenRenovacion) }
    }

    override suspend fun obtener(): String? = ds.data.first()[CLAVE]?.let(cifrador::descifrar)

    override suspend fun borrar() {
        ds.edit { it.remove(CLAVE) }
    }

    private companion object {
        val CLAVE = stringPreferencesKey("revocacion_pendiente")
    }
}
