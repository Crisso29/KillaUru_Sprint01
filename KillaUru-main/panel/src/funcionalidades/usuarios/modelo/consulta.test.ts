// T051 — aConsulta: UUID → ids=; <3 → sin filtro; ≥3 → busqueda=; rol/estado; pagina
import { describe, it, expect } from 'vitest'
import { aConsulta } from './consulta'

const UUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'

describe('aConsulta', () => {
  it('UUID completo → ids=', () => {
    const p = aConsulta(UUID, '', '', 1)
    expect(p.get('ids')).toBe(UUID)
    expect(p.get('busqueda')).toBeNull()
  })

  it('texto ≥ 3 caracteres → busqueda=', () => {
    const p = aConsulta('jose', '', '', 1)
    expect(p.get('busqueda')).toBe('jose')
    expect(p.get('ids')).toBeNull()
  })

  it('texto < 3 caracteres → sin filtro de texto', () => {
    const p = aConsulta('jo', '', '', 1)
    expect(p.get('busqueda')).toBeNull()
    expect(p.get('ids')).toBeNull()
  })

  it('texto vacío → sin filtro de texto', () => {
    const p = aConsulta('', '', '', 1)
    expect(p.get('busqueda')).toBeNull()
    expect(p.get('ids')).toBeNull()
  })

  it('con rol incluye rol=', () => {
    const p = aConsulta('', 'Administrador', '', 1)
    expect(p.get('rol')).toBe('Administrador')
  })

  it('con estado incluye estado=', () => {
    const p = aConsulta('', '', 'Activa', 1)
    expect(p.get('estado')).toBe('Activa')
  })

  it('sin rol y sin estado no incluye esos params', () => {
    const p = aConsulta('', '', '', 1)
    expect(p.get('rol')).toBeNull()
    expect(p.get('estado')).toBeNull()
  })

  it('pagina siempre incluida', () => {
    const p = aConsulta('', '', '', 3)
    expect(p.get('pagina')).toBe('3')
  })

  it('busqueda y rol y estado y pagina juntos', () => {
    const p = aConsulta('jose', 'Administrador', 'Activa', 2)
    expect(p.get('busqueda')).toBe('jose')
    expect(p.get('rol')).toBe('Administrador')
    expect(p.get('estado')).toBe('Activa')
    expect(p.get('pagina')).toBe('2')
  })
})
