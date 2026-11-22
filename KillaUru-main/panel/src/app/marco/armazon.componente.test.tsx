import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { MenuLateral } from './MenuLateral'
import { BarraSuperior } from './BarraSuperior'
import { iniciales } from '@/compartido/formato/iniciales'
import { migas } from './secciones'

// Armazón del prototipo (`.ops-nav`, `.ops-top`, `.av`): menú, migas e iniciales. Patrón AAA.
describe('Armazón del panel', () => {
  it('las migas siguen la ruta, y la raíz es el Resumen', () => {
    // Arrange + Act + Assert
    expect(migas('/usuarios')).toEqual({ grupo: 'Administración', seccion: 'Usuarios' })
    expect(migas('/')).toEqual({ grupo: 'Operación', seccion: 'Resumen' })
    expect(migas('/no-existe')).toBeNull()
  })

  it('las iniciales salen del nombre de usuario del correo', () => {
    // Arrange + Act + Assert
    expect(iniciales('admin.operaciones@killauru.pe')).toBe('AO')
    expect(iniciales('ops@killauru.pe')).toBe('OP')
  })

  it('el menú marca la página actual y muestra cuántas esperan revisión', () => {
    // Arrange
    render(<MemoryRouter initialEntries={['/moderacion']}><MenuLateral porRevisar={3} /></MemoryRouter>)
    // Act
    const actual = screen.getByRole('link', { name: /Moderación/ })
    // Assert
    expect(actual.getAttribute('aria-current')).toBe('page')
    expect(screen.getByLabelText('3 por revisar')).toBeTruthy()
    expect(screen.getByRole('link', { name: 'Resumen' }).getAttribute('aria-current')).toBeNull()
  })

  it('la barra superior muestra dónde estás y la marca DEMO fuera de producción', () => {
    // Arrange
    render(<MemoryRouter initialEntries={['/auditoria']}><BarraSuperior entorno="desarrollo" /></MemoryRouter>)
    // Act + Assert
    expect(screen.getByLabelText('Estás en').textContent).toBe('Administración / Auditoría')
    expect(screen.getByText('DEMO')).toBeTruthy()
  })
})
