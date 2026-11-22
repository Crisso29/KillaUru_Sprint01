// T045 — capas: configuración MapLibre de la capa de anomalías
import type { LayerSpecification } from 'maplibre-gl'
import { claveIcono, ESTADOS_MAPA, SEVERIDADES } from '../modelo/simbolos'

export const FUENTE_ANOMALIAS = 'anomalias'
export const CAPA_ANOMALIAS = 'anomalias-capa'

/** Tamaño del ícono: la anomalía seleccionada se ve más grande. */
export function tamanoIcono(idSeleccionada: string | null) {
  return ['case', ['==', ['get', 'id'], idSeleccionada ?? ''], 1.35, 1] as const
}

/** Capa de símbolos: cada anomalía usa el ícono ya calculado en `prepararPropiedades` (propiedad `icono`). */
export function capaAnomalias(idSeleccionada: string | null): LayerSpecification {
  return {
    id: CAPA_ANOMALIAS,
    type: 'symbol',
    source: FUENTE_ANOMALIAS,
    layout: {
      'icon-image': ['get', 'icono'],
      'icon-size': tamanoIcono(idSeleccionada) as unknown as number,
      'icon-allow-overlap': true,
      // Las más graves encima: se dibujan al final.
      'symbol-sort-key': ['get', 'severidad'],
    },
  }
}

/** Añade a cada anomalía la clave de su ícono (FR-023). Sin esto la capa no sabe qué dibujar. */
export function prepararPropiedades<T extends GeoJSON.Feature>(features: T[]): T[] {
  return features.map(f => {
    if (!f.properties) return f
    const { estado, severidad } = f.properties as { estado: string; severidad: number }
    return { ...f, properties: { ...f.properties, icono: claveIcono(estado, severidad) } }
  })
}

export function todasLasClaves(): string[] {
  return ESTADOS_MAPA.flatMap(e => SEVERIDADES.map(s => claveIcono(e, s)))
}
