# Mapa: recorte por rol y filtros (spec 006, T021 y T024)

> 2026-11-23, local con Docker y Testcontainers (PostgreSQL/PostGIS real). [Volver a evidencias](../../README.md) ·
> [Pruebas de 006](../../../../../specs/006-modulo-consulta-mapa/pruebas.md)

## 1. Resultado

| Medida | Resultado |
|---|---|
| Pruebas nuevas | **9** de integración: [RecorteRolApiTests](../../../../../backend/tests/KillaUru.Integracion.Tests/specs/006-modulo-consulta-mapa/integracion/RecorteRolApiTests.cs) (4) y [FiltrosApiTests](../../../../../backend/tests/KillaUru.Integracion.Tests/specs/006-modulo-consulta-mapa/integracion/FiltrosApiTests.cs) (5) |
| Suite de integración completa | **264: 261 aprobadas, 0 fallidas, 3 omitidas** (1 min 32 s) |
| Escenarios del quickstart cubiertos | 1, 2, 5, 6, 7, 8 y 13; además «`bbox` sin el demo → vacío» y «100 % de lo anónimo dentro del demo» (RNF-27) |

Repetir desde `backend/`: `dotnet test --project tests/KillaUru.Integracion.Tests --filter-class "*RecorteRolApiTests" --filter-class "*FiltrosApiTests"`.

## 2. Por qué son de integración

Lo que se prueba es la **cadena completa**: token → rol → política de alcance → celdas o filtros → consulta PostGIS →
GeoJSON → cabeceras de caché. Las piezas sueltas ya tenían pruebas unitarias con dobles (T022, T025); lo que faltaba era
demostrar que, juntas y contra la base real, el anónimo no ve nada fuera del demo y nadie más que el administrador
filtra por `estado`.

## 3. Cómo se prepararon los datos

- [`EscenarioMapa`](../../../../../backend/tests/KillaUru.Integracion.Tests/specs/006-modulo-consulta-mapa/soporte/EscenarioMapa.cs)
  crea anomalías **reales** (procesadas por Anomalías) y las lleva al estado pedido por SQL, como harían la
  confirmación, el tiempo o la moderación. Reutiliza `EscenarioAnomalias` de 004 (se le agregó un parámetro opcional).
- La vía sintética de las pruebas no pasa por el polígono real de Ayacucho, así que se usa un **demo de prueba**.
- Cada anomalía queda a 0,002° (≈ 216 m) de las demás: más que el radio de agrupación (100 m).
- La base es compartida: cada prueba afirma sobre **sus** anomalías (por id) o sobre propiedades de toda la respuesta.

## 4. Mutaciones: ¿las pruebas detectan errores de verdad?

Las pruebas se escribieron sobre código que ya existía y pasaron, así que se rompió el código a propósito:

| Mutación en el código de producción | Resultado |
|---|---|
| El filtro punto por punto del anónimo deja pasar todo | **Primero sobrevivió.** El borde del demo de prueba coincidía con la rejilla de celdas (0,005°), así que no había celdas «de borde». Se movieron los bordes a mitad de celda (−75,6025 / −75,4025) y ahora **falla** «todo lo anónimo dentro del demo» |
| El repositorio ignora el filtro de severidad | **Detectada** (falla el escenario 8) |
| El conductor puede filtrar por `estado` | **Detectada** (fallan los escenarios 5 y 13) |
| El anónimo consulta también las celdas «Fuera» | **Sobrevive, y es correcto:** el filtro punto por punto da el mismo resultado por HTTP; solo cambia el rendimiento. La detecta T019, una prueba **unitaria** que sigue pendiente |

## 5. Lo que se aprendió al escribirlas

- Una anomalía **grave** (4–5) nace «PendienteConfirmacion» hasta que la confirman dos vehículos, uno con sesión
  (004 FR-011), y el conductor no la ve. Al principio la prueba de severidad falló por eso: el error era de la prueba,
  no de la API. `EscenarioMapa` ahora fija siempre el estado pedido.
- `Cache-Control` se comprueba por su **significado** (público 60 s / privado sin caché), no por el texto: .NET
  reordena las directivas y el orden no importa en HTTP.

## 6. Lo que sigue abierto en 006

T018 (caché y ETag), T019 (celdas «Fuera» no se consultan), T027 (`updatedSince`), T030 (Cloudflare), T031 (carga),
T032 y T033 (métricas). Sin tocar en esta parte, que se limitó a T021 y T024 según el plan.
