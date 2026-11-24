// T084 — nombre de archivo por formato y rango de fechas
export function nombreArchivo(formato: 'CSV' | 'GeoJSON', desde: string, hasta: string): string {
  const ext = formato === 'GeoJSON' ? 'geojson' : 'csv'
  if (!desde && !hasta) return `killauru-anomalias.${ext}`
  const sinGuiones = (fecha: string) => fecha.replace(/-/g, '')
  return `killauru-anomalias-${sinGuiones(desde)}-${sinGuiones(hasta)}.${ext}`
}

export function iniciarDescarga(blob: Blob, nombre: string): void {
  const url = URL.createObjectURL(blob)
  const enlace = document.createElement('a')
  enlace.href = url
  enlace.download = nombre
  document.body.appendChild(enlace)
  enlace.click()
  document.body.removeChild(enlace)
  URL.revokeObjectURL(url)
}
