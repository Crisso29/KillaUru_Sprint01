// T045 — LienzoMapa: MapLibre + PMTiles + íconos FR-023 (FR-021, FR-023, R-010; prototipo `.omap`)
import { useEffect, useRef, useState } from 'react'
import maplibregl, { type StyleSpecification } from 'maplibre-gl'
import { Protocol } from 'pmtiles'
import { entorno } from '@/config/entorno'
import type { ColeccionAnomalias } from '@/api/dtos/002-mapa'
import { obtenerIconos, RELACION_PIXELES } from './iconos'
import { FUENTE_ANOMALIAS, CAPA_ANOMALIAS, capaAnomalias, prepararPropiedades, tamanoIcono } from './capas'
import { ControlesMapa } from './ControlesMapa'
import { TEXTOS_MAPA } from '../textos'

maplibregl.setWorkerUrl(new URL('maplibre-gl/dist/maplibre-gl-csp-worker.js', import.meta.url).toString())

/** Si el estilo del CDN no responde, el mapa sigue mostrando las anomalías sobre un fondo neutro (no se cae). */
const ESTILO_RESPALDO: StyleSpecification = {
  version: 8,
  sources: {},
  layers: [{ id: 'fondo', type: 'background', paint: { 'background-color': '#E8EEEF' } }],
}

interface Props {
  datos: ColeccionAnomalias | null | undefined
  anomaliaId: string | null
  onSeleccionar: (id: string) => void
}

function encuadrar(mapa: maplibregl.Map, datos: ColeccionAnomalias) {
  const puntos = datos.features.map(f => f.geometry.coordinates as [number, number])
  if (puntos.length === 0) return
  const limites = puntos.reduce((b, p) => b.extend(p), new maplibregl.LngLatBounds(puntos[0], puntos[0]))
  mapa.fitBounds(limites, { padding: 80, maxZoom: 13, duration: 0 })
}

export function LienzoMapa({ datos, anomaliaId, onSeleccionar }: Props) {
  const ref = useRef<HTMLDivElement>(null)
  const mapaRef = useRef<maplibregl.Map | null>(null)
  const [listo, setListo] = useState(false)
  const [respaldo, setRespaldo] = useState(false)
  const encuadrado = useRef(false)
  const onSeleccionarRef = useRef(onSeleccionar)
  onSeleccionarRef.current = onSeleccionar

  useEffect(() => {
    if (!ref.current) return
    const protocol = new Protocol()
    maplibregl.addProtocol('pmtiles', protocol.tile)
    const mapa = new maplibregl.Map({
      container: ref.current,
      style: entorno.urlEstiloMapa,
      center: [-75.2, -13.4],
      zoom: 7,
      attributionControl: { customAttribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>' },
    })
    // Antes de cargar, un error es el estilo del CDN: se cambia al de respaldo una sola vez. Después, los errores de
    // una tesela no deben borrar el mapa: el resto sigue visible.
    let cargado = false
    mapa.on('error', () => {
      if (cargado) return
      cargado = true
      setRespaldo(true)
      mapa.setStyle(ESTILO_RESPALDO)
    })
    mapa.on('style.load', () => {
      cargado = true
      for (const { clave, datos: img } of obtenerIconos()) {
        if (!mapa.hasImage(clave)) mapa.addImage(clave, img, { pixelRatio: RELACION_PIXELES })
      }
      if (!mapa.getSource(FUENTE_ANOMALIAS)) {
        mapa.addSource(FUENTE_ANOMALIAS, { type: 'geojson', data: { type: 'FeatureCollection', features: [] } })
        mapa.addLayer(capaAnomalias(null))
      }
      setListo(true)
      performance.mark('mapa-listo')
    })
    mapa.on('click', CAPA_ANOMALIAS, (e) => {
      const id = e.features?.[0]?.properties?.id as string | undefined
      if (id) onSeleccionarRef.current(id)
    })
    mapa.on('mouseenter', CAPA_ANOMALIAS, () => { mapa.getCanvas().style.cursor = 'pointer' })
    mapa.on('mouseleave', CAPA_ANOMALIAS, () => { mapa.getCanvas().style.cursor = '' })
    mapaRef.current = mapa
    return () => { mapa.remove(); maplibregl.removeProtocol('pmtiles') }
  }, [])

  // Los datos se entregan solo con el estilo listo; antes se perdían porque la fuente aún no existía.
  useEffect(() => {
    const mapa = mapaRef.current
    if (!mapa || !listo || !datos) return
    const fuente = mapa.getSource(FUENTE_ANOMALIAS) as maplibregl.GeoJSONSource | undefined
    fuente?.setData({ ...datos, features: prepararPropiedades(datos.features) } as GeoJSON.FeatureCollection)
    if (!encuadrado.current && datos.features.length > 0) { encuadrar(mapa, datos); encuadrado.current = true }
  }, [datos, listo])

  useEffect(() => {
    const mapa = mapaRef.current
    if (!mapa || !listo) return
    mapa.setLayoutProperty(CAPA_ANOMALIAS, 'icon-size', tamanoIcono(anomaliaId) as unknown as number)
  }, [anomaliaId, listo])

  return (
    <div className="omap" style={{ height: '100%' }}>
      <div ref={ref} style={{ position: 'absolute', inset: 0 }} aria-label={TEXTOS_MAPA.tituloMapa} role="region" tabIndex={0} />
      <ControlesMapa
        onAcercar={() => mapaRef.current?.zoomIn()}
        onAlejar={() => mapaRef.current?.zoomOut()}
        onEncuadrar={() => { if (mapaRef.current && datos) encuadrar(mapaRef.current, datos) }}
      />
      <div className="mfoot">
        <span>{datos ? `${datos.features.length} anomalías en el mapa` : 'Cargando…'}</span>
        <span style={{ marginLeft: 'auto' }}>
          {respaldo ? TEXTOS_MAPA.errorMapaBase + ' Se muestran solo las anomalías.' : 'Mapa base: OpenStreetMap · km aproximados'}
        </span>
      </div>
    </div>
  )
}
