// Modo simulado (demo y E2E): Turnstile sin red. En vez del script de Cloudflare se sirve uno que imita su API
// (`render`, `reset`, `onload`) y entrega un token al instante; cada `reset` entrega uno nuevo, como el widget real.
import { http, HttpResponse } from 'msw'

const SCRIPT_SIMULADO = `(() => {
  const widgets = {}
  let contador = 0
  const entregar = (id) => setTimeout(() => widgets[id] && widgets[id].callback('token-simulado-' + (++contador)), 0)
  window.turnstile = {
    render: (contenedor, opciones) => { const id = 'w' + Object.keys(widgets).length; widgets[id] = opciones; entregar(id); return id },
    reset: (id) => entregar(id),
  }
  if (typeof window.onTurnstileLoad === 'function') window.onTurnstileLoad()
})()`

export const manejadoresTurnstile = [
  http.get('https://challenges.cloudflare.com/turnstile/v0/api.js', () =>
    new HttpResponse(SCRIPT_SIMULADO, { headers: { 'Content-Type': 'text/javascript; charset=utf-8' } })),
]
