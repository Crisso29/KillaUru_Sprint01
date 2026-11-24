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
      {pestanas.map(pestana => (
        <button key={pestana.id} type="button" role="tab" aria-selected={activa === pestana.id} onClick={() => onChange(pestana.id)}>
          {pestana.etiqueta}
        </button>
      ))}
    </div>
  )
}
