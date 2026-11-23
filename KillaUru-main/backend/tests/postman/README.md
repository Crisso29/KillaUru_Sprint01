# Colección de Postman de la API

> Pruebas funcionales de la API **desplegada** (HTTP real, base real). Complementan, no reemplazan, a las pruebas
> de integración del backend (`KillaUru.Integracion.Tests`) y a las de carga (k6, [`../carga`](../carga/README.md)).
> [Volver a las pruebas del backend](../README.md)

## Qué hay

| Archivo | Qué es |
|---|---|
| `KillaUru.postman_collection.json` | La colección (formato v2.1). **Generada**: no se edita a mano |
| `local.postman_environment.json` | Entorno `killauru-local` (`http://localhost:18080`). Solo la URL |
| `generar_coleccion.py` y `escenarios_*.py` | Generador: de aquí sale la colección |
| `contratos.py` | Lee las operaciones de los contratos OpenAPI de las specs 001–007 |
| `test_coleccion.py` | Prueba de contrato de la colección (corre en CI) |

## Qué prueba (por carpeta)

| Carpeta | Contenido |
|---|---|
| 01 Plataforma | `/health/live` y `/health/ready` |
| 02 Identidad pública | Versiones legales, registro (201, 409 `correo_en_uso`, 400 con 9 caracteres), inicio de sesión por la app, renovación, enlaces inventados y recuperación sin revelar si el correo existe (RN-19) |
| 03 Mi cuenta | `GET /me` (y que no se guarde en caché), aceptar versión, reenviar verificación |
| 04 Ingesta | Lote anónimo, reenvío idempotente (duplicados), lote con sesión, lote vacío |
| 05 Mapa | GeoJSON con `ETag`, `304` con `If-None-Match`, `bbox` válido e inválido |
| 06 Panel protegido | Los **16** endpoints de administrador: 401 sin sesión y 403 con un conductor. Se generan del contrato |
| 07 Cierre | Cerrar sesión y borrar la cuenta creada: la base queda como estaba |

Después de **cada** petición se comprueba además: `nosniff` y HSTS (FR-013), que todo error sea
`application/problem+json` con `codigo` y `correlacionId` (constitución IX), y que responda en menos de 2 s.

**Qué no prueba:** las respuestas correctas de los endpoints de administrador. Entrar como administrador exige el
captcha Turnstile verificado con Cloudflare, que en local no se simula (decisión del autor, 2026-11-23). Esas
respuestas las cubren las pruebas de integración del backend.

## Usarla

**En Postman:** *Import* → los dos `.json` → elegir el entorno «KillaUru local» → *Run collection*. Correr la
colección **entera y en orden**: cada carpeta usa lo que dejó la anterior (la cuenta, los tokens, el ETag).

**Con Newman** (lo mismo, por consola), con la API local levantada
([`deploy/local`](../../deploy/local/README.md)):

```powershell
npx --yes newman@6.2.1 run KillaUru.postman_collection.json -e local.postman_environment.json
```

**Contra la VM de pruebas:** el mismo comando con `--env-var "baseUrl=https://…"`.

## Cambiarla

1. Editar el `escenarios_*.py` que corresponda (o el contrato OpenAPI, si cambia la API).
2. `python generar_coleccion.py`
3. `python -m unittest -v` (necesita `PyYAML`, el mismo de `herramientas/cdn/requisitos.txt`).

Si se edita el `.json` a mano, `test_coleccion.py` falla: así la colección no se desincroniza de los contratos.
