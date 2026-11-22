package pe.killauru.app.sesion.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import pe.killauru.app.comun.ui.BotonPrincipal
import pe.killauru.app.comun.ui.IconosKillaUru
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.Paso
import pe.killauru.app.comun.ui.tema.KillaUruTema

/**
 * FR-006 (prototipo «Permisos», paso 3 de 5): para qué sirve cada permiso y qué se pierde sin él. Se puede continuar sin
 * todos; el indicador del mapa explica después lo que falta.
 */
@Composable
internal fun PermisosPantalla(
    tieneUbicacion: Boolean,
    tieneNotificaciones: Boolean,
    tieneBateria: Boolean,
    onSolicitarUbicacion: () -> Unit,
    onSolicitarNotificaciones: () -> Unit,
    onSolicitarBateria: () -> Unit,
    onContinuar: () -> Unit,
    alVolver: (() -> Unit)? = null,
) {
    PantallaApp(
        titulo = "Permisos",
        alVolver = alVolver,
        paso = Paso(3, PASOS_PRIMER_USO),
        pie = { BotonPrincipal("Continuar", onContinuar) },
    ) {
        Text("Para qué sirve cada uno. Puedes cambiarlos cuando quieras.", style = MaterialTheme.typography.bodyLarge,
            color = MaterialTheme.colorScheme.onSurfaceVariant)
        FilaPermiso("pin", "Ubicación precisa, todo el tiempo", "Para ubicar cada bache y detectar con la pantalla apagada.",
            "ves el mapa, pero no detecta ni avisa.", tieneUbicacion, onSolicitarUbicacion)
        FilaPermiso("bell", "Notificaciones", "Para el estado de la detección y las alertas con pantalla bloqueada.",
            "las alertas solo suenan con la app abierta.", tieneNotificaciones, onSolicitarNotificaciones)
        FilaPermiso("battery", "Sin restricción de batería", "Para que Android no apague la detección en viajes largos.",
            "la detección puede detenerse sola.", tieneBateria, onSolicitarBateria)
    }
}

/** Fila del prototipo (`.perm`): ícono, para qué sirve, «Sin él: …» y «Permitir» o «✓ Permitido». */
@Composable
private fun FilaPermiso(icono: String, titulo: String, paraQue: String, sinEl: String, concedido: Boolean, alPedir: () -> Unit) {
    val paleta = KillaUruTema.paleta
    Column {
        Row(Modifier.fillMaxWidth().padding(vertical = 13.dp), horizontalArrangement = Arrangement.spacedBy(13.dp)) {
            Box(Modifier.size(40.dp).background(paleta.killaSuave, RoundedCornerShape(10.dp)), contentAlignment = Alignment.Center) {
                Icon(IconosKillaUru.de(icono), contentDescription = null, tint = paleta.killa, modifier = Modifier.size(20.dp))
            }
            Column(Modifier.weight(1f)) {
                Text(titulo, fontSize = 16.sp, fontWeight = FontWeight.Bold)
                Text(paraQue, fontSize = 14.sp, color = MaterialTheme.colorScheme.onSurfaceVariant)
                Text("Sin él: $sinEl", fontSize = 13.sp, color = paleta.tinta3)
                if (concedido) {
                    Row(Modifier.padding(top = 8.dp), verticalAlignment = Alignment.CenterVertically) {
                        Icon(IconosKillaUru.de("check"), contentDescription = null, tint = paleta.ok, modifier = Modifier.size(16.dp))
                        Text(" Permitido", color = paleta.ok, fontWeight = FontWeight.Bold, fontSize = 14.sp)
                    }
                } else {
                    OutlinedButton(onClick = alPedir, modifier = Modifier.padding(top = 8.dp)) {
                        Text("Permitir", fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
                    }
                }
            }
        }
        HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
    }
}
