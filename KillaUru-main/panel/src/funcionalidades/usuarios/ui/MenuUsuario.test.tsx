// T053 — MenuUsuario: acciones por estado; fila propia; motivo; 409 con textos
import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { MenuUsuario } from './MenuUsuario'
import type { Usuario } from '@/api/generado/001-identidad'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

const ADMIN: Usuario = {
  id: 'a1a1a1a1-0000-0000-0000-000000000001',
  correo: 'otro@killauru.pe',
  rol: 'Administrador',
  estado: 'Activa',
  correoVerificado: true,
  fechaAltaUtc: '2026-01-01T00:00:00Z',
  ultimaSesionUtc: null,
}

const DESACTIVADO: Usuario = { ...ADMIN, correo: 'off@killauru.pe', estado: 'Desactivada' }
const PENDIENTE: Usuario = { ...ADMIN, correo: 'inv@killauru.pe', estado: 'InvitacionPendiente', correoVerificado: false }

describe('MenuUsuario', () => {
  it('fila de otro usuario activo → muestra Cambiar rol y Desactivar', async () => {
    renderizar(<MenuUsuario usuario={ADMIN} correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /acciones/i }))
    await waitFor(() => {
      expect(screen.getByRole('menuitem', { name: /cambiar rol/i })).toBeTruthy()
      expect(screen.getByRole('menuitem', { name: /desactivar/i })).toBeTruthy()
    })
  })

  it('fila propia → no hay Cambiar rol ni Desactivar', async () => {
    renderizar(<MenuUsuario usuario={ADMIN} correoActual={ADMIN.correo} />)
    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /acciones/i }))
    await waitFor(() => {
      expect(screen.queryByRole('menuitem', { name: /cambiar rol/i })).toBeNull()
      expect(screen.queryByRole('menuitem', { name: /desactivar/i })).toBeNull()
    })
  })

  it('usuario desactivado → muestra Activar', async () => {
    renderizar(<MenuUsuario usuario={DESACTIVADO} correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /acciones/i }))
    await waitFor(() => {
      expect(screen.getByRole('menuitem', { name: /activar/i })).toBeTruthy()
    })
  })

  it('invitación pendiente → muestra Reenviar invitación', async () => {
    renderizar(<MenuUsuario usuario={PENDIENTE} correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /acciones/i }))
    await waitFor(() => {
      expect(screen.getByRole('menuitem', { name: /reenviar invitaci/i })).toBeTruthy()
    })
  })

  it('desactivar requiere motivo (campo obligatorio)', async () => {
    renderizar(<MenuUsuario usuario={ADMIN} correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /acciones/i }))
    await user.click(screen.getByRole('menuitem', { name: /desactivar/i }))
    await waitFor(() => expect(screen.getByRole('dialog')).toBeTruthy())
    await user.click(screen.getByRole('button', { name: /confirmar/i }))
    await waitFor(() => expect(screen.getByRole('alert')).toBeTruthy())
  })

  it('409 accion_sobre_si_mismo → muestra texto correcto', async () => {
    server.use(
      http.post(`${BASE}/api/v1/usuarios/${ADMIN.id}/desactivar`, () =>
        HttpResponse.json({ codigo: 'accion_sobre_si_mismo' }, { status: 409 }),
      ),
    )
    renderizar(<MenuUsuario usuario={ADMIN} correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /acciones/i }))
    await user.click(screen.getByRole('menuitem', { name: /desactivar/i }))
    await waitFor(() => expect(screen.getByRole('dialog')).toBeTruthy())
    await user.type(screen.getByRole('textbox', { name: /motivo/i }), 'Motivo de prueba')
    await user.click(screen.getByRole('button', { name: /confirmar/i }))
    await waitFor(() =>
      expect(screen.getByText(/no puedes realizar esta acción sobre tu propia cuenta/i)).toBeTruthy(),
    )
  })

  it('409 ultimo_administrador → muestra texto correcto', async () => {
    server.use(
      http.put(`${BASE}/api/v1/usuarios/${ADMIN.id}/rol`, () =>
        HttpResponse.json({ codigo: 'ultimo_administrador' }, { status: 409 }),
      ),
    )
    renderizar(<MenuUsuario usuario={ADMIN} correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    await user.click(screen.getByRole('button', { name: /acciones/i }))
    await user.click(screen.getByRole('menuitem', { name: /cambiar rol/i }))
    await waitFor(() => expect(screen.getByRole('dialog')).toBeTruthy())
    await user.selectOptions(screen.getByRole('combobox', { name: /rol/i }), 'ConductorRegistrado')
    await user.click(screen.getByRole('button', { name: /confirmar/i }))
    await waitFor(() =>
      expect(screen.getByText(/debe haber al menos un administrador/i)).toBeTruthy(),
    )
  })
})
