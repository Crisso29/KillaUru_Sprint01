// T084 — ExportacionesRecientes: lista las últimas exportaciones del dataset
import { useRecientes } from '../api/useRecientes'
import { useActores } from '@/funcionalidades/usuarios/api/useActores'
import { TEXTOS_EXPORTACION as T } from '../textos'

interface Props { correoActual: string }

export function ExportacionesRecientes({ correoActual }: Props) {
  const { data: registros = [] } = useRecientes()
  const actorIds = registros.map(r => r.actorId)
  const actores = useActores(actorIds)

  if (registros.length === 0) {
    return <p style={{ color: 'var(--ink-3)' }}>{T.sinRecientes}</p>
  }

  return (
    <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
      {registros.map(r => {
        const correoActor = r.actorId ? (actores[r.actorId] ?? r.actorId) : 'sistema'
        const esTu = correoActor === correoActual
        return (
          <li key={r.id} style={{ padding: 'var(--s-5) 0', borderBottom: '1px solid var(--border)' }}>
            <span style={{ marginRight: 'var(--s-8)' }}>
              {new Date(r.ocurridoUtc).toLocaleString('es-PE', { timeZone: 'America/Lima' })}
            </span>
            <strong>{esTu ? T.tu : correoActor}</strong>
          </li>
        )
      })}
    </ul>
  )
}
