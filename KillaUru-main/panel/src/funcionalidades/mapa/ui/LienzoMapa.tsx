// T045 — LienzoMapa: MapLibre + PMTiles + íconos FR-023 (FR-021, FR-023, R-010; prototipo `.omap`)
import { useEffect, useRef, type RefObject } from 'react'
import maplibregl from 'maplibre-gl'
import type { ColeccionAnomalias } from '@/api/dtos/002-mapa'
import { FUENTE_ANOMALIAS, CAPA_ANOMALIAS, prepararPropiedades, tamanoIcono } from './capas'
import { ControlesMapa } from './ControlesMapa'
import { TEXTOS_MAPA } from '../textos'
import { useMapaBase } from './useMapaBase'

interface Props {
  datos: ColeccionAnomalias | null | undefined
  anomaliaId: string | null
  onSeleccionar: (id: string) => void
}

type RefMapa = RefObject<maplibregl.Map | null>

function encuadrar(mapa: maplibregl.Map, datos: ColeccionAnomalias) {
  const puntos = datos.features.map((feature) => feature.geometry.coordinates as [number, number])
  if (puntos.length === 0) return
  const limites = puntos.reduce((caja, punto) => caja.extend(punto), new maplibregl.LngLatBounds(puntos[0], puntos[0]))
  mapa.fitBounds(limites, { padding: 80, maxZoom: 13, duration: 0 })
}

export function LienzoMapa({ datos, anomaliaId, onSeleccionar }: Props) {
  const { contenedor, mapaRef, listo, respaldo } = useMapaBase(onSeleccionar)
  useDatosEnMapa(mapaRef, listo, datos)
  useResaltado(mapaRef, listo, anomaliaId)
  const encuadrarTodo = () => { if (mapaRef.current && datos) encuadrar(mapaRef.current, datos) }
  return (
    <div className="omap" style={{ height: '100%' }}>
      <div ref={contenedor} style={{ position: 'absolute', inset: 0 }} aria-label={TEXTOS_MAPA.tituloMapa} role="region" tabIndex={0} />
      <ControlesMapa onAcercar={() => mapaRef.current?.zoomIn()} onAlejar={() => mapaRef.current?.zoomOut()} onEncuadrar={encuadrarTodo} />
      <div className="mfoot">
        <span>{datos ? `${datos.features.length} anomalías en el mapa` : 'Cargando…'}</span>
        <span style={{ marginLeft: 'auto' }}>
          {respaldo ? TEXTOS_MAPA.errorMapaBase + ' Se muestran solo las anomalías.' : 'Mapa base: OpenStreetMap · km aproximados'}
        </span>
      </div>
    </div>
  )
}

/** Los datos se entregan solo con el estilo listo; antes se perdían porque la fuente aún no existía. Se encuadra una vez. */
function useDatosEnMapa(mapaRef: RefMapa, listo: boolean, datos: ColeccionAnomalias | null | undefined) {
  const encuadrado = useRef(false)
  useEffect(() => {
    const mapa = mapaRef.current
    if (!mapa || !listo || !datos) return
    const fuente = mapa.getSource(FUENTE_ANOMALIAS) as maplibregl.GeoJSONSource | undefined
    fuente?.setData({ ...datos, features: prepararPropiedades(datos.features) } as GeoJSON.FeatureCollection)
    if (!encuadrado.current && datos.features.length > 0) { encuadrar(mapa, datos); encuadrado.current = true }
  }, [mapaRef, datos, listo])
}

/** La anomalía elegida se dibuja más grande. */
function useResaltado(mapaRef: RefMapa, listo: boolean, anomaliaId: string | null) {
  useEffect(() => {
    if (!mapaRef.current || !listo) return
    mapaRef.current.setLayoutProperty(CAPA_ANOMALIAS, 'icon-size', tamanoIcono(anomaliaId) as unknown as number)
  }, [mapaRef, anomaliaId, listo])
}
