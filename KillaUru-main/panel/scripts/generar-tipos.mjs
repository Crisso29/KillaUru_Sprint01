// Genera los contratos canonicos; los DTO de presentacion viven en src/api/dtos.
import openapiTS, { astToString } from 'openapi-typescript'
import { writeFileSync, mkdirSync } from 'node:fs'
import { resolve } from 'node:path'
import { pathToFileURL } from 'node:url'

const contratos = [
  ['identidad', '003-modulo-identidad-accesos', 'identidad'],
  ['mapa', '006-modulo-consulta-mapa', 'mapa'],
  ['anomalias', '004-modulo-anomalias', 'anomalias'],
  ['dashboard', '007-modulo-dashboard', 'dashboard'],
  ['auditoria', '002-modulo-auditoria', 'auditoria'],
]
const destino = resolve(import.meta.dirname, '../src/api/generado')
mkdirSync(destino, { recursive: true })
for (const [nombre, spec, contrato] of contratos) {
  const ruta = resolve(import.meta.dirname, `../../specs/${spec}/contracts/${contrato}.openapi.yaml`)
  const ast = await openapiTS(pathToFileURL(ruta))
  writeFileSync(resolve(destino, `${nombre}.d.ts`), astToString(ast))
}
