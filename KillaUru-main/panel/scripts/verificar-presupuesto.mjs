import { readdirSync, readFileSync } from 'node:fs'
import { gzipSync } from 'node:zlib'

const archivos = readdirSync('dist/assets')
for (const [prefijo, limite] of [['index-', 120_000], ['mapa-', 300_000]]) {
  const seleccion = archivos.filter(a => a.startsWith(prefijo) && a.endsWith('.js'))
  if (!seleccion.length) throw new Error(`No se encontro el bundle ${prefijo}`)
  const bytes = seleccion.reduce((total, a) => total + gzipSync(readFileSync(`dist/assets/${a}`)).length, 0)
  console.log(`${prefijo}: ${bytes} bytes gzip / ${limite}`)
  if (bytes > limite) throw new Error(`Presupuesto excedido: ${prefijo}`)
}
