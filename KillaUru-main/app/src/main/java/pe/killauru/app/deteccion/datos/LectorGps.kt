package pe.killauru.app.deteccion.datos

import android.content.Context
import android.location.Location
import android.location.LocationManager
import androidx.core.location.LocationListenerCompat
import androidx.core.location.LocationManagerCompat
import androidx.core.location.LocationRequestCompat
import java.util.concurrent.Executors

/** FR-009, FR-010, R-020: lee GPS a 1 Hz (0.1 Hz cuando no hay viaje). */
internal class LectorGps(
    context: Context,
    private val onFix: (Location) -> Unit,
) {
    private val locationManager = context.getSystemService(LocationManager::class.java)
    private val executor = Executors.newSingleThreadExecutor()

    private val listener = LocationListenerCompat { onFix(it) }

    fun iniciar(enViaje: Boolean) {
        val intervaloMs = if (enViaje) 1_000L else 10_000L
        try {
            LocationManagerCompat.requestLocationUpdates(
                locationManager,
                LocationManager.GPS_PROVIDER,
                LocationRequestCompat.Builder(intervaloMs).build(),
                executor, listener,
            )
        } catch (_: SecurityException) {}
    }

    fun detener() {
        try {
            LocationManagerCompat.removeUpdates(locationManager, listener)
        } catch (_: SecurityException) {
            // El permiso pudo ser revocado mientras el servicio estaba activo.
        } finally {
            executor.shutdown()
        }
    }
}
