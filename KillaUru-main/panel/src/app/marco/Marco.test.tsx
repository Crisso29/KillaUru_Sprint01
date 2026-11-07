import { describe, it, expect, vi } from 'vitest'
import { renderizar, screen } from '@/../pruebas/renderizar'
import { Marco } from './Marco'

// T019 — menú Operación/Administración, contador, «DEMO», sesión, «Salir»

const SESION_ACTIVA = {
  tipo: 'activa' as const,
  expiraAccesoUtc: new Date(Date.now() + 5 * 60_000),
  sesionVenceUtc: new Date(Date.now() + 4 * 3600_000),
  desfaseMs: 0,
  correo: 'ops@killauru.pe',
}

function renderMarco(props?: Partial<Parameters<typeof Marco>[0]>) {
  return renderizar(
    <Marco
      sesion={SESION_ACTIVA}
      porRevisar={0}
      onSalir={() => {}}
      entorno="pruebas"
      {...props}
    >
      <main>Contenido</main>
    </Marco>,
  )
}

describe('Marco', () => {
  it('muestra «Operación» y «Administración» en el menú', () => {
    renderMarco()
    expect(screen.getByText('Operación')).toBeTruthy()
    expect(screen.getByText('Administración')).toBeTruthy()
  })

  it('muestra contador de «Por revisar» cuando > 0', () => {
    renderMarco({ porRevisar: 5 })
    expect(screen.getByText('5')).toBeTruthy()
  })

  it('muestra «DEMO» cuando entorno ≠ produccion', () => {
    renderMarco({ entorno: 'pruebas' })
    expect(screen.getByText('DEMO')).toBeTruthy()
  })

  it('NO muestra «DEMO» en produccion', () => {
    renderMarco({ entorno: 'produccion' })
    expect(screen.queryByText('DEMO')).toBeNull()
  })

  it('muestra «Sesión hasta» con hora de vencimiento', () => {
    renderMarco()
    expect(screen.getByText(/Sesión hasta/)).toBeTruthy()
  })

  it('«Salir» llama a onSalir', async () => {
    const onSalir = vi.fn()
    renderMarco({ onSalir })
    const user = (await import('@testing-library/user-event')).default.setup()
    await user.click(screen.getByRole('button', { name: /Salir/i }))
    expect(onSalir).toHaveBeenCalledOnce()
  })

  it('enlace «Saltar al contenido» apunta a #contenido', () => {
    renderMarco()
    const enlace = screen.getByRole('link', { name: /Saltar/i })
    expect(enlace.getAttribute('href')).toBe('#contenido')
  })
})
