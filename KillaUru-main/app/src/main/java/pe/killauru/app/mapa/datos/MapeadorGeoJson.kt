package pe.killauru.app.mapa.datos

import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.mapa.dominio.AnomaliaLocal

/**
 * FR-044, FR-045: convierte `FeatureCollectionDto` a listas de altas y bajas del dominio.
 * Criterios de baja (R-011):
 * - `geometry == null` → `BajaFeature` explícita.
 * - `properties.eliminada == true` → baja según el contrato.
 * - `venceUtc == null` con un `AnomaliaFeature` → estado final → baja.
 */
internal object MapeadorGeoJson {

    data class Lote(
        val altas: List<AnomaliaLocal>,
        val bajas: List<String>,
    )

    fun mapear(dto: FeatureCollectionDto): Lote {
        val altas = mutableListOf<AnomaliaLocal>()
        val bajas = mutableListOf<String>()

        for (f in dto.features) {
            val esBaja = f.geometry == null ||
                f.properties.eliminada ||
                f.properties.venceUtc == null

            if (esBaja) {
                bajas += f.properties.id
            } else {
                val anomalia = toAnomalia(f) ?: continue
                altas += anomalia
            }
        }
        return Lote(altas, bajas)
    }

    private fun toAnomalia(f: FeatureDto): AnomaliaLocal? {
        val geo = f.geometry ?: return null
        if (geo.coordinates.size < 2) return null
        val p = f.properties
        val tipo = p.tipo?.let { TipoAnomalia.deCodigo(it) } ?: return null
        val severidad = p.severidad ?: return null
        val km = p.km ?: return null
        val ultimaDet = p.ultimaDeteccionUtc ?: return null
        return AnomaliaLocal(
            id = p.id,
            tipo = tipo,
            severidad = severidad,
            persistente = p.persistente,
            coordenada = Coordenada(geo.coordinates[1], geo.coordinates[0]),
            km = km,
            venceUtc = p.venceUtc,   // null queda como null; el trigger lo valida en BD
            ultimaDeteccionUtc = ultimaDet,
            enDemo = p.enDemo,
        )
    }
}
