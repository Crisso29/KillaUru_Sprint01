package pe.killauru.app.comun.datos.bd

import android.content.Context
import androidx.room.Room
import androidx.room.RoomDatabase
import androidx.sqlite.db.SupportSQLiteDatabase
import androidx.sqlite.db.SupportSQLiteStatement
import androidx.test.core.app.ApplicationProvider
import kotlinx.coroutines.runBlocking
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * T065 / FR-025, RNF-15, RNF-33: 85 000 eventos + 5 000 anomalías + 400 recorridos
 * deben caber en < 25 MB; los PENDING sobreviven 14 días sin sincronización.
 */
@RunWith(RobolectricTestRunner::class)
@Config(sdk = [34])
class TamanoBaseTest {

    private lateinit var db: KillaUruBd
    private val dbName = "test-tamano.db"

    private val ctx: Context get() = ApplicationProvider.getApplicationContext()

    @Before
    fun crearBd() {
        db = Room.databaseBuilder(ctx, KillaUruBd::class.java, dbName)
            .addCallback(object : RoomDatabase.Callback() {
                override fun onCreate(db: SupportSQLiteDatabase) {
                    TriggersValidacion.crear(db)
                }
            })
            .allowMainThreadQueries()
            .build()
    }

    @After
    fun limpiar() {
        runCatching { db.close() }
        ctx.deleteDatabase(dbName)
        ctx.deleteDatabase("$dbName-wal")
        ctx.deleteDatabase("$dbName-shm")
    }

    @Test
    fun `85000 eventos 5000 anomalias 400 recorridos caben en menos de 25 MB`() {
        val sqDb = db.openHelper.writableDatabase
        insertarEventos(sqDb, 85_000)
        insertarAnomalias(sqDb, 5_000)
        insertarRecorridos(sqDb, 400)
        sqDb.query("PRAGMA wal_checkpoint(TRUNCATE)", emptyArray()).close()
        db.close()
        val bytes = ctx.getDatabasePath(dbName).length()
        assertTrue(bytes < 25L * 1024 * 1024, "DB = ${bytes / 1024} KB supera 25 MB")
    }

    @Test
    fun `eventos PENDING sobreviven 14 dias sin sincronizacion`() = runBlocking {
        val sqDb = db.openHelper.writableDatabase
        insertarEventos(sqDb, 10, offset = 900_000)
        db.purgaDao().purgarEventosFinales(2_000_000_000_000L)
        val pendientes = db.eventoDao().contarPendientes()
        assertEquals(10, pendientes, "PENDING perdidos tras purga — esperados 10, hay $pendientes")
    }

    // --- helpers de inserción masiva ---

    private fun insertarEventos(sqDb: SupportSQLiteDatabase, n: Int, offset: Int = 0) {
        sqDb.beginTransaction()
        try {
            val s = sqDb.compileStatement(SQL_EVENTO)
            repeat(n) { i -> bindEvento(s, offset + i, offset + n + i); s.executeInsert(); s.clearBindings() }
            sqDb.setTransactionSuccessful()
        } finally { sqDb.endTransaction() }
    }

    private fun insertarAnomalias(sqDb: SupportSQLiteDatabase, n: Int) {
        sqDb.beginTransaction()
        try {
            val s = sqDb.compileStatement(SQL_ANOMALIA)
            repeat(n) { i -> bindAnomalia(s, i); s.executeInsert(); s.clearBindings() }
            sqDb.setTransactionSuccessful()
        } finally { sqDb.endTransaction() }
    }

    private fun insertarRecorridos(sqDb: SupportSQLiteDatabase, n: Int) {
        sqDb.beginTransaction()
        try {
            val s = sqDb.compileStatement(SQL_RECORRIDO)
            repeat(n) { i -> bindRecorrido(s, i); s.executeInsert(); s.clearBindings() }
            sqDb.setTransactionSuccessful()
        } finally { sqDb.endTransaction() }
    }

    private fun bindEvento(s: SupportSQLiteStatement, idx: Int, seuIdx: Int) {
        s.bindString(1, uuid("10", idx)); s.bindLong(2, BASE_MS + idx)
        s.bindDouble(3, -13.1627); s.bindDouble(4, -74.2242); s.bindDouble(5, 5.0)
        s.bindLong(6, 1L); s.bindLong(7, 3L); s.bindDouble(8, 0.80)
        s.bindString(9, "0.1.0"); s.bindString(10, uuid("10", seuIdx))
        s.bindLong(11, 0L); s.bindLong(12, BASE_MS + idx)
    }

    private fun bindAnomalia(s: SupportSQLiteStatement, idx: Int) {
        s.bindString(1, uuid("20", idx)); s.bindLong(2, 1L); s.bindLong(3, 2L)
        s.bindLong(4, 0L); s.bindDouble(5, -13.1627); s.bindDouble(6, -74.2242)
        s.bindDouble(7, idx.toDouble()); s.bindLong(8, 1_750_000_000_000L)
        s.bindLong(9, BASE_MS); s.bindLong(10, 0L)
    }

    private fun bindRecorrido(s: SupportSQLiteStatement, idx: Int) {
        val inicio = BASE_MS + idx * 3_600_000L
        s.bindLong(1, inicio); s.bindLong(2, inicio + 3_600_000L)
        s.bindDouble(3, 0.0); s.bindDouble(4, 50.0)
        s.bindDouble(5, 50.0); s.bindLong(6, 100L)
    }

    private fun uuid(prefix: String, i: Int) = "${prefix}000000-0000-0000-0000-%012d".format(i)

    companion object {
        private const val BASE_MS = 1_730_000_000_000L
        private const val SQL_EVENTO =
            "INSERT INTO evento(eventoId,detectadoUtc,lat,lon,precisionM,tipo,severidad," +
            "confidence,modeloVersion,seudonimo,estado,estadoDesde) VALUES(?,?,?,?,?,?,?,?,?,?,?,?)"
        private const val SQL_ANOMALIA =
            "INSERT INTO anomalia(id,tipo,severidad,persistente,lat,lon,km," +
            "venceUtc,ultimaDeteccionUtc,enDemo) VALUES(?,?,?,?,?,?,?,?,?,?)"
        private const val SQL_RECORRIDO =
            "INSERT INTO recorrido(inicioUtc,finUtc,kmDesde,kmHasta,kmRecorridos,eventos)" +
            " VALUES(?,?,?,?,?,?)"
    }
}
