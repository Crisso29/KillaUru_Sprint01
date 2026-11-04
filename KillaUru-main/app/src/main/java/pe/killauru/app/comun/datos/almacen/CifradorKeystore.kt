package pe.killauru.app.comun.datos.almacen

import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import java.security.KeyStore
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.spec.GCMParameterSpec

/** ADR-009, R-022: AES-256-GCM con clave en Android Keystore (no exportable). */
internal class CifradorKeystore(private val alias: String = ALIAS_SESION) : Cifrador {
    companion object {
        const val ALIAS_SESION = "killauru_sesion"
        private const val PROVIDER = "AndroidKeyStore"
        private const val ALGO = "AES/GCM/NoPadding"
        private const val TAG_BITS = 128
        private const val IV_BYTES = 12
    }

    private fun obtenerOCrearClave(): javax.crypto.SecretKey {
        val ks = KeyStore.getInstance(PROVIDER).also { it.load(null) }
        return (ks.getEntry(alias, null) as? KeyStore.SecretKeyEntry)?.secretKey
            ?: KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, PROVIDER).apply {
                init(
                    KeyGenParameterSpec.Builder(alias,
                        KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT)
                        .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                        .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                        .setKeySize(256)
                        .build()
                )
            }.generateKey()
    }

    override fun cifrar(datos: ByteArray): ByteArray {
        val cipher = Cipher.getInstance(ALGO)
        cipher.init(Cipher.ENCRYPT_MODE, obtenerOCrearClave())
        val iv = cipher.iv
        val cifrado = cipher.doFinal(datos)
        return iv + cifrado
    }

    override fun descifrar(datos: ByteArray): ByteArray {
        val iv = datos.copyOf(IV_BYTES)
        val contenido = datos.copyOfRange(IV_BYTES, datos.size)
        val cipher = Cipher.getInstance(ALGO)
        cipher.init(Cipher.DECRYPT_MODE, obtenerOCrearClave(), GCMParameterSpec(TAG_BITS, iv))
        return cipher.doFinal(contenido)
    }
}
