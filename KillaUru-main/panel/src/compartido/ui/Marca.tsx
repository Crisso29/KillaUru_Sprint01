import { LogoSenal } from './LogoSenal'

interface Props {
  tamano?: 'sm' | 'md' | 'lg'
  /** Muestra «Ops» junto al nombre, como la barra lateral del prototipo (`.ops-logo`). */
  conOps?: boolean
}

const LADO = { sm: 28, md: 34, lg: 55 } as const

/** Marca de KillaUru: logo «Señal» y el nombre en Overpass (prototipo `.ops-logo` y `.brand`). */
export function Marca({ tamano = 'md', conOps = false }: Props) {
  return (
    <span className={`marca marca--${tamano}`}>
      <LogoSenal tamano={LADO[tamano]} />
      <b className="marca__nombre">KillaUru</b>
      {conOps && <small className="marca__ops">Ops</small>}
    </span>
  )
}
