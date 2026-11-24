import { describe, it, expect } from 'vitest'
import { readdirSync, readFileSync } from 'fs'
import { join, relative } from 'path'

// T005: FR-047 — ningún archivo .ts/.tsx supera 199 líneas (excepto src/api/generado/)
const LIMITE = 199
const RAIZ = join(__dirname, '../..')
const SRC = join(RAIZ, 'src')
const PRUEBAS = join(RAIZ, 'pruebas')

function listarArchivos(dir: string): string[] {
  const entradas = readdirSync(dir, { withFileTypes: true })
  return entradas.flatMap((entrada) => {
    const ruta = join(dir, entrada.name)
    if (entrada.isDirectory()) return listarArchivos(ruta)
    if (entrada.isFile() && (entrada.name.endsWith('.ts') || entrada.name.endsWith('.tsx'))) return [ruta]
    return []
  })
}

describe('FR-047: límite de líneas por archivo', () => {
  it(`ningún .ts/.tsx en src/ o pruebas/ supera ${LIMITE} líneas`, () => {
    // Arrange
    const archivos = [
      ...listarArchivos(SRC),
      ...listarArchivos(PRUEBAS),
    ].filter(
      (archivo) => !archivo.includes('/api/generado/') && !archivo.includes('\\api\\generado\\'),
    )


    // Act
    const infractores = archivos
      .map((ruta) => {
        const lineas = readFileSync(ruta, 'utf-8').split('\n').length
        return { ruta: relative(RAIZ, ruta), lineas }
      })
      .filter(({ lineas }) => lineas > LIMITE)


    // Assert
    expect(infractores, `Archivos con > ${LIMITE} líneas:\n${
      infractores.map(({ ruta, lineas }) => `  ${ruta}: ${lineas}`).join('\n')
    }`).toHaveLength(0)
  })
})
