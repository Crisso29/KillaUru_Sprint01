// T051 — aConsulta: UUID → ids=; <3 → sin filtro; ≥3 → busqueda=; rol/estado; pagina
import { describe, it, expect } from 'vitest'
import { aConsulta } from './consulta'

const UUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'

describe('aConsulta', () => {
  it('UUID completo → ids=', () => {
    // Arrange y Act
    const consulta = aConsulta(UUID, '', '', 1)

    // Assert
    expect(consulta.get('ids')).toBe(UUID)
    expect(consulta.get('busqueda')).toBeNull()
  })

  it('texto ≥ 3 caracteres → busqueda=', () => {
    // Arrange y Act
    const consulta = aConsulta('jose', '', '', 1)

    // Assert
    expect(consulta.get('busqueda')).toBe('jose')
    expect(consulta.get('ids')).toBeNull()
  })

  it('texto < 3 caracteres → sin filtro de texto', () => {
    // Arrange y Act
    const consulta = aConsulta('jo', '', '', 1)

    // Assert
    expect(consulta.get('busqueda')).toBeNull()
    expect(consulta.get('ids')).toBeNull()
  })

  it('texto vacío → sin filtro de texto', () => {
    // Arrange y Act
    const consulta = aConsulta('', '', '', 1)

    // Assert
    expect(consulta.get('busqueda')).toBeNull()
    expect(consulta.get('ids')).toBeNull()
  })

  it('con rol incluye rol=', () => {
    // Arrange y Act
    const consulta = aConsulta('', 'Administrador', '', 1)

    // Assert
    expect(consulta.get('rol')).toBe('Administrador')
  })

  it('con estado incluye estado=', () => {
    // Arrange y Act
    const consulta = aConsulta('', '', 'Activa', 1)

    // Assert
    expect(consulta.get('estado')).toBe('Activa')
  })

  it('sin rol y sin estado no incluye esos params', () => {
    // Arrange y Act
    const consulta = aConsulta('', '', '', 1)

    // Assert
    expect(consulta.get('rol')).toBeNull()
    expect(consulta.get('estado')).toBeNull()
  })

  it('pagina siempre incluida', () => {
    // Arrange y Act
    const consulta = aConsulta('', '', '', 3)

    // Assert
    expect(consulta.get('pagina')).toBe('3')
  })

  it('busqueda y rol y estado y pagina juntos', () => {
    // Arrange y Act
    const consulta = aConsulta('jose', 'Administrador', 'Activa', 2)

    // Assert
    expect(consulta.get('busqueda')).toBe('jose')
    expect(consulta.get('rol')).toBe('Administrador')
    expect(consulta.get('estado')).toBe('Activa')
    expect(consulta.get('pagina')).toBe('2')
  })
})
