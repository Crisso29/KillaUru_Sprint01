interface Props { children: React.ReactNode; caption: string }

// Tabla accesible solo para lectores de pantalla, detrás de un gráfico visual
export function TablaOculta({ children, caption }: Props) {
  return (
    <table style={{ position: 'absolute', width: 1, height: 1, overflow: 'hidden', clip: 'rect(0,0,0,0)', whiteSpace: 'nowrap' }}>
      <caption>{caption}</caption>
      {children}
    </table>
  )
}
