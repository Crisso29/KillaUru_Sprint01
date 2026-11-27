import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import '@/compartido/estilos/tokens.css'
import '@/compartido/estilos/base.css'
import '@/compartido/estilos/fuentes.css'
import '@/compartido/estilos/componentes.css'
import '@/compartido/estilos/ops.css'
import { App } from '@/app/App'

const raiz = document.getElementById('raiz')
if (!raiz) throw new Error('Elemento #raiz no encontrado')
async function iniciar() {
  if (__SIMULADO__) {
    const { worker, exponerParaPruebas } = await import('../pruebas/msw/navegador')
    await worker.start({ onUnhandledRequest: 'bypass' })
    // Firefox CI: CDP no establece controller; location.assign a URL diferente crea
    // navegación nativa que el SW sí intercepta. Parámetro _sw evita bucle infinito.
    if (!navigator.serviceWorker.controller) {
      const url = new URL(location.href)
      if (!url.searchParams.has('_sw')) {
        url.searchParams.set('_sw', '1')
        location.assign(url.toString())
        return
      }
      url.searchParams.delete('_sw')
      history.replaceState(null, '', url.toString())
    }
    exponerParaPruebas()
  }
  createRoot(raiz!).render(<StrictMode><App /></StrictMode>)
}

void iniciar()
