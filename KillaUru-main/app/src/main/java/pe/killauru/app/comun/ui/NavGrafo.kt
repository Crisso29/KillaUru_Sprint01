package pe.killauru.app.comun.ui

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument

/**
 * T024 / FR-008: grafo de navegación de la app. Siempre empieza en el primer uso, que pasa solo al mapa si ya estaba
 * completo; al completarlo se saca de la pila (volver desde el mapa cierra la app). Mapa, Mis viajes y Ajustes son
 * pestañas con barra inferior; el detalle y las pantallas de cuenta se abren encima, sin barra. Al terminar una
 * acción de cuenta (entrar, verificar, cerrar sesión, eliminar) se vuelve al mapa sin dejar formularios en la pila.
 */
@Composable
internal fun NavGrafo(pantallas: PantallasApp, navController: NavHostController = rememberNavController()) {
    val entrada by navController.currentBackStackEntryAsState()
    val rutaActual = entrada?.destination?.route
    Scaffold(
        bottomBar = {
            if (rutaActual in PESTANAS) {
                BarraInferior(
                    rutaActual = rutaActual.orEmpty(),
                    onMapa = { navController.irAPestana(Rutas.Mapa) },
                    onHistorial = { navController.irAPestana(Rutas.Historial) },
                    onAjustes = { navController.irAPestana(Rutas.Ajustes) },
                )
            }
        },
    ) { relleno ->
        NavHost(navController, startDestination = Rutas.PrimerUso.ruta, modifier = Modifier.padding(relleno)) {
            destinos(pantallas, navController)
        }
    }
}

/**
 * Destinos con barra inferior (FR-008: cada acción principal en ≤ 3 toques). Va fuera de `Rutas` a propósito: dentro
 * de su `companion` formaba un ciclo de inicialización (Rutas → Mapa → Rutas) y la JVM podía dejarlo en null.
 */
private val PESTANAS = setOf(Rutas.Mapa.ruta, Rutas.Historial.ruta, Rutas.Ajustes.ruta)

private fun NavGraphBuilder.destinos(pantallas: PantallasApp, nav: NavHostController) {
    composable(Rutas.PrimerUso.ruta) { pantallas.PrimerUso(alCompletar = nav::trasPrimerUso) }
    destinosPestanas(pantallas, nav)
    destinosAcceso(pantallas, nav)
    destinosPerfil(pantallas, nav)
}

/** Mapa, Mis viajes y Ajustes, y el detalle de una anomalía encima del mapa. */
private fun NavGraphBuilder.destinosPestanas(pantallas: PantallasApp, nav: NavHostController) {
    composable(Rutas.Mapa.ruta) { pantallas.Mapa(alVerDetalle = { id -> nav.navigate(Rutas.Detalle.con(id)) }) }
    composable(Rutas.Historial.ruta) { pantallas.Historial(alCrearCuenta = { nav.navigate(Rutas.Registro.ruta) }) }
    composable(Rutas.Ajustes.ruta) {
        pantallas.Ajustes(
            alVerPerfil = { nav.navigate(Rutas.Perfil.ruta) },
            alCrearCuenta = { nav.navigate(Rutas.Registro.ruta) },
            alIniciarSesion = { nav.navigate(Rutas.Login.ruta) },
        )
    }
    composable(Rutas.Detalle.ruta, arguments = listOf(navArgument(Rutas.ARG_ANOMALIA) { type = NavType.StringType })) { e ->
        pantallas.Detalle(e.arguments?.getString(Rutas.ARG_ANOMALIA).orEmpty(), alVolver = { nav.popBackStack() })
    }
}

/** FR-060, FR-061, FR-064: crear cuenta, revisar el correo, iniciar sesión y recuperar la contraseña. */
private fun NavGraphBuilder.destinosAcceso(pantallas: PantallasApp, nav: NavHostController) {
    val atras: () -> Unit = { nav.popBackStack() }
    composable(Rutas.Login.ruta) {
        pantallas.Login(alEntrado = nav::volverAlMapa, alOlvide = { nav.navigate(Rutas.OlvideContrasena.ruta) }, alVolver = atras)
    }
    composable(Rutas.Registro.ruta) { pantallas.Registro(alCreada = nav::irAVerificar, alVolver = atras) }
    composable(Rutas.VerificarCorreo.ruta, arguments = listOf(navArgument(Rutas.ARG_CORREO) { type = NavType.StringType })) { e ->
        pantallas.VerificarCorreo(e.arguments?.getString(Rutas.ARG_CORREO).orEmpty(), alIrAlMapa = nav::volverAlMapa)
    }
    composable(Rutas.OlvideContrasena.ruta) { pantallas.OlvideContrasena(alVolverAlLogin = atras, alVolver = atras) }
}

/** FR-066…FR-069: mi perfil, cerrar sesión y eliminar la cuenta. */
private fun NavGraphBuilder.destinosPerfil(pantallas: PantallasApp, nav: NavHostController) {
    val atras: () -> Unit = { nav.popBackStack() }
    composable(Rutas.Perfil.ruta) {
        pantallas.Perfil(
            alCerrarSesion = { nav.navigate(Rutas.CerrarSesion.ruta) },
            alEliminar = { nav.navigate(Rutas.EliminarCuenta.ruta) },
            alVolver = atras,
        )
    }
    composable(Rutas.CerrarSesion.ruta) { pantallas.CerrarSesion(alCerrada = nav::volverAlMapa, alCancelar = atras) }
    composable(Rutas.EliminarCuenta.ruta) { pantallas.EliminarCuenta(alEliminada = nav::volverAlMapa, alCancelar = atras) }
}

/**
 * Al completar el primer uso se saca de la pila (volver desde el mapa cierra la app). Si eligió crear cuenta o entrar,
 * esa pantalla se abre sobre el mapa: atrás lleva al mapa, no a la bienvenida.
 */
private fun NavHostController.trasPrimerUso(tras: TrasPrimerUso) {
    navigate(Rutas.Mapa.ruta) { popUpTo(Rutas.PrimerUso.ruta) { inclusive = true } }
    when (tras) {
        TrasPrimerUso.MAPA -> Unit
        TrasPrimerUso.CREAR_CUENTA -> navigate(Rutas.Registro.ruta)
        TrasPrimerUso.INICIAR_SESION -> navigate(Rutas.Login.ruta)
    }
}

/** Vuelve al mapa (la raíz tras el primer uso) y quita de la pila los formularios que había encima. */
private fun NavHostController.volverAlMapa() {
    popBackStack(Rutas.Mapa.ruta, inclusive = false)
}

/** Creada la cuenta, el registro se reemplaza por «Revisa tu correo»: atrás no vuelve a un formulario ya enviado. */
private fun NavHostController.irAVerificar(correo: String) = navigate(Rutas.VerificarCorreo.con(correo)) {
    popUpTo(Rutas.Registro.ruta) { inclusive = true }
}

/**
 * Cambio de pestaña estándar: una sola copia de cada pestaña en la pila, sobre el mapa (la raíz tras el primer uso),
 * guardando y restaurando el estado de cada una.
 */
private fun NavHostController.irAPestana(destino: Rutas) = navigate(destino.ruta) {
    popUpTo(Rutas.Mapa.ruta) { saveState = true }
    launchSingleTop = true
    restoreState = true
}
