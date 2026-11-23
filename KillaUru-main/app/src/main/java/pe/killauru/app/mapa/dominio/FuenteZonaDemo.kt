package pe.killauru.app.mapa.dominio

/**
 * FR-049, FR-051, RN-08: el polígono del tramo demo que publica la CDN (el mismo del servidor, 006 T002). Null mientras
 * no se haya descargado el mapa.
 */
internal interface FuenteZonaDemo {
    suspend fun zona(): ZonaDemo?
}
