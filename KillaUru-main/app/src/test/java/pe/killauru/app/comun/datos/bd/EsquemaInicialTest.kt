package pe.killauru.app.comun.datos.bd

import androidx.room.Room
import androidx.test.core.app.ApplicationProvider
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import kotlin.test.assertTrue

/** T013 / FR-019, data-model §1: 5 tablas, índices del data-model y WAL. */
@RunWith(RobolectricTestRunner::class)
class EsquemaInicialTest {
    private lateinit var bd: KillaUruBd

    @Before
    fun setUp() {
        bd = Room.inMemoryDatabaseBuilder(
            ApplicationProvider.getApplicationContext(),
            KillaUruBd::class.java
        ).allowMainThreadQueries()
         .addCallback(object : RoomDatabase.Callback() {
             override fun onCreate(db: androidx.sqlite.db.SupportSQLiteDatabase) {
                 TriggersValidacion.crear(db)
             }
         })
         .build()
        // Trigger lazy initialization
        bd.openHelper.writableDatabase
    }

    @After
    fun tearDown() { bd.close() }

    private fun tablas(): List<String> =
        bd.openHelper.writableDatabase.query(
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE 'android_%' AND name NOT LIKE 'room_%'"
        ).use { c -> buildList { while (c.moveToNext()) add(c.getString(0)) } }

    private fun indices(): List<String> =
        bd.openHelper.writableDatabase.query(
            "SELECT name FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%'"
        ).use { c -> buildList { while (c.moveToNext()) add(c.getString(0)) } }

    @Test
    fun `hay exactamente cinco tablas`() {
        // Arrange
        val esperadas = setOf("evento", "anomalia", "estado_copia", "recorrido", "contador_local")
        // Act
        val actuales = tablas().toSet()
        // Assert
        assertTrue(esperadas.all { it in actuales }, "Tablas encontradas: $actuales")
        assertTrue(actuales.size == 5, "Se esperaban 5 tablas pero hay ${actuales.size}: $actuales")
    }

    @Test
    fun `existen los indices del data-model`() {
        // Arrange
        val esperados = listOf(
            "index_evento_estado_detectadoUtc",
            "index_evento_estado_estadoDesde",
            "index_anomalia_km",
            "index_recorrido_finUtc",
        )
        // Act
        val actuales = indices()
        // Assert
        esperados.forEach { assertTrue(it in actuales, "Índice ausente: $it") }
    }

    @Test
    fun `la base de datos se abre correctamente`() {
        // Arrange / Act / Assert
        assertTrue(bd.isOpen)
    }
}
