interface Pestana { id: string; etiqueta: React.ReactNode }
interface Props { pestanas: Pestana[]; activa: string; onChange: (id: string) => void }

export function Pestanas({ pestanas, activa, onChange }: Props) {
  return (
    <div role="tablist" style={{ display: 'flex', borderBottom: '2px solid var(--line)' }}>
      {pestanas.map(p => (
        <button
          key={p.id}
          role="tab"
          aria-selected={activa === p.id}
          onClick={() => onChange(p.id)}
          style={{
            padding: 'var(--s-8) var(--s-21)',
            border: 'none',
            background: 'none',
            cursor: 'pointer',
            borderBottom: activa === p.id ? '2px solid var(--killa)' : '2px solid transparent',
            color: activa === p.id ? 'var(--killa)' : 'var(--ink-2)',
            fontWeight: activa === p.id ? 700 : 400,
            fontFamily: 'var(--f-body)',
            fontSize: 'var(--t-16)',
            marginBottom: -2,
          }}
        >
          {p.etiqueta}
        </button>
      ))}
    </div>
  )
}
