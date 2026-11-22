interface Pestana { id: string; etiqueta: React.ReactNode }
interface Props {
  pestanas: Pestana[]
  activa: string
  onChange: (id: string) => void
  /** `panel`: pestañas del panel lateral (`.rp-tabs`); `seccion`: subpestañas de una página (`.subtabs`). */
  estilo?: 'panel' | 'seccion'
}

/** Pestañas accesibles con las clases del prototipo. */
export function Pestanas({ pestanas, activa, onChange, estilo = 'seccion' }: Props) {
  return (
    <div role="tablist" className={estilo === 'panel' ? 'rp-tabs' : 'subtabs'}>
      {pestanas.map(p => (
        <button key={p.id} type="button" role="tab" aria-selected={activa === p.id} onClick={() => onChange(p.id)}>
          {p.etiqueta}
        </button>
      ))}
    </div>
  )
}
