// T067 — TablaPorRevisar: filas con MantenerDescartada (sin diálogo) y Reabrir (con diálogo); prototipo tabla `.dt`
import { useState } from 'react'
import { Link } from 'react-router'
import { Boton, Severidad } from '@/compartido/ui'
import { formatearKm } from '@/compartido/formato/km'
import { formatearRelativo } from '@/compartido/formato/relativo'
import { relojReal } from '@/compartido/reloj'
import { useModerar } from '../api/useModerar'
import { DialogoModeracion } from './DialogoModeracion'
import { TEXTOS_MODERACION as T } from '../textos'
import type { ElementoPorRevisar } from '@/api/dtos/003-anomalias'

interface Props {
  items: ElementoPorRevisar[]
}

export function TablaPorRevisar({ items }: Props) {
  const [reabrirId, setReabrirId] = useState<string | null>(null)
  const moderar = useModerar()

  async function mantener(id: string) {
    await moderar.mutateAsync({ id, accion: 'MantenerDescartada' })
  }

  async function reabrir() {
    if (!reabrirId) return
    await moderar.mutateAsync({ id: reabrirId, accion: 'Reabrir' })
    setReabrirId(null)
  }

  if (items.length === 0) {
    return <div className="card"><p className="card-b hint">{T.vacioPorRevisar}</p></div>
  }

  const ahora = relojReal.ahora()
  return (
    <>
      <div className="card">
        <table className="dt">
          <thead>
            <tr>
              <th scope="col">Anomalía</th>
              <th scope="col">Ubicación</th>
              <th scope="col" className="r">Vehículos en 7 d</th>
              <th scope="col" className="r">Reportes</th>
              <th scope="col">Último reporte</th>
              <th scope="col" className="r">Decisión</th>
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.id}>
                <td>
                  <span style={{ display: 'inline-flex', gap: 'var(--s-5)', alignItems: 'center' }}>
                    <Severidad nivel={item.severidadMostrada} tamano="sm" />
                    <b>{item.tipo}</b> <span className="mono">{item.id.slice(0, 8)}</span>
                  </span>
                </td>
                <td><Link to={`/mapa?anomalia=${item.id}`}>km {formatearKm(item.kmVia)}</Link></td>
                <td className="r"><b>{item.vehiculos7d}</b></td>
                <td className="r">{item.reportes}</td>
                <td>{formatearRelativo(item.ultimaDeteccionUtc, ahora)}</td>
                <td className="r">
                  <span style={{ display: 'inline-flex', gap: 'var(--s-5)' }}>
                    <Boton variante="secundario" onClick={() => void mantener(item.id)} disabled={moderar.isPending}>
                      {T.accionMantenerDescartada}
                    </Boton>
                    <Boton variante="primario" onClick={() => setReabrirId(item.id)}>{T.accionReabrir}</Boton>
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="tfoot">
          <span>{items.length} {items.length === 1 ? 'elemento' : 'elementos'}</span><span>RN-06 · RN-07</span>
        </div>
      </div>
      {reabrirId && (
        <DialogoModeracion
          accion="Reabrir"
          idCorto={reabrirId.slice(0, 8)}
          cargando={moderar.isPending}
          onConfirmar={reabrir}
          onCancelar={() => setReabrirId(null)}
        />
      )}
    </>
  )
}
