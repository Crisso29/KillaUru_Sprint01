package pe.killauru.app.soporte

import pe.killauru.app.sesion.dominio.RepositorioPreferencias
import pe.killauru.app.sesion.dominio.CategoriaVehiculo

internal class PreferenciasMemoria : RepositorioPreferencias {
    var limpiezas = 0
    var terminosVersion: String? = null
    override suspend fun obtenerTerminosVersion() = terminosVersion
    override suspend fun guardarTerminosVersion(version: String) { this.terminosVersion = version }
    var terminosAceptadoUtc: Long? = null
    override suspend fun obtenerTerminosAceptadoUtc() = terminosAceptadoUtc
    override suspend fun guardarTerminosAceptadoUtc(utc: Long) { this.terminosAceptadoUtc = utc }
    var politicaVersion: String? = null
    override suspend fun obtenerPoliticaVersion() = politicaVersion
    override suspend fun guardarPoliticaVersion(version: String) { this.politicaVersion = version }
    var politicaAceptadoUtc: Long? = null
    override suspend fun obtenerPoliticaAceptadoUtc() = politicaAceptadoUtc
    override suspend fun guardarPoliticaAceptadoUtc(utc: Long) { this.politicaAceptadoUtc = utc }
    var categoria: CategoriaVehiculo? = null
    override suspend fun obtenerCategoria() = categoria
    override suspend fun guardarCategoria(categoria: CategoriaVehiculo) { this.categoria = categoria }
    var soloVerMapa: Boolean = false
    override suspend fun obtenerSoloVerMapa() = soloVerMapa
    override suspend fun guardarSoloVerMapa(valor: Boolean) { this.soloVerMapa = valor }
    var usarDatosMoviles: Boolean = false
    override suspend fun obtenerUsarDatosMoviles() = usarDatosMoviles
    override suspend fun guardarUsarDatosMoviles(valor: Boolean) { this.usarDatosMoviles = valor }
    var backoffPaso: Int = 0
    override suspend fun obtenerBackoffPaso() = backoffPaso
    override suspend fun guardarBackoffPaso(paso: Int) { this.backoffPaso = paso }
    var mapaVersion: String? = null
    override suspend fun obtenerMapaVersion() = mapaVersion
    override suspend fun guardarMapaVersion(version: String) { this.mapaVersion = version }
    var modeloVersion: String? = null
    override suspend fun obtenerModeloVersion() = modeloVersion
    override suspend fun guardarModeloVersion(version: String) { this.modeloVersion = version }
    var seudonimo: String? = null
    var desde: Long? = null
    override suspend fun obtenerSeudonimo() = seudonimo
    override suspend fun obtenerSeudonimoDesdeUtc() = desde
    override suspend fun guardarSeudonimo(seudonimo: String, desdeUtc: Long) {
        this.seudonimo = seudonimo
        desde = desdeUtc
    }
    override suspend fun limpiarSesion() { limpiezas++ }
}
