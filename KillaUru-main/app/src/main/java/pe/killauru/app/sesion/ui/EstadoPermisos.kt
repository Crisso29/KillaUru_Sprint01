package pe.killauru.app.sesion.ui

import android.Manifest.permission.ACCESS_BACKGROUND_LOCATION
import android.Manifest.permission.ACCESS_FINE_LOCATION
import android.Manifest.permission.POST_NOTIFICATIONS
import android.content.Context
import android.content.pm.PackageManager
import android.os.Build
import android.os.PowerManager
import androidx.core.content.ContextCompat

/** FR-006: qué permisos tiene hoy la app, leído del sistema (se relee al volver de cada diálogo). */
internal data class EstadoPermisos(
    val tienePrecisa: Boolean,
    val tieneFondo: Boolean,
    val notificaciones: Boolean,
    val bateria: Boolean,
) {
    val ubicacion get() = PermisosUbicacion.concedida(tienePrecisa, tieneFondo, Build.VERSION.SDK_INT)
    val ubicacionPendiente get() = PermisosUbicacion.siguientes(tienePrecisa, tieneFondo, Build.VERSION.SDK_INT)

    companion object {
        fun leer(contexto: Context): EstadoPermisos {
            fun tiene(permiso: String) =
                ContextCompat.checkSelfPermission(contexto, permiso) == PackageManager.PERMISSION_GRANTED
            val energia = contexto.getSystemService(PowerManager::class.java)
            return EstadoPermisos(
                tienePrecisa = tiene(ACCESS_FINE_LOCATION),
                // Antes de Android 10 no existe «todo el tiempo»: la precisa ya incluye el segundo plano.
                tieneFondo = Build.VERSION.SDK_INT < Build.VERSION_CODES.Q || tiene(ACCESS_BACKGROUND_LOCATION),
                // Antes de Android 13 las notificaciones no requieren permiso.
                notificaciones = Build.VERSION.SDK_INT < Build.VERSION_CODES.TIRAMISU || tiene(POST_NOTIFICATIONS),
                bateria = energia?.isIgnoringBatteryOptimizations(contexto.packageName) == true,
            )
        }
    }
}
