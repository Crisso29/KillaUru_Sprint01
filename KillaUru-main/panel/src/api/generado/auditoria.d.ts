export interface paths {
    "/api/v1/auditoria": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Consultar el registro de auditoría
         * @description Rol: Administrador.
         *     Devuelve los registros del más reciente al más antiguo, filtrados y paginados (HU-15, RF-15).
         *     La propia consulta queda auditada como `AccesoAdministrativoDatos` con `RegistrarLecturaAsync` (FR-012, D15).
         *     Sin token → 401; con token de otro rol → 403 `sin_permiso`; cuota superada → 429 (convenciones-api, T-SEG19, T-SEG20).
         *     El correo del actor no se devuelve: el panel lo resuelve con `GET /api/v1/usuarios?ids=` (feature 003).
         *
         */
        get: operations["consultarAuditoria"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
}
export type webhooks = Record<string, never>;
export interface components {
    schemas: {
        /** @enum {string} */
        CategoriaAuditoria: "Usuarios" | "Sesion" | "CuentaDelTitular" | "Moderacion" | "Exportacion" | "AccesoADatos";
        /** @enum {string} */
        AccionAuditada: "UsuarioCreado" | "RolCambiado" | "CuentaDesactivada" | "CuentaActivada" | "CuentaEliminada" | "InicioSesionFallido" | "CuentaBloqueada" | "InicioSesionAdministrador" | "RecuperacionSolicitada" | "ContrasenaCambiada" | "RevisionPrivilegios" | "AnomaliaModerada" | "DatasetExportado" | "AccesoAdministrativoDatos";
        RegistroAuditoria: {
            /** Format: int64 */
            id: number;
            /** Format: date-time */
            ocurridoUtc: string;
            /**
             * Format: uuid
             * @description null si el actor es el sistema
             */
            actorId?: string | null;
            /** @enum {string} */
            actorRol: "ConductorRegistrado" | "Administrador" | "Sistema";
            categoria: components["schemas"]["CategoriaAuditoria"];
            accion: components["schemas"]["AccionAuditada"];
            objetivo?: string | null;
            /** @description Datos no personales de la acción */
            contexto: {
                [key: string]: unknown;
            };
        };
        PaginaAuditoria: {
            elementos: components["schemas"]["RegistroAuditoria"][];
            pagina: number;
            tamano: number;
            /** Format: int64 */
            total: number;
        };
        /** @description Error según RFC 9457 con un código estable en español. */
        Problema: {
            /**
             * Format: uri
             * @example https://api.killauru.pe/errores/token_invalido
             */
            type: string;
            /** @example El token de acceso no es válido o venció */
            title: string;
            /** @example 401 */
            status: number;
            detail?: string;
            /** @example token_invalido */
            codigo: string;
            /** @example 0HN7B1Q3C2K8L */
            correlacionId: string;
            /** @description Solo en 400 de validación. Campo → lista de mensajes. */
            errores?: {
                [key: string]: string[];
            };
        };
    };
    responses: {
        /** @description Datos de entrada inválidos (`codigo = datos_invalidos`). */
        Validacion: {
            headers: {
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["Problema"];
            };
        };
        /** @description Sin token en un endpoint que exige sesión, o token inválido, vencido o con rol desconocido (`codigo = token_invalido`). Regla 401/403 en `convenciones-api.md`. */
        NoAutenticado: {
            headers: {
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["Problema"];
            };
        };
        /** @description Con token y rol insuficiente, o parámetro reservado usado por otro rol con o sin token (`codigo = sin_permiso`). */
        Prohibido: {
            headers: {
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["Problema"];
            };
        };
        /** @description Límite de tasa superado (`codigo = limite_excedido`): cuota por rol, techo por IP de los anónimos o tope de registros (RNF-11; D2, D8). */
        LimiteExcedido: {
            headers: {
                "Retry-After": components["headers"]["RetryAfter"];
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["Problema"];
            };
        };
    };
    parameters: never;
    requestBodies: never;
    headers: {
        /** @description Segundos que el cliente debe esperar antes de reintentar. */
        RetryAfter: number;
    };
    pathItems: never;
}
export type $defs = Record<string, never>;
export interface operations {
    consultarAuditoria: {
        parameters: {
            query?: {
                /** @description Inicio del rango (UTC */
                desde?: string;
                /** @description Fin del rango (UTC */
                hasta?: string;
                categoria?: components["schemas"]["CategoriaAuditoria"];
                accion?: components["schemas"]["AccionAuditada"];
                actorId?: string;
                pagina?: number;
                tamano?: number;
            };
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Página de registros */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["PaginaAuditoria"];
                };
            };
            400: components["responses"]["Validacion"];
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
}
