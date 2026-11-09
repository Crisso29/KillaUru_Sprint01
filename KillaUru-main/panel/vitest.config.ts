import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import { resolve } from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { '@': resolve(__dirname, 'src') },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    exclude: ['**/node_modules/**', '**/pruebas/e2e/**'],
    setupFiles: ['./pruebas/configuracion.ts'],
    env: {
      VITE_URL_API: 'https://api.killauru.pe',
      VITE_URL_ESTILO_MAPA: 'https://cdn.killauru.pe/estilo.json',
      VITE_URL_PMTILES: 'https://cdn.killauru.pe/mapa.pmtiles',
      VITE_TURNSTILE_SITEKEY: '1x00000000000000000000AA',
      VITE_URL_ESTADO: 'https://estado.killauru.pe',
      VITE_ENTORNO: 'desarrollo',
    },
    coverage: {
      provider: 'v8',
      include: ['src/**'],
      exclude: ['src/api/generado/**', 'src/**/*.test.*', 'src/**/*.d.ts'],
      thresholds: {
        // cobertura ≥ 90% para modelos — T009
        'src/**/modelo/**': { statements: 90, branches: 90, functions: 90, lines: 90 },
      },
    },
  },
})
