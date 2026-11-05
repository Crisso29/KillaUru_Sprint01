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
    setupFiles: ['./pruebas/configuracion.ts'],
    coverage: {
      provider: 'v8',
      include: ['src/**'],
      exclude: ['src/api/generado/**'],
      thresholds: {
        // cobertura ≥ 90% para modelos — T009
        'src/**/modelo/**': { statements: 90, branches: 90, functions: 90, lines: 90 },
      },
    },
  },
})
