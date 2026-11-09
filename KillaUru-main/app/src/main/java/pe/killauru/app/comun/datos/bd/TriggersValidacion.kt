package pe.killauru.app.comun.datos.bd

import androidx.sqlite.db.SupportSQLiteDatabase

/** data-model §2: disparadores de integridad; se instalan en onCreate y en cada migración. */
internal object TriggersValidacion {
    fun crear(db: SupportSQLiteDatabase) {
        validarEvento(db)
        validarTransicion(db)
        eventoInmutable(db)
        validarAnomalia(db)
        validarCambioAnomalia(db)
        validarCopia(db)
        validarRecorrido(db)
    }

    private fun validarEvento(db: SupportSQLiteDatabase) {
        db.execSQL("""
            CREATE TRIGGER IF NOT EXISTS trg_evento_valida BEFORE INSERT ON evento
            WHEN NEW.estado <> 0 OR NEW.tipo NOT IN (1,2) OR NEW.severidad NOT BETWEEN 1 AND 5
              OR NEW.confidence < 0.75 OR NEW.confidence > 1
              OR NEW.lat NOT BETWEEN -90 AND 90 OR NEW.lon NOT BETWEEN -180 AND 180
              OR NEW.precisionM < 0 OR length(NEW.eventoId) <> 36
              OR length(NEW.seudonimo) <> 36 OR length(NEW.modeloVersion) NOT BETWEEN 1 AND 32
            BEGIN SELECT RAISE(ABORT,'evento_invalido'); END""".trimIndent())
    }

    private fun validarTransicion(db: SupportSQLiteDatabase) {
        db.execSQL("""
            CREATE TRIGGER IF NOT EXISTS trg_evento_transicion BEFORE UPDATE OF estado ON evento
            WHEN NOT ((OLD.estado=0 AND NEW.estado IN (1,4))
                   OR (OLD.estado=1 AND NEW.estado IN (0,2,3)))
            BEGIN SELECT RAISE(ABORT,'transicion_invalida'); END""".trimIndent())
    }

    private fun eventoInmutable(db: SupportSQLiteDatabase) {
        db.execSQL("""
            CREATE TRIGGER IF NOT EXISTS trg_evento_inmutable
            BEFORE UPDATE OF eventoId,detectadoUtc,lat,lon,tipo,severidad,confidence,modeloVersion,seudonimo ON evento
            BEGIN SELECT RAISE(ABORT,'evento_inmutable'); END""".trimIndent())
    }

    private fun validarAnomalia(db: SupportSQLiteDatabase) {
        db.execSQL("""
            CREATE TRIGGER IF NOT EXISTS trg_anomalia_valida BEFORE INSERT ON anomalia
            WHEN NEW.tipo NOT IN (1,2) OR NEW.severidad NOT BETWEEN 1 AND 5
              OR NEW.persistente NOT IN (0,1) OR NEW.enDemo NOT IN (0,1)
              OR NEW.km < 0 OR length(NEW.id) <> 36
            BEGIN SELECT RAISE(ABORT,'anomalia_invalida'); END""".trimIndent())
    }

    private fun validarCambioAnomalia(db: SupportSQLiteDatabase) {
        db.execSQL("""
            CREATE TRIGGER IF NOT EXISTS trg_anomalia_valida_upd BEFORE UPDATE ON anomalia
            WHEN NEW.tipo NOT IN (1,2) OR NEW.severidad NOT BETWEEN 1 AND 5
              OR NEW.persistente NOT IN (0,1) OR NEW.enDemo NOT IN (0,1)
              OR NEW.km < 0 OR length(NEW.id) <> 36
            BEGIN SELECT RAISE(ABORT,'anomalia_invalida'); END""".trimIndent())
    }

    private fun validarCopia(db: SupportSQLiteDatabase) {
        db.execSQL("""
            CREATE TRIGGER IF NOT EXISTS trg_estado_copia_unica BEFORE INSERT ON estado_copia
            WHEN NEW.id <> 1 OR NEW.alcance NOT IN (0,1)
            BEGIN SELECT RAISE(ABORT,'estado_copia_invalido'); END""".trimIndent())
    }

    private fun validarRecorrido(db: SupportSQLiteDatabase) {
        db.execSQL("""
            CREATE TRIGGER IF NOT EXISTS trg_recorrido_valido BEFORE INSERT ON recorrido
            WHEN NEW.finUtc < NEW.inicioUtc OR NEW.eventos < 0 OR NEW.kmRecorridos < 0
            BEGIN SELECT RAISE(ABORT,'recorrido_invalido'); END""".trimIndent())
    }
}
