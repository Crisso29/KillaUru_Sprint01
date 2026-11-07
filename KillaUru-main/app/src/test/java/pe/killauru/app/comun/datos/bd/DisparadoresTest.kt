package pe.killauru.app.comun.datos.bd

import androidx.room.Room
import androidx.sqlite.db.SupportSQLiteDatabase
import androidx.test.core.app.ApplicationProvider
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import kotlin.test.assertEquals
import kotlin.test.assertFails

/** T014 / FR-019, data-model §2: validaciones de integridad por disparadores SQLite. */
@RunWith(RobolectricTestRunner::class)
class DisparadoresTest {
    private lateinit var bd: KillaUruBd
    private lateinit var db: SupportSQLiteDatabase

    @Before
    fun setUp() {
        bd = Room.inMemoryDatabaseBuilder(
            ApplicationProvider.getApplicationContext(),
            KillaUruBd::class.java
        ).allowMainThreadQueries()
         .addCallback(object : RoomDatabase.Callback() {
             override fun onCreate(db: SupportSQLiteDatabase) { TriggersValidacion.crear(db) }
         })
         .build()
        db = bd.openHelper.writableDatabase
    }

    @After
    fun tearDown() { bd.close() }

    private fun insertarEvento(
        eventoId: String = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        estado: Int = 0, tipo: Int = 1, severidad: Int = 3,
        confidence: Double = 0.80, lat: Double = -13.2, lon: Double = -74.2,
        precisionM: Double = 5.0, modeloVersion: String = "v1.0",
        seudonimo: String = "s1b2c3d4-e5f6-7890-abcd-ef1234567890",
    ) = db.execSQL("""
        INSERT INTO evento(eventoId,detectadoUtc,lat,lon,precisionM,tipo,severidad,
            confidence,modeloVersion,seudonimo,estado,estadoDesde)
        VALUES('$eventoId',1000,'$lat','$lon','$precisionM',$tipo,$severidad,
            $confidence,'$modeloVersion','$seudonimo',$estado,1000)""".trimIndent())

    @Test
    fun `insertar evento con estado distinto de cero lanza evento_invalido`() {
        // Arrange / Act / Assert
        val error = assertFails { insertarEvento(estado = 1) }
        assertEquals("evento_invalido", error.message)
    }

    @Test
    fun `insertar evento con tipo invalido lanza evento_invalido`() {
        // Arrange / Act / Assert
        val error = assertFails { insertarEvento(tipo = 3) }
        assertEquals("evento_invalido", error.message)
    }

    @Test
    fun `insertar evento con severidad fuera de rango lanza evento_invalido`() {
        // Arrange / Act / Assert
        val e1 = assertFails { insertarEvento(severidad = 0) }
        val e2 = assertFails { insertarEvento(severidad = 6) }
        assertEquals("evento_invalido", e1.message)
        assertEquals("evento_invalido", e2.message)
    }

    @Test
    fun `insertar evento con confidence menor a 0_75 lanza evento_invalido`() {
        // Arrange / Act / Assert
        val error = assertFails { insertarEvento(confidence = 0.74) }
        assertEquals("evento_invalido", error.message)
    }

    @Test
    fun `insertar evento con lat invalida lanza evento_invalido`() {
        // Arrange / Act / Assert
        val error = assertFails { insertarEvento(lat = 91.0) }
        assertEquals("evento_invalido", error.message)
    }

    @Test
    fun `transicion invalida lanza transicion_invalida`() {
        // Arrange
        insertarEvento()
        // Act / Assert
        val error = assertFails {
            db.execSQL("UPDATE evento SET estado=2,estadoDesde=1001 WHERE eventoId='a1b2c3d4-e5f6-7890-abcd-ef1234567890'")
        }
        assertEquals("transicion_invalida", error.message)
    }

    @Test
    fun `cambiar eventoId lanza evento_inmutable`() {
        // Arrange
        insertarEvento()
        // Act / Assert
        val error = assertFails {
            db.execSQL("UPDATE evento SET eventoId='x1b2c3d4-e5f6-7890-abcd-ef1234567890' WHERE eventoId='a1b2c3d4-e5f6-7890-abcd-ef1234567890'")
        }
        assertEquals("evento_inmutable", error.message)
    }

    @Test
    fun `insertar estado_copia con id distinto de 1 lanza estado_copia_invalido`() {
        // Arrange / Act / Assert
        val error = assertFails {
            db.execSQL("INSERT INTO estado_copia(id,alcance,generadoUtc,etag,verificadoUtc) VALUES(2,0,NULL,NULL,NULL)")
        }
        assertEquals("estado_copia_invalido", error.message)
    }
}
