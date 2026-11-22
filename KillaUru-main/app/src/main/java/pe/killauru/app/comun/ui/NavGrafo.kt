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
 * pestañas con barra inferior; el detalle se abre encima del mapa, sin barra.
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
    composable(Rutas.PrimerUso.ruta) {
        pantallas.PrimerUso(alCompletar = {
            nav.navigate(Rutas.Mapa.ruta) { popUpTo(Rutas.PrimerUso.ruta) { inclusive = true } }
        })
    }
    composable(Rutas.Mapa.ruta) { pantallas.Mapa(alVerDetalle = { id -> nav.navigate(Rutas.Detalle.con(id)) }) }
    composable(Rutas.Historial.ruta) { pantallas.Historial() }
    composable(Rutas.Ajustes.ruta) { pantallas.Ajustes() }
    composable(
        Rutas.Detalle.ruta,
        arguments = listOf(navArgument(Rutas.ARG_ANOMALIA) { type = NavType.StringType }),
    ) { e ->
        pantallas.Detalle(e.arguments?.getString(Rutas.ARG_ANOMALIA).orEmpty(), alVolver = { nav.popBackStack() })
    }
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
