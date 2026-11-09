package pe.killauru.app.sesion.ui

import pe.killauru.app.comun.ui.MarcoFormulario

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp

/** FR-001, FR-008: pantalla de elección entre crear cuenta o continuar anónimamente. */
@Composable
internal fun CuentaONoPantalla(
    onCrearCuenta: () -> Unit,
    onContinuarSinCuenta: () -> Unit,
) {
    MarcoFormulario(centrado = true, alineado = true) {
            ExplicacionCuenta()
            Button(onClick = onCrearCuenta, modifier = Modifier.fillMaxWidth()) {
                Text("Crear cuenta")
            }
            Spacer(Modifier.height(12.dp))
            OutlinedButton(
                onClick = onContinuarSinCuenta,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text("Continuar sin cuenta")
            }
    }
}

@Composable
private fun ExplicacionCuenta() {
            Text(
                "¿Quieres crear una cuenta?",
                style = MaterialTheme.typography.headlineSmall,
                textAlign = TextAlign.Center,
            )
            Spacer(Modifier.height(16.dp))
            Text(
                "Con una cuenta puedes ver las anomalías que tú reportaste " +
                    "y recibir alertas de las que reportaron otros usuarios registrados.",
                style = MaterialTheme.typography.bodyMedium,
                textAlign = TextAlign.Center,
            )
            Spacer(Modifier.height(32.dp))
}
