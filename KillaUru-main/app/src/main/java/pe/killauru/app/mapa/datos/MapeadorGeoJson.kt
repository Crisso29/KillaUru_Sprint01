package pe.killauru.app.mapa.datos

import pe.killauru.app.comun.dominio.Coordenada
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.mapa.dominio.AnomaliaLocal
import java.time.Instant

/**
 * FR-044, FR-045: traduce `ColeccionAnomalias` (contrato 006) a altas y bajas del dominio.
 * Es una baja (R-011) la `BajaFeature` (`geometry: null` o `eliminada`) y la anomalía sin `venceUtc` (estado final).
 * Una feature con un `tipo` o `persistencia` desconocidos se omite en lugar de invalidar toda la respuesta, así una
 * versión nueva del servidor con más valores no deja a la app sin mapa.
 */
internal object MapeadorGeoJson {

    data class Lote(val altas: List<AnomaliaLocal>, val bajas: List<String>)

    fun mapear(dto: ColeccionAnomaliasDto): Lote {
        val (bajas, altas) = dto.features.partition(::esBaja)
        return Lote(altas = altas.mapNotNull(::anomalia), bajas = bajas.map { it.properties.id })
    }

    fun aMilisegundos(fechaIso: String): Long = Instant.parse(fechaIso).toEpochMilli()

    private fun esBaja(f: FeatureDto) = f.geometry == null || f.properties.eliminada || f.properties.venceUtc == null

    private fun anomalia(f: FeatureDto): AnomaliaLocal? {
        val p = f.properties
        val (longitud, latitud) = f.geometry?.coordinates?.takeIf { it.size == 2 } ?: return null
        return AnomaliaLocal(
            id = p.id,
            tipo = TipoAnomalia.entries.firstOrNull { it.texto == p.tipo } ?: return null,
            severidad = p.severidad ?: return null,
            persistente = when (p.persistencia) { "Persistente" -> true; "Episodica" -> false; else -> return null },
            coordenada = Coordenada(latitud = latitud, longitud = longitud),
            km = p.kmVia ?: return null,
            venceUtc = p.venceUtc?.let(::aMilisegundos),
            ultimaDeteccionUtc = p.ultimaDeteccionUtc?.let(::aMilisegundos) ?: return null,
            // El contrato no trae `enDemo`: la app debe calcularlo con su ZonaDemo (data-model §1). Pendiente; ver cierre de 008.
            enDemo = false,
        )
    }
}
