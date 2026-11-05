// T021 — formato de kilómetros del corredor
const FMT = new Intl.NumberFormat('es-PE', { minimumFractionDigits: 1, maximumFractionDigits: 1 })

export function formatearKm(km: number): string {
  return FMT.format(km)
}
