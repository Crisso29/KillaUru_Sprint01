// T021 — tiempo relativo en español, zona Lima
import { formatearHoraLima } from './fechas'

const ZONA = 'America/Lima'
const UN_MIN = 60_000
const UNA_HORA = 3600_000
const UN_DIA = 86400_000

function diaEnLima(fecha: Date): string {
  return fecha.toLocaleDateString('es-PE', { timeZone: ZONA, year: 'numeric', month: '2-digit', day: '2-digit' })
}

export function formatearRelativo(iso: string, ahora: Date): string {
  const diferencia = ahora.getTime() - new Date(iso).getTime()
  if (diferencia < UN_MIN) return 'ahora mismo'
  // Se compara el día de calendario en Lima antes que las horas: algo de las 23:00 de ayer visto a las 08:00 es «ayer»,
  // no «hace 9 h».
  const dia = diaEnLima(new Date(iso))
  if (dia === diaEnLima(ahora)) {
    return diferencia < UNA_HORA ? `hace ${Math.floor(diferencia / UN_MIN)} min` : `hace ${Math.floor(diferencia / UNA_HORA)} h`
  }
  if (dia === diaEnLima(new Date(ahora.getTime() - UN_DIA))) return `ayer a las ${formatearHoraLima(iso)}`
  return `hace ${Math.floor(diferencia / UN_DIA)} d`
}
