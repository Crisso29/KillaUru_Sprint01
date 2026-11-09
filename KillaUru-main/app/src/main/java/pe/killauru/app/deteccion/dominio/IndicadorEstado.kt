package pe.killauru.app.deteccion.dominio

/** R-032, FR-016: indicador de estado del servicio de detección. */
internal enum class IndicadorEstado {
    SinServicio,
    SinPermiso,
    SinGps,
    ModoAhorro,
    SoloMapa,
    EnPausa,
    Detectando,
    ;

    companion object {
        /** R-032: prioridad descendente; el primero que aplica gana. */
        fun calcular(
            servicioActivo: Boolean,
            tienePermiso: Boolean,
            tieneGps: Boolean,
            enAhorro: Boolean,
            soloVerMapa: Boolean,
            velocidadOk: Boolean,
        ): IndicadorEstado = when {
            !servicioActivo -> SinServicio
            !tienePermiso -> SinPermiso
            enAhorro -> ModoAhorro
            soloVerMapa -> SoloMapa
            !tieneGps -> SinGps
            !velocidadOk -> EnPausa
            else -> Detectando
        }
    }
}
