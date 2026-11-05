import { useState, useId } from 'react'

interface Props { texto: string }

export function Ayuda({ texto }: Props) {
  const [visible, setVisible] = useState(false)
  const id = useId()
  return (
    <span style={{ position: 'relative', display: 'inline-block' }}>
      <button
        type="button"
        aria-label="Ayuda"
        aria-expanded={visible}
        aria-controls={id}
        onClick={() => setVisible(v => !v)}
        style={{ background: 'none', border: '1px solid var(--line-2)', borderRadius: '50%', width: 18, height: 18, cursor: 'pointer', fontSize: 11, color: 'var(--ink-3)' }}
      >
        ?
      </button>
      {visible && (
        <span
          id={id}
          role="tooltip"
          style={{ position: 'absolute', bottom: '120%', left: '50%', transform: 'translateX(-50%)', background: 'var(--frame)', color: 'var(--killa-ink)', padding: 'var(--s-8) var(--s-13)', borderRadius: 'var(--s-5)', fontSize: 'var(--t-13)', whiteSpace: 'nowrap', zIndex: 10 }}
        >
          {texto}
        </span>
      )}
    </span>
  )
}
