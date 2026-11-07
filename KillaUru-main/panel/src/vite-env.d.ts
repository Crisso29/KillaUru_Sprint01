/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_URL_API: string
  readonly VITE_URL_ESTILO_MAPA: string
  readonly VITE_URL_PMTILES: string
  readonly VITE_TURNSTILE_SITEKEY: string
  readonly VITE_URL_ESTADO: string
  readonly VITE_ENTORNO: 'desarrollo' | 'produccion'
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

declare const __SIMULADO__: boolean
