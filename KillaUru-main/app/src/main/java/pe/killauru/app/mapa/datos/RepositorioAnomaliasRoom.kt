package pe.killauru.app.mapa.datos

import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.mapa.dominio.AnomaliaLocal
import pe.killauru.app.mapa.dominio.RepositorioAnomalias
import javax.inject.Inject
import javax.inject.Singleton

/** FR-040: implementación Room del repositorio de anomalías. */
@Singleton
internal class RepositorioAnomaliasRoom @Inject constructor(
    private val dao: AnomaliaDao,
) : RepositorioAnomalias {

    override suspend fun vigentes(ahoraMs: Long): List<AnomaliaLocal> =
        dao.vigentes(ahoraMs).map { it.aDominio() }

    override suspend fun actualizarLote(altas: List<AnomaliaLocal>, bajas: List<String>) {
        dao.actualizarLote(altas.map { it.aEntity() }, bajas)
    }

    override suspend fun recortarAlDemo() { dao.recortarAlDemo() }
}

private fun AnomaliaEntity.aDominio() = AnomaliaLocal(
    id = id,
    tipo = TipoAnomalia.deCodigo(tipo),
    severidad = severidad,
    persistente = persistente != 0,
    coordenada = Coordenada(lat, lon),
    km = km,
    venceUtc = if (venceUtc == 0L) null else venceUtc,
    ultimaDeteccionUtc = ultimaDeteccionUtc,
    enDemo = enDemo != 0,
)

private fun AnomaliaLocal.aEntity() = AnomaliaEntity(
    id = id,
    tipo = tipo.codigo,
    severidad = severidad,
    persistente = if (persistente) 1 else 0,
    lat = coordenada.latitud,
    lon = coordenada.longitud,
    km = km,
    venceUtc = venceUtc ?: 0L,
    ultimaDeteccionUtc = ultimaDeteccionUtc,
    enDemo = if (enDemo) 1 else 0,
)
