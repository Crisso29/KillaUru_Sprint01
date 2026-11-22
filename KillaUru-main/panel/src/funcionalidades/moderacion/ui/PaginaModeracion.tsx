// T067 — PaginaModeracion: pestañas Por revisar / Esperan confirmación (FR-028, FR-029; prototipo «Moderación»)
import { useSearchParams } from 'react-router'
import { EstadoError, Esqueleto, Icono } from '@/compartido/ui'
import { usePorRevisar, usePendientes } from '../api/useListasModeracion'
import { TablaPorRevisar } from './TablaPorRevisar'
import { TablaPendientes } from './TablaPendientes'
import { TEXTOS_MODERACION as T } from '../textos'

type Pestana = 'por-revisar' | 'pendientes'

/** La pestaña vive en la dirección (`?pestana=pendientes`): así el Resumen enlaza directo a la que corresponde. */
export function PaginaModeracion() {
  const [params, setParams] = useSearchParams()
  const pestana: Pestana = params.get('pestana') === 'pendientes' ? 'pendientes' : 'por-revisar'
  const { data: porRevisar, isLoading: loadingPR, error: errPR } = usePorRevisar()
  const { data: pendientes, isLoading: loadingP, error: errP } = usePendientes()
  const elegir = (p: Pestana) => setParams(p === 'por-revisar' ? {} : { pestana: p }, { replace: true })

  return (
    <>
      <div className="pg-h">
        <div><h2>Moderación</h2><p>Decisiones que necesitan a una persona.</p></div>
      </div>
      <div role="tablist" className="subtabs">
        <button type="button" role="tab" aria-selected={pestana === 'por-revisar'} onClick={() => elegir('por-revisar')}>
          {T.pestanaPorRevisar} <span className="cnt">{porRevisar?.length ?? 0}</span>
        </button>
        <button type="button" role="tab" aria-selected={pestana === 'pendientes'} onClick={() => elegir('pendientes')}>
          {T.pestanaPendientes}{' '}
          <span className="cnt" style={{ background: 'var(--sunken)', color: 'var(--ink-2)' }}>{pendientes?.length ?? 0}</span>
        </button>
      </div>

      {pestana === 'por-revisar' && (
        <>
          <div className="banner-ops">
            <Icono nombre="info" />
            <p>
              Un punto <b>Descartada</b> absorbe sus reportes para siempre (un rompemuelles no vuelve al mapa). Si muchos
              vehículos lo siguen reportando, puede haber un bache real al lado. <b>Reabrir</b> lo publica ya;{' '}
              <b>mantener</b> reinicia el contador.
            </p>
          </div>
          {loadingPR && <Esqueleto alto={200} />}
          {errPR && <EstadoError error={errPR as { status: number; codigo: string }} />}
          {porRevisar && <TablaPorRevisar items={porRevisar} />}
        </>
      )}

      {pestana === 'pendientes' && (
        <>
          {loadingP && <Esqueleto alto={200} />}
          {errP && <EstadoError error={errP as { status: number; codigo: string }} />}
          {pendientes && <TablaPendientes items={pendientes} />}
        </>
      )}
    </>
  )
}
