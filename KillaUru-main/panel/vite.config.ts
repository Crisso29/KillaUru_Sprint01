import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { resolve } from 'path'

// FR-001: SPA estática · FR-044: bloque «mapa» perezoso · T002
export default defineConfig(({ mode }) => ({
  plugins: [react()],
  resolve: {
    alias: { '@': resolve(__dirname, 'src') },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('maplibre-gl') || id.includes('pmtiles')) {
            return 'mapa'
          }
        },
      },
    },
  },
  define: {
    // modo simulado: habilita MSW en el navegador
    __SIMULADO__: JSON.stringify(mode === 'simulado'),
  },
}))
