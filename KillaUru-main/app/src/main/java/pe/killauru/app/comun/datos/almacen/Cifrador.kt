package pe.killauru.app.comun.datos.almacen

/** R-022: puerto de cifrado; la implementación real usa Android Keystore. */
internal interface Cifrador {
    fun cifrar(datos: ByteArray): ByteArray
    fun descifrar(datos: ByteArray): ByteArray
}
