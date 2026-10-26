# Trazabilidad Checklist: Módulo Anomalías

**Purpose**: Todo lo que docs pide a Anomalías → FR → tarea
**Created**: 2026-10-20
**Feature**: [spec.md](../spec.md)

## Historias y requisitos funcionales

- [x] CHK001 HU-05 · validar, descartar por evento, confirmar severidad alta con 2 dispositivos a ≥ 3 min → FR-001…FR-013 → T006…T031
- [x] CHK002 HU-06 · persistente/episódica y caducidad automática → FR-016…FR-019 → T011, T012, T033…T036
- [x] CHK003 HU-19 · reparada, descartada, «Por revisar», auditado → FR-020, FR-025…FR-027 → T013, T044…T047
- [x] CHK004 HU-20 · CSV y GeoJSON sin seudónimos, auditado → FR-028 → T048…T050
- [x] CHK005 HU-21 · seudónimo purgado a 72 h; sin vínculo con cuentas → FR-021, FR-023 → T037…T040
- [x] CHK006 RF-12 · reglas de recepción y procesamiento, tope, confirmación con sesión → FR-001…FR-013
- [x] CHK007 RF-13 · agrupación ≤ 100 m, mediana 15 d, persistencia UTC−5 → FR-008, FR-009, FR-016
- [x] CHK008 RF-20 · exportación sin seudónimos → FR-028
- [x] CHK009 RF-23 · 5 estados, caducidad parametrizable revisada cada hora con hora de detección → FR-017, FR-018
- [x] CHK010 RF-24 · acciones del administrador según RN-07 + «Por revisar» → FR-025…FR-027
- [x] CHK011 RF-27 (parte Anomalías) · purga del seudónimo en reportes → FR-021
- [x] CHK012 RF-18 (datos) · resumen diario con conteos → FR-029 → T041…T043

## Reglas de negocio

- [x] CHK013 RN-01 · Bache, Hundimiento; Normal nunca se guarda → T007
- [x] CHK014 RN-02 · severidad 1–5, alta = 4–5 → T010
- [x] CHK015 RN-05 · posición fija, la más cercana, mediana redondeada hacia arriba, hora de detección → T012, T026
- [x] CHK016 RN-06 · confirmación, mismo vehículo = uno, vuelve a nacer, `Activa` no retrocede → T010, T012
- [x] CHK017 RN-07 · ciclo, tabla de acciones, caducidad horaria, «Por revisar», `Descartada` absorbe → T012, T013, T036
- [x] CHK018 RN-09 · purga 72 h desde la recepción; `recibido_utc` a la hora (D3) → FR-021 → T037, T039
- [x] CHK019 RN-10 · 2 años; `Descartada` solo tras 2 años sin reportes (D7) → FR-022 → T037, T039
- [x] CHK020 RN-11 · 7 motivos + `SeudonimoInconsistente = 8` (lo evalúa Ingesta, D2), recepción vs procesamiento, antigüedad desde la recepción → FR-034 → T004, T007, T008
- [x] CHK021 RN-12 · 3 días locales en 15 d, UTC−5 → T011
- [x] CHK022 RN-13 · 50 m del eje → T006, T008
- [x] CHK023 RN-14 · ≥ 1 reporte con sesión para lo grave; solo sí/no → T010
- [x] CHK024 RN-15 · tope 120 por hora de detección (no retroactivo, solo nuevas) y 150 km/h con margen GPS ≥ 10 s → T009, T025

## Calidad y decisiones

- [ ] CHK025 RNF-06 / EQ-09 · idempotencia y reanudación → T027 (y 005)
- [x] CHK026 RNF-10 · sin vínculo cuenta ↔ eventos → T040
- [ ] CHK027 RNF-23 · ≤ 100 ms por evento → T053
- [x] CHK028 RNF-25 · purga en reportes → T039
- [ ] CHK029 ADR-005 · bloqueo por celda, dueño de su transacción, idempotencia de extremo a extremo → T027, T029
- [x] CHK030 ADR-006 · publicar tras el commit → T025
- [x] CHK031 ADR-008 · estados y caducidad parametrizable → T012, T036
- [x] CHK032 ADR-013 · moderación auditada en su misma transacción; exportación con auditoría de lectura confirmada antes de escribir → T044, T049
- [ ] CHK033 ADR-015 / EQ-12 · defensas por contenido → T054

## Derivados del prototipo y huecos de docs (confirmar)

