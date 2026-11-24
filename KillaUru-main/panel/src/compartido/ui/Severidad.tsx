interface Props {
  nivel: number
  tamano?: 'sm' | 'md' | 'lg'
}

/** Nombre de cada nivel (prototipo `SEV`): se lee en voz alta junto al número. */
export const NOMBRE_SEVERIDAD = ['', 'Muy leve', 'Leve', 'Moderada', 'Alta', 'Muy alta'] as const

/** Círculo de severidad del prototipo (`.sev`): color del nivel, número y anillo en 4 y 5. */
export function Severidad({ nivel, tamano = 'md' }: Props) {
  const nivelAcotado = Math.min(5, Math.max(1, Math.round(nivel)))
  return (
    <span className={`sev s${nivelAcotado}${tamano === 'md' ? '' : ` ${tamano}`}`} role="img"
      aria-label={`Severidad ${nivelAcotado}: ${NOMBRE_SEVERIDAD[nivelAcotado]}`}>
      {nivelAcotado}
    </span>
  )
}
