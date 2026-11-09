package pe.killauru.app.sesion.datos

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.preferencesDataStore
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Named
import javax.inject.Singleton
import org.json.JSONObject
import pe.killauru.app.comun.datos.almacen.CifradorKeystore
import pe.killauru.app.comun.datos.almacen.SerializadorCifrado
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.dominio.casos.AceptarVersionesUseCase
import pe.killauru.app.sesion.dominio.casos.RevisarVersionesLegalesUseCase

private val Context.sesionStore by preferencesDataStore("sesion")

@Module
@InstallIn(SingletonComponent::class)
internal object PreferenciasSesionModulo {
    @Provides @Singleton @Named("sesion")
    fun sesion(@ApplicationContext ctx: Context): DataStore<Preferences> = ctx.sesionStore

    @Provides @Singleton
    fun cifrador() = SerializadorCifrado(CifradorKeystore())

    @Provides
    fun aceptar(prefs: RepositorioPreferencias, ahora: () -> Long) = AceptarVersionesUseCase(prefs, ahora)

    @Provides
    fun revisar(@ApplicationContext ctx: Context, prefs: RepositorioPreferencias): RevisarVersionesLegalesUseCase {
        val legal = ctx.assets.open("legal/textos.json").bufferedReader().use { JSONObject(it.readText()) }
        val terminos = legal.getJSONObject("terminos")
        val politica = legal.getJSONObject("politica")
        return RevisarVersionesLegalesUseCase(prefs, terminos.getString("version"),
            politica.getString("version"), terminos.getString("texto"), politica.getString("texto"))
    }
}
