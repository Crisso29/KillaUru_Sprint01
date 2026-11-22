// Punto de entrada de la página del Mapa, solo para rutas.tsx (que lo carga con lazy). Va aparte del índice porque
// arrastra MapLibre (~800 KB): así el armazón, la búsqueda rápida y el Resumen, que usan el índice, no lo descargan.
export { PaginaMapa } from './ui/PaginaMapa'
