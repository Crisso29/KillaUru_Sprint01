import { Boton } from './Boton'
import type { ErrorApi } from '@/api/problema'
import { mensajeDeError } from '@/compartido/textos/errores'

interface Props { error: ErrorApi; onReintentar?: () => void; className?: string }

export function EstadoError({ error, onReintentar, className }: Props) {
  const msg = mensajeDeError(error)
  return (
    <div className={`note bad ${className ?? ''}`.trim()} role="alert" style={{ margin: 'var(--s-13)', display: 'block' }}>
      <p>{msg}</p>
      {error.correlacionId && (
        <p style={{ fontSize: 'var(--t-13)', color: 'var(--ink-3)', marginTop: 'var(--s-5)' }}>
          Código de soporte: {error.correlacionId}
        </p>
      )}
      {onReintentar && (
        <Boton variante="secundario" onClick={onReintentar} style={{ marginTop: 'var(--s-13)' }}>
          Reintentar
        </Boton>
      )}
    </div>
  )
}
