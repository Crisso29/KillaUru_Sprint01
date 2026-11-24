// T061 — resultado de abrir un enlace del correo (verificar, restablecer, invitación): mensaje y vuelta al login
import { Link } from 'react-router'
import { PaginaAcceso } from '@/compartido/ui'

interface Props { titulo: string; mensaje: string; exito: boolean }

export function ResultadoEnlace({ titulo, mensaje, exito }: Props) {
  return (
    <PaginaAcceso titulo={titulo}>
      <p role={exito ? 'status' : 'alert'} className={exito ? 'note ok' : 'note bad'}>{mensaje}</p>
      <Link to="/login" className="lnk">Ir al inicio de sesión</Link>
    </PaginaAcceso>
  )
}
