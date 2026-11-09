// T075 — TarjetaIndicador: valor, variación y mini serie accesible (FR-015, FR-043)
import type { Indicador } from '@/api/dtos/004-dashboard'
import type { ClaveIndicador } from '../modelo/indicadores'
import { INDICADORES_META, formatearVariacion, formatearValor } from '../modelo/indicadores'
import { TEXTOS_RESUMEN } from '../textos'

interface Props {
  clave: ClaveIndicador
  dato: Indicador
}

export function TarjetaIndicador({ clave, dato }: Props) {
  const meta = INDICADORES_META[clave]
  const valor = formatearValor(dato.valor, dato.unidad)
  const variacion = formatearVariacion(clave, dato.variacion)
  const esMalo = meta.subirEsMalo && (dato.variacion ?? 0) > 0
  const esBueno = !meta.subirEsMalo && (dato.variacion ?? 0) > 0

  return (
    <article aria-label={meta.rotulo}>
      <h3>{meta.rotulo}</h3>
      <p aria-label="Valor" className="valor">
        {dato.valor === null ? (
          <span title={TEXTOS_RESUMEN.indicadores.sinCalcular}>—</span>
        ) : (
          valor
        )}
      </p>
      <p
        aria-label={`Variación ${TEXTOS_RESUMEN.indicadores.variacionSufijo}`}
        data-malo={esMalo || undefined}
        data-bueno={esBueno || undefined}
      >
        {variacion}
      </p>
      <p className="sufijo">{TEXTOS_RESUMEN.indicadores.variacionSufijo}</p>
      <span className="ayuda" role="note">
        {meta.ayuda}
      </span>
    </article>
  )
}
