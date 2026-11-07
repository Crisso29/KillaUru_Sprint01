// T021 — tiempo relativo en español, zona Lima
import { formatearHoraLima } from './fechas'

const ZONA = 'America/Lima'
const UN_MIN = 60_000
const UNA_HORA = 3600_000
const UN_DIA = 86400_000

function diaEnLima(d: Date): string {
  return d.toLocaleDateString('es-PE', { timeZone: ZONA, year: 'numeric', month: '2-digit', day: '2-digit' })
}

export function formatearRelativo(iso: string, ahora: Date): string {
  const ts = new Date(iso)
  const diff = ahora.getTime() - ts.getTime()

  if (diff < UN_MIN) return 'ahora mismo'

  // Comparar días de calendario en zona Lima antes que el umbral de horas
  const diaTs = diaEnLima(ts)
  const diaHoy = diaEnLima(ahora)
  const diaAyer = diaEnLima(new Date(ahora.getTime() - UN_DIA))

  if (diaTs === diaHoy) {
    if (diff < UNA_HORA) {
      const min = Math.floor(diff / UN_MIN)
      return `hace ${min} min`
    }
    const h = Math.floor(diff / UNA_HORA)
    return `hace ${h} h`
  }

  if (diaTs === diaAyer) {
    return `ayer a las ${formatearHoraLima(iso)}`
  }

  const dias = Math.floor(diff / UN_DIA)
  return `hace ${dias} d`
}
