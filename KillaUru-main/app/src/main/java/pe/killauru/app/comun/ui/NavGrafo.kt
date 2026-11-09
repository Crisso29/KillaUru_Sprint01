package pe.killauru.app.comun.ui

import androidx.compose.runtime.Composable
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController

/**
 * T024 / FR-008: grafo de navegación completo; las pantallas se añaden fase a fase.
 * Punto de entrada: Bienvenida (primer uso) o Mapa (uso recurrente).
 */
@Composable
internal fun NavGrafo() {
    val navController = rememberNavController()
    NavHost(navController = navController, startDestination = Rutas.Bienvenida.ruta) {
        composable(Rutas.Bienvenida.ruta) {
            androidx.compose.material3.Text("KillaUru — Bienvenida")
        }
        composable(Rutas.Mapa.ruta) {
            androidx.compose.material3.Text("KillaUru — Mapa")
        }
        composable(Rutas.Ajustes.ruta) {
            androidx.compose.material3.Text("KillaUru — Ajustes")
        }
        composable(Rutas.Historial.ruta) {
            androidx.compose.material3.Text("KillaUru — Mis viajes")
        }
    }
}
