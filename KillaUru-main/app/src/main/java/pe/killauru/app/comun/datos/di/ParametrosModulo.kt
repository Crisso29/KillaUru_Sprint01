package pe.killauru.app.comun.datos.di

import android.content.Context
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton
import pe.killauru.app.comun.datos.CargadorParametros
import pe.killauru.app.comun.dominio.ParametrosApp

/** T024: carga parametros.json del APK una vez al arrancar; falla si no es válido. */
@Module
@InstallIn(SingletonComponent::class)
internal object ParametrosModulo {
    @Provides
    @Singleton
    fun proveerParametros(@ApplicationContext ctx: Context): ParametrosApp {
        val texto = ctx.assets.open("parametros.json").bufferedReader().readText()
        return CargadorParametros.cargar(texto)
            ?: error("parametros.json inválido — la detección no puede arrancar")
    }
}
