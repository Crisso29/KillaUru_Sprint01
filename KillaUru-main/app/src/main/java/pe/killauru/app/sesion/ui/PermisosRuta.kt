package pe.killauru.app.sesion.ui

import android.Manifest.permission.POST_NOTIFICATIONS
import android.annotation.SuppressLint
import android.content.Intent
import android.os.Build
import android.provider.Settings
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts.RequestMultiplePermissions
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.platform.LocalContext
import androidx.core.net.toUri
import androidx.lifecycle.compose.LifecycleResumeEffect

/**
 * FR-006: une [PermisosPantalla] con los diálogos del sistema. Relee el estado al volver de cada diálogo o de los
 * ajustes de Android; si el usuario niega uno, puede continuar igual (el indicador del mapa explica qué se pierde).
 */
@Composable
internal fun PermisosRuta(alContinuar: () -> Unit) {
    val contexto = LocalContext.current
    var estado by remember { mutableStateOf(EstadoPermisos.leer(contexto)) }
    LifecycleResumeEffect(Unit) {
        estado = EstadoPermisos.leer(contexto)
        onPauseOrDispose { }
    }
    val pedir = rememberLauncherForActivityResult(RequestMultiplePermissions()) { estado = EstadoPermisos.leer(contexto) }
    PermisosPantalla(
        tieneUbicacion = estado.ubicacion,
        tieneNotificaciones = estado.notificaciones,
        tieneBateria = estado.bateria,
        onSolicitarUbicacion = { pedir.launch(estado.ubicacionPendiente) },
        onSolicitarNotificaciones = {
            // Antes de Android 13 no hay permiso que pedir (EstadoPermisos ya las da por concedidas).
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) pedir.launch(arrayOf(POST_NOTIFICATIONS))
        },
        onSolicitarBateria = { contexto.startActivity(intentExclusionBateria(contexto.packageName)) },
        onContinuar = alContinuar,
    )
}

/** La detección corre en segundo plano todo el viaje (FR-009): por eso se pide salir del ahorro de batería. */
@SuppressLint("BatteryLife")
private fun intentExclusionBateria(paquete: String) =
    Intent(Settings.ACTION_REQUEST_IGNORE_BATTERY_OPTIMIZATIONS, "package:$paquete".toUri())
