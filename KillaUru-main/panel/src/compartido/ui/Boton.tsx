import type { ButtonHTMLAttributes } from 'react'

type Variante = 'primario' | 'secundario' | 'peligro' | 'fantasma'

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: Variante
  cargando?: boolean
}

export function Boton({ variante = 'primario', cargando, children, disabled, className, ...rest }: Props) {
  return (
    <button
      className={`btn btn--${variante} ${className ?? ''}`}
      disabled={disabled || cargando}
      aria-busy={cargando}
      {...rest}
    >
      {cargando ? <span aria-hidden="true">…</span> : children}
    </button>
  )
}
