# Trazabilidad Checklist: Módulo Consulta de Mapa

**Purpose**: Todo lo que docs pide a Consulta de Mapa → FR → tarea
**Created**: 2026-10-21
**Feature**: [spec.md](../spec.md)

## Historias y requisitos funcionales

- [ ] CHK001 HU-07 · anónimo: solo el tramo demo aunque pida un `bbox` mayor (recorte en el servidor) → FR-002…FR-004 → T019…T021
- [ ] CHK002 HU-08 · registrado: 333 km, `Activa`, severidad y tiempo relativo; datos para usar sin señal → FR-002, FR-005, FR-012 → T011…T018, T025…T027
- [ ] CHK003 HU-12 · administrador: filtros por tipo, severidad, fecha y estado; incluye no confirmadas → FR-010, FR-011 → T022…T024
- [ ] CHK004 RF-14 · GeoJSON, filtros, `estado` solo administrador, `updatedSince` con bajas `{id, eliminada: true}`, `ETag`, borde solo anónimo → FR-001…FR-013 → T010…T027
- [ ] CHK005 RF-17 · mapa completo del administrador con filtros (sin `estado`, los 5 estados, D6) → FR-002, FR-010, FR-011 → T022…T024
- [ ] CHK006 RF-08 (parte servidor) · el servidor aplica la restricción del demo → FR-003, FR-004, FR-012 → T019…T021, T025
- [ ] CHK007 RF-26 (parte servidor) · `venceUtc` en cada anomalía (nulo en estados finales); incremental con bajas → FR-005, FR-012 → T010, T025…T027

## Reglas de negocio

- [ ] CHK008 RN-08 · tabla de alcance por rol; el recorte lo hace siempre el servidor → T006, T021

## Calidad y decisiones

- [ ] CHK009 RNF-01 · p95 < 250 ms en la API; ≥ 80 % de aciertos anónimos en el borde; aciertos de la caché propia → FR-016 → T030, T031, T033
- [ ] CHK010 RNF-11 · política `consultas` (60/h anónimo, 1 000/h conductor, 3 000/h administrador) → T016
- [ ] CHK011 RNF-27 · 100 % de respuestas anónimas recortadas → T021
- [ ] CHK012 ADR-006 · caché en memoria del módulo (HybridCache), Observer tras el *commit*, claves por celda y alcance, `public` solo sin token → T013, T014, T016, T030
- [ ] CHK013 ADR-011 · el mapa base no pasa por la API → (fuera de alcance, sin tarea)
- [ ] CHK014 EQ-11 · mismo `bbox` con y sin token a través de la caché → T021
- [ ] CHK015 EQ-14 · 300 consultando y 300 sincronizando, p95 < 250 ms → T031 (scripts de 001, D11)

## Diseño (`diseño-interno` §4, §6.5; `patrones-de-diseño.md`)

- [ ] CHK016 *Decorator* `FuenteCeldasCacheada` sobre `IFuenteCeldas` de un método → T013
- [ ] CHK017 *Observer* `InvalidarCeldasMapa` → T014
- [ ] CHK018 `ZonaDemo` precalcula celdas dentro y en el borde; borde filtrado antes de cachear → T005, T020
- [ ] CHK019 `ETag` con instante de arranque, alcance y versiones → T009
- [ ] CHK020 Filtros sin caché por celdas, sin `ETag` → T022…T024
- [ ] CHK021 Valor ya serializado; respuesta por concatenación → T010, T016
- [ ] CHK022 Sin esquema, `DbContext` ni SQL (regla 3) → FR-014 → T017

## Decisiones del autor (confirmadas el 2026-10-21)

- [x] CHK023 `CeldasDelCorredor` (R-001; 004 R-012) y `Celda.Limites` (R-002) → D1
- [x] CHK024 Sin `estado`, el administrador recibe los 5 estados; el panel abre con «Activa + PendienteConfirmacion» (R-004) → D6 → FR-002 → T022
- [x] CHK025 La app anónima pide sin `bbox` ni `updatedSince` (R-007) → D17: pasa a la spec 008-app-android
- [x] CHK026 Polígono demo km 268 → 333, margen 200 m, ≤ 60 vértices (R-010) → D1 → T002
- [ ] CHK027 `kmVia` en cada *feature*, todos los roles (R-011) → D13 → FR-005 → T010
- [ ] CHK028 `venceUtc` nulo en estados finales → T-PO08 → FR-005 → T010
- [ ] CHK029 Bajas del anónimo solo dentro del polígono demo (R-012) → T-SEG16 → FR-012 → T025, T027
- [ ] CHK030 Regla 401/403: `estado` de otro rol → 403 con o sin token → T-SEG20 → FR-011 → T024
- [ ] CHK031 Métrica `killauru.mapa.cache{resultado}` de baja cardinalidad (R-013) → T-OPS15 → FR-016 → T033
