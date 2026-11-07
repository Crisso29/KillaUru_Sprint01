import { createContext, useCallback, useContext, useRef, useState } from 'react'

interface Aviso { id: string; tipo: 'info' | 'exito' | 'error' | 'advertencia'; texto: string }
interface CtxAvisos { agregar: (a: Omit<Aviso, 'id'>) => void }

const Ctx = createContext<CtxAvisos | null>(null)

export function useAvisos(): CtxAvisos {
  const ctx = useContext(Ctx)
  if (!ctx) throw new Error('useAvisos requiere ProveedorAvisos')
  return ctx
}

export function ProveedorAvisos({ children }: { children: React.ReactNode }) {
  const [avisos, setAvisos] = useState<Aviso[]>([])
  const contador = useRef(0)

  const agregar = useCallback((a: Omit<Aviso, 'id'>) => {
    const id = `av-${++contador.current}`
    setAvisos(prev => [...prev, { ...a, id }])
    setTimeout(() => setAvisos(prev => prev.filter(x => x.id !== id)), 5000)
  }, [])

  return (
    <Ctx.Provider value={{ agregar }}>
      {children}
      <div aria-live="polite" style={{ position: 'fixed', bottom: 'var(--s-21)', right: 'var(--s-21)', zIndex: 200, display: 'flex', flexDirection: 'column', gap: 'var(--s-8)' }}>
        {avisos.filter(a => a.tipo !== 'error').map(a => (
          <div key={a.id} style={{ background: 'var(--surface)', border: '1px solid var(--line)', borderRadius: 'var(--s-5)', padding: 'var(--s-8) var(--s-13)', boxShadow: 'var(--shadow)' }}>
            {a.texto}
          </div>
        ))}
      </div>
      <div aria-live="assertive" style={{ position: 'fixed', bottom: 'var(--s-21)', left: 'var(--s-21)', zIndex: 200, display: 'flex', flexDirection: 'column', gap: 'var(--s-8)' }}>
        {avisos.filter(a => a.tipo === 'error').map(a => (
          <div key={a.id} role="alert" style={{ background: 'var(--bad-soft)', border: '1px solid var(--bad)', borderRadius: 'var(--s-5)', padding: 'var(--s-8) var(--s-13)' }}>
            {a.texto}
          </div>
        ))}
      </div>
    </Ctx.Provider>
  )
}
