import { formatearKm } from '@/compartido/formato/km'

/** Hito kilométrico del prototipo (`.hito`): la placa amarilla con «KM 287,4» que se ve en la vía. */
export function Hito({ km }: { km: number }) {
  return (
    <span className="hito" role="img" aria-label={`Kilómetro ${formatearKm(km)}`}>
      <i /><span><em>KM</em>{formatearKm(km)}</span>
    </span>
  )
}
