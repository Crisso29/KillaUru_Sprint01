// T067 — TablaPorRevisar: filas con MantenerDescartada (sin diálogo) y Reabrir (con diálogo)
import { useState } from 'react'
import { Boton } from '@/compartido/ui'
import { useModerar } from '../api/useModerar'
import { DialogoModeracion } from './DialogoModeracion'
import { TEXTOS_MODERACION as T } from '../textos'
import type { ElementoPorRevisar } from '@/api/generado/003-anomalias'

interface Props {
  items: ElementoPorRevisar[]
}

export function TablaPorRevisar({ items }: Props) {
  const [reabrirId, setRreabrirId] = useState<string | null>(null)
  const moderar = useModerar()

  async function mantener(id: string) {
    await moderar.mutateAsync({ id, accion: 'MantenerDescartada' })
  }

  async function reabrir() {
    if (!reabrirId) return
    await moderar.mutateAsync({ id: reabrirId, accion: 'Reabrir' })
    setRreabrirId(null)
  }

  if (items.length === 0) {
    return <p>{T.vacioPorRevisar}</p>
  }

  return (
    <>
      <table>
        <thead>
          <tr>
            <th>Tipo</th>
            <th>km Vía</th>
            <th>Severidad</th>
            <th>Vehículos 7d</th>
            <th>Reportes</th>
            <th>Última detección</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id}>
              <td>{item.tipo}</td>
              <td>{item.kmVia.toFixed(1)}</td>
              <td>{item.severidadMostrada}</td>
              <td>{item.vehiculos7d}</td>
              <td>{item.reportes}</td>
              <td>{new Date(item.ultimaDeteccionUtc).toLocaleString('es-PE')}</td>
              <td style={{ display: 'flex', gap: 'var(--s-08)' }}>
                <Boton
                  variante="secundario"
                  onClick={() => mantener(item.id)}
                  disabled={moderar.isPending}
                >
                  {T.accionMantenerDescartada}
                </Boton>
                <Boton
                  variante="primario"
                  onClick={() => setRreabrirId(item.id)}
                >
                  {T.accionReabrir}
                </Boton>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {reabrirId && (
        <DialogoModeracion
          accion="Reabrir"
          idCorto={reabrirId.slice(0, 8)}
          cargando={moderar.isPending}
          onConfirmar={reabrir}
          onCancelar={() => setRreabrirId(null)}
        />
      )}
    </>
  )
}
