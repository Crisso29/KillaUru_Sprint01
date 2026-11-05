// T023 — BroadcastChannel para cerrar sesión en todas las pestañas (data-model.md §1)
const CANAL = 'ku-sesion'
const MSG_CERRADA = 'cerrada'

export function crearCanalSesion(onCerrada: () => void) {
  const canal = new BroadcastChannel(CANAL)
  canal.onmessage = (e: MessageEvent) => {
    if (e.data === MSG_CERRADA) onCerrada()
  }

  function emitirCerrada() {
    canal.postMessage(MSG_CERRADA)
  }

  function destruir() {
    canal.close()
  }

  return { emitirCerrada, destruir }
}
