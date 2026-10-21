# Research: Módulo Consulta de Mapa

## R-001 · De `bbox` a celdas: hace falta saber qué celdas toca la vía — hueco en docs

- **Problema**: `diseño-interno` §4 dice «bbox → celdas» y «el corredor tiene ~1 000 celdas». Pero el rectángulo que envuelve los 333 km (≈ 2,6° × 1,5°) contiene ~150 000 celdas de 0,005°, casi todas cerro vacío. Sin saber cuáles tocan la vía, cada petición del corredor haría 150 000 búsquedas en la caché.
- **Decision**: `IAnomaliaLectura.CeldasDelCorredor()` (agregado en 004, R-012): celdas que toca la franja de 50 m alrededor de la polilínea. `CorredorMapa` las pide una vez (perezoso, cuando dejan de estar vacías) y guarda también la clasificación de la zona demo. Celdas de una petición = celdas del corredor cuyo rectángulo intersecta el `bbox`.
- **Rationale**: toda anomalía nace a ≤ 50 m de la vía (RN-13), así que su celda siempre está en ese conjunto. Mapa no copia la polilínea (un dato, un dueño).
- **Pendiente para el autor**: agregar la operación a `componentes-arquitectonicos.md` §3.1 y Nivel3 §2.2.

## R-002 · Tamaño de celda fijo y `Celda.Limites()`

- **Decision**: la celda es la de `Shared` (0,005°). Se agrega `Celda.Limites()` en Shared (001) para obtener su rectángulo sin repetir la fórmula. `MapaOptions` no tiene «tamaño de celda» aunque `enfoque` §5 lo lista.
- **Rationale**: `AnomaliaCambiada` trae la celda que calculó Anomalías y que está guardada en `anomalia.celda`; si Mapa usara otra rejilla, invalidaría la celda equivocada.
- **Pendiente para el autor**: quitar «tamaño de celda» de la fila `MapaOptions` en `enfoque-arquitectonico.md` §5.

## R-003 · `bbox` opcional y validación

- **Decision**: formato `minLon,minLat,maxLon,maxLat` (orden de GeoJSON). 400 `datos_invalidos` si no son 4 números, si `min > max` o si sale de [−180, 180] / [−90, 90]. Sin `bbox` = todo el alcance del rol (la app lo usa para la primera descarga offline).
- **Rationale**: RF-14 usa `?bbox=`; hacerlo opcional evita que la app tenga que conocer el rectángulo del corredor.

## R-004 · Estados por defecto del administrador

- **Decision**: sin `estado`, el administrador recibe `Activa` y `PendienteConfirmacion` (`EstadosAdministradorPorDefecto`). Con `estado=Expirada,Reparada,…` recibe lo que pida.
- **Rationale**: RF-17 pide «todas las anomalías, incluidas las no confirmadas»; las finales (`Expirada`, `Reparada`, `Descartada`) se acumulan 2 años (RN-10) y llenarían el mapa de puntos que ya no existen.
- **Pendiente para el autor**: confirmar el valor por defecto (RN-08 dice «todos los estados, con filtros»).

## R-005 · Filtros de conductores y anónimos

- **Decision**: `tipo`, `severidad`, `desde` y `hasta` se permiten a todos, siempre dentro del alcance del rol: `FiltrarAsync` con `Estados = [Activa]` y el `bbox` recortado; para el anónimo, además `ZonaDemo.Contiene` punto por punto. La respuesta anónima filtrada también es `public, max-age=60` (los filtros forman parte de la URL). `estado` de un no administrador → 403 `sin_permiso`.
- **Fuente**: `diseño-interno` §4, fila «Filtros».

## R-006 · Forma de `updatedSince`

- **Decision**:
  - Respuesta: la misma `FeatureCollection`; una baja es una *feature* con `geometry: null` y `properties: {id, eliminada: true}` (GeoJSON admite geometría nula). Nunca lleva estado ni posición (RF-14: «sin revelar su estado interno»).
  - Visible = `Activa` y, para el anónimo, dentro del polígono. Un cambio no visible → baja; para el anónimo, una `Activa` fuera del demo se omite (nunca la vio).
  - Administrador con `updatedSince`: todas las cambiadas como altas con su `estado`, sin bajas.
  - `generadoUtc` en la raíz de toda respuesta: la app lo guarda y lo manda como próximo `updatedSince`; así no depende del reloj del celular (RF-03 desconfía de él).
  - `updatedSince` con filtros → 400 `parametros_incompatibles`; en el futuro (> 5 min) → 400 `datos_invalidos`.
- **Rationale**: un solo formato para la app; las bajas no filtran información a un rol que no debe verla.

## R-007 · Aciertos en el borde (RNF-01: ≥ 80 % anónimo en el demo)

- **Decision**: Cloudflare cachea por URL exacta. Para que las URL se repitan, la app anónima pide **sin `bbox`** (todo el demo, ~65 km, pocos KB) y refresca la copia completa en lugar de usar `updatedSince` (cuyo valor cambia en cada petición). El conductor registrado sí usa `updatedSince` (no pasa por el borde).
- **Pendiente para el autor**: es una regla de la app; anotarla en RF-26 o en la spec de la app.

## R-008 · `ETag` y 304

- **Decision**: `ETag` fuerte = `"` + base64url(SHA-256(instante de arranque ‖ alcance ‖ (celda, versión)… en orden ascendente))[0..22] + `"`. `If-None-Match` igual → 304 con el mismo `ETag` y las mismas cabeceras de caché. Solo en el camino por celdas.
- **Rationale**: se calcula con datos en memoria; Cloudflare también revalida con él las respuestas anónimas.

## R-009 · Armar la respuesta sin volver a serializar

- **Decision**: `CeldaGeoJson` guarda las *features* de la celda ya en UTF-8, separadas por comas. El controlador escribe en el `PipeWriter`: encabezado (`{"type":"FeatureCollection","generadoUtc":"…","features":[`), los bloques de cada celda no vacía separados por comas, y el cierre `]}`. La compresión la hace Cloudflare (Brotli) en el borde, no el origen.
- **Rationale**: `diseño-interno` §4, fila «Valor»; ahorra CPU en la VM ARM gratuita.

## R-010 · Polígono del tramo demo

- **Decision**: el polígono se arma una vez a partir de la polilínea de la PE-28A entre el km 268 (Vinchos) y el km 333 (Ayacucho), con un margen de 200 m a cada lado, simplificado a ≤ 60 vértices, y se guarda en `appsettings.json` (`Mapa:PoligonoDemo`) y como GeoJSON en `db/datos/tramo_demo.geojson` (la app usa el mismo archivo para sombrear, RF-08).
- **Pendiente para el autor**: confirmar el km de Vinchos (268, el que usa el prototipo) y el margen de 200 m.

## Pendientes de confirmar por el autor

1. R-001 `CeldasDelCorredor` en `IAnomaliaLectura` (y R-012 de 004).
2. R-002 quitar «tamaño de celda» de `MapaOptions` en docs.
3. R-004 estados por defecto del administrador.
4. R-007 la app anónima pide sin `bbox` y sin `updatedSince`.
5. R-010 límites y margen del polígono demo.
