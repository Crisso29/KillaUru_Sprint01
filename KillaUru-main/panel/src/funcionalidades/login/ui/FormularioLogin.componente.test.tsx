// T029 — FormularioLogin: canal=panel, turnstileToken, respuestas y desafíos
import { beforeEach, describe, it, expect, vi } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { RutaActual, ID_RUTA_ACTUAL } from '@/../pruebas/RutaActual'
import { http, HttpResponse } from 'msw'
import { FormularioLogin } from './FormularioLogin'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'
const URL_LOGIN = `${BASE}/api/v1/auth/inicio-sesion`
const SESION = { expiraAccesoUtc: '2026-11-05T10:05:00Z', sesionVenceUtc: '2026-11-05T18:00:00Z', rol: 'Administrador' }

// Doble de Turnstile: al hacer clic entrega un token; el reinicio del desafío es un espía compartido para comprobarlo
const turnstile = vi.hoisted(() => ({ reiniciar: vi.fn() }))
vi.mock('./Turnstile', () => ({
  Turnstile: ({ onToken, resetRef }: { onToken: (token: string) => void; resetRef: React.MutableRefObject<(() => void) | null> }) => {
    resetRef.current = turnstile.reiniciar
    return <button data-testid="turnstile-resolver" type="button" onClick={() => onToken('token-prueba')}>Resolver desafío</button>
  },
}))

beforeEach(() => { turnstile.reiniciar.mockClear() })

function renderFormulario() {
  return renderizar(<><FormularioLogin /><RutaActual /></>)
}

/** Escribe correo y contraseña y resuelve el desafío; devuelve cómo pulsar «Entrar» (el acto de cada prueba). */
async function completar(correo = 'ops@killauru.pe', contrasena = 'Contrasena1!') {
  const user = userEvent.setup()
  await user.type(screen.getByLabelText(/correo/i), correo)
  await user.type(screen.getByLabelText(/contraseña/i), contrasena)
  await user.click(screen.getByTestId('turnstile-resolver'))
  return { entrar: () => user.click(screen.getByRole('button', { name: /entrar/i })) }
}

describe('FormularioLogin', () => {
  it('envía canal=panel y turnstileToken en el cuerpo', async () => {
    // Arrange
    const pedido: { cuerpo?: unknown } = {}
    server.use(http.post(URL_LOGIN, async ({ request }) => {
      pedido.cuerpo = await request.json()
      return HttpResponse.json(SESION)
    }))
    renderFormulario()
    const { entrar } = await completar()

    // Act
    await entrar()

    // Assert
    await waitFor(() => expect(pedido.cuerpo).toMatchObject({ canal: 'panel', turnstileToken: 'token-prueba' }))
  })

  it('200 → navega a /resumen', async () => {
    // Arrange
    server.use(http.post(URL_LOGIN, () => HttpResponse.json(SESION)))
    renderFormulario()
    const { entrar } = await completar()

    // Act
    await entrar()

    // Assert
    await waitFor(() => expect(screen.getByTestId(ID_RUTA_ACTUAL)).toHaveTextContent('/resumen'))
  })

  it.each([
    [401, 'credenciales_invalidas', 'Correo o contraseña incorrectos.'],
    [403, 'solo_administradores', 'Esta cuenta no tiene acceso al panel.'],
    [403, 'antibot_fallido', 'No pudimos comprobar que eres una persona.'],
    [429, 'limite_excedido', 'muchas consultas'],
  ])('%i %s → mensaje y un desafío anti-bot nuevo', async (estado, codigo, mensaje) => {
    // Arrange
    server.use(http.post(URL_LOGIN, () => HttpResponse.json({ codigo }, { status: estado })))
    renderFormulario()
    const { entrar } = await completar()

    // Act
    await entrar()

    // Assert — un token de Turnstile sirve una sola vez: tras cualquier fallo se pide otro
    expect(await screen.findByRole('alert')).toHaveTextContent(mensaje)
    expect(turnstile.reiniciar).toHaveBeenCalledOnce()
  })

  it('423 cuenta_bloqueada → aviso con minutos y hora Lima', async () => {
    // Arrange
    server.use(http.post(URL_LOGIN, () => HttpResponse.json(
      { codigo: 'cuenta_bloqueada', correlacionId: 'c-1', bloqueadoHastaUtc: '2030-01-01T10:05:00Z' }, { status: 423 })))
    renderFormulario()
    const { entrar } = await completar()

    // Act
    await entrar()

    // Assert — 10:05 UTC = 05:05 en Lima (UTC−5): el cuerpo trae la hora en camelCase, como el contrato
    expect(await screen.findByRole('alert')).toHaveTextContent('Vuelve a intentar a las 05:05')
  })

  it('botón desactivado mientras envía', async () => {
    // Arrange — el servidor no responde hasta que la prueba lo libera
    const pendiente: { liberar?: () => void } = {}
    server.use(http.post(URL_LOGIN, () => new Promise<Response>((responder) => {
      pendiente.liberar = () => responder(HttpResponse.json(SESION))
    })))
    renderFormulario()
    const { entrar } = await completar('ops@killauru.pe', 'Clave1234!')

    // Act
    await entrar()

    // Assert — el texto cambia a «Entrando…» y no se puede volver a enviar
    expect(await screen.findByRole('button', { name: /entrando/i })).toBeDisabled()
    pendiente.liberar?.()
  })
})
