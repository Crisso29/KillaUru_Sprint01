export interface paths {
    "/api/v1/anomalies/{id}": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Detalle de una anomalía
         * @description Rol: administrador. Datos del panel lateral del prototipo: clasificación, días distintos, reportes por día (15 d) e historial de estados.
         */
        get: operations["obtenerDetalleAnomalia"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/anomalies/{id}/moderacion": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Marcar reparada, descartar, reabrir o mantener descartada
         * @description Rol: administrador. Solo desde los estados de la tabla de RN-07 (RF-24).
         *     Se audita como `AnomaliaModerada` en la misma transacción (`RegistrarAsync`) y se invalida el mapa.
         *     La anomalía se lee bloqueada (`FOR UPDATE`) y su estado se revalida antes de cambiarla (T-BD02).
         *
         */
        post: operations["moderarAnomalia"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/anomalies/transiciones": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Últimos cambios de estado
         * @description Rol: administrador. Tarjeta «Últimos cambios de estado» del Resumen del prototipo (sistema y administradores).
         */
        get: operations["listarTransicionesRecientes"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/moderacion/por-revisar": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Anomalías descartadas que siguen recibiendo reportes
         * @description Rol: administrador. ≥ 3 vehículos en 7 días (RN-07).
         */
        get: operations["listarPorRevisar"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/moderacion/pendientes": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Anomalías que esperan confirmación
         * @description Rol: administrador. Solo lectura; se publican solas con el 2.º vehículo o expiran en 72 h (RN-06, RN-07). Orden: vencen primero.
         */
        get: operations["listarPendientes"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/exportaciones": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Exportar anomalías en CSV o GeoJSON
         * @description Rol: administrador. Nunca incluye seudónimos (RF-20). Primero cuenta las filas con los mismos filtros, luego audita
         *     `DatasetExportado` con formato, filtros y `filas` (lectura sensible, acción de riesgo, RNF-29) y solo después escribe (D15).
         *     Columnas del CSV (en este orden) y `properties` de cada *feature* GeoJSON: ver `FilaExportacion` (T-PO11).
         *     Las exportaciones recientes se consultan en `GET /api/v1/auditoria?accion=DatasetExportado` (feature 002).
         *
         */
        post: operations["exportarDataset"];
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
        /** @enum {string} */
        Persistencia: "Episodica" | "Persistente";
        /**
         * @description Acción de Application (`AccionModeracion`); se mapea a `AccionAdmin` del dominio (D4).
         * @enum {string}
         */
        AccionModeracion: "MarcarReparada" | "Descartar" | "Reabrir" | "MantenerDescartada";
        DetalleAnomalia: {
            /** Format: uuid */
            id: string;
            tipo: components["schemas"]["TipoAnomalia"];
            severidadMostrada: number;
            estado: components["schemas"]["EstadoAnomalia"];
            persistencia: components["schemas"]["Persistencia"];
            latitud: number;
            longitud: number;
            /** @description Km sobre la vía desde Pisco (km 0); anomalia.km_via, fijo al nacer (D13) */
            kmVia: number;
            diasDistintos15d: number;
            reportes: number;
            /** Format: date-time */
            ultimaDeteccionUtc: string;
            /**
             * Format: date-time
             * @description null en Expirada, Reparada y Descartada (T-PO08)
             */
            venceUtc: string | null;
            visibleConductores: boolean;
            porRevisar: boolean;
            vehiculosRevision: number;
            reportesPorDia: {
                /** Format: date */
                dia: string;
                cantidad: number;
            }[];
            historial: {
                desde?: components["schemas"]["EstadoAnomalia"] | null;
                hacia: components["schemas"]["EstadoAnomalia"];
                /** Format: date-time */
                ocurridaUtc: string;
                /**
                 * Format: uuid
                 * @description null = sistema
                 */
                actorId?: string | null;
            }[];
        };
        ElementoPorRevisar: {
            /** Format: uuid */
            id: string;
            tipo: components["schemas"]["TipoAnomalia"];
            kmVia: number;
            /** @description T-PO19 */
            severidadMostrada: number;
            vehiculos7d: number;
            reportes: number;
            /** Format: date-time */
            ultimaDeteccionUtc: string;
        };
        ElementoPendiente: {
            /** Format: uuid */
            id: string;
            tipo: components["schemas"]["TipoAnomalia"];
            kmVia: number;
            severidadMostrada: number;
            reportes: number;
            /** Format: date-time */
            primerReporteUtc: string;
            /** Format: date-time */
            venceUtc: string;
        };
        TransicionReciente: {
            /** Format: uuid */
            anomaliaId: string;
            tipo: components["schemas"]["TipoAnomalia"];
            kmVia: number;
            desde?: components["schemas"]["EstadoAnomalia"] | null;
            hacia: components["schemas"]["EstadoAnomalia"];
            /** Format: date-time */
            ocurridaUtc: string;
            /** Format: uuid */
            actorId?: string | null;
        };
        SolicitudExportacion: {
            /** @enum {string} */
            formato: "CSV" | "GeoJSON";
            /**
             * Format: date-time
             * @description Sobre la última detección
             */
            desdeUtc?: string | null;
            /** Format: date-time */
            hastaUtc?: string | null;
            tipo?: components["schemas"]["TipoAnomalia"] | null;
            /** @description Vacío = todos */
            estados?: components["schemas"]["EstadoAnomalia"][];
        };
        /** @description Una anomalía exportada; sin seudónimos (RF-20). Mismas columnas que el prototipo (T-PO11). */
        FilaExportacion: {
            /** Format: uuid */
            id: string;
            tipo: components["schemas"]["TipoAnomalia"];
            /** @description Severidad mostrada */
            severidad: number;
            estado: components["schemas"]["EstadoAnomalia"];
            persistencia: components["schemas"]["Persistencia"];
            /** @description anomalia.km_via */
            km: number;
            latitud: number;
            longitud: number;
            reportes: number;
            /** Format: date-time */
            primer_reporte: string;
            /** Format: date-time */
            ultimo_reporte: string;
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
        /** @description El recurso no existe (`codigo = no_encontrado`). */
        NoEncontrado: {
            headers: {
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["Problema"];
            };
        };
        /** @description Datos de entrada inválidos (`codigo = datos_invalidos`). */
        Validacion: {
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
        IdAnomalia: string;
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
    obtenerDetalleAnomalia: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["parameters"]["IdAnomalia"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Detalle */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["DetalleAnomalia"];
                };
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            404: components["responses"]["NoEncontrado"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    moderarAnomalia: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["parameters"]["IdAnomalia"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                /** @example {
                 *       "accion": "MarcarReparada"
                 *     } */
                "application/json": {
                    accion: components["schemas"]["AccionModeracion"];
                };
            };
        };
        responses: {
            /** @description Moderación aplicada; devuelve el detalle actualizado */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["DetalleAnomalia"];
                };
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            404: components["responses"]["NoEncontrado"];
            /** @description La acción no está permitida desde el estado actual (`codigo = transicion_no_permitida`) */
            409: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Problema"];
                };
            };
            429: components["responses"]["LimiteExcedido"];
        };
    };
    listarTransicionesRecientes: {
        parameters: {
            query?: {
                limite?: number;
            };
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Transiciones, la más reciente primero */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["TransicionReciente"][];
                };
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    listarPorRevisar: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Lista «Por revisar» */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["ElementoPorRevisar"][];
                };
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    listarPendientes: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Lista «Esperan confirmación» */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["ElementoPendiente"][];
                };
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    exportarDataset: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                /** @example {
                 *       "formato": "CSV",
                 *       "desdeUtc": "2026-10-13T00:00:00Z",
                 *       "hastaUtc": "2026-10-20T23:59:59Z",
                 *       "tipo": null,
                 *       "estados": [
                 *         "Activa"
                 *       ]
                 *     } */
                "application/json": components["schemas"]["SolicitudExportacion"];
            };
        };
        responses: {
            /** @description Archivo (con `Content-Disposition: attachment`) */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "text/csv": string;
                    "application/geo+json": {
                        /** @constant */
                        type: "FeatureCollection";
                        features: {
                            /** @constant */
                            type: "Feature";
                            /** @description Point [longitud, latitud] */
                            geometry: Record<string, never>;
                            properties: components["schemas"]["FilaExportacion"];
                        }[];
                    };
                };
            };
            400: components["responses"]["Validacion"];
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
}
