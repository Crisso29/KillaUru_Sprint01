import { useId } from 'react'

interface Props {
  /** Lado en píxeles. */
  tamano?: number
  /** Texto para lectores de pantalla; vacío si va junto al nombre (decorativo). */
  etiqueta?: string
}

/**
 * Logo «Señal» (opción C de `referencia/prototipo/killauru-logos.html`): la luciérnaga es un marcador de mapa y su luz
 * señala el punto de la vía. Es el mismo dibujo que `public/favicon.svg` y que el ícono de la app Android.
 * `useId` da identificadores únicos a los degradados: dos logos en la misma página no se pisan.
 */
export function LogoSenal({ tamano = 28, etiqueta = '' }: Props) {
  const unico = useId().replace(/:/g, '')
  const decorativo = etiqueta === ''
  return (
    <svg width={tamano} height={tamano} viewBox="0 0 120 120" focusable="false"
      role={decorativo ? undefined : 'img'} aria-label={decorativo ? undefined : etiqueta} aria-hidden={decorativo ? true : undefined}>
      <Degradados unico={unico} />
      <g clipPath={`url(#${unico}c)`}>
        <PaisajeNocturno unico={unico} />
        <Luciernaga unico={unico} />
      </g>
    </svg>
  )
}

/** Ids únicos por logo (de `useId`): fondo, núcleo y halo de la luz, y el recorte de esquinas redondeadas. */
function Degradados({ unico }: { unico: string }) {
  return (
    <defs>
      <linearGradient id={`${unico}bg`} x1="0" y1="0" x2="0" y2="1">
        <stop offset="0" stopColor="#0B2433" /><stop offset="1" stopColor="#11475A" />
      </linearGradient>
      <radialGradient id={`${unico}core`}>
        <stop offset="0" stopColor="#FFF6CF" />
        <stop offset=".55" stopColor="#FFD45C" />
        <stop offset="1" stopColor="#F2A81E" />
      </radialGradient>
      <radialGradient id={`${unico}halo`}>
        <stop offset="0" stopColor="#FFD45C" stopOpacity=".55" /><stop offset="1" stopColor="#FFD45C" stopOpacity="0" />
      </radialGradient>
      <clipPath id={`${unico}c`}><rect width="120" height="120" rx="27" /></clipPath>
    </defs>
  )
}

/** Cielo con estrellas, cerros y la vía con su línea central. */
function PaisajeNocturno({ unico }: { unico: string }) {
  return (
    <>
      <rect width="120" height="120" fill={`url(#${unico}bg)`} />
      <g fill="#BFEDE3" opacity=".8">
        <circle cx="16" cy="22" r="1" /><circle cx="104" cy="20" r="1.1" />
        <circle cx="96" cy="58" r=".8" /><circle cx="20" cy="60" r=".8" />
      </g>
      <path d="M0 98 L18 84 L32 92 L48 78 L60 88 L72 78 L88 92 L102 84 L120 98 V120 H0Z" fill="#0A3445" />
      <path d="M0 106 L22 96 L40 104 L60 96 L80 104 L98 96 L120 106 V120 H0Z" fill="#082834" />
      <path d="M42 120 L58 90 H62 L78 120Z" fill="#06202A" />
      <path d="M42 120 L58 90 M78 120 L62 90" stroke="#F2C14E" strokeWidth="1.5" />
      <path d="M60 120 V92" stroke="#FFFFFF" strokeWidth="1.3" strokeDasharray="3.5 3.5" opacity=".85" />
      <ellipse cx="60" cy="90" rx="24" ry="5" fill="#FFD45C" opacity=".35" />
    </>
  )
}

/** La luciérnaga como marcador de mapa: alas, antenas y la luz que señala la vía. */
function Luciernaga({ unico }: { unico: string }) {
  return (
    <>
      <circle cx="60" cy="44" r="30" fill={`url(#${unico}halo)`} />
      <ellipse cx="39" cy="34" rx="15" ry="7" transform="rotate(-28 39 34)" fill="#9FE6D6" opacity=".55" />
      <ellipse cx="81" cy="34" rx="15" ry="7" transform="rotate(28 81 34)" fill="#9FE6D6" opacity=".55" />
      <path d="M54 25 Q50 16 45 14 M66 25 Q70 16 75 14" stroke="#F4EFE0" strokeWidth="2.2" fill="none" strokeLinecap="round" />
      <circle cx="45" cy="14" r="2" fill="#F4EFE0" /><circle cx="75" cy="14" r="2" fill="#F4EFE0" />
      <path d="M60 86 C 52 74, 42 64, 42 46 A18 18 0 1 1 78 46 C 78 64, 68 74, 60 86Z" fill="#F4EFE0" />
      <circle cx="60" cy="46" r="11" fill={`url(#${unico}core)`} />
      <circle cx="56.5" cy="42.5" r="2.6" fill="#FFFFFF" opacity=".85" />
    </>
  )
}
