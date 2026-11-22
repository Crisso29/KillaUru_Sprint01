package pe.killauru.app

import androidx.compose.runtime.Composable
import pe.killauru.app.comun.ui.PantallasApp
import pe.killauru.app.historial.ui.HistorialPantalla
import pe.killauru.app.mapa.ui.DetallePantalla
import pe.killauru.app.mapa.ui.MapaPantalla
import pe.killauru.app.sesion.ui.AjustesPantalla
import pe.killauru.app.sesion.ui.PrimerUsoRuta

/** T024: raíz de composición de la interfaz; une cada destino del grafo con su pantalla real (ViewModels de Hilt). */
internal object PantallasReales : PantallasApp {
    @Composable override fun PrimerUso(alCompletar: () -> Unit) = PrimerUsoRuta(alCompletar)
    @Composable override fun Mapa(alVerDetalle: (String) -> Unit) = MapaPantalla(onDetalle = alVerDetalle)
    @Composable override fun Historial() = HistorialPantalla()
    @Composable override fun Ajustes() = AjustesPantalla()
    @Composable override fun Detalle(anomaliaId: String, alVolver: () -> Unit) = DetallePantalla(anomaliaId, alVolver)
}
