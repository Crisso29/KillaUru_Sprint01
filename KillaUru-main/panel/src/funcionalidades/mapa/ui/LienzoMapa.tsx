// T045 — LienzoMapa: MapLibre + PMTiles + íconos + tramo demo (FR-021, FR-023, R-010)
import { useEffect, useRef, useState } from 'react'
import maplibregl from 'maplibre-gl'
import { Protocol } from 'pmtiles'
import { entorno } from '@/config/entorno'
import type { ColeccionAnomalias } from '@/api/generado/002-mapa'
import { obtenerIconos } from './iconos'
import { FUENTE_ANOMALIAS, capaAnomalias } from './capas'
import { TEXTOS_MAPA } from '../textos'

maplibregl.workerUrl = new URL('maplibre-gl/dist/maplibre-gl-csp-worker', import.meta.url).toString()

interface Props {
  datos: ColeccionAnomalias | null | undefined
  anomaliaId: string | null
  onSeleccionar: (id: string) => void
}

export function LienzoMapa({ datos, anomaliaId, onSeleccionar }: Props) {
  const ref = useRef<HTMLDivElement>(null)
  const mapaRef = useRef<maplibregl.Map | null>(null)
  const [errorMapa, setErrorMapa] = useState(false)
  const onSeleccionarRef = useRef(onSeleccionar)
  onSeleccionarRef.current = onSeleccionar

  useEffect(() => {
    if (!ref.current) return
    const protocol = new Protocol()
    maplibregl.addProtocol('pmtiles', protocol.tile)

    const mapa = new maplibregl.Map({
      container: ref.current,
      style: entorno.urlEstiloMapa,
      attributionControl: { customAttribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>' },
    })

    mapa.on('error', () => setErrorMapa(true))

    mapa.on('load', async () => {
      const iconos = obtenerIconos()
      for (const { clave, datos: img } of iconos) mapa.addImage(clave, img)
      mapa.addSource(FUENTE_ANOMALIAS, { type: 'geojson', data: { type: 'FeatureCollection', features: [] } })
      mapa.addLayer(capaAnomalias(null))
      performance.mark('mapa-listo')
    })

    mapa.on('click', FUENTE_ANOMALIAS, (e) => {
      const id = e.features?.[0]?.properties?.id as string | undefined
      if (id) onSeleccionarRef.current(id)
    })

    mapa.on('mouseenter', FUENTE_ANOMALIAS, () => { mapa.getCanvas().style.cursor = 'pointer' })
    mapa.on('mouseleave', FUENTE_ANOMALIAS, () => { mapa.getCanvas().style.cursor = '' })

    mapaRef.current = mapa
    return () => { mapa.remove(); maplibregl.removeProtocol('pmtiles') }
  }, [])

  useEffect(() => {
    const mapa = mapaRef.current
    if (!mapa || !datos) return
    const src = mapa.getSource(FUENTE_ANOMALIAS) as maplibregl.GeoJSONSource | undefined
    src?.setData(datos)
  }, [datos])

  useEffect(() => {
    const mapa = mapaRef.current
    if (!mapa) return
    mapa.setLayoutProperty(FUENTE_ANOMALIAS, 'icon-size',
      ['case', ['==', ['get', 'id'], anomaliaId ?? ''], 1.4, 1])
  }, [anomaliaId])

  if (errorMapa) return <p role="alert">{TEXTOS_MAPA.errorMapaBase}</p>

  return (
    <div
      ref={ref}
      style={{ width: '100%', height: '100%' }}
      aria-label={TEXTOS_MAPA.tituloMapa}
      tabIndex={0}
    />
  )
}
