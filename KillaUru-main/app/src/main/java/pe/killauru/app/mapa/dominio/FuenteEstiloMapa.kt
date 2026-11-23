package pe.killauru.app.mapa.dominio

/** FR-040, T096: el estilo MapLibre del mapa base listo para usarse sin red; null si aún no se descargó el mapa. */
internal interface FuenteEstiloMapa {
    suspend fun estilo(): String?
}
