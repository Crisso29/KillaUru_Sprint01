package pe.killauru.app.comun.datos.almacen

import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import pe.killauru.app.comun.datos.almacen.SerializadorCifrado
import kotlin.test.assertEquals
import kotlin.test.assertFails
import kotlin.test.assertNotEquals

/** T016 / FR-062, R-022: AES-GCM; un byte alterado debe fallar, nunca producir datos falsos. */
@RunWith(RobolectricTestRunner::class)
class SerializadorCifradoTest {
    private val cifrador = CifradorPrueba()
    private val serializador = SerializadorCifrado(cifrador)

    @Test
    fun `ida y vuelta preserva el texto original`() {
        // Arrange
        val original = "tokenAcceso=eyJhbGciOiJSUzI1NiJ9.test"
        // Act
        val cifrado = serializador.cifrar(original)
        val descifrado = serializador.descifrar(cifrado)
        // Assert
        assertEquals(original, descifrado)
    }

    @Test
    fun `un byte alterado en el cifrado produce error no datos falsos`() {
        // Arrange
        val original = "datos sensibles"
        val cifrado = serializador.cifrar(original).toByteArray()
        cifrado[cifrado.size / 2] = cifrado[cifrado.size / 2].inc()
        // Act / Assert
        assertFails { serializador.descifrar(String(cifrado)) }
    }

    @Test
    fun `dos cifrados del mismo texto producen resultados distintos (IV aleatorio)`() {
        // Arrange
        val texto = "valor_repetido"
        // Act
        val c1 = serializador.cifrar(texto)
        val c2 = serializador.cifrar(texto)
        // Assert
        assertNotEquals(c1, c2, "El IV debe ser aleatorio; dos cifrados deben diferir")
    }
}

/** Cifrador sin Android Keystore para pruebas unitarias puras. */
private class CifradorPrueba : Cifrador {
    private val clave: ByteArray = ByteArray(32) { it.toByte() }

    override fun cifrar(datos: ByteArray): ByteArray {
        val cipher = javax.crypto.Cipher.getInstance("AES/GCM/NoPadding")
        val iv = ByteArray(12).also { java.security.SecureRandom().nextBytes(it) }
        val spec = javax.crypto.spec.GCMParameterSpec(128, iv)
        cipher.init(javax.crypto.Cipher.ENCRYPT_MODE, javax.crypto.spec.SecretKeySpec(clave, "AES"), spec)
        val cifrado = cipher.doFinal(datos)
        return iv + cifrado
    }

    override fun descifrar(datos: ByteArray): ByteArray {
        val iv = datos.copyOf(12)
        val contenido = datos.copyOfRange(12, datos.size)
        val cipher = javax.crypto.Cipher.getInstance("AES/GCM/NoPadding")
        val spec = javax.crypto.spec.GCMParameterSpec(128, iv)
        cipher.init(javax.crypto.Cipher.DECRYPT_MODE, javax.crypto.spec.SecretKeySpec(clave, "AES"), spec)
        return cipher.doFinal(contenido)
    }
}
