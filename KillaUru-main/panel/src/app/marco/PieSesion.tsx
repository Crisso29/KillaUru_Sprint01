import { Icono } from '@/compartido/ui'
import { formatearHoraLima } from '@/compartido/formato/fechas'
import { fraccionRestante } from '@/sesion/planificador'
import { iniciales } from '@/compartido/formato/iniciales'

interface Props {
  correo: string
  sesionVenceUtc: Date
  urlEstado: string
  onSalir: () => void
}

/** Pie de la barra lateral (`.ops-me`): quién está, hasta cuándo dura la sesión (8 h máx.), estado del servicio y salir. */
export function PieSesion({ correo, sesionVenceUtc, urlEstado, onSalir }: Props) {
  return (
    <div className="ops-me">
      <div className="me">
        <span className="av" aria-hidden="true">{iniciales(correo)}</span>
        <div><b title={correo}>{correo.split('@')[0] ?? correo}</b><small>Administrador</small></div>
      </div>
      <TiempoSesion venceUtc={sesionVenceUtc} />
      <div className="ops-links">
        <a className="lnk" href={urlEstado} target="_blank" rel="noopener noreferrer"><Icono nombre="ext" tamano={12} />Estado del servicio</a>
        <button type="button" className="lnk" onClick={onSalir}><Icono nombre="out" tamano={12} />Salir</button>
      </div>
    </div>
  )
}

/** Hora de fin y barra con lo que queda de las 8 h (se lee como progressbar). */
function TiempoSesion({ venceUtc }: { venceUtc: Date }) {
  const porcentaje = Math.round(fraccionRestante(venceUtc, new Date()) * 100)
  return (
    <div className="sess">
      <Icono nombre="clock" tamano={12} />
      <span>Sesión hasta {formatearHoraLima(venceUtc.toISOString())}</span>
      <span className="bar" role="progressbar" aria-label="Tiempo de sesión restante" aria-valuemin={0} aria-valuemax={100} aria-valuenow={porcentaje}>
        <i style={{ width: `${porcentaje}%` }} />
      </span>
    </div>
  )
}
