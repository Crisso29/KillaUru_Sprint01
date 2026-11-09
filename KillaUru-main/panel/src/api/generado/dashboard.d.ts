export interface paths {
    "/api/v1/dashboard/indicadores": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Resumen de indicadores de negocio
         * @description Rol: administrador (sin token → 401; token de otro rol → 403, T-SEG20). Valores del último día cerrado (hora de Lima) comparados con los 7 días previos,
         *     serie de 7 días por indicador y eventos por día del periodo elegido junto al periodo anterior.
         *     Un día sin calcular llega como `null`, nunca como 0. `eventosDia` son los eventos válidos (reportes procesados) del último
         *     día cerrado: el panel lo rotula «Eventos válidos (ayer)» (D13, D14). Reaparición = vuelven a nacer después de expirar.
         *
         */
        get: operations["consultarIndicadores"];
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
        Indicador: {
            valor: number | null;
            /** @description Promedio de los 7 días previos (conteos) o tasa de los 7 días previos (tasas) */
            comparacion: number | null;
            /** @description % relativo (eventos), diferencia absoluta (cuentas, dispositivos) o puntos (porcentajes) */
            variacion: number | null;
            /** @enum {string} */
            unidad: "eventos" | "cuentas" | "dispositivos" | "%";
            serie: (number | null)[];
        };
        ResumenDashboard: {
            /** Format: date */
            ultimoDiaCerrado: string | null;
            /** Format: date-time */
            calculadoUtc: string | null;
            /** Format: uri */
            urlPanelesTecnicos: string;
            indicadores: {
                eventosDia: components["schemas"]["Indicador"];
                conductoresActivos: components["schemas"]["Indicador"];
                dispositivosDia: components["schemas"]["Indicador"];
                coberturaPct: components["schemas"]["Indicador"];
                confirmacionPct: components["schemas"]["Indicador"];
                reaparicionPct: components["schemas"]["Indicador"];
            };
            eventosPorDia: {
                /** Format: date */
                dia: string;
                eventos: number | null;
                eventosPeriodoAnterior: number | null;
            }[];
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
    consultarIndicadores: {
        parameters: {
            query?: {
                /** @description Largo del periodo del gráfico de eventos válidos. */
                dias?: 7 | 14 | 30;
            };
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Resumen */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["ResumenDashboard"];
                };
            };
            /** @description `dias` fuera de 7, 14 o 30 (`codigo = datos_invalidos`) */
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
