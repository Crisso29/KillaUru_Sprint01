package pe.killauru.app.mapa.ui

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.LifecycleEventObserver
import androidx.lifecycle.compose.LocalLifecycleOwner
import org.maplibre.android.MapLibre
import org.maplibre.android.maps.MapLibreMap
import org.maplibre.android.maps.MapView
import org.maplibre.android.maps.Style
import androidx.compose.foundation.layout.padding
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.comun.ui.tema.KillaUruTema
import pe.killauru.app.mapa.dominio.AnomaliaLocal

/**
 * FR-040, FR-049, T096: la pestaña Mapa. Sin mapa descargado, «Aún no tienes el mapa»; con él, el mapa base desde el
 * PMTiles del celular (funciona sin señal), la PE-28A y las anomalías vigentes. Tocar una anomalía abre su detalle.
 */
@Composable
internal fun MapaPantalla(viewModel: MapaViewModel = hiltViewModel(), onDetalle: (String) -> Unit = {}) {
    val mapa by viewModel.mapa.collectAsState()
    val anomalias by viewModel.anomalias.collectAsState()
    val tramosDemo by viewModel.tramosDemo.collectAsState()
    val avisoFueraDemo by viewModel.avisoFueraDemo.collectAsState()
    when (val estado = mapa) {
        EstadoMapaUi.Cargando -> Box(Modifier.fillMaxSize())
        EstadoMapaUi.SinMapa -> SinMapaRuta(alListo = viewModel::cargar)
        is EstadoMapaUi.ConMapa -> MapaBase(estado, anomalias, tramosDemo, avisoFueraDemo, viewModel::descartarAvisoFueraDemo, onDetalle)
    }
}

@Composable
private fun MapaBase(
    mapa: EstadoMapaUi.ConMapa,
    anomalias: List<AnomaliaLocal>,
    tramosDemo: List<Coordenada>,
    avisoFueraDemo: Boolean,
    alDescartarAviso: () -> Unit,
    onDetalle: (String) -> Unit,
) {
    val context = LocalContext.current
    val colores = coloresMapa()
    MapLibre.getInstance(context)
    val vista = remember { MapView(context) }
    var estilo by remember { mutableStateOf<Style?>(null) }
    CicloMapa(LocalLifecycleOwner.current.lifecycle, vista)
    LaunchedEffect(mapa.estilo) {
        vista.getMapAsync { mapLibre ->
            mapLibre.setStyle(Style.Builder().fromJson(mapa.estilo)) { cargado ->
                CapasMapa.agregarVia(cargado, mapa.via, colores)
                CapasMapa.encuadrar(mapLibre, mapa.via)
                estilo = cargado
            }
            mapLibre.addOnMapClickListener { punto -> anomaliaEn(mapLibre, punto)?.let(onDetalle) != null }
        }
    }
    LaunchedEffect(estilo, anomalias) { estilo?.let { CapasMapa.ponerAnomalias(it, anomalias, colores) } }
    LaunchedEffect(estilo, tramosDemo) { estilo?.let { CapasMapa.ponerZonaDemo(it, tramosDemo, colores.zonaDemo) } }
    Box(Modifier.fillMaxSize()) {
        AndroidView(factory = { vista }, modifier = Modifier.fillMaxSize())
        AvisoDemoUi(
            visible = avisoFueraDemo,
            alDescartar = alDescartarAviso,
            modifier = Modifier.align(Alignment.BottomCenter).padding(horizontal = 16.dp, vertical = 16.dp),
        )
    }
}

/** El id de la anomalía bajo el dedo, si la hay. */
private fun anomaliaEn(mapa: MapLibreMap, punto: org.maplibre.android.geometry.LatLng): String? =
    mapa.queryRenderedFeatures(mapa.projection.toScreenLocation(punto), CapasMapa.CAPA_ANOMALIAS)
        .firstOrNull()?.getStringProperty("id")

@Composable
private fun coloresMapa(): ColoresMapa {
    val paleta = KillaUruTema.paleta
    return ColoresMapa(via = paleta.killa.toArgb(), bordeVia = paleta.luna.toArgb(), contorno = paleta.superficie.toArgb(),
        severidades = paleta.severidades.map { it.toArgb() }, zonaDemo = paleta.info.toArgb())
}

@Composable
private fun CicloMapa(lifecycle: Lifecycle, mapView: MapView) {
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
}
