import type { ButtonHTMLAttributes } from 'react'

type Variante = 'primario' | 'secundario' | 'peligro' | 'fantasma'

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: Variante
  cargando?: boolean
}

/** Variante → clases del prototipo (`.ob`, `.ob.pri`, `.ob.dngf`, `.lnk`). Antes usaba `btn--*`, que no existían. */
const CLASE: Record<Variante, string> = { primario: 'ob pri', secundario: 'ob', peligro: 'ob dngf', fantasma: 'lnk' }

export function Boton({ variante = 'primario', cargando, children, disabled, className, type = 'button', ...rest }: Props) {
  return (
    <button
      type={type}
      className={`${CLASE[variante]} ${className ?? ''}`.trim()}
      disabled={disabled || cargando}
      aria-busy={cargando}
      {...rest}
    >
      {cargando ? <span aria-hidden="true">…</span> : children}
    </button>
  )
}
