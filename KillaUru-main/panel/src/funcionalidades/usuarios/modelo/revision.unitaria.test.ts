// T051 — diasRestantes y armarRevision (data-model §5)
import { describe, it, expect } from 'vitest'
import { diasRestantes, armarRevision } from './revision'

describe('diasRestantes (días completos; una fracción se trunca)', () => {
  const ahora = new Date('2026-11-05T05:00:00Z')

  it.each([
    ['15 días antes del vencimiento', '2026-11-20T05:00:00Z', 15],
    ['en el mismo momento exacto', '2026-11-05T05:00:00Z', 0],
    ['ya vencida hace un día', '2026-11-04T05:00:00Z', -1],
    ['1 día y 5 h → 1', '2026-11-06T10:00:00Z', 1],
  ])('%s → %i', (_caso, venceUtc, esperado) => {
    // Arrange y Act
    const dias = diasRestantes(venceUtc, ahora)

    // Assert
    expect(dias).toBe(esperado)
  })
})

describe('armarRevision', () => {
  it.each([
    ['uno de cada decisión', { id1: 'confirmar', id2: 'retirar' }, { confirmados: ['id1'], retirados: ['id2'] }],
    ['todos confirmados', { id1: 'confirmar', id2: 'confirmar' }, { confirmados: ['id1', 'id2'], retirados: [] }],
    ['todos retirados', { id1: 'retirar', id2: 'retirar' }, { confirmados: [], retirados: ['id1', 'id2'] }],
    ['cada administrador cae en una sola lista', { id1: 'confirmar', id2: 'retirar', id3: 'confirmar' },
      { confirmados: ['id1', 'id3'], retirados: ['id2'] }],
  ] as const)('%s', (_caso, decisiones, esperado) => {
    // Arrange y Act
    const revision = armarRevision(decisiones)

    // Assert
    expect({ confirmados: [...revision.confirmados].sort(), retirados: [...revision.retirados].sort() }).toEqual(esperado)
  })
})
