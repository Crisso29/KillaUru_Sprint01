package pe.killauru.grabador

import android.content.Context
import android.location.Location
import android.location.LocationManager
import android.os.Looper
import androidx.core.location.LocationListenerCompat
import androidx.core.location.LocationManagerCompat
import androidx.core.location.LocationRequestCompat

/**
 * T018 / FR-001, RF-01 — captura GPS a 1 Hz con LocationManagerCompat.
 * No usa FusedLocationProvider (requiere Play Services); LocationManagerCompat
 * es la API de Jetpack compatible con API 26+.
 */
internal class GpsGrabacion(
    context: Context,
    private val onFix: (tNs: Long, utcMs: Long, lat: Double, lon: Double, precisionM: Float, velocidadMs: Float, rumbo: Float) -> Unit,
) {
    private val locationManager = context.getSystemService(Context.LOCATION_SERVICE) as LocationManager

    private val listener = LocationListenerCompat { location ->
        onFix(
            location.elapsedRealtimeNanos,
            location.time,
            location.latitude,
            location.longitude,
            location.accuracy,
            if (location.hasSpeed()) location.speed else 0f,
            if (location.hasBearing()) location.bearing else 0f,
        )
    }

    fun iniciar() {
        val request = LocationRequestCompat.Builder(1_000L).build()  // 1 Hz
        LocationManagerCompat.requestLocationUpdates(
            locationManager,
            LocationManager.GPS_PROVIDER,
            request,
            listener,
            Looper.getMainLooper(),
        )
    }

    fun detener() {
        LocationManagerCompat.removeUpdates(locationManager, listener)
    }
}
