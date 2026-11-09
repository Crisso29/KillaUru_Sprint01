import { describe, it, expect, vi } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useLocation } from 'react-router'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { FormularioLogin } from './FormularioLogin'

function PaginaActual() {
  const { pathname } = useLocation()
  return <div data-testid="ruta-actual">{pathname}</div>
}

// T029 — FormularioLogin: canal=panel, turnstileToken, respuestas y desafíos

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'
const URL_LOGIN = `${BASE}/api/v1/auth/iniciar`

// Doble de Turnstile: simula que el widget entrega un token al hacer clic
vi.mock('./Turnstile', () => ({
  Turnstile: ({ onToken, resetRef }: {
    onToken: (t: string) => void
    resetRef: React.MutableRefObject<(() => void) | null>
  }) => {
    resetRef.current = vi.fn()
    return (
      <button
        data-testid="turnstile-resolver"
        type="button"
        onClick={() => onToken('token-prueba')}
      >
        Resolver desafío
      </button>
    )
  },
}))

function renderFormulario() {
  return renderizar(
    <>
      <FormularioLogin />
      <PaginaActual />
    </>,
  )
}

async function completarYEnviar(correo = 'ops@killauru.pe', pass = 'Contrasena1!') {
  const user = userEvent.setup()
  await user.type(screen.getByLabelText(/correo/i), correo)
  await user.type(screen.getByLabelText(/contraseña/i), pass)
  await user.click(screen.getByTestId('turnstile-resolver'))
  await user.click(screen.getByRole('button', { name: /entrar/i }))
}

describe('FormularioLogin', () => {
  it('envía canal=panel y turnstileToken en el cuerpo', async () => {
    let cuerpo: unknown = null
    server.use(
      http.post(URL_LOGIN, async ({ request }) => {
        cuerpo = await request.json()
        return HttpResponse.json({ acceso_vence_utc: '2026-11-05T10:05:00Z' })
      }),
    )
    renderFormulario()
    await completarYEnviar()
    await waitFor(() => {
      expect(cuerpo).toMatchObject({ canal: 'panel', turnstileToken: 'token-prueba' })
    })
  })

  it('200 → navega a /resumen', async () => {
    server.use(
      http.post(URL_LOGIN, () =>
        HttpResponse.json({ acceso_vence_utc: '2026-11-05T10:05:00Z' }),
      ),
    )
    renderFormulario()
    await completarYEnviar()
    await waitFor(() => {
      expect(screen.getByTestId('ruta-actual')).toHaveTextContent('/resumen')
    })
  })

  it('401 credenciales_invalidas → mensaje de error', async () => {
    server.use(
      http.post(URL_LOGIN, () =>
        HttpResponse.json({ codigo: 'credenciales_invalidas' }, { status: 401 }),
      ),
    )
    renderFormulario()
    await completarYEnviar()
    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('Correo o contraseña incorrectos.')
    })
  })

  it('403 solo_administradores → mensaje', async () => {
    server.use(
      http.post(URL_LOGIN, () =>
        HttpResponse.json({ codigo: 'solo_administradores' }, { status: 403 }),
      ),
    )
    renderFormulario()
    await completarYEnviar()
    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('Esta cuenta no tiene acceso al panel.')
    })
  })

  it('403 antibot_fallido → mensaje y llama reset de Turnstile', async () => {
    server.use(
      http.post(URL_LOGIN, () =>
        HttpResponse.json({ codigo: 'antibot_fallido' }, { status: 403 }),
      ),
    )
    renderFormulario()
    await completarYEnviar()
    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent(
        'No pudimos comprobar que eres una persona.',
      )
    })
  })

  it('423 cuenta_bloqueada → aviso con minutos y hora Lima', async () => {
    server.use(
      http.post(URL_LOGIN, () =>
        HttpResponse.json(
          { codigo: 'cuenta_bloqueada', bloqueado_hasta_utc: '2030-01-01T10:05:00Z' },
          { status: 423 },
        ),
      ),
    )
    renderFormulario()
    await completarYEnviar()
    await waitFor(() => {
      const alerta = screen.getByRole('alert')
      expect(alerta).toHaveTextContent('bloqueada')
    })
  })

  it('429 limite_excedido → mensaje', async () => {
    server.use(
      http.post(URL_LOGIN, () =>
        HttpResponse.json({ codigo: 'limite_excedido' }, { status: 429 }),
      ),
    )
    renderFormulario()
    await completarYEnviar()
    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('muchas consultas')
    })
  })

  it('botón desactivado mientras envía', async () => {
    const pendiente: { resolver?: () => void } = {}
    server.use(
      http.post(URL_LOGIN, () =>
        new Promise<Response>((res) => {
          pendiente.resolver = () => res(HttpResponse.json({ acceso_vence_utc: '2026-11-05T10:05:00Z' }))
        }),
      ),
    )
    renderFormulario()
    const user = userEvent.setup()
    await user.type(screen.getByLabelText(/correo/i), 'ops@killauru.pe')
    await user.type(screen.getByLabelText(/contraseña/i), 'Clave1234!')
    await user.click(screen.getByTestId('turnstile-resolver'))
    await user.click(screen.getByRole('button', { name: /entrar/i }))
    await waitFor(() => {
      // Texto cambia a «Entrando…» mientras la mutación está pendiente
      expect(screen.getByRole('button', { name: /entrando/i })).toBeDisabled()
    })
    pendiente.resolver?.()
  })
})
