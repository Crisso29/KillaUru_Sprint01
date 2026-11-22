// T021 — fechas en zona horaria Lima (America/Lima = UTC-5, sin horario de verano)
const ZONA = 'America/Lima'

const FMT_FECHA = new Intl.DateTimeFormat('es-PE', {
  timeZone: ZONA,
  day: 'numeric',
  month: 'short',
})

const FMT_HORA = new Intl.DateTimeFormat('es-PE', {
  timeZone: ZONA,
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
})

export function formatearFechaLima(iso: string): string {
  const fecha = new Date(iso)
  return FMT_FECHA.format(fecha)
    .replace('.', '')  // quita el punto del mes abreviado
    .toLowerCase()
}

export function formatearHoraLima(iso: string): string {
  return FMT_HORA.format(new Date(iso))
}

export function enLima(iso: string): Date {
  // Devuelve un Date en hora Lima (simulado como local)
  const utc = new Date(iso)
  const limaOffset = -5 * 60 * 60_000
  return new Date(utc.getTime() + limaOffset)
}

const FMT_DIA_LARGO = new Intl.DateTimeFormat('es-PE', { timeZone: ZONA, weekday: 'long', day: 'numeric', month: 'long' })

/** «Martes 20 de octubre» en hora de Lima (subtítulo del Resumen en el prototipo). */
export function formatearDiaLargoLima(iso: string): string {
  const texto = FMT_DIA_LARGO.format(new Date(iso)).replace(',', '')
  return texto.charAt(0).toUpperCase() + texto.slice(1)
}

const FMT_UTC_CORTO = new Intl.DateTimeFormat('es-PE', { timeZone: 'UTC', day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit', hour12: false })

/** «20 oct 10:12 UTC»: historial, vencimientos y auditoría se muestran en UTC (prototipo). */
export function formatearUtcCorto(iso: string): string {
  return `${FMT_UTC_CORTO.format(new Date(iso)).replace('.', '').replace(',', '')} UTC`
}
