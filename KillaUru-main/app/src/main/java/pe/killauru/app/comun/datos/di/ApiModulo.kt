package pe.killauru.app.comun.datos.di

import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton
import retrofit2.Retrofit
import pe.killauru.app.mapa.datos.MapaApi
import pe.killauru.app.mapa.datos.CdnApi
import pe.killauru.app.sesion.datos.IdentidadApi
import pe.killauru.app.sincronizacion.datos.IngestaApi

@Module
@InstallIn(SingletonComponent::class)
internal object ApiModulo {
    @Provides fun mapaApi(retrofit: Retrofit): MapaApi = retrofit.create(MapaApi::class.java)
    @Provides fun cdnApi(retrofit: Retrofit): CdnApi = retrofit.create(CdnApi::class.java)
    @Provides fun identidadApi(retrofit: Retrofit): IdentidadApi = retrofit.create(IdentidadApi::class.java)
    @Provides fun ingestaApi(retrofit: Retrofit): IngestaApi = retrofit.create(IngestaApi::class.java)
    @Provides fun mapeador() = pe.killauru.app.mapa.datos.MapeadorGeoJson
    @Provides fun descarga() = pe.killauru.app.mapa.dominio.PoliticaDescarga()
    @Provides fun activacion(@dagger.hilt.android.qualifiers.ApplicationContext ctx: android.content.Context) =
        pe.killauru.app.actualizador.dominio.PoliticaActivacionModelo(ctx.packageManager.getPackageInfo(ctx.packageName, 0).versionName.orEmpty())
}
