import { describe, it, expect } from 'vitest'
import { readFileSync } from 'fs'
import { join } from 'path'

// T017 — WCAG: texto ≥ 4,5:1; gráficos ≥ 3:1

function hexARgb(hex: string) {
  const hexadecimal = hex.replace('#', '')
  return [
    parseInt(hexadecimal.slice(0, 2), 16) / 255,
    parseInt(hexadecimal.slice(2, 4), 16) / 255,
    parseInt(hexadecimal.slice(4, 6), 16) / 255,
  ] as [number, number, number]
}

function lineal(canal: number) {
  return canal <= 0.03928 ? canal / 12.92 : ((canal + 0.055) / 1.055) ** 2.4
}

function luminancia([rojo, verde, azul]: [number, number, number]) {
  return 0.2126 * lineal(rojo) + 0.7152 * lineal(verde) + 0.0722 * lineal(azul)
}

function contraste(hex1: string, hex2: string) {
  const l1 = luminancia(hexARgb(hex1))
  const l2 = luminancia(hexARgb(hex2))
  const [hi, lo] = l1 >= l2 ? [l1, l2] : [l2, l1]
  return (hi + 0.05) / (lo + 0.05)
}

function extraerTokens(css: string) {
  const tokens: Record<string, string> = {}
  for (const coincidencia of css.matchAll(/--([a-z0-9-]+):\s*(#[0-9a-fA-F]{6})/g)) {
    tokens[`--${coincidencia[1]}`] = coincidencia[2]!
  }
  return tokens
}

function cargarTema(css: string, esOscuro: boolean) {
  if (!esOscuro) {
    const bloque = css.match(/:root\s*\{([^}]+)\}/)?.[1] ?? ''
    return extraerTokens(bloque)
  }
  const bloque = css.match(/\[data-theme="dark"\]\s*\{([^}]*)\}/s)?.[1] ?? ''
  return extraerTokens(bloque)
}

const CSS_PATH = join(__dirname, 'tokens.css')

describe('Contraste WCAG — tokens.css', () => {
  const css = readFileSync(CSS_PATH, 'utf-8')

  for (const [tema, esOscuro] of [['claro', false], ['oscuro', true]] as const) {
    describe(`Tema ${tema}`, () => {
      const tema = cargarTema(css, esOscuro)

      // Texto: 4,5:1 (WCAG 1.4.3). Elementos gráficos (marca, barras de severidad): 3:1 (WCAG 1.4.11).
      it.each([
        ['--ink', '--surface', 4.5],
        ['--ink', '--bg', 4.5],
        // Texto secundario (subtítulos, notas `.hint`, pestaña inactiva): axe lo encontró en 4,33:1 sobre --bg (E2E)
        ['--ink-3', '--surface', 4.5],
        ['--ink-3', '--bg', 4.5],
        ['--ink-3', '--killa-soft', 4.5], // notas dentro de la opción elegida (formato de exportación)
        ['--killa', '--surface', 3],
        ['--s1', '--surface', 3],
        ['--s2', '--surface', 3],
        ['--s3', '--surface', 3],
      ])('%s sobre %s ≥ %s:1', (primerPlano, fondo, minimo) => {
        // Arrange
        const colores = [tema[primerPlano], tema[fondo]]
        expect(colores.every(Boolean), `faltan ${primerPlano} o ${fondo} en tokens.css`).toBe(true)

        // Act
        const razon = contraste(colores[0]!, colores[1]!)

        // Assert
        expect(razon).toBeGreaterThanOrEqual(minimo)
      })
    })
  }
})
