// T075 — TarjetaIndicador: valor, variación y minigráfico de 7 días (FR-015, FR-043; prototipo `.k`)
import type { Indicador } from '@/api/dtos/004-dashboard'
import type { ClaveIndicador } from '../modelo/indicadores'
import { INDICADORES_META, formatearVariacion, formatearValor } from '../modelo/indicadores'
import { TEXTOS_RESUMEN } from '../textos'
import { Ayuda, Minigrafico } from '@/compartido/ui'

interface Props {
  clave: ClaveIndicador
  dato: Indicador
}

const { sinCalcular, variacionSufijo } = TEXTOS_RESUMEN.indicadores

/** Verde si mejora, rojo si empeora; en Reaparición subir es malo (prototipo: `--ok` / `--bad`). */
function colorVariacion(subirEsMalo: boolean, variacion: number | null): string | undefined {
  if (!variacion) return undefined
  return (variacion > 0) !== subirEsMalo ? 'var(--ok)' : 'var(--bad)'
}

export function TarjetaIndicador({ clave, dato }: Props) {
  const meta = INDICADORES_META[clave]
  const color = colorVariacion(meta.subirEsMalo, dato.variacion)
  const serie = dato.serie.filter((valor): valor is number => valor !== null)
  return (
    <article className="k" aria-label={meta.rotulo}>
      <span className="l"><h3 style={{ font: 'inherit' }}>{meta.rotulo}</h3><Ayuda texto={meta.ayuda} /></span>
      <p className="v" aria-label="Valor"><Valor dato={dato} /></p>
      <span className="d">
        <span>
          <b style={{ color }} aria-label={`Variación ${variacionSufijo}`}>{formatearVariacion(clave, dato.variacion)}</b>{' '}{variacionSufijo}
        </span>
        <Minigrafico datos={serie} ancho={55} alto={21} color={color === 'var(--bad)' ? 'var(--bad)' : 'var(--killa)'} />
      </span>
    </article>
  )
}

/** «—» con la explicación al pasar el puntero cuando el indicador aún no se calculó (p. ej. sin datos del día). */
function Valor({ dato }: { dato: Indicador }) {
  if (dato.valor === null) return <span title={sinCalcular}>—</span>
  return <>{formatearValor(dato.valor)}{dato.unidad === '%' && <small>%</small>}</>
}
