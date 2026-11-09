// T045 — capas: configuraciones de capas MapLibre para anomalías
import type { LayerSpecification } from 'maplibre-gl'
import { claveIcono, ESTADOS_MAPA, SEVERIDADES } from '../modelo/simbolos'

export const FUENTE_ANOMALIAS = 'anomalias'
export const CAPA_ANOMALIAS = 'anomalias-capa'

export function capaAnomalias(idSeleccionada: string | null): LayerSpecification {
  return {
    id: CAPA_ANOMALIAS,
    type: 'symbol',
    source: FUENTE_ANOMALIAS,
    layout: {
      'icon-image': [
        'concat',
        ['get', 'forma'],
        '-',
        ['to-string', ['get', 'severidad']],
        ['case', ['all', ['==', ['get', 'estado'], 'Activa'], ['>=', ['get', 'severidad'], 4]], '-anillo', ''],
      ],
      'icon-size': ['case', ['==', ['get', 'id'], idSeleccionada ?? ''], 1.4, 1],
      'icon-allow-overlap': true,
    },
  }
}

export function prepararPropiedades(features: GeoJSON.Feature[]): GeoJSON.Feature[] {
  return features.map(f => {
    if (!f.properties) return f
    const { estado, severidad } = f.properties as { estado: string; severidad: number }
    return {
      ...f,
      properties: {
        ...f.properties,
        forma: claveIcono(estado, severidad).split('-')[0],
      },
    }
  })
}

export function todasLasClaves(): string[] {
  return ESTADOS_MAPA.flatMap(e => SEVERIDADES.map(s => claveIcono(e, s)))
}
