// T084 — nombre de archivo por formato y rango de fechas
export function nombreArchivo(formato: 'CSV' | 'GeoJSON', desde: string, hasta: string): string {
  const ext = formato === 'GeoJSON' ? 'geojson' : 'csv'
  if (!desde && !hasta) return `killauru-anomalias.${ext}`
  const f = (d: string) => d.replace(/-/g, '')
  return `killauru-anomalias-${f(desde)}-${f(hasta)}.${ext}`
}

export function iniciarDescarga(blob: Blob, nombre: string): void {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = nombre
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}
