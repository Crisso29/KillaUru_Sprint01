package pe.killauru.grabador

import android.content.Intent
import android.os.Bundle
import android.view.View
import androidx.appcompat.app.AppCompatActivity

/**
 * T019 / FR-002, FR-004 — pantalla principal: inicia y detiene solo con velocidad GPS < 1 km/h.
 * El botón «Marca» es grande para el copiloto; el conductor no toca el celular en marcha (R-003).
 */
internal class MainActivity : AppCompatActivity() {

    private var velocidadActualKmh = 0f
    private var grabando = false

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        // La UI real (PantallaGrabador) se monta aquí; pendiente T019 completo con Compose/Views
        setContentView(android.R.layout.simple_list_item_1)
    }

    /** Inicia la grabación solo si el vehículo está detenido (FR-002). */
    fun onIniciarClick(view: View) {
        if (velocidadActualKmh >= 1.0f) return
        startForegroundService(Intent(this, ServicioGrabacion::class.java))
        grabando = true
    }

    /** Detiene la grabación solo si el vehículo está detenido (FR-002). */
    fun onDetenerClick(view: View) {
        if (velocidadActualKmh >= 1.0f) return
        stopService(Intent(this, ServicioGrabacion::class.java))
        grabando = false
    }

    /** Marca un evento (copiloto) — disponible en marcha (R-003). */
    fun onMarcaClick(view: View) {
        // Enviar intent al servicio para registrar la marca
        // La implementación detallada es parte del T019 completo (UI con ViewBinding)
    }

    /** Actualiza la velocidad desde el GPS para el guard de inicio/stop. */
    fun actualizarVelocidad(kmh: Float) {
        velocidadActualKmh = kmh
    }
}
