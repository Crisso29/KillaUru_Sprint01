package pe.killauru.app

import androidx.compose.runtime.Composable
import pe.killauru.app.comun.ui.PantallasApp
import pe.killauru.app.comun.ui.TrasPrimerUso
import pe.killauru.app.historial.ui.HistorialPantalla
import pe.killauru.app.mapa.ui.DetallePantalla
import pe.killauru.app.mapa.ui.MapaPantalla
import pe.killauru.app.sesion.ui.AjustesPantalla
import pe.killauru.app.sesion.ui.CerrarSesionRuta
import pe.killauru.app.sesion.ui.EliminarCuentaRuta
import pe.killauru.app.sesion.ui.LoginRuta
import pe.killauru.app.sesion.ui.OlvideContrasenaRuta
import pe.killauru.app.sesion.ui.PerfilRuta
import pe.killauru.app.sesion.ui.PrimerUsoRuta
import pe.killauru.app.sesion.ui.RegistroRuta
import pe.killauru.app.sesion.ui.VerificarCorreoRuta

/** T024: raíz de composición de la interfaz; une cada destino del grafo con su pantalla real (ViewModels de Hilt). */
@Suppress("TooManyFunctions") // implementa PantallasApp: una función por destino
internal object PantallasReales : PantallasApp {
    @Composable override fun PrimerUso(alCompletar: (TrasPrimerUso) -> Unit) = PrimerUsoRuta(alCompletar)
    @Composable override fun Mapa(alVerDetalle: (String) -> Unit) = MapaPantalla(onDetalle = alVerDetalle)
    @Composable override fun Historial(alCrearCuenta: () -> Unit) = HistorialPantalla(alCrearCuenta)
    @Composable override fun Ajustes(alVerPerfil: () -> Unit, alCrearCuenta: () -> Unit, alIniciarSesion: () -> Unit) =
        AjustesPantalla(onVerPerfil = alVerPerfil, onCrearCuenta = alCrearCuenta, onIniciarSesion = alIniciarSesion)
    @Composable override fun Detalle(anomaliaId: String, alVolver: () -> Unit) = DetallePantalla(anomaliaId, alVolver)

    @Composable override fun Login(alEntrado: () -> Unit, alOlvide: () -> Unit, alVolver: () -> Unit) =
        LoginRuta(alEntrado, alOlvide, alVolver)
    @Composable override fun Registro(alCreada: (correo: String) -> Unit, alVolver: () -> Unit) = RegistroRuta(alCreada, alVolver)
    @Composable override fun VerificarCorreo(correo: String, alIrAlMapa: () -> Unit) = VerificarCorreoRuta(correo, alIrAlMapa)
    @Composable override fun OlvideContrasena(alVolverAlLogin: () -> Unit, alVolver: () -> Unit) =
        OlvideContrasenaRuta(alVolverAlLogin, alVolver)
    @Composable override fun Perfil(alCerrarSesion: () -> Unit, alEliminar: () -> Unit, alVolver: () -> Unit) =
        PerfilRuta(alCerrarSesion, alEliminar, alVolver)
    @Composable override fun CerrarSesion(alCerrada: () -> Unit, alCancelar: () -> Unit) = CerrarSesionRuta(alCerrada, alCancelar)
    @Composable override fun EliminarCuenta(alEliminada: () -> Unit, alCancelar: () -> Unit) =
        EliminarCuentaRuta(alEliminada, alCancelar)
}
