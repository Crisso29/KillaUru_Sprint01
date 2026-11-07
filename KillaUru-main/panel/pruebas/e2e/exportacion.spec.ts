// T085 — E2E stub: flujos de exportación del dataset (implementar en T090+)
import { test } from '@playwright/test'

test.describe('Exportación del dataset', () => {
  test.skip('admin puede exportar en formato CSV', async () => {
    // TODO T090: login → navegar a /exportacion → seleccionar CSV → Exportar → verificar descarga
  })

  test.skip('admin puede exportar en formato GeoJSON con filtro de tipo', async () => {
    // TODO T090: login → navegar a /exportacion → GeoJSON + tipo Bache → Exportar → verificar descarga
  })
})
