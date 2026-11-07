package pe.killauru.app.deteccion.dominio

import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flowOf
import kotlinx.coroutines.runBlocking
import org.junit.Test
import kotlin.test.assertEquals

/** T043 / FR-016, R-032: 7 estados del indicador y «N por enviar». */
class CalcularIndicadorUseCaseTest {

    private class RepositorioFalso(private val pendientesN: Int = 0) : RepositorioEventos {
        override suspend fun guardar(e: Evento) {}
        override fun pendientes(): Flow<List<Evento>> = flowOf(emptyList())
        override suspend fun contarPendientes() = pendientesN
        override suspend fun contarEnviando() = 0
        override suspend fun tomarLote(s: String, m: Int) = emptyList<Evento>()
        override suspend fun expirar(a: Long) {}
        override suspend fun recuperarEnviando() {}
        override suspend fun aplicarEstados(c: Map<String, EstadoLocal>) {}
    }

    private fun entrada(
        servicioActivo: Boolean = true,
        tienePermiso: Boolean = true,
        tieneGps: Boolean = true,
        enAhorro: Boolean = false,
        soloVerMapa: Boolean = false,
        velocidadOk: Boolean = true,
    ) = CalcularIndicadorUseCase.Entrada(
        servicioActivo, tienePermiso, tieneGps, enAhorro, soloVerMapa, velocidadOk
    )

    @Test fun `sin servicio devuelve SinServicio`() = runBlocking {
        val uc = CalcularIndicadorUseCase(RepositorioFalso())
        // Arrange / Act
        val r = uc.calcular(entrada(servicioActivo = false))
        // Assert
        assertEquals(IndicadorEstado.SinServicio, r.indicador)
    }

    @Test fun `sin permiso devuelve SinPermiso`() = runBlocking {
        val uc = CalcularIndicadorUseCase(RepositorioFalso())
        val r = uc.calcular(entrada(tienePermiso = false))
        assertEquals(IndicadorEstado.SinPermiso, r.indicador)
    }

    @Test fun `ahorro de bateria devuelve ModoAhorro`() = runBlocking {
        val uc = CalcularIndicadorUseCase(RepositorioFalso())
        val r = uc.calcular(entrada(enAhorro = true))
        assertEquals(IndicadorEstado.ModoAhorro, r.indicador)
    }

    @Test fun `solo ver mapa devuelve SoloMapa`() = runBlocking {
        val uc = CalcularIndicadorUseCase(RepositorioFalso())
        val r = uc.calcular(entrada(soloVerMapa = true))
        assertEquals(IndicadorEstado.SoloMapa, r.indicador)
    }

    @Test fun `sin GPS devuelve SinGps`() = runBlocking {
        val uc = CalcularIndicadorUseCase(RepositorioFalso())
        val r = uc.calcular(entrada(tieneGps = false))
        assertEquals(IndicadorEstado.SinGps, r.indicador)
    }

    @Test fun `velocidad baja devuelve EnPausa`() = runBlocking {
        val uc = CalcularIndicadorUseCase(RepositorioFalso())
        val r = uc.calcular(entrada(velocidadOk = false))
        assertEquals(IndicadorEstado.EnPausa, r.indicador)
    }

    @Test fun `detectando con pendientes informa cuantos hay`() = runBlocking {
        val uc = CalcularIndicadorUseCase(RepositorioFalso(pendientesN = 5))
        val r = uc.calcular(entrada())
        assertEquals(IndicadorEstado.Detectando, r.indicador)
        assertEquals(5, r.pendientes)
    }
}
