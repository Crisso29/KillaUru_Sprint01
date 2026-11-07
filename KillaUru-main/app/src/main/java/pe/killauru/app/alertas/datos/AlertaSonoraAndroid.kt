package pe.killauru.app.alertas.datos

import android.content.Context
import android.media.AudioAttributes
import android.media.SoundPool
import pe.killauru.app.alertas.dominio.Alerta
import pe.killauru.app.alertas.dominio.AlertaSonora
import javax.inject.Inject

/** FR-057: reproduce TTS/sonido al detectar proximidad a anomalía. */
internal class AlertaSonoraAndroid @Inject constructor(
    private val context: Context,
) : AlertaSonora {

    private var soundPool: SoundPool? = null
    private var sonidoId: Int = 0

    private fun pool(): SoundPool {
        return soundPool ?: SoundPool.Builder()
            .setMaxStreams(2)
            .setAudioAttributes(
                AudioAttributes.Builder()
                    .setUsage(AudioAttributes.USAGE_ASSISTANCE_NAVIGATION_GUIDANCE)
                    .build()
            )
            .build()
            .also { soundPool = it }
    }

    override fun reproducir(alerta: Alerta) {
        pool().play(sonidoId, 1f, 1f, 1, 0, 1f)
    }

    override fun detener() {
        soundPool?.autoPause()
    }
}
