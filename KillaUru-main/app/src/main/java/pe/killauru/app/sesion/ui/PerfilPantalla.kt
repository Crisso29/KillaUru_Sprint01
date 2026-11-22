package pe.killauru.app.sesion.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import pe.killauru.app.comun.ui.Aviso
import pe.killauru.app.comun.ui.BotonSecundario
import pe.killauru.app.comun.ui.PantallaApp
import pe.killauru.app.comun.ui.TonoAviso
import pe.killauru.app.comun.ui.textoConNegritas
import pe.killauru.app.comun.ui.tema.KillaUruTema
import pe.killauru.app.sesion.dominio.DocumentoLegal
import pe.killauru.app.sesion.dominio.MisDatos
import pe.killauru.app.sesion.dominio.Rol

/**
 * FR-068, HU-24 (prototipo «Mi perfil»): todo lo que KillaUru guarda de la cuenta, tal como lo devuelve el servidor.
 * Recuerda que los viajes y las detecciones no están unidos a la cuenta (RNF-10).
 */
@Composable
internal fun PerfilPantalla(
    estado: PerfilUiState,
    alReintentar: () -> Unit,
    alCerrarSesion: () -> Unit,
    alEliminar: () -> Unit,
    alVolver: (() -> Unit)? = null,
) {
    PantallaApp(titulo = "Mi perfil", alVolver = alVolver) {
        val datos = estado.datos
        when {
            estado.cargando -> CircularProgressIndicator()
            datos == null -> {
                Aviso("warn", TonoAviso.AVISO, estado.error ?: "No se pudieron cargar tus datos.")
                TextButton(onClick = alReintentar) { Text("Reintentar", fontWeight = FontWeight.Bold) }
            }
            else -> DatosCuenta(datos)
        }
        Aviso("shield", TonoAviso.KILLA) {
            Text(textoConNegritas("Tus viajes y lo que detectas **no** están unidos a tu cuenta. Por eso no aparecen aquí."))
        }
        BotonSecundario("Cerrar sesión", alCerrarSesion)
        TextButton(onClick = alEliminar, modifier = Modifier.fillMaxWidth()) {
            Text("Eliminar mi cuenta", fontWeight = FontWeight.Bold, color = KillaUruTema.paleta.mal)
        }
    }
}

@Composable
private fun DatosCuenta(datos: MisDatos) = Column {
    Text("Esto es todo lo que KillaUru guarda sobre ti.", style = MaterialTheme.typography.bodyLarge,
        color = MaterialTheme.colorScheme.onSurfaceVariant, modifier = Modifier.padding(bottom = 8.dp))
    FilaDato("Correo", datos.correo)
    FilaDato("Tipo de cuenta", if (datos.rol == Rol.ConductorRegistrado) "Conductor registrado" else "Administración")
    FilaDato("Cuenta creada", FechaCorta.deUtcMs(datos.fechaAltaUtcMs))
    FilaDato("Política de privacidad", textoAceptado(datos, DocumentoLegal.PoliticaPrivacidad))
    FilaDato("Términos de uso", textoAceptado(datos, DocumentoLegal.TerminosUso))
    FilaDato("Correo verificado", if (datos.correoVerificado) "Sí" else "No, revisa tu correo",
        color = if (datos.correoVerificado) KillaUruTema.paleta.ok else KillaUruTema.paleta.aviso)
}

/** «v1.2 · 14 oct 2026», o «—» si no hay aceptación guardada. */
internal fun textoAceptado(datos: MisDatos, documento: DocumentoLegal): String =
    datos.aceptacion(documento)?.let { "v${it.version} · ${FechaCorta.deUtcMs(it.aceptadoUtcMs)}" } ?: "—"

/** Fila etiqueta–valor del prototipo (`.kv`); TalkBack la lee entera: «Correo, conductor@ejemplo.pe». */
@Composable
private fun FilaDato(etiqueta: String, valor: String, color: Color = Color.Unspecified) {
    Row(Modifier.fillMaxWidth().padding(vertical = 10.dp).semantics(mergeDescendants = true) {}) {
        Text(etiqueta, color = MaterialTheme.colorScheme.onSurfaceVariant, modifier = Modifier.weight(1f))
        Text(valor, fontWeight = FontWeight.SemiBold, color = color)
    }
    HorizontalDivider(color = MaterialTheme.colorScheme.outlineVariant)
}

/** Une [PerfilPantalla] con su ViewModel. */
@Composable
internal fun PerfilRuta(alCerrarSesion: () -> Unit, alEliminar: () -> Unit, alVolver: () -> Unit, vm: PerfilViewModel = hiltViewModel()) {
    val estado by vm.estado.collectAsState()
    PerfilPantalla(estado, vm::cargar, alCerrarSesion, alEliminar, alVolver)
}
