package pe.killauru.app.mapa.ui

import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.viewinterop.AndroidView
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.LifecycleEventObserver
import androidx.lifecycle.compose.LocalLifecycleOwner
import com.mapbox.geojson.Feature
import com.mapbox.geojson.FeatureCollection
import com.mapbox.geojson.Point
import org.maplibre.android.MapLibre
import org.maplibre.android.maps.MapView
import org.maplibre.android.style.layers.CircleLayer
import org.maplibre.android.style.layers.PropertyFactory.circleColor
import org.maplibre.android.style.layers.PropertyFactory.circleRadius
import org.maplibre.android.style.sources.GeoJsonSource
import pe.killauru.app.mapa.dominio.AnomaliaLocal

// TODO(producción): reemplazar ESTILO_URL por la ruta pmtiles:// del archivo
// descargado por DescargarMapaWorker via AlmacenArchivos.rutaMapa() — T096
private const val ESTILO_URL = "https://demotiles.maplibre.org/style.json"
private const val FUENTE_ID = "anomalias"
private const val CAPA_ID = "anomalias-puntos"

/** FR-040, FR-049: mapa real con MapLibre y marcadores de anomalías vigentes. */
@Composable
internal fun MapaPantalla(
    viewModel: MapaViewModel = hiltViewModel(),
    onDetalle: (String) -> Unit = {},
) {
    val anomalias by viewModel.anomalias.collectAsState()
    val context = LocalContext.current
    val lifecycle = LocalLifecycleOwner.current.lifecycle

    MapLibre.getInstance(context)
    val mapView = remember { MapView(context) }

    DisposableEffect(lifecycle) {
        val observer = LifecycleEventObserver { _, event ->
            when (event) {
                Lifecycle.Event.ON_START -> mapView.onStart()
                Lifecycle.Event.ON_RESUME -> mapView.onResume()
                Lifecycle.Event.ON_PAUSE -> mapView.onPause()
                Lifecycle.Event.ON_STOP -> mapView.onStop()
                Lifecycle.Event.ON_DESTROY -> mapView.onDestroy()
                else -> Unit
            }
        }
        lifecycle.addObserver(observer)
        onDispose { lifecycle.removeObserver(observer); mapView.onDestroy() }
    }

    AndroidView(
        factory = { mapView },
        modifier = Modifier.fillMaxSize(),
        update = { mv ->
            mv.getMapAsync { map ->
                map.setStyle(ESTILO_URL) { style ->
                    actualizarAnomalias(style, anomalias)
                }
            }
        },
    )
}

private fun actualizarAnomalias(
    style: org.maplibre.android.maps.Style,
    anomalias: List<AnomaliaLocal>,
) {
    val features = anomalias.map { a ->
        Feature.fromGeometry(Point.fromLngLat(a.coordenada.longitud, a.coordenada.latitud))
            .apply {
                addStringProperty("id", a.id)
                addNumberProperty("severidad", a.severidad)
            }
    }
    if (style.getSource(FUENTE_ID) == null) {
        style.addSource(GeoJsonSource(FUENTE_ID, FeatureCollection.fromFeatures(features)))
        style.addLayer(
            CircleLayer(CAPA_ID, FUENTE_ID).withProperties(
                circleRadius(10f),
                circleColor("#E53935"),
            )
        )
    } else {
        (style.getSource(FUENTE_ID) as? GeoJsonSource)
            ?.setGeoJson(FeatureCollection.fromFeatures(features))
    }
}
