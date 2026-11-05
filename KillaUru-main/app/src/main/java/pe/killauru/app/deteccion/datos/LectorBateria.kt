package pe.killauru.app.deteccion.datos

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.os.BatteryManager
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import pe.killauru.app.deteccion.dominio.PoliticaBateria

/** FR-017, RN-22: escucha cambios de batería vía ACTION_BATTERY_CHANGED. */
internal class LectorBateria(context: Context) {

    private val _estado = MutableStateFlow(false)
    val enAhorro: StateFlow<Boolean> = _estado

    private val politica = PoliticaBateria(porcentajeMinimo = 15)

    private val receptor = object : BroadcastReceiver() {
        override fun onReceive(ctx: Context?, intent: Intent?) {
            if (intent == null) return
            val nivel = intent.getIntExtra(BatteryManager.EXTRA_LEVEL, -1)
            val escala = intent.getIntExtra(BatteryManager.EXTRA_SCALE, 100)
            val cargando = intent.getIntExtra(BatteryManager.EXTRA_STATUS, -1) in
                listOf(BatteryManager.BATTERY_STATUS_CHARGING, BatteryManager.BATTERY_STATUS_FULL)
            val porcentaje = if (escala > 0) (nivel * 100 / escala) else 0
            _estado.value = politica.enAhorro(porcentaje, cargando)
        }
    }

    fun iniciar() {
        context.registerReceiver(receptor, IntentFilter(Intent.ACTION_BATTERY_CHANGED))
    }

    fun detener() {
        runCatching { context.unregisterReceiver(receptor) }
    }
}
