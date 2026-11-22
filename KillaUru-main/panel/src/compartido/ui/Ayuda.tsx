import { useState, useId } from 'react'
import { Icono } from './Icono'

interface Props { texto: string }

/**
 * Ícono de información con su explicación (prototipo: ícono `info` y globo `.tip`). Se abre al pasar el ratón, al
 * enfocar con el teclado o al pulsar, y se cierra con Escape (WCAG 1.4.13: contenido al pasar el puntero).
 */
export function Ayuda({ texto }: Props) {
  const [visible, setVisible] = useState(false)
  const id = useId()
  return (
    <span style={{ position: 'relative', display: 'inline-flex' }}
      onMouseEnter={() => setVisible(true)} onMouseLeave={() => setVisible(false)}>
      <button
        type="button"
        aria-label="Ayuda"
        aria-expanded={visible}
        aria-describedby={visible ? id : undefined}
        onClick={() => setVisible(v => !v)}
        onFocus={() => setVisible(true)}
        onBlur={() => setVisible(false)}
        onKeyDown={e => { if (e.key === 'Escape') setVisible(false) }}
        style={{ background: 'none', border: 0, padding: 0, cursor: 'help', color: 'var(--ink-3)', display: 'inline-flex' }}
      >
        <Icono nombre="info" tamano={13} />
      </button>
      {visible && (
        <span id={id} role="tooltip" className="tip"
          style={{ bottom: '140%', left: '50%', transform: 'translateX(-50%)', width: 'max-content' }}>
          {texto}
        </span>
      )}
    </span>
  )
}
