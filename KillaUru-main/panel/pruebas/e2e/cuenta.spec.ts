// T062 — E2E stub: flujos de verificación de correo y cambio de contraseña (implementar en T090+)
import { test } from '@playwright/test'

test.describe('Verificación de correo', () => {
  test.skip('link válido verifica el correo y muestra confirmación', async () => {
    // TODO T090: navegar a /verificar-correo#token=<válido> → ver "Listo, tu correo está verificado."
  })

  test.skip('link vencido muestra error de enlace inválido', async () => {
    // TODO T090: navegar a /verificar-correo#token=<vencido> → ver "El enlace no es válido o ya venció."
  })
})

test.describe('Fijar contraseña', () => {
  test.skip('flujo restablecer contraseña completo', async () => {
    // TODO T090: navegar a /restablecer#token=<válido> → rellenar formulario → ver "Tu contraseña cambió."
  })
})
