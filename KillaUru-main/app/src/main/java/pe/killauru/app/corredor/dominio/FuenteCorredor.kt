package pe.killauru.app.corredor.dominio

/** FR-055, FR-074: el eje de la PE-28A con su km (publicado por la CDN). Null mientras no se haya descargado el mapa. */
internal interface FuenteCorredor {
    suspend fun corredor(): Corredor?
}
