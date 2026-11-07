// T067 — TablaPendientes: solo lectura con Ver en el mapa (FR-029)
import { TEXTOS_MODERACION as T } from '../textos'
import type { ElementoPendiente } from '@/api/generado/003-anomalias'

interface Props {
  items: ElementoPendiente[]
}

export function TablaPendientes({ items }: Props) {
  if (items.length === 0) {
    return <p>{T.vacioPendientes}</p>
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Tipo</th>
          <th>km Vía</th>
          <th>Severidad</th>
          <th>Reportes</th>
          <th>Primer reporte</th>
          <th>Vence</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {items.map((item) => (
          <tr key={item.id}>
            <td>{item.tipo}</td>
            <td>{item.kmVia.toFixed(1)}</td>
            <td>{item.severidadMostrada}</td>
            <td>{item.reportes}</td>
            <td>{new Date(item.primerReporteUtc).toLocaleString('es-PE')}</td>
            <td>{new Date(item.venceUtc).toLocaleString('es-PE')}</td>
            <td>
              <a href={`/mapa?anomalia=${item.id}`}>{T.verEnMapa}</a>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
