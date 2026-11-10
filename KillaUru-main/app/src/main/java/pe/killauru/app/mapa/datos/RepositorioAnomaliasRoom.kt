package pe.killauru.app.mapa.datos

import androidx.room.withTransaction
import pe.killauru.app.comun.datos.bd.KillaUruBd
import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.mapa.dominio.Alcance
import pe.killauru.app.mapa.dominio.AnomaliaLocal
import pe.killauru.app.mapa.dominio.EstadoCopia
import pe.killauru.app.mapa.dominio.RepositorioAnomalias
import javax.inject.Inject
import javax.inject.Singleton

/**
 * FR-040, FR-052: implementación Room del repositorio de anomalías.
 * Altas, bajas y `EstadoCopia` se guardan en la misma transacción (R-011).
 */
@Singleton
internal class RepositorioAnomaliasRoom @Inject constructor(
    private val bd: KillaUruBd,
    private val dao: AnomaliaDao,
    private val estadoDao: EstadoCopiaDao,
) : RepositorioAnomalias {

    override suspend fun vigentes(ahoraMs: Long): List<AnomaliaLocal> =
        dao.vigentes(ahoraMs).map { it.aDominio() }

    override suspend fun actualizarLote(
        altas: List<AnomaliaLocal>,
        bajas: List<String>,
        estado: EstadoCopia,
    ) = bd.withTransaction {
        if (bajas.isNotEmpty()) dao.borrar(bajas)
        if (altas.isNotEmpty()) dao.upsert(altas.map { it.aEntity() })
        estadoDao.guardar(estado.aEntity())
    }

    override suspend fun recortarAlDemo() {
        dao.recortarAlDemo()
    }

    override suspend fun obtenerEstado(): EstadoCopia? =
        estadoDao.obtener()?.aDominio()

    override suspend fun limpiarCopia() = bd.withTransaction {
        dao.borrarTodas()
        estadoDao.limpiar()
    }
}

// ── Mapeos ──────────────────────────────────────────────────────────────────

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

private fun EstadoCopiaEntity.aDominio() = EstadoCopia(
    alcance = Alcance.deCodigo(alcance),
    generadoUtc = generadoUtc,
    etag = etag,
    verificadoUtc = verificadoUtc,
)

private fun EstadoCopia.aEntity() = EstadoCopiaEntity(
    id = 1,
    alcance = alcance.codigo,
    generadoUtc = generadoUtc,
    etag = etag,
    verificadoUtc = verificadoUtc,
)
