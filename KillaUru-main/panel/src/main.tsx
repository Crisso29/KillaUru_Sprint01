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
    // Firefox (CI): clients.claim() puede tardar; verificar con un fetch real que el SW
    // intercepta antes de exponer el simulador y arrancar React con peticiones API.
    const urlPing = `${import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'}/api/v1/anomalies/transiciones`
    let interceptando = false
    for (let intento = 0; intento < 40 && !interceptando; intento++) {
      try {
        const resp = await fetch(urlPing)
        interceptando = resp.ok
      } catch {
        // SW aún no intercepta; el servidor real lanza error de red
      }
      if (!interceptando) await new Promise<void>((res) => setTimeout(res, 50))
    }
    exponerParaPruebas()
  }
  createRoot(raiz!).render(<StrictMode><App /></StrictMode>)
}

void iniciar()
