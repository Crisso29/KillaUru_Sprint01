// T084 — ExportacionesRecientes: lista las últimas exportaciones del dataset
import { useRecientes } from '../api/useRecientes'
import { formatearFechaLima, formatearHoraLima } from '@/compartido/formato/fechas'
import { useActores } from '@/funcionalidades/usuarios/api/useActores'
import { TEXTOS_EXPORTACION as T } from '../textos'

interface Props { correoActual: string }

export function ExportacionesRecientes({ correoActual }: Props) {
  const { data: registros = [] } = useRecientes()
  const actorIds = registros.map(r => r.actorId)
  const actores = useActores(actorIds)

  if (registros.length === 0) {
    return <p className="card-b hint">{T.sinRecientes}</p>
  }

  return (
    <table className="dt" style={{ marginTop: 'var(--s-8)' }}>
      <thead><tr><th scope="col">Fecha</th><th scope="col">Quién</th></tr></thead>
      <tbody>
        {registros.map(r => {
          const correoActor = r.actorId ? (actores[r.actorId] ?? r.actorId) : 'sistema'
          return (
            <tr key={r.id}>
              <td><b>{formatearFechaLima(r.ocurridoUtc)} {formatearHoraLima(r.ocurridoUtc)}</b></td>
              <td>{correoActor === correoActual ? T.tu : correoActor}</td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}
