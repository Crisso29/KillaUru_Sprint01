# Trazabilidad Checklist: Módulo Consulta de Mapa

**Purpose**: Todo lo que docs pide a Consulta de Mapa → FR → tarea
**Created**: 2026-10-21
**Feature**: [spec.md](../spec.md)

## Historias y requisitos funcionales

- [ ] CHK001 HU-07 · anónimo: solo el tramo demo aunque pida un `bbox` mayor (recorte en el servidor) → FR-002…FR-004 → T019…T021
- [ ] CHK002 HU-08 · registrado: 333 km, `Activa`, severidad y tiempo relativo; datos para usar sin señal → FR-002, FR-005, FR-012 → T011…T018, T025…T027
- [ ] CHK003 HU-12 · administrador: filtros por tipo, severidad, fecha y estado; incluye no confirmadas → FR-010, FR-011 → T022…T024
- [ ] CHK004 RF-14 · GeoJSON, filtros, `estado` solo administrador, `updatedSince` con bajas `{id, eliminada: true}`, `ETag`, borde solo anónimo → FR-001…FR-013
- [ ] CHK005 RF-17 · mapa completo del administrador con filtros → FR-010, FR-011
- [ ] CHK006 RF-08 (parte servidor) · el servidor aplica la restricción del demo → FR-003, FR-004
- [ ] CHK007 RF-26 (parte servidor) · `venceUtc` en cada anomalía; incremental con bajas → FR-005, FR-012

## Reglas de negocio

- [ ] CHK008 RN-08 · tabla de alcance por rol; el recorte lo hace siempre el servidor → T006, T021

## Calidad y decisiones

- [ ] CHK009 RNF-01 · p95 < 250 ms en la API; ≥ 80 % de aciertos anónimos en el borde → T030, T031
- [ ] CHK010 RNF-11 · política `consultas` (60/h anónimo, 1 000/h conductor, 3 000/h administrador) → T016
- [ ] CHK011 RNF-27 · 100 % de respuestas anónimas recortadas → T021
- [ ] CHK012 ADR-006 · caché en memoria del módulo (HybridCache), Observer tras el *commit*, claves por celda y alcance, `public` solo sin token → T013, T014, T016, T030
- [ ] CHK013 ADR-011 · el mapa base no pasa por la API → (fuera de alcance, sin tarea)
- [ ] CHK014 EQ-11 · mismo `bbox` con y sin token a través de la caché → T021
- [ ] CHK015 EQ-14 · 300 clientes, p95 < 250 ms → T031

## Diseño (`diseño-interno` §4, §6.5; `patrones-de-diseño.md`)

- [ ] CHK016 *Decorator* `FuenteCeldasCacheada` sobre `IFuenteCeldas` de un método → T013
- [ ] CHK017 *Observer* `InvalidarCeldasMapa` → T014
- [ ] CHK018 `ZonaDemo` precalcula celdas dentro y en el borde; borde filtrado antes de cachear → T005, T020
- [ ] CHK019 `ETag` con instante de arranque, alcance y versiones → T009
- [ ] CHK020 Filtros sin caché por celdas, sin `ETag` → T022…T024
- [ ] CHK021 Valor ya serializado; respuesta por concatenación → T010, T016
- [ ] CHK022 Sin esquema, `DbContext` ni SQL (regla 3) → FR-014 → T017

## Huecos de docs (confirmar)

- [ ] CHK023 `CeldasDelCorredor` (R-001; 004 R-012) y `Celda.Limites` (R-002)
- [ ] CHK024 Estados por defecto del administrador (R-004)
- [ ] CHK025 La app anónima pide sin `bbox` ni `updatedSince` (R-007)
- [ ] CHK026 Límites y margen del polígono demo (R-010)
