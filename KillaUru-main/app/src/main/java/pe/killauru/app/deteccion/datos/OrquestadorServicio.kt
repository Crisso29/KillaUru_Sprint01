package pe.killauru.app.deteccion.datos

import android.content.Context
import android.location.Location
import kotlinx.coroutines.*
import pe.killauru.app.comun.dominio.Reloj
import pe.killauru.app.deteccion.dominio.*
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import java.time.Instant
import javax.inject.Inject

/** FR-009, FR-010, R-020: conecta sensores, GPS, clasificador y casos de uso. */
internal class OrquestadorServicio @Inject constructor(
    @dagger.hilt.android.qualifiers.ApplicationContext private val context: Context,
    private val procesarVentana: ProcesarVentanaUseCase,
    private val detectarViaje: DetectorViaje,
    private val politicaDeteccion: PoliticaDeteccion,
    private val prefs: RepositorioPreferencias,
    private val reloj: Reloj,
) {
    private var scope: CoroutineScope? = null
    private var buffer: BufferVentanas? = null
    private var lectorSensores: LectorSensores? = null
    private var lectorGps: LectorGps? = null
    private var lectorBateria: LectorBateria? = null

    private var ultimaUbicacion: Location? = null
    private var categoria: CategoriaVehiculo? = null
    private var soloVerMapa = false
    private var tienePermiso = true

    fun iniciar(scope: CoroutineScope) {
        this.scope = scope
        scope.launch { cargarPrefs() }
        lectorBateria = LectorBateria(context).also { it.iniciar() }
        val buf = BufferVentanas(onVentana = { ventana ->
            val loc = ultimaUbicacion ?: return@BufferVentanas
            scope.launch { procesarConUbicacion(ventana, loc) }
        })
        buffer = buf
        lectorSensores = LectorSensores(context) { muestra ->
            scope.launch { buf.agregar(muestra, reloj.ahoraConfiable() ?: Instant.EPOCH) }
        }.also { it.iniciar() }
        lectorGps = LectorGps(context) { loc ->
            ultimaUbicacion = loc
            detectarViaje.registrar(loc.speed * 3.6 > 5.0, Instant.ofEpochMilli(loc.time), 0.0)
        }.also { it.iniciar(true) }
    }

    private suspend fun cargarPrefs() {
        categoria = prefs.obtenerCategoria()
        soloVerMapa = prefs.obtenerSoloVerMapa()
    }

    private suspend fun procesarConUbicacion(ventana: pe.killauru.app.deteccion.dominio.Ventana, loc: Location) {
        val estado = PoliticaDeteccion.EstadoDeteccion(
            categoria = categoria,
            soloVerMapa = soloVerMapa,
            enAhorro = false,
            tienePermiso = tienePermiso,
        )
        if (!politicaDeteccion.puedeDetectar(estado)) return
        val fixUtc = Instant.ofEpochMilli(loc.time)
        val ahoraUtc = reloj.ahoraConfiable() ?: return
        procesarVentana.procesar(
            ventana, loc.latitude, loc.longitude,
            loc.accuracy.toDouble(), fixUtc, ahoraUtc,
        )
    }

    fun detener() {
        lectorSensores?.detener()
        lectorGps?.detener()
        lectorBateria?.detener()
        buffer?.limpiar()
    }
}
