package pe.killauru.app.deteccion.datos

import android.content.Context
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import java.time.Duration
import java.util.UUID
import org.json.JSONObject
import pe.killauru.app.comun.dominio.ParametrosApp
import pe.killauru.app.deteccion.dominio.*
import pe.killauru.app.seudonimo.datos.RepositorioSeudonimoDataStore
import pe.killauru.app.seudonimo.dominio.ObtenerSeudonimoUseCase
import pe.killauru.app.seudonimo.dominio.PoliticaRotacion

@Module
@InstallIn(SingletonComponent::class)
internal object DeteccionModulo {
    @Provides fun politica(p: ParametrosApp) = PoliticaDeteccion(p.categoriasQueDetectan)

    @Provides fun viaje(p: ParametrosApp) =
        DetectorViaje(Duration.ofMinutes(p.paradaFinViajeMinutos.toLong())) { UUID.randomUUID().toString() }

    @Provides fun seudonimo(repo: RepositorioSeudonimoDataStore, p: ParametrosApp, ahora: () -> Long) =
        ObtenerSeudonimoUseCase(repo, PoliticaRotacion(Duration.ofDays(p.rotacionSeudonimoDias.toLong())),
            { UUID.randomUUID().toString() }, ahora)

    @Provides fun guardar(repo: RepositorioEventos, p: ParametrosApp, fuentes: FuentesDeteccion) =
        GuardarEventoUseCase(repo, PoliticaGuardado(p.confidenceMinimo), fuentes, p.antiguedadMaximaFixMs.toLong())

    @Provides fun procesar(@ApplicationContext ctx: Context, p: ParametrosApp,
        guardar: GuardarEventoUseCase): ProcesarVentanaUseCase {
        val manifiesto = ctx.assets.open("modelo/manifiesto.json").bufferedReader().use { JSONObject(it.readText()) }
        return ProcesarVentanaUseCase(Prefiltro(manifiesto.getDouble("umbralPrefiltro")),
            clasificador(ctx, manifiesto), FusionVentanas(p.fusionVentanasMs.toLong()), guardar)
    }

    private fun clasificador(ctx: Context, m: JSONObject): Clasificador {
        val etiquetas = m.getJSONArray("etiquetas")
        return ClasificadorLiteRt(ctx.assets, "modelo/" + m.getString("archivo"),
            List(etiquetas.length()) { etiquetas.getString(it) }, m.getInt("severidades"),
            NormalizadorEntrada(m.getDouble("mediaAceleracion").toFloat(),
                m.getDouble("desviacionAceleracion").toFloat(), m.getInt("entradaLongitud")), m.getString("version"))
    }
}
