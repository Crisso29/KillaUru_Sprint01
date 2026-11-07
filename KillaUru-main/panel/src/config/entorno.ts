// Valida las 6 variables de entorno VITE_ al arrancar — falla rápido si faltan
function requerir(clave: keyof ImportMetaEnv): string {
  const valor = import.meta.env[clave]
  if (!valor) throw new Error(`Variable de entorno requerida: ${clave}`)
  return valor as string
}

export const entorno = {
  urlApi: requerir('VITE_URL_API'),
  urlEstiloMapa: requerir('VITE_URL_ESTILO_MAPA'),
  urlPmtiles: requerir('VITE_URL_PMTILES'),
  turnstileSitekey: requerir('VITE_TURNSTILE_SITEKEY'),
  urlEstado: requerir('VITE_URL_ESTADO'),
  entorno: requerir('VITE_ENTORNO') as 'desarrollo' | 'produccion',
} as const
