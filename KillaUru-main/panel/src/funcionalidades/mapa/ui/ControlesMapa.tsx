// Controles del mapa del prototipo (`.mctl`): acercar, alejar y encuadrar todas las anomalías.
import { Icono } from '@/compartido/ui'

interface Props {
  onAcercar: () => void
  onAlejar: () => void
  onEncuadrar: () => void
}

export function ControlesMapa({ onAcercar, onAlejar, onEncuadrar }: Props) {
  return (
    <div className="mctl">
      <div className="grp">
        <button type="button" onClick={onAcercar} aria-label="Acercar"><Icono nombre="plus" tamano={14} /></button>
        <button type="button" onClick={onAlejar} aria-label="Alejar"><Icono nombre="minus" tamano={14} /></button>
      </div>
      <div className="grp">
        <button type="button" onClick={onEncuadrar} aria-label="Ver todas las anomalías"><Icono nombre="expand" tamano={14} /></button>
      </div>
    </div>
  )
}
