// T021 — formato de kilómetros del corredor (1 decimal, coma decimal)
export function formatearKm(km: number): string {
  return km.toLocaleString('en-US', { minimumFractionDigits: 1, maximumFractionDigits: 1, useGrouping: false })
    .replace('.', ',')
}
