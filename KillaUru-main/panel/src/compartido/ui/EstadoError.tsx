import { Boton } from './Boton'
import type { ErrorApi } from '@/api/problema'
import { mensajeDeError } from '@/compartido/textos/errores'

interface Props { error: ErrorApi; onReintentar?: () => void; className?: string }

export function EstadoError({ error, onReintentar, className }: Props) {
  const msg = mensajeDeError(error)
  return (
    <div className={className} role="alert" style={{ padding: 'var(--s-21)', color: 'var(--bad)' }}>
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
