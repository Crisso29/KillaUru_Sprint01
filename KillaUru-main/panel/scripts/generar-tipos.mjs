// T006: genera tipos TypeScript desde los contratos OpenAPI YAML
// Ejecutar: npm run tipos
import { execSync } from 'child_process'
import { existsSync } from 'fs'
import { join } from 'path'

const CONTRATOS = [
  ['001-identidad', '../specs/contratos/001-identidad.yaml'],
  ['002-mapa', '../specs/contratos/002-mapa.yaml'],
  ['003-anomalias', '../specs/contratos/003-anomalias.yaml'],
  ['004-dashboard', '../specs/contratos/004-dashboard.yaml'],
  ['006-auditoria', '../specs/contratos/006-auditoria.yaml'],
  ['007-exportacion', '../specs/contratos/007-exportacion.yaml'],
]

const DESTINO = 'src/api/generado'

for (const [nombre, origen] of CONTRATOS) {
  const ruta = join(process.cwd(), origen)
  if (!existsSync(ruta)) {
    console.warn(`Contrato no encontrado (omitido): ${ruta}`)
    continue
  }
  const salida = `${DESTINO}/${nombre}.d.ts`
  execSync(`npx openapi-typescript "${ruta}" -o "${salida}"`, { stdio: 'inherit' })
  console.log(`✓ ${nombre} → ${salida}`)
}
