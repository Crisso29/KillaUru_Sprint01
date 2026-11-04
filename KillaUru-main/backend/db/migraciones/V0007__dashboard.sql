-- Dashboard: tabla de indicadores diarios.
-- RN-17: solo cifras; sin IDs, seudónimos ni correos en este esquema.
CREATE SCHEMA IF NOT EXISTS dashboard;

CREATE TABLE dashboard.indicador_diario (
    dia        date     NOT NULL,
    indicador  smallint NOT NULL CHECK (indicador BETWEEN 1 AND 9),
    valor      numeric  NOT NULL CHECK (valor >= 0),
    PRIMARY KEY (dia, indicador)
);

REVOKE ALL ON SCHEMA dashboard FROM PUBLIC;
GRANT USAGE             ON SCHEMA dashboard                     TO killauru_app;
GRANT SELECT, INSERT, DELETE ON TABLE dashboard.indicador_diario TO killauru_app;
