import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { MenuLateral } from './MenuLateral'
import { BarraSuperior } from './BarraSuperior'
import { iniciales } from '@/compartido/formato/iniciales'
import { migas } from './secciones'

// Armazón del prototipo (`.ops-nav`, `.ops-top`, `.av`): menú, migas e iniciales. Patrón AAA.
describe('Armazón del panel', () => {
  it.each([
    ['/usuarios', { grupo: 'Administración', seccion: 'Usuarios' }],
    ['/', { grupo: 'Operación', seccion: 'Resumen' }], // la raíz es el Resumen
    ['/no-existe', null],
  ])('las migas de %s → %j', (ruta, esperado) => {
    // Arrange y Act
    const resultado = migas(ruta)

    // Assert
    expect(resultado).toEqual(esperado)
  })

  it.each([
    ['admin.operaciones@killauru.pe', 'AO'], // una letra por parte del nombre de usuario
    ['ops@killauru.pe', 'OP'], // una sola parte: sus dos primeras letras
  ])('las iniciales de %s → %s', (correo, esperado) => {
    // Arrange y Act
    const resultado = iniciales(correo)

    // Assert
    expect(resultado).toBe(esperado)
  })

  it('el menú marca la página actual y muestra cuántas esperan revisión', () => {
    // Arrange y Act
    render(<MemoryRouter initialEntries={['/moderacion']}><MenuLateral porRevisar={3} /></MemoryRouter>)

    // Assert
    const actual = screen.getByRole('link', { name: /Moderación/ })
    expect(actual.getAttribute('aria-current')).toBe('page')
    expect(screen.getByLabelText('3 por revisar')).toBeTruthy()
    expect(screen.getByRole('link', { name: 'Resumen' }).getAttribute('aria-current')).toBeNull()
  })

  it('la barra superior muestra dónde estás y la marca DEMO fuera de producción', () => {
    // Arrange y Act
    render(<MemoryRouter initialEntries={['/auditoria']}><BarraSuperior entorno="desarrollo" /></MemoryRouter>)

    // Assert
    expect(screen.getByLabelText('Estás en').textContent).toBe('Administración / Auditoría')
    expect(screen.getByText('DEMO')).toBeTruthy()
  })
})
