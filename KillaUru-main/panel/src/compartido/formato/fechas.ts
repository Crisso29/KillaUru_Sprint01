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
