package pe.killauru.app.sesion.datos

import kotlinx.coroutines.flow.first
import pe.killauru.app.comun.datos.almacen.PreferenciasDataStore
import pe.killauru.app.sesion.dominio.CategoriaVehiculo
import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import javax.inject.Inject
import javax.inject.Singleton

/** FR-003, FR-005: implementación de RepositorioPreferencias sobre DataStore. */
@Singleton
internal class RepositorioPreferenciasDataStore @Inject constructor(
    private val ds: PreferenciasDataStore,
) : RepositorioPreferencias {

    override suspend fun obtenerTerminosVersion() =
        ds.leer(PreferenciasDataStore.K_TERMINOS_VERSION).first()

    override suspend fun guardarTerminosVersion(version: String) =
        ds.guardar(PreferenciasDataStore.K_TERMINOS_VERSION, version)

    override suspend fun obtenerTerminosAceptadoUtc() =
        ds.leer(PreferenciasDataStore.K_TERMINOS_ACEPTADO).first()

    override suspend fun guardarTerminosAceptadoUtc(utc: Long) =
        ds.guardar(PreferenciasDataStore.K_TERMINOS_ACEPTADO, utc)

    override suspend fun obtenerPoliticaVersion() =
        ds.leer(PreferenciasDataStore.K_POLITICA_VERSION).first()

    override suspend fun guardarPoliticaVersion(version: String) =
        ds.guardar(PreferenciasDataStore.K_POLITICA_VERSION, version)

    override suspend fun obtenerPoliticaAceptadoUtc() =
        ds.leer(PreferenciasDataStore.K_POLITICA_ACEPTADO).first()

    override suspend fun guardarPoliticaAceptadoUtc(utc: Long) =
        ds.guardar(PreferenciasDataStore.K_POLITICA_ACEPTADO, utc)

    override suspend fun obtenerCategoria(): CategoriaVehiculo? =
        ds.leer(PreferenciasDataStore.K_CATEGORIA).first()
            ?.let { CategoriaVehiculo.deTexto(it) }

    override suspend fun guardarCategoria(categoria: CategoriaVehiculo) =
        ds.guardar(PreferenciasDataStore.K_CATEGORIA, categoria.texto)

    override suspend fun obtenerSoloVerMapa() =
        ds.leer(PreferenciasDataStore.K_SOLO_MAPA).first() ?: false

    override suspend fun guardarSoloVerMapa(valor: Boolean) =
        ds.guardar(PreferenciasDataStore.K_SOLO_MAPA, valor)

    override suspend fun obtenerUsarDatosMoviles() =
        ds.leer(PreferenciasDataStore.K_DATOS_MOVILES).first() ?: false

    override suspend fun guardarUsarDatosMoviles(valor: Boolean) =
        ds.guardar(PreferenciasDataStore.K_DATOS_MOVILES, valor)

    override suspend fun obtenerSeudonimo() =
        ds.leer(PreferenciasDataStore.K_SEUDONIMO).first()

    override suspend fun guardarSeudonimo(seudonimo: String, desdeUtc: Long) {
        ds.guardar(PreferenciasDataStore.K_SEUDONIMO, seudonimo)
        ds.guardar(PreferenciasDataStore.K_SEUDONIMO_DESDE, desdeUtc)
    }

    override suspend fun obtenerSeudonimoDesdeUtc() =
        ds.leer(PreferenciasDataStore.K_SEUDONIMO_DESDE).first()

    override suspend fun obtenerBackoffPaso() =
        ds.leer(PreferenciasDataStore.K_BACKOFF_PASO).first() ?: 0

    override suspend fun guardarBackoffPaso(paso: Int) =
        ds.guardar(PreferenciasDataStore.K_BACKOFF_PASO, paso)

    override suspend fun obtenerMapaVersion() =
        ds.leer(PreferenciasDataStore.K_MAPA_VERSION).first()

    override suspend fun guardarMapaVersion(version: String) =
        ds.guardar(PreferenciasDataStore.K_MAPA_VERSION, version)

    override suspend fun obtenerModeloVersion() =
        ds.leer(PreferenciasDataStore.K_MODELO_VERSION).first()

    override suspend fun guardarModeloVersion(version: String) =
        ds.guardar(PreferenciasDataStore.K_MODELO_VERSION, version)

    override suspend fun limpiarSesion() = ds.limpiar()
}
