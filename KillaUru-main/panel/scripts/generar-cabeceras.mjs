// T027: escribe dist/_headers con las variables de entorno reemplazadas
import { readFileSync, writeFileSync, mkdirSync } from 'fs'
import { join } from 'path'

const PLANTILLA = join(import.meta.dirname, '../cabeceras/_headers.plantilla')
const DESTINO = join(import.meta.dirname, '../dist/_headers')

const urlApi = process.env['VITE_URL_API'] ?? ''
const urlCdn = process.env['VITE_URL_CDN'] ?? ''
const sitekey = process.env['VITE_TURNSTILE_SITEKEY'] ?? ''

if (!urlApi) { console.error('VITE_URL_API requerida'); process.exit(1) }

const contenido = readFileSync(PLANTILLA, 'utf-8')
  .replace(/\{\{VITE_URL_API\}\}/g, urlApi)
  .replace(/\{\{VITE_URL_CDN\}\}/g, urlCdn)
  .replace(/\{\{VITE_TURNSTILE_SITEKEY\}\}/g, sitekey)

mkdirSync(join(import.meta.dirname, '../dist'), { recursive: true })
writeFileSync(DESTINO, contenido, 'utf-8')
console.log(`✓ _headers → ${DESTINO}`)
