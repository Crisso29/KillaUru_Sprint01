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

export function Turnstile({ onToken, resetRef }: Props) {
  const container = useRef<HTMLDivElement>(null)
  const widgetId = useRef<string | null>(null)

  useEffect(() => {
    function montarWidget() {
      if (!container.current || !window.turnstile) return
      widgetId.current = window.turnstile.render(container.current, {
        sitekey: entorno.turnstileSitekey,
        callback: onToken,
        'error-callback': () => { widgetId.current = null },
        theme: 'light',
      })
    }

    if (!document.getElementById(SCRIPT_ID)) {
      window.onTurnstileLoad = montarWidget
      const script = document.createElement('script')
      script.id = SCRIPT_ID
      script.src =
        'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit&onload=onTurnstileLoad'
      script.async = true
      script.defer = true
      document.head.appendChild(script)
    } else if (window.turnstile) {
      montarWidget()
    }

    return () => {
      widgetId.current = null
    }
  }, [onToken])

  resetRef.current = () => {
    if (window.turnstile && widgetId.current) {
      window.turnstile.reset(widgetId.current)
      widgetId.current = null
    }
  }

  return <div ref={container} />
}
