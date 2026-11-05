import { describe, it, expect } from 'vitest'
import { readdirSync, readFileSync, statSync } from 'fs'
import { join, relative } from 'path'

// T005: FR-047 — ningún archivo .ts/.tsx supera 199 líneas (excepto src/api/generado/)
const LIMITE = 199
const RAIZ = join(__dirname, '../..')
const SRC = join(RAIZ, 'src')
const PRUEBAS = join(RAIZ, 'pruebas')

function listarArchivos(dir: string): string[] {
  const entradas = readdirSync(dir, { withFileTypes: true })
  return entradas.flatMap((e) => {
    const ruta = join(dir, e.name)
    if (e.isDirectory()) return listarArchivos(ruta)
    if (e.isFile() && (e.name.endsWith('.ts') || e.name.endsWith('.tsx'))) return [ruta]
    return []
  })
}

describe('FR-047: límite de líneas por archivo', () => {
  it(`ningún .ts/.tsx en src/ o pruebas/ supera ${LIMITE} líneas`, () => {
    const archivos = [
      ...listarArchivos(SRC),
      ...listarArchivos(PRUEBAS),
    ].filter(
      (f) => !f.includes('/api/generado/') && !f.includes('\\api\\generado\\'),
    )

    const infractores = archivos
      .map((ruta) => {
        const lineas = readFileSync(ruta, 'utf-8').split('\n').length
        return { ruta: relative(RAIZ, ruta), lineas }
      })
      .filter(({ lineas }) => lineas > LIMITE)

    expect(infractores, `Archivos con > ${LIMITE} líneas:\n${
      infractores.map(({ ruta, lineas }) => `  ${ruta}: ${lineas}`).join('\n')
    }`).toHaveLength(0)
  })
})
