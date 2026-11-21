// T058 — E2E stub: flujos de gestión de usuarios (implementar en T090+)
import { test } from '@playwright/test'

test.describe('Gestión de usuarios', () => {
  test.skip('admin puede ver la lista de usuarios', async () => {
    // TODO T090: login → navegar a /usuarios → verificar tabla con al menos una fila
  })

  test.skip('admin puede cambiar el rol de un conductor', async () => {
    // TODO T090: abrir menú de fila → Cambiar rol → Administrador → confirmar → badge actualizado
  })

  test.skip('admin puede invitar a un nuevo usuario', async () => {
    // TODO T090: click Invitar → correo → rol → confirmar → fila InvitacionPendiente
  })

  test.skip('admin puede registrar revisión de privilegios', async () => {
    // TODO T090: click banner revisión → decidir cada admin → Guardar → banner desaparece
  })
})
