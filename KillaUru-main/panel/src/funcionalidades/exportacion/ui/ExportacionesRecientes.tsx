// T084 — ExportacionesRecientes: lista las últimas exportaciones del dataset
import { useRecientes } from '../api/useRecientes'
import { formatearFechaLima, formatearHoraLima } from '@/compartido/formato/fechas'
import { useActores } from '@/funcionalidades/usuarios/api/useActores'
import { TEXTOS_EXPORTACION as textos } from '../textos'

interface Props { correoActual: string }

export function ExportacionesRecientes({ correoActual }: Props) {
  const { data: registros = [] } = useRecientes()
  const actores = useActores(registros.map((registro) => registro.actorId))
  if (registros.length === 0) return <p className="card-b hint">{textos.sinRecientes}</p>
  return (
    <table className="dt" style={{ marginTop: 'var(--s-8)' }}>
      <thead><tr><th scope="col">Fecha</th><th scope="col">Quién</th></tr></thead>
      <tbody>
        {registros.map((registro) => (
          <tr key={registro.id}>
            <td><b>{formatearFechaLima(registro.ocurridoUtc)} {formatearHoraLima(registro.ocurridoUtc)}</b></td>
            <td>{quienExporto(registro.actorId, actores, correoActual)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

/** «tú» si fue el administrador conectado; si no, su correo (o el id mientras llega la búsqueda de correos). */
function quienExporto(actorId: string | null, actores: Record<string, string>, correoActual: string): string {
  const correo = actorId ? actores[actorId] ?? actorId : 'sistema'
  return correo === correoActual ? textos.tu : correo
}
