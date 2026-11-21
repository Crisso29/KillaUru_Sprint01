// T060 — PaginaFijarContrasena: restablecer/invitacion, 400 enlace_invalido, validación
import { describe, it, expect, afterEach } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaFijarContrasena } from './PaginaFijarContrasena'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

afterEach(() => { window.location.hash = '' })

async function completarForm(clave = 'ClaveSegura1234', confirmacion?: string) {
  const user = userEvent.setup()
  await user.type(screen.getByLabelText(/nueva contraseña/i), clave)
  await user.type(screen.getByLabelText(/confirmar/i), confirmacion ?? clave)
  await user.click(screen.getByRole('button', { name: /guardar/i }))
}

describe('PaginaFijarContrasena — modo restablecer', () => {
  it('204 → "Tu contraseña cambió. Cerramos todas tus sesiones: vuelve a entrar."', async () => {
    window.location.hash = '#token=tok-rest'
    renderizar(<PaginaFijarContrasena modo="restablecer" />)
    await completarForm()
    await waitFor(() => {
      expect(screen.getByText(/contraseña cambió.*cerramos/i)).toBeTruthy()
    })
  })

  it('400 enlace_invalido → texto de error', async () => {
    server.use(
      http.post(`${BASE}/api/v1/auth/fijar-contrasena`, () =>
        HttpResponse.json({ codigo: 'enlace_invalido' }, { status: 400 }),
      ),
    )
    window.location.hash = '#token=tok-vencido'
    renderizar(<PaginaFijarContrasena modo="restablecer" />)
    await completarForm()
    await waitFor(() => {
      expect(screen.getByText(/no es válido o ya venció/i)).toBeTruthy()
    })
  })

  it('contraseñas no coinciden → error de validación', async () => {
    window.location.hash = '#token=tok-rest'
    renderizar(<PaginaFijarContrasena modo="restablecer" />)
    await completarForm('ClaveSegura1234', 'OtraDistinta1234')
    await waitFor(() => {
      expect(screen.getByText(/no coinciden/i)).toBeTruthy()
    })
  })
})

describe('PaginaFijarContrasena — modo invitacion', () => {
  it('204 → "Tu cuenta está lista. Ya puedes iniciar sesión."', async () => {
    window.location.hash = '#token=tok-inv'
    renderizar(<PaginaFijarContrasena modo="invitacion" />)
    await completarForm()
    await waitFor(() => {
      expect(screen.getByText(/cuenta está lista/i)).toBeTruthy()
    })
  })
})
