package pe.killauru.app.sesion.dominio

/** RF-29, FR-004, RN-18: categoría del vehículo del conductor. */
internal enum class CategoriaVehiculo(val texto: String) {
    LIVIANO("LIVIANO"),
    PESADO("PESADO"),
    MOTO("MOTO");

    companion object {
        fun deTexto(texto: String): CategoriaVehiculo? =
            entries.firstOrNull { it.texto == texto }
    }
}
