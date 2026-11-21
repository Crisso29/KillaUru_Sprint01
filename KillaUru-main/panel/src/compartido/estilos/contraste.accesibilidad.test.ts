import { describe, it, expect } from 'vitest'
import { readFileSync } from 'fs'
import { join } from 'path'

// T017 — WCAG: texto ≥ 4,5:1; gráficos ≥ 3:1

function hexARgb(hex: string) {
  const h = hex.replace('#', '')
  return [
    parseInt(h.slice(0, 2), 16) / 255,
    parseInt(h.slice(2, 4), 16) / 255,
    parseInt(h.slice(4, 6), 16) / 255,
  ] as [number, number, number]
}

function lineal(c: number) {
  return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4
}

function luminancia([r, g, b]: [number, number, number]) {
  return 0.2126 * lineal(r) + 0.7152 * lineal(g) + 0.0722 * lineal(b)
}

function contraste(hex1: string, hex2: string) {
  const l1 = luminancia(hexARgb(hex1))
  const l2 = luminancia(hexARgb(hex2))
  const [hi, lo] = l1 >= l2 ? [l1, l2] : [l2, l1]
  return (hi + 0.05) / (lo + 0.05)
}

function extraerTokens(css: string) {
  const tokens: Record<string, string> = {}
  for (const m of css.matchAll(/--([a-z0-9-]+):\s*(#[0-9a-fA-F]{6})/g)) {
    tokens[`--${m[1]}`] = m[2]!
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
      const t = cargarTema(css, esOscuro)

      it('--ink sobre --surface ≥ 4,5:1', () => {
        expect(contraste(t['--ink']!, t['--surface']!)).toBeGreaterThanOrEqual(4.5)
      })

      it('--ink sobre --bg ≥ 4,5:1', () => {
        expect(contraste(t['--ink']!, t['--bg']!)).toBeGreaterThanOrEqual(4.5)
      })

      it('--killa sobre --surface ≥ 3:1', () => {
        expect(contraste(t['--killa']!, t['--surface']!)).toBeGreaterThanOrEqual(3)
      })

      it('severidades s1..s3 sobre fondo ≥ 3:1 (gráficos)', () => {
        for (const s of ['--s1', '--s2', '--s3']) {
          expect(contraste(t[s]!, t['--surface']!)).toBeGreaterThanOrEqual(3)
        }
      })
    })
  }
})
