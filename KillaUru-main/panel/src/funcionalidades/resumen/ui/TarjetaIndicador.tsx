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

/** Verde si mejora, rojo si empeora; en Reaparición subir es malo (prototipo: `--ok` / `--bad`). */
function colorVariacion(subirEsMalo: boolean, variacion: number | null): string | undefined {
  if (!variacion) return undefined
  return (variacion > 0) !== subirEsMalo ? 'var(--ok)' : 'var(--bad)'
}

export function TarjetaIndicador({ clave, dato }: Props) {
  const meta = INDICADORES_META[clave]
  const esPorcentaje = dato.unidad === '%'
  const color = colorVariacion(meta.subirEsMalo, dato.variacion)
  const serie = dato.serie.filter((v): v is number => v !== null)

  return (
    <article className="k" aria-label={meta.rotulo}>
      <span className="l">
        <h3 style={{ font: 'inherit' }}>{meta.rotulo}</h3>
        <Ayuda texto={meta.ayuda} />
      </span>
      <p className="v" aria-label="Valor">
        {dato.valor === null ? (
          <span title={TEXTOS_RESUMEN.indicadores.sinCalcular}>—</span>
        ) : (
          <>{formatearValor(dato.valor)}{esPorcentaje && <small>%</small>}</>
        )}
      </p>
      <span className="d">
        <span>
          <b style={{ color }} aria-label={`Variación ${TEXTOS_RESUMEN.indicadores.variacionSufijo}`}>
            {formatearVariacion(clave, dato.variacion)}
          </b>{' '}
          {TEXTOS_RESUMEN.indicadores.variacionSufijo}
        </span>
        <Minigrafico datos={serie} ancho={55} alto={21} color={color === 'var(--bad)' ? 'var(--bad)' : 'var(--killa)'} />
      </span>
    </article>
  )
}
