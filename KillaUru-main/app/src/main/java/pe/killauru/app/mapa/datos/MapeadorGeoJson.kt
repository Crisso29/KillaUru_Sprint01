package pe.killauru.app.mapa.datos

import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.mapa.dominio.AnomaliaLocal

/** FR-040: convierte FeatureCollectionDto a AnomaliaLocal para el dominio. */
internal object MapeadorGeoJson {
    fun mapear(dto: FeatureCollectionDto): List<AnomaliaLocal> =
        dto.features.mapNotNull { f ->
            if (f.geometry.coordinates.size < 2) return@mapNotNull null
            AnomaliaLocal(
                id = f.properties.id,
                tipo = TipoAnomalia.deCodigo(f.properties.tipo),
                severidad = f.properties.severidad,
                persistente = f.properties.persistente,
                coordenada = Coordenada(
                    lat = f.geometry.coordinates[1],
                    lon = f.geometry.coordinates[0],
                ),
                km = f.properties.km,
                venceUtc = f.properties.venceUtc,
                ultimaDeteccionUtc = f.properties.ultimaDeteccionUtc,
                enDemo = f.properties.enDemo,
            )
        }
}
