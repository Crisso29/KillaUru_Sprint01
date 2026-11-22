package pe.killauru.app.sesion.ui

import pe.killauru.app.sesion.dominio.CategoriaVehiculo

/**
 * Prototipo «¿Qué manejas?»: cada opción dice qué vehículos incluye y qué pasará (RN-18: Pesado y Moto no detectan),
 * para que nadie elija una categoría falsa sin querer. `detecta` decide el color del efecto (ok o neutro).
 */
internal data class TextosVehiculo(val nombre: String, val ejemplos: String, val efecto: String, val detecta: Boolean)

internal fun CategoriaVehiculo.textos(): TextosVehiculo = when (this) {
    CategoriaVehiculo.LIVIANO -> TextosVehiculo(
        "Liviano", "Auto, camioneta, station wagon o colectivo", "Detecta baches, ve el mapa y recibe alertas", true,
    )
    CategoriaVehiculo.PESADO -> TextosVehiculo(
        "Pesado", "Bus, combi grande o camión", "Mapa y alertas con más anticipación. No detecta.", false,
    )
    CategoriaVehiculo.MOTO -> TextosVehiculo("Moto", "Moto o mototaxi", "Mapa y alertas. No detecta.", false)
}
