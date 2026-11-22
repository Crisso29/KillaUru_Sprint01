package pe.killauru.app.comun.ui

import androidx.compose.runtime.Composable

/**
 * T024: lo que `NavGrafo` dibuja en cada destino. El grafo solo decide **a dónde** ir; qué pantalla se muestra lo pone
 * quien lo usa (inversión de dependencias): la app pasa las reales con Hilt (`PantallasReales`) y la prueba, dobles.
 * Así `comun` no depende de ninguna funcionalidad.
 */
internal interface PantallasApp {
    @Composable fun PrimerUso(alCompletar: () -> Unit)
    @Composable fun Mapa(alVerDetalle: (String) -> Unit)
    @Composable fun Historial()
    @Composable fun Ajustes()
    @Composable fun Detalle(anomaliaId: String, alVolver: () -> Unit)
}
