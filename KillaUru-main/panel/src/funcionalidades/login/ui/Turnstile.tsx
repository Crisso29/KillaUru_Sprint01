// T031 — widget Cloudflare Turnstile (R-009): script cargado solo aquí, render=explicit
import { useEffect, useRef } from 'react'
import { entorno } from '@/config/entorno'

declare global {
  interface Window {
    turnstile?: {
      render: (container: HTMLElement, opts: TurnstileOpts) => string
      reset: (widgetId: string) => void
    }
    onTurnstileLoad?: () => void
  }
}

interface TurnstileOpts {
  sitekey: string
  callback: (token: string) => void
  'error-callback': () => void
  theme: 'light' | 'dark'
}

interface Props {
  onToken: (token: string) => void
  resetRef: React.MutableRefObject<(() => void) | null>
}

const SCRIPT_ID = 'turnstile-script'
const URL_SCRIPT = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit&onload=onTurnstileLoad'

export function Turnstile({ onToken, resetRef }: Props) {
  const contenedor = useRef<HTMLDivElement>(null)
  const widgetId = useRef<string | null>(null)
  useEffect(() => {
    const montar = () => { widgetId.current = montarWidget(contenedor.current, onToken, () => { widgetId.current = null }) }
    if (!document.getElementById(SCRIPT_ID)) cargarScript(montar)
    else if (window.turnstile) montar()
    return () => { widgetId.current = null }
  }, [onToken])
  // Reiniciar el MISMO widget pide un desafío nuevo. Antes se olvidaba su id y el segundo intento fallido ya no
  // reiniciaba: se reenviaba un token usado y el servidor lo rechazaba siempre.
  resetRef.current = () => {
    if (window.turnstile && widgetId.current) window.turnstile.reset(widgetId.current)
  }
  return <div ref={contenedor} />
}

function montarWidget(contenedor: HTMLDivElement | null, onToken: (token: string) => void, onError: () => void): string | null {
  if (!contenedor || !window.turnstile) return null
  return window.turnstile.render(contenedor, { sitekey: entorno.turnstileSitekey, callback: onToken, 'error-callback': onError, theme: 'light' })
}

/** El script se agrega una sola vez por página y avisa con `onTurnstileLoad` cuando está listo. */
function cargarScript(alCargar: () => void) {
  window.onTurnstileLoad = alCargar
  const script = document.createElement('script')
  Object.assign(script, { id: SCRIPT_ID, src: URL_SCRIPT, async: true, defer: true })
  document.head.appendChild(script)
}
