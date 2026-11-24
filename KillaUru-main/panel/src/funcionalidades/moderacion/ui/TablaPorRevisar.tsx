// T067 — TablaPorRevisar: filas con MantenerDescartada (sin diálogo) y Reabrir (con diálogo); prototipo tabla `.dt`
import { useState } from 'react'
import { Link } from 'react-router'
import { Boton } from '@/compartido/ui'
import { formatearKm } from '@/compartido/formato/km'
import { formatearRelativo } from '@/compartido/formato/relativo'
import { relojReal } from '@/compartido/reloj'
import { useModerar } from '../api/useModerar'
import { DialogoModeracion } from './DialogoModeracion'
import { TEXTOS_MODERACION as textos } from '../textos'
import type { ElementoPorRevisar } from '@/api/dtos/003-anomalias'
import { Cabecera, CeldaAnomalia, PieTabla, type Columna } from './PartesTabla'

const COLUMNAS: Columna[] = [
  { texto: 'Anomalía' }, { texto: 'Ubicación' }, { texto: 'Vehículos en 7 d', derecha: true },
  { texto: 'Reportes', derecha: true }, { texto: 'Último reporte' }, { texto: 'Decisión', derecha: true },
]

export function TablaPorRevisar({ items }: { items: ElementoPorRevisar[] }) {
  const { reabrirId, setReabrirId, mantener, reabrir, moderar } = useDecisionesPorRevisar()
  if (items.length === 0) return <div className="card"><p className="card-b hint">{textos.vacioPorRevisar}</p></div>
  return (
    <>
      <div className="card">
        <table className="dt">
          <Cabecera columnas={COLUMNAS} />
          <tbody>
            {items.map((item) => <FilaPorRevisar key={item.id} item={item} ocupado={moderar.isPending}
              onMantener={() => mantener(item.id)} onReabrir={() => setReabrirId(item.id)} />)}
          </tbody>
        </table>
        <PieTabla cantidad={items.length} reglas="RN-06 · RN-07" />
      </div>
      {reabrirId && <DialogoModeracion accion="Reabrir" idCorto={reabrirId.slice(0, 8)} cargando={moderar.isPending}
        onConfirmar={reabrir} onCancelar={() => setReabrirId(null)} />}
    </>
  )
}

interface PropsFila { item: ElementoPorRevisar; ocupado: boolean; onMantener: () => void; onReabrir: () => void }

function FilaPorRevisar({ item, ocupado, onMantener, onReabrir }: PropsFila) {
  return (
    <tr>
      <CeldaAnomalia id={item.id} tipo={item.tipo} severidad={item.severidadMostrada} />
      <td><Link to={`/mapa?anomalia=${item.id}`}>km {formatearKm(item.kmVia)}</Link></td>
      <td className="r"><b>{item.vehiculos7d}</b></td>
      <td className="r">{item.reportes}</td>
      <td>{formatearRelativo(item.ultimaDeteccionUtc, relojReal.ahora())}</td>
      <td className="r">
        <span style={{ display: 'inline-flex', gap: 'var(--s-5)' }}>
          <Boton variante="secundario" onClick={onMantener} disabled={ocupado}>{textos.accionMantenerDescartada}</Boton>
          <Boton variante="primario" onClick={onReabrir}>{textos.accionReabrir}</Boton>
        </span>
      </td>
    </tr>
  )
}

/** RN-06: «Mantener descartada» se aplica al instante (reinicia el contador); «Reabrir» publica la anomalía y se confirma. */
function useDecisionesPorRevisar() {
  const [reabrirId, setReabrirId] = useState<string | null>(null)
  const moderar = useModerar()
  const mantener = (id: string) => void moderar.mutateAsync({ id, accion: 'MantenerDescartada' })
  const reabrir = async () => {
    if (!reabrirId) return
    await moderar.mutateAsync({ id: reabrirId, accion: 'Reabrir' })
    setReabrirId(null)
  }
  return { reabrirId, setReabrirId, mantener, reabrir, moderar }
}
