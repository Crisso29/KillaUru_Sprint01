// T031 — modelo de bloqueo de cuenta (data-model.md §2, R-020)
const ZONA = 'America/Lima'

export function minutosRestantes(bloqueadoHastaUtc: Date, ahora: Date): number {
  const ms = bloqueadoHastaUtc.getTime() - ahora.getTime()
  return Math.ceil(ms / 60_000)
}

export function horaEnLima(d: Date): string {
  return d.toLocaleTimeString('es-PE', {
    timeZone: ZONA,
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
}

export function estaDesbloqueado(bloqueadoHastaUtc: Date, ahora: Date): boolean {
  return ahora.getTime() >= bloqueadoHastaUtc.getTime()
}
