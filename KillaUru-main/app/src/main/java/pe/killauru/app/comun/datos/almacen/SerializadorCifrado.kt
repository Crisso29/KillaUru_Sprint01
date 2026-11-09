package pe.killauru.app.comun.datos.almacen

import java.util.Base64

/** T016 / FR-062: cifra strings con AES-GCM; delega la clave al Cifrador inyectado. */
internal class SerializadorCifrado(private val cifrador: Cifrador) {
    fun cifrar(texto: String): String =
        Base64.getEncoder().encodeToString(cifrador.cifrar(texto.toByteArray(Charsets.UTF_8)))

    fun descifrar(cifrado: String): String =
        String(cifrador.descifrar(Base64.getDecoder().decode(cifrado)), Charsets.UTF_8)
}
