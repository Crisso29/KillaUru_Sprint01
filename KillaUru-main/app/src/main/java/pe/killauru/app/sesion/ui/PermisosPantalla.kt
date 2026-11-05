package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/** FR-006, FR-009, FR-008: pantalla de solicitud de permisos con explicación de lo que se pierde. */
@Composable
internal fun PermisosPantalla(
    tieneUbicacion: Boolean,
    tieneNotificaciones: Boolean,
    tieneBateria: Boolean,
    onSolicitarUbicacion: () -> Unit,
    onSolicitarNotificaciones: () -> Unit,
    onSolicitarBateria: () -> Unit,
    onContinuar: () -> Unit,
) {
    Surface(modifier = Modifier.fillMaxSize()) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
        ) {
            Text("Permisos necesarios", style = MaterialTheme.typography.headlineSmall)
            Spacer(Modifier.height(16.dp))

            PermisoFila(
                titulo = "Ubicación todo el tiempo",
                descripcion = if (tieneUbicacion) "Concedido"
                    else "Sin este permiso no podemos detectar baches ni mostrar tu posición en el mapa.",
                concedido = tieneUbicacion,
                onSolicitar = onSolicitarUbicacion,
            )
            Spacer(Modifier.height(12.dp))
            PermisoFila(
                titulo = "Notificaciones",
                descripcion = if (tieneNotificaciones) "Concedido"
                    else "Sin este permiso no recibirás alertas de anomalías próximas.",
                concedido = tieneNotificaciones,
                onSolicitar = onSolicitarNotificaciones,
            )
            Spacer(Modifier.height(12.dp))
            PermisoFila(
                titulo = "Sin restricción de batería",
                descripcion = if (tieneBateria) "Concedido"
                    else "Sin esto el sistema puede cerrar la app durante el recorrido.",
                concedido = tieneBateria,
                onSolicitar = onSolicitarBateria,
            )

            Spacer(Modifier.weight(1f))
            Button(onClick = onContinuar, modifier = Modifier.fillMaxWidth()) {
                Text("Continuar")
            }
        }
    }
}

@Composable
private fun PermisoFila(
    titulo: String,
    descripcion: String,
    concedido: Boolean,
    onSolicitar: () -> Unit,
) {
    Row(modifier = Modifier.fillMaxWidth()) {
        Column(modifier = Modifier.weight(1f)) {
            Text(titulo, style = MaterialTheme.typography.labelLarge)
            Text(descripcion, style = MaterialTheme.typography.bodySmall)
        }
        if (!concedido) {
            TextButton(onClick = onSolicitar) { Text("Conceder") }
        }
    }
}
