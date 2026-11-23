package pe.killauru.app.comun.dominio

import pe.killauru.app.mapa.dominio.DescargaMapa
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sincronizacion.dominio.ProgramadorSincronizacion
import pe.killauru.app.sincronizacion.dominio.RecuperarEnviandoUseCase
import javax.inject.Inject

/**
 * T072 / FR-021, FR-022, FR-033, FR-043: lo que hace la app cada vez que arranca.
 * 1. Los eventos que quedaron en `ENVIANDO` (la app se cerró a mitad de un envío) vuelven a `PENDING`.
 * 2. Solo después se programan la sincronización y la purga: así no se envía dos veces ni se pierde nada.
 * 3. Si ya pasó el primer uso, se comprueba si hay mapa nuevo, sin cortar una descarga que ya esté en curso.
 */
internal class IniciarApp @Inject constructor(
    private val recuperarEnviando: RecuperarEnviandoUseCase,
    private val sincronizacion: ProgramadorSincronizacion,
    private val descargaMapa: DescargaMapa,
    private val prefs: RepositorioPreferencias,
) {
    suspend fun ejecutar() {
        recuperarEnviando.ejecutar()
        sincronizacion.programar()
        if (prefs.obtenerCategoria() != null) descargaMapa.programar(prefs.obtenerUsarDatosMoviles(), reemplazar = false)
    }
}
