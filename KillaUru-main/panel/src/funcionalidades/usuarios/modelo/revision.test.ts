// T051 — diasRestantes y armarRevision (data-model §5)
import { describe, it, expect } from 'vitest'
import { diasRestantes, armarRevision } from './revision'

describe('diasRestantes', () => {
  it('15 días antes del vencimiento', () => {
    const ahora = new Date('2026-11-05T05:00:00Z')
    expect(diasRestantes('2026-11-20T05:00:00Z', ahora)).toBe(15)
  })

  it('0 en el mismo momento exacto', () => {
    const ahora = new Date('2026-11-05T05:00:00Z')
    expect(diasRestantes('2026-11-05T05:00:00Z', ahora)).toBe(0)
  })

  it('negativo cuando ya venció', () => {
    const ahora = new Date('2026-11-05T05:00:00Z')
    expect(diasRestantes('2026-11-04T05:00:00Z', ahora)).toBeLessThan(0)
  })

  it('fracción de día → trunca hacia abajo', () => {
    const ahora = new Date('2026-11-05T05:00:00Z')
    expect(diasRestantes('2026-11-06T10:00:00Z', ahora)).toBe(1)
  })
})

describe('armarRevision', () => {
  it('un confirmado y un retirado en listas distintas', () => {
    const r = armarRevision({ id1: 'confirmar', id2: 'retirar' })
    expect(r.confirmados).toContain('id1')
    expect(r.retirados).toContain('id2')
    expect(r.confirmados).not.toContain('id2')
    expect(r.retirados).not.toContain('id1')
  })

  it('cada admin aparece exactamente en una lista', () => {
    const r = armarRevision({ id1: 'confirmar', id2: 'retirar', id3: 'confirmar' })
    const todos = [...r.confirmados, ...r.retirados]
    expect(new Set(todos).size).toBe(todos.length)
  })

  it('todos confirmados → retirados vacío', () => {
    const r = armarRevision({ id1: 'confirmar', id2: 'confirmar' })
    expect(r.confirmados.length).toBe(2)
    expect(r.retirados.length).toBe(0)
  })

  it('todos retirados → confirmados vacío', () => {
    const r = armarRevision({ id1: 'retirar', id2: 'retirar' })
    expect(r.confirmados.length).toBe(0)
    expect(r.retirados.length).toBe(2)
  })
})
