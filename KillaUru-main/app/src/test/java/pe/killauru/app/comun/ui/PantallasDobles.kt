package pe.killauru.app.comun.ui

import androidx.compose.foundation.layout.Column
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable

/**
 * Dobles de [PantallasApp] para las pruebas del grafo: cada pantalla es un texto o un botón con un nombre único, así
 * las pruebas miran solo a dónde lleva cada acción, sin Hilt, GPS ni mapa.
 */
@Suppress("TooManyFunctions") // implementa PantallasApp: una función por destino
internal object PantallasDobles : PantallasApp {
    @Composable override fun PrimerUso(alCompletar: (TrasPrimerUso) -> Unit) = Column {
        Boton("terminar primer uso") { alCompletar(TrasPrimerUso.MAPA) }
        Boton("primer uso crear cuenta") { alCompletar(TrasPrimerUso.CREAR_CUENTA) }
        Boton("primer uso ya tengo cuenta") { alCompletar(TrasPrimerUso.INICIAR_SESION) }
    }
    @Composable override fun Mapa(alVerDetalle: (String) -> Unit) = Boton("pantalla mapa") { alVerDetalle("a-1") }
    @Composable override fun Historial(alCrearCuenta: () -> Unit) = Boton("pantalla historial", alCrearCuenta)
    @Composable override fun Ajustes(alVerPerfil: () -> Unit, alCrearCuenta: () -> Unit, alIniciarSesion: () -> Unit) =
        Column {
            Text("pantalla ajustes")
            Boton("ajustes perfil", alVerPerfil)
            Boton("ajustes iniciar sesion", alIniciarSesion)
        }
    @Composable override fun Detalle(anomaliaId: String, alVolver: () -> Unit) = Boton("detalle $anomaliaId", alVolver)
    @Composable override fun Login(alEntrado: () -> Unit, alOlvide: () -> Unit, alVolver: () -> Unit) = Column {
        Boton("login entrar", alEntrado)
        Boton("login olvide", alOlvide)
    }
    @Composable override fun Registro(alCreada: (String) -> Unit, alVolver: () -> Unit) =
        Boton("registro crear") { alCreada("conductor+1@ejemplo.pe") }
    @Composable override fun VerificarCorreo(correo: String, alIrAlMapa: () -> Unit) = Boton("verificar $correo", alIrAlMapa)
    @Composable override fun OlvideContrasena(alVolverAlLogin: () -> Unit, alVolver: () -> Unit) =
        Boton("olvide volver al login", alVolverAlLogin)
    @Composable override fun Perfil(alCerrarSesion: () -> Unit, alEliminar: () -> Unit, alVolver: () -> Unit) = Column {
        Boton("perfil cerrar sesion", alCerrarSesion)
        Boton("perfil eliminar", alEliminar)
    }
    @Composable override fun CerrarSesion(alCerrada: () -> Unit, alCancelar: () -> Unit) = Column {
        Boton("confirmar cierre", alCerrada)
        Boton("cancelar cierre", alCancelar)
    }
    @Composable override fun EliminarCuenta(alEliminada: () -> Unit, alCancelar: () -> Unit) = Boton("confirmar eliminar", alEliminada)
}

@Composable private fun Boton(texto: String, alPulsar: () -> Unit) = Button(onClick = alPulsar) { Text(texto) }
