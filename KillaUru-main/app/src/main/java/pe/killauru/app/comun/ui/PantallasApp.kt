package pe.killauru.app.comun.ui

import androidx.compose.runtime.Composable

/**
 * T024: lo que `NavGrafo` dibuja en cada destino. El grafo solo decide **a dónde** ir; qué pantalla se muestra lo pone
 * quien lo usa (inversión de dependencias): la app pasa las reales con Hilt (`PantallasReales`) y la prueba, dobles.
 * Así `comun` no depende de ninguna funcionalidad.
 */
@Suppress("TooManyFunctions") // una función por destino del grafo: dividirla escondería el mapa completo de la app
internal interface PantallasApp {
    @Composable fun PrimerUso(alCompletar: (TrasPrimerUso) -> Unit)
    @Composable fun Mapa(alVerDetalle: (String) -> Unit)
    @Composable fun Historial(alCrearCuenta: () -> Unit)
    @Composable fun Ajustes(alVerPerfil: () -> Unit, alCrearCuenta: () -> Unit, alIniciarSesion: () -> Unit)
    @Composable fun Detalle(anomaliaId: String, alVolver: () -> Unit)

    @Composable fun Login(alEntrado: () -> Unit, alOlvide: () -> Unit, alVolver: () -> Unit)
    @Composable fun Registro(alCreada: (correo: String) -> Unit, alVolver: () -> Unit)
    @Composable fun VerificarCorreo(correo: String, alIrAlMapa: () -> Unit)
    @Composable fun OlvideContrasena(alVolverAlLogin: () -> Unit, alVolver: () -> Unit)
    @Composable fun Perfil(alCerrarSesion: () -> Unit, alEliminar: () -> Unit, alVolver: () -> Unit)
    @Composable fun CerrarSesion(alCerrada: () -> Unit, alCancelar: () -> Unit)
    @Composable fun EliminarCuenta(alEliminada: () -> Unit, alCancelar: () -> Unit)
}
