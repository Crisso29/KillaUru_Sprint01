// T067 — PaginaModeracion: pestañas Por revisar / Esperan confirmación (FR-028, FR-029)
import { useState } from 'react'
import { EstadoError, Esqueleto } from '@/compartido/ui'
import { usePorRevisar, usePendientes } from '../api/useListasModeracion'
import { TablaPorRevisar } from './TablaPorRevisar'
import { TablaPendientes } from './TablaPendientes'
import { TEXTOS_MODERACION as T } from '../textos'

type Pestana = 'por-revisar' | 'pendientes'

export function PaginaModeracion() {
  const [pestana, setPestana] = useState<Pestana>('por-revisar')
  const { data: porRevisar, isLoading: loadingPR, error: errPR } = usePorRevisar()
  const { data: pendientes, isLoading: loadingP, error: errP } = usePendientes()

  const contPR = porRevisar?.length ?? 0
  const contP = pendientes?.length ?? 0

  return (
    <article style={{ padding: 'var(--s-13)' }}>
      <div role="tablist" style={{ display: 'flex', gap: 'var(--s-08)', marginBottom: 'var(--s-13)' }}>
        <button
          role="tab"
          aria-selected={pestana === 'por-revisar'}
          onClick={() => setPestana('por-revisar')}
        >
          {T.pestanaPorRevisar} {contPR}
        </button>
        <button
          role="tab"
          aria-selected={pestana === 'pendientes'}
          onClick={() => setPestana('pendientes')}
        >
          {T.pestanaPendientes} {contP}
        </button>
      </div>

      {pestana === 'por-revisar' && (
        <>
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
    </article>
  )
}
