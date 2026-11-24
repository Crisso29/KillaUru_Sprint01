// T067 — PaginaModeracion: pestañas Por revisar / Esperan confirmación (FR-028, FR-029; prototipo «Moderación»)
import { useSearchParams } from 'react-router'
import { EstadoError, Esqueleto, Icono } from '@/compartido/ui'
import type { ErrorApi } from '@/api/problema'
import { usePorRevisar, usePendientes } from '../api/useListasModeracion'
import { TablaPorRevisar } from './TablaPorRevisar'
import { TablaPendientes } from './TablaPendientes'
import { TEXTOS_MODERACION as textos } from '../textos'

type Pestana = 'por-revisar' | 'pendientes'

/** La pestaña vive en la dirección (`?pestana=pendientes`): así el Resumen enlaza directo a la que corresponde. */
export function PaginaModeracion() {
  const [params, setParams] = useSearchParams()
  const pestana: Pestana = params.get('pestana') === 'pendientes' ? 'pendientes' : 'por-revisar'
  const porRevisar = usePorRevisar()
  const pendientes = usePendientes()
  const elegir = (elegida: Pestana) => setParams(elegida === 'por-revisar' ? {} : { pestana: elegida }, { replace: true })
  return (
    <>
      <div className="pg-h"><div><h2>Moderación</h2><p>Decisiones que necesitan a una persona.</p></div></div>
      <PestanasModeracion activa={pestana} onElegir={elegir} cantidades={[porRevisar.data?.length ?? 0, pendientes.data?.length ?? 0]} />
      {pestana === 'por-revisar' ? <ContenidoPorRevisar consulta={porRevisar} /> : <ContenidoPendientes consulta={pendientes} />}
    </>
  )
}

function ContenidoPorRevisar({ consulta }: { consulta: ReturnType<typeof usePorRevisar> }) {
  return (
    <>
      <div className="banner-ops">
        <Icono nombre="info" />
        <p>
          Un punto <b>Descartada</b> absorbe sus reportes para siempre (un rompemuelles no vuelve al mapa). Si muchos
          vehículos lo siguen reportando, puede haber un bache real al lado. <b>Reabrir</b> lo publica ya;{' '}
          <b>mantener</b> reinicia el contador.
        </p>
      </div>
      {consulta.isLoading && <Esqueleto alto={200} />}
      {consulta.error && <EstadoError error={consulta.error as ErrorApi} />}
      {consulta.data && <TablaPorRevisar items={consulta.data} />}
    </>
  )
}

function ContenidoPendientes({ consulta }: { consulta: ReturnType<typeof usePendientes> }) {
  return (
    <>
      {consulta.isLoading && <Esqueleto alto={200} />}
      {consulta.error && <EstadoError error={consulta.error as ErrorApi} />}
      {consulta.data && <TablaPendientes items={consulta.data} />}
    </>
  )
}

interface PropsPestanas { activa: Pestana; onElegir: (pestana: Pestana) => void; cantidades: [number, number] }

function PestanasModeracion({ activa, onElegir, cantidades: [porRevisar, pendientes] }: PropsPestanas) {
  return (
    <div role="tablist" className="subtabs">
      <button type="button" role="tab" aria-selected={activa === 'por-revisar'} onClick={() => onElegir('por-revisar')}>
        {textos.pestanaPorRevisar} <span className="cnt">{porRevisar}</span>
      </button>
      <button type="button" role="tab" aria-selected={activa === 'pendientes'} onClick={() => onElegir('pendientes')}>
        {textos.pestanaPendientes}{' '}
        <span className="cnt" style={{ background: 'var(--sunken)', color: 'var(--ink-2)' }}>{pendientes}</span>
      </button>
    </div>
  )
}