- [x] CHK034 Detalle, listas y últimos cambios (prototipo) → FR-027 → T044…T047
- [x] CHK035 `RecibidoUtc` en `ContextoReporte` (R-001, confirmado D1) → T004
- [x] CHK036 Acción `MantenerDescartada` (R-005, confirmado D1) → T013
- [x] CHK037 `transicion.actor_id` y `reporte_anomalia.km_via` (R-006, R-011, confirmado D1) → T001, T030

## Revisión 2026-10-21 (decisiones del autor y correcciones técnicas)

- [x] CHK038 D13 · `anomalia.km_via` al nacer; `KmVia` en `AnomaliaVisible`/`AnomaliaCambio`; detalle y listas desde la columna → FR-027, FR-032 → T001, T012, T025, T031, T041, T044
- [x] CHK039 D14 · `Reparada` renace tras `estado_desde_utc`; `Renacimientos` = reaparición; `EventosRecibidos` = eventos válidos → FR-019, FR-029 → T012, T043
- [x] CHK040 D4 · controladores solo con tipos de Application (`ComandoModeracion`, `ResultadoModeracion`); `AccionAdmin` en Domain → FR-025 → T057, T046
- [x] CHK041 D15 · moderación con `RegistrarAsync` en su transacción; exportación: contar → `RegistrarLecturaAsync` con `filas` → escribir → FR-025, FR-028 → T044, T049, T050
- [x] CHK042 T-BD02 · `FOR UPDATE` + revalidación en trabajador y moderación; caducidad `SKIP LOCKED` → FR-030 → T026, T027, T036, T047
- [x] CHK043 T-OPS05 · tareas al arrancar y por reloj (`:05`, `:10`, 03:30) con `ProgramaReloj` → FR-018, FR-021, FR-022, FR-024 → T035, T038, T058
- [x] CHK044 T-OPS14 · `RegistrarRetenidoPorTope` y `RegistrarDuracionProceso`; sin `RegistrarAccionRiesgo` → FR-013, FR-031 → T025, T053
- [x] CHK045 T-OPS19 · comprobación `corredor` en `/health/ready` → FR-033 → T024
- [x] CHK046 T-BD-CHK · CHECK nuevos y sin `ix_anomalia_pendiente` → T001, T056
- [x] CHK047 T-ARQ06/07/08 · `IAnomaliaRepository`, `AnomaliaRepositoryPostgis`, puertos de listas, detalle, exportación y resumen; archivos partidos → T028, T030, T033, T037, T041, T045, T049
- [x] CHK048 T-PO08 / T-PO11 / T-PO19 · `venceUtc` nulo en estados finales; columnas de exportación; `severidadMostrada` en «Por revisar» → FR-027, FR-028 → T044, T048
- [x] CHK049 T-QA18 · 150/151 km/h y futuro 5 min ± 1 s → T008, T009
- [x] CHK050 T-SEG19 · 429 en las 6 operaciones del contrato → T052

## Evidencia de geometría — 2026-10-24

- [x] Eje OSM conservado y reproducible; V0005 verificada en PostGIS, 6689 vértices y 335.0729634613575 km → T002 → `CorredorMigracionTests`.
- [x] Veinte referencias con tolerancia ≤ 1 m y extremo demo obtenido de la longitud real → `CorredorRealTests`, `backend/db/datos/km_referencia.csv`.
- [x] RN-08, glosario, espejo de docs y specs dependientes actualizados; cobertura total = 336 tramos.
- [ ] Polígono demo de 200 m y ≤ 60 vértices, pendiente de 006 T002.

Implementación local, evidencias y condiciones de cierre: [revisión de implementación](../revision-implementacion.md).

## Evidencia de cierre local — 2026-10-26

Matriz de archivos, pruebas y resultados en [revision-implementacion.md](../revision-implementacion.md). CHK025 y CHK029 tienen bloqueo ordenado e idempotencia local comprobados; quedan abiertos por reanudación e idempotencia de extremo a extremo en 005. CHK027 no cumple aún el umbral: p95 local Release 185,995 ms > 100 ms. CHK033 tiene defensas verificadas en el servicio, pero espera la recepción HTTP/cola de 005. CHK044 cierra la instrumentación y separación de métricas, no el objetivo de latencia de CHK027. CHK020 verifica el contrato de motivos; la detección de `SeudonimoInconsistente` corresponde a Ingesta.
