-- Semilla: 60 días de indicadores con 2 huecos a propósito (días -15 y -8).
-- Para pruebas: ejecutar después de V0007__dashboard.sql.
DO $$
DECLARE
    base_date date := CURRENT_DATE - 60;
    d         date;
    skip1     date := CURRENT_DATE - 15;
    skip2     date := CURRENT_DATE - 8;
BEGIN
    FOR i IN 0..59 LOOP
        d := base_date + i;
        CONTINUE WHEN d = skip1 OR d = skip2;
        INSERT INTO dashboard.indicador_diario (dia, indicador, valor) VALUES
            (d, 1, 300 + (i * 3 % 100)),
            (d, 2, 42  + (i % 10)),
            (d, 3, 15  + (i % 8)),
            (d, 4, 237),
            (d, 5, 336),
            (d, 6, 18  + (i % 5)),
            (d, 7, 6   + (i % 3)),
            (d, 8, 3   + (i % 4)),
            (d, 9, 2   + (i % 3));
    END LOOP;
END $$;
