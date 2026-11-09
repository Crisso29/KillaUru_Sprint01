export interface paths {
    "/api/v1/anomalies": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Anomalías del mapa según el rol
         * @description Rol: público (anónimo, conductor registrado o administrador). Límite: política `consultas` (RNF-11).
         *     Anónimo → solo `Activa` dentro del tramo demo Ayacucho–Vinchos, aunque pida un `bbox` mayor.
         *     Conductor → solo `Activa` en el corredor. Administrador → los estados pedidos; sin `estado`, los 5 estados (RN-08, D6).
         *     `estado` es un parámetro reservado: otro rol, con o sin token → 403 `sin_permiso`; token inválido o vencido → 401 (T-SEG20).
         *     Sin filtros, anónimos y conductores reciben `ETag` y pueden revalidar con `If-None-Match` (304).
         *     Con `updatedSince`, las anomalías que dejaron de ser visibles llegan como `{id, eliminada: true}`; al anónimo solo las de dentro del tramo demo.
         *     Solo las respuestas sin token son `public, max-age=60`; con token, `private, no-store` (ADR-006).
         *
         */
        get: operations["consultarMapa"];
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
        /** @enum {string} */
        TipoAnomalia: "Bache" | "Hundimiento";
        /** @enum {string} */
        EstadoAnomalia: "Activa" | "PendienteConfirmacion" | "Expirada" | "Reparada" | "Descartada";
        ColeccionAnomalias: {
            /** @constant */
            type: "FeatureCollection";
            /**
             * Format: date-time
             * @description Hora del servidor; próximo `updatedSince`
             */
            generadoUtc: string;
            features: (components["schemas"]["AnomaliaFeature"] | components["schemas"]["BajaFeature"])[];
        };
        AnomaliaFeature: {
            /** @constant */
            type: "Feature";
            /** Format: uuid */
            id: string;
            geometry: {
                /** @constant */
                type: "Point";
                /** @description [longitud, latitud] */
                coordinates: number[];
            };
            properties: {
                /** Format: uuid */
                id: string;
                /**
                 * @description Km sobre la vía, calculado al nacer (D13)
                 * @example 287.4
                 */
                kmVia: number;
                tipo: components["schemas"]["TipoAnomalia"];
                severidad: number;
                /** @enum {string} */
                persistencia: "Episodica" | "Persistente";
                /** Format: date-time */
                ultimaDeteccionUtc: string;
                /**
                 * Format: date-time
                 * @description `null` en los estados finales (`Expirada`, `Reparada`, `Descartada`)
                 */
                venceUtc: string | null;
                /** @description Solo para el administrador */
                estado?: components["schemas"]["EstadoAnomalia"];
            };
        };
        /** @description Anomalía que dejó de ser visible para el rol desde `updatedSince` (RF-14). No revela su estado. */
        BajaFeature: {
            /** @constant */
            type: "Feature";
            /** Format: uuid */
            id: string;
            geometry: null;
            properties: {
                /** Format: uuid */
                id: string;
                /** @constant */
                eliminada: true;
            };
        };
    };
    responses: {
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
    parameters: {
        /** @description Seudónimo rotativo de la instalación; solo en peticiones sin token, para el límite de tasa (RF-27, RNF-11). */
        Seudonimo: string;
    };
    requestBodies: never;
    headers: {
        /** @description Segundos que el cliente debe esperar antes de reintentar. */
        RetryAfter: number;
    };
    pathItems: never;
}
export type $defs = Record<string, never>;
export interface operations {
    consultarMapa: {
        parameters: {
            query?: {
                /** @description `minLon,minLat,maxLon,maxLat`. Sin `bbox` = todo el alcance del rol. */
                bbox?: string;
                tipo?: components["schemas"]["TipoAnomalia"];
                /** @description Lista separada por comas (severidad mostrada). */
                severidad?: string;
                /** @description Sobre la última detección */
                desde?: string;
                hasta?: string;
                /** @description Solo administrador (otro rol, con o sin token → 403). Lista separada por comas. Sin `estado`, el administrador recibe los 5 estados. */
                estado?: string;
                /** @description Cambios desde esa hora (usar el `generadoUtc` de la respuesta anterior). No se combina con filtros. */
                updatedSince?: string;
            };
            header?: {
                /** @description Seudónimo rotativo de la instalación; solo en peticiones sin token, para el límite de tasa (RF-27, RNF-11). */
                "X-KillaUru-Seudonimo"?: components["parameters"]["Seudonimo"];
                "If-None-Match"?: string;
            };
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Anomalías visibles para el rol */
            200: {
                headers: {
                    /** @description Solo sin filtros ni `updatedSince` */
                    ETag?: string;
                    /** @description `public, max-age=60` sin token; `private, no-store` con token */
                    "Cache-Control"?: string;
                    [name: string]: unknown;
                };
                content: {
                    "application/geo+json": components["schemas"]["ColeccionAnomalias"];
                };
            };
            /** @description Ninguna celda pedida cambió desde el `ETag` enviado */
            304: {
                headers: {
                    ETag?: string;
                    [name: string]: unknown;
                };
                content?: never;
            };
            /** @description `bbox`, fechas o filtros inválidos (`datos_invalidos`), o `updatedSince` con filtros (`parametros_incompatibles`) */
            400: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Problema"];
                };
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
}
