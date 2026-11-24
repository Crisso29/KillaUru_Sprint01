// T045 — crea el mapa MapLibre + PMTiles una sola vez y avisa cuando está listo (FR-021, R-010)
import { useEffect, useRef, useState } from 'react'
import maplibregl, { type StyleSpecification } from 'maplibre-gl'
import { Protocol } from 'pmtiles'
import { entorno } from '@/config/entorno'
import { obtenerIconos, RELACION_PIXELES } from './iconos'
import { FUENTE_ANOMALIAS, CAPA_ANOMALIAS, capaAnomalias } from './capas'

maplibregl.setWorkerUrl(new URL('maplibre-gl/dist/maplibre-gl-csp-worker.js', import.meta.url).toString())

/** Si el estilo del CDN no responde, el mapa sigue mostrando las anomalías sobre un fondo neutro (no se cae). */
const ESTILO_RESPALDO: StyleSpecification = {
  version: 8,
  sources: {},
  layers: [{ id: 'fondo', type: 'background', paint: { 'background-color': '#E8EEEF' } }],
}

const ATRIBUCION = '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'

export function useMapaBase(onSeleccionar: (id: string) => void) {
  const contenedor = useRef<HTMLDivElement>(null)
  const mapaRef = useRef<maplibregl.Map | null>(null)
  const [listo, setListo] = useState(false)
  const [respaldo, setRespaldo] = useState(false)
  // El mapa se crea una vez; el clic lee siempre la última función recibida, sin recrear el mapa
  const onSeleccionarRef = useRef(onSeleccionar)
  onSeleccionarRef.current = onSeleccionar
  useEffect(() => {
    if (!contenedor.current) return
    maplibregl.addProtocol('pmtiles', new Protocol().tile)
    const mapa = new maplibregl.Map({ container: contenedor.current, style: entorno.urlEstiloMapa, center: [-75.2, -13.4], zoom: 7,
      attributionControl: { customAttribution: ATRIBUCION } })
    vigilarCarga(mapa, () => setListo(true), () => setRespaldo(true))
    escucharClics(mapa, (id) => onSeleccionarRef.current(id))
    mapaRef.current = mapa
    return () => { mapa.remove(); maplibregl.removeProtocol('pmtiles') }
  }, [])
  return { contenedor, mapaRef, listo, respaldo }
}

/**
 * Antes de cargar, un error es el estilo del CDN: se cambia al de respaldo una sola vez. Después, los errores de
 * una tesela no deben borrar el mapa: el resto sigue visible.
 */
function vigilarCarga(mapa: maplibregl.Map, alListo: () => void, alUsarRespaldo: () => void) {
  let cargado = false
  mapa.on('error', () => {
    if (cargado) return
    cargado = true
    alUsarRespaldo()
    mapa.setStyle(ESTILO_RESPALDO)
  })
  mapa.on('style.load', () => {
    cargado = true
    agregarCapaAnomalias(mapa)
    alListo()
    performance.mark('mapa-listo')
  })
}

/** Cambiar de estilo borra íconos y capas: se vuelven a agregar en cada carga, sin duplicar. */
function agregarCapaAnomalias(mapa: maplibregl.Map) {
  for (const { clave, datos } of obtenerIconos()) {
    if (!mapa.hasImage(clave)) mapa.addImage(clave, datos, { pixelRatio: RELACION_PIXELES })
  }
  if (mapa.getSource(FUENTE_ANOMALIAS)) return
  mapa.addSource(FUENTE_ANOMALIAS, { type: 'geojson', data: { type: 'FeatureCollection', features: [] } })
  mapa.addLayer(capaAnomalias(null))
}

function escucharClics(mapa: maplibregl.Map, alElegir: (id: string) => void) {
  mapa.on('click', CAPA_ANOMALIAS, (evento) => {
    const id = evento.features?.[0]?.properties?.id as string | undefined
    if (id) alElegir(id)
  })
  mapa.on('mouseenter', CAPA_ANOMALIAS, () => { mapa.getCanvas().style.cursor = 'pointer' })
  mapa.on('mouseleave', CAPA_ANOMALIAS, () => { mapa.getCanvas().style.cursor = '' })
}
