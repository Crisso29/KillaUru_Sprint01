# Implementation Plan: Módulo Auditoría

**Branch**: `002-modulo-auditoria` | **Date**: 2026-10-20 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-modulo-auditoria/spec.md`

## Summary

Módulo pequeño con un dominio liviano (`componentes-arquitectonicos.md`: «los módulos de soporte mantienen un dominio liviano»): una tabla *append-only*, un contrato `IAuditoriaService` que se une a la transacción del llamador mediante `IContextoTransaccion` (feature 001), un catálogo cerrado de acciones y una consulta paginada de solo lectura para el administrador.

## Technical Context

**Language/Version**: C# 14 / .NET 10

**Primary Dependencies**: EF Core 10 + Npgsql (`System.Text.Json` para el `jsonb`); `KillaUru.Shared`

**Storage**: esquema `auditoria`, tabla `registro` (ver `data-model.md`)

**Testing**: xUnit (dominio), NSubstitute + `FakeTimeProvider` (aplicación), Testcontainers (transacción compartida, `REVOKE`), `WebApplicationFactory` (endpoint y roles)

**Target Platform / Project Type**: igual que 001

**Performance Goals**: insertar un registro ≤ 5 ms dentro de la transacción del llamador; consulta de un mes < 500 ms p95 (SC-004)

**Constraints**: inmutable a nivel de BD; sin correos ni seudónimos; ningún archivo > 100 líneas

**Scale/Scope**: miles de registros por mes, retención permanente

## Constitution Check

| Principio | Cómo lo cumple | Estado |
|---|---|---|
| I. Docs manda | FR con fuente; catálogo = RF-15 (+1 derivado del prototipo, señalado) | ✅ |
| II. Monolito modular | Solo depende de `Shared`; esquema y `DbContext` propios; todo `internal` salvo `Contracts/` | ✅ |
| III. Calibrable | Sin umbrales de negocio; hora por `TimeProvider` | ✅ |
| IV. Datos confiables | Misma transacción que la acción, exigida (ADR-013, D15); métrica de riesgo solo tras el commit | ✅ |
| V. Privacidad | Contexto sin correo, contraseña ni seudónimo; actor solo por id | ✅ |
| VI. Acceso en el servidor | Consulta con política `SoloAdministrador`; `REVOKE UPDATE, DELETE, TRUNCATE` | ✅ |
| VII. Pruebas primero | Dominio y transacción compartida probados antes del código | ✅ |
| VIII. Código pequeño | 1 caso de uso por archivo; árbol abajo | ✅ |
| IX. API documentada | `auditoria.openapi.yaml` + XML | ✅ |
| X. Costo cero | Nada nuevo | ✅ |

## Project Structure

### Documentation

```text
specs/002-modulo-auditoria/
├── spec.md · plan.md · research.md · data-model.md · quickstart.md · tasks.md
├── contracts/auditoria.openapi.yaml · contracts/contratos-internos.md
└── checklists/requirements.md · checklists/trazabilidad.md
```

### Source Code — árbol exacto

```text
backend/src/Modules/KillaUru.Auditoria/
├── KillaUru.Auditoria.csproj                 # referencia: KillaUru.Shared
├── AuditoriaModule.cs                        # AddAuditoriaModule: DbContext, repositorio, casos de uso, IAuditoriaService
├── README.md
├── Contracts/                                # PÚBLICO
│   ├── IAuditoriaService.cs                  # RegistrarAsync (exige transacción) y RegistrarLecturaAsync (propia) (D15)
│   ├── AccionAuditada.cs                     # enum con los 14 códigos del catálogo
│   └── ActorAuditoria.cs                     # record (Guid? Id, string? Rol) + ActorAuditoria.Sistema
├── Domain/                                   # internal
│   ├── RegistroAuditoria.cs                  # entidad inmutable; Crear(...) valida acción y contexto
│   ├── CategoriaAuditoria.cs                 # enum: Usuarios, Sesion, CuentaDelTitular, Moderacion, Exportacion, AccesoADatos
│   ├── CatalogoAcciones.cs                   # acción → categoría y ¿es de riesgo?
│   └── ClavesProhibidas.cs                   # correo, email, contrasena, password, seudonimo
├── Application/                              # internal
│   ├── Puertos/IRegistroAuditoriaRepositorio.cs   # AgregarEnTransaccionAsync, AgregarConTransaccionPropiaAsync, ConsultarAsync
│   ├── RegistrarAuditoriaUseCase.cs          # exige transacción unida (si no, InvalidOperationException); guarda; métrica de riesgo con AlConfirmar
│   ├── RegistrarLecturaUseCase.cs            # transacción propia; guarda, confirma y emite la métrica de riesgo después (D15)
│   ├── ConsultarAuditoriaUseCase.cs          # valida filtros, consulta y se audita con RegistrarLecturaUseCase (FR-012)
│   ├── FiltrosAuditoria.cs                   # record: desde, hasta, categoría, acción, actorId, página, tamaño
│   ├── PaginaAuditoria.cs                    # record: elementos, página, tamaño, total
│   └── AuditoriaService.cs                   # implementa IAuditoriaService → RegistrarAuditoriaUseCase / RegistrarLecturaUseCase
├── Adapters/                                 # internal
│   ├── AuditoriaController.cs                # GET /api/v1/auditoria  [Authorize(SoloAdministrador)]
│   └── Dtos/ ConsultaAuditoriaDto.cs · RegistroAuditoriaDto.cs · PaginaAuditoriaDto.cs
└── Infrastructure/                           # internal
    ├── AuditoriaDbContext.cs                 # HasDefaultSchema("auditoria")
    ├── RegistroAuditoriaConfiguracion.cs     # mapeo EF (jsonb, smallint)
    ├── FabricaContextoAuditoria.cs           # CrearUnido() sobre la conexión de IContextoTransaccion (ADR-013) · CrearPropio()
    └── RegistroAuditoriaRepositorio.cs       # Agregar… (UseTransaction o transacción propia) y ConsultarAsync (AsNoTracking)

backend/db/migraciones/V0002__auditoria.sql
backend/tests/KillaUru.Auditoria.Tests/      Domain/ · Application/
backend/tests/KillaUru.Integracion.Tests/Auditoria/   TransaccionCompartidaTests · InmutabilidadTests · AuditoriaApiTests
backend/deploy/runbooks/incidentes-datos-personales.md · deploy/runbooks/restaurar.md (sección RN-10)
```

**Structure Decision**: plantilla de módulo de `enfoque-arquitectonico.md` §4 con dominio liviano (§4.1: «Registro inmutable», `RegistrarAuditoriaUseCase`, `AuditoriaController` de solo lectura, repositorio *append-only*).

## Complexity Tracking

Sin violaciones.
