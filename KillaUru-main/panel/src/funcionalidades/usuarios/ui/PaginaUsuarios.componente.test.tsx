// T052 — PaginaUsuarios: lista, contador, filtros, búsqueda, vacío, error
import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaUsuarios } from './PaginaUsuarios'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

describe('PaginaUsuarios', () => {
  it('muestra correos de la lista', async () => {
    // Arrange y Act
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/admin@killauru\.pe/)).toBeTruthy()
      expect(screen.getByText(/conductor@killauru\.pe/)).toBeTruthy()
    })
  })

  it('muestra contador «N de total»', async () => {
    // Arrange y Act
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/4 de 4/)).toBeTruthy()
    })
  })

  it('filtro estado tiene opción «InvitacionPendiente»', async () => {
    // Arrange y Act
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)

    // Assert
    await waitFor(() => {
      const sel = screen.getByRole('combobox', { name: /estado/i })
      expect(sel.querySelector('option[value="InvitacionPendiente"]')).toBeTruthy()
    })
  })

  it('cambio de estado envía el param correcto', async () => {
    // Arrange
    const consultas = servirUsuarios()
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    const estado = await screen.findByRole('combobox', { name: /estado/i })

    // Act
    await user.selectOptions(estado, 'InvitacionPendiente')

    // Assert
    await waitFor(() => expect(consultas.at(-1)?.get('estado')).toBe('InvitacionPendiente'))
  })

  it('Enter lanza la búsqueda inmediatamente', async () => {
    // Arrange
    const consultas = servirUsuarios()
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    await user.type(await screen.findByRole('searchbox'), 'jose')

    // Act
    await user.keyboard('{Enter}')

    // Assert
    await waitFor(() => expect(busquedas(consultas)).toContain('jose'))
  })

  it('lista vacía muestra estado vacío', async () => {
    // Arrange
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, () =>
        HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 }),
      ),
    )

    // Act
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)

    // Assert
    await waitFor(() => expect(screen.getByRole('status')).toBeTruthy())
  })

  it('error 503 muestra botón Reintentar', async () => {
    // Arrange
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, () =>
        HttpResponse.json({ codigo: 'sin_conexion' }, { status: 503 }),
      ),
    )

    // Act
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)

    // Assert
    await waitFor(() =>
      expect(screen.getByRole('button', { name: /reintentar/i })).toBeTruthy(),
    )
  })
})

/** Responde una lista vacía y guarda los parámetros de cada consulta a /usuarios, en orden. */
function servirUsuarios() {
  const consultas: URLSearchParams[] = []
  server.use(http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
    consultas.push(new URL(request.url).searchParams)
    return HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 })
  }))
  return consultas
}

/** Los textos buscados (`?busqueda=`), sin las consultas de solo filtros. */
const busquedas = (consultas: URLSearchParams[]) => consultas.flatMap((params) => params.getAll('busqueda'))
