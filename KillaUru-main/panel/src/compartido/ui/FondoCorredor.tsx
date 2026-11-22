/**
 * Fondo decorativo del login: la carretera del corredor sobre curvas de nivel suaves (prototipo `.login > svg`). El
 * trazado es `R_OPS` de referencia/prototipo/killauru-diseno.html; las curvas son estáticas para no cargar una imagen.
 */
const CARRETERA =
  'M86 452 C 120 440, 150 452, 182 430 C 214 408, 236 420, 268 398 C 300 376, 318 392, 350 366 C 380 342, 372 316, 404 300 ' +
  'C 436 284, 456 300, 486 276 C 514 254, 506 226, 540 216 C 574 206, 596 236, 628 238 C 660 240, 676 214, 704 206 ' +
  'C 734 198, 744 232, 774 226 C 806 220, 812 188, 846 180 C 876 173, 890 196, 914 186 C 932 179, 938 168, 950 164'

/** Curvas de nivel: la carretera desplazada en vertical, cada vez más tenue. */
const CURVAS = [-180, -120, -60, 60, 120, 180, 240]

export function FondoCorredor() {
  return (
    <svg viewBox="0 0 1000 600" preserveAspectRatio="xMidYMid slice" aria-hidden="true" focusable="false">
      <rect width="1000" height="600" fill="var(--sunken)" />
      {CURVAS.map((dy, i) => (
        <path key={dy} d={CARRETERA} transform={`translate(0 ${dy})`} fill="none"
          stroke="var(--line-2)" strokeWidth="1" opacity={0.9 - i * 0.08} />
      ))}
      <path d={CARRETERA} fill="none" stroke="var(--road-case)" strokeWidth="8" strokeLinecap="round" />
      <path d={CARRETERA} fill="none" stroke="var(--road-fill)" strokeWidth="5" strokeLinecap="round" />
    </svg>
  )
}
