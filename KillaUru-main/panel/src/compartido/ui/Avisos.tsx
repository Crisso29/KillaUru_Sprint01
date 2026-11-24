import { createContext, useCallback, useContext, useRef, useState } from 'react'

interface Aviso { id: string; tipo: 'info' | 'exito' | 'error' | 'advertencia'; texto: string }
interface CtxAvisos { agregar: (a: Omit<Aviso, 'id'>) => void }

const Ctx = createContext<CtxAvisos | null>(null)

export function useAvisos(): CtxAvisos {
  const ctx = useContext(Ctx)
  if (!ctx) throw new Error('useAvisos requiere ProveedorAvisos')
  return ctx
}

const DURACION_MS = 5000
const PILA: React.CSSProperties = { position: 'fixed', bottom: 'var(--s-21)', zIndex: 200, display: 'flex', flexDirection: 'column', gap: 'var(--s-8)' }
const CAJA: React.CSSProperties = { borderRadius: 'var(--s-5)', padding: 'var(--s-8) var(--s-13)' }
const NORMAL: React.CSSProperties = { ...CAJA, background: 'var(--surface)', border: '1px solid var(--line)', boxShadow: 'var(--shadow)' }
const ERROR: React.CSSProperties = { ...CAJA, background: 'var(--bad-soft)', border: '1px solid var(--bad)' }

export function ProveedorAvisos({ children }: { children: React.ReactNode }) {
  const [avisos, setAvisos] = useState<Aviso[]>([])
  const contador = useRef(0)
  // Cada aviso se va solo a los 5 s; el id distingue dos avisos con el mismo texto
  const agregar = useCallback((aviso: Omit<Aviso, 'id'>) => {
    const id = `av-${++contador.current}`
    setAvisos((actuales) => [...actuales, { ...aviso, id }])
    setTimeout(() => setAvisos((actuales) => actuales.filter((otro) => otro.id !== id)), DURACION_MS)
  }, [])
  // Errores a la izquierda y con `assertive` (se leen de inmediato); el resto a la derecha y `polite`
  return (
    <Ctx.Provider value={{ agregar }}>
      {children}
      <div aria-live="polite" style={{ ...PILA, right: 'var(--s-21)' }}>
        {avisos.filter((aviso) => aviso.tipo !== 'error').map((aviso) => <div key={aviso.id} style={NORMAL}>{aviso.texto}</div>)}
      </div>
      <div aria-live="assertive" style={{ ...PILA, left: 'var(--s-21)' }}>
        {avisos.filter((aviso) => aviso.tipo === 'error').map((aviso) => <div key={aviso.id} role="alert" style={ERROR}>{aviso.texto}</div>)}
      </div>
    </Ctx.Provider>
  )
}
