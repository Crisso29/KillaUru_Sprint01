export interface paths {
    "/api/v1/legal/versiones": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Versiones vigentes de la política de privacidad y de los términos de uso
         * @description Rol: público. La app las muestra antes de detectar (también al anónimo) y vuelve a pedirlas si cambian (RF-29, RF-37).
         */
        get: operations["obtenerVersionesLegales"];
        put?: never;
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/auth/registro": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Crear cuenta de conductor
         * @description Rol: público. Exige aceptar las versiones vigentes de la política y los términos (HU-22, HU-27). Envía el correo de verificación (RF-22) y devuelve una sesión (RF-21) cuyo token lleva `verificado = false` hasta verificar el correo (D8). Máximo `LimiteTasa:RegistrosPorHoraPorIp` (5) registros por hora por IP: el 6.º → 429 (D8).
         */
        post: operations["registrarConductor"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/auth/inicio-sesion": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Iniciar sesión (app o panel)
         * @description Rol: público.
         *     - `canal = app`: tokens en el cuerpo (conductor; RF-21).
         *     - `canal = panel`: tokens en cookies `ku_acceso` y `ku_renovacion` (solo administradores; RF-16); exige `turnstileToken`.
         *     Correo inexistente, contraseña incorrecta y cuenta desactivada responden igual (RN-19).
         *
         */
        post: operations["iniciarSesion"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/auth/renovar": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Renovar el token de acceso
         * @description Rol: público (se autentica con el token de actualización).
         *     App: token en el cuerpo. Panel: cookie `ku_renovacion` (sin cuerpo).
         *     Reuso del mismo token dentro de 60 s → mismo par; después → se revoca la sesión (ADR-009).
         *     `cuenta_desactivada` y `sesion_invalida` indican a la app que borre su permiso local (RN-21 c).
         *
         */
        post: operations["renovarSesion"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/auth/cerrar-sesion": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Cerrar sesión
         * @description Rol: conductor o administrador. Revoca la familia de la sesión (RF-38). En el panel borra las cookies. El resto de RN-21 d lo hace la app.
         */
        post: operations["cerrarSesion"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/auth/verificar-correo": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Verificar el correo con el enlace recibido
         * @description Rol: público. Token del fragmento del enlace, un solo uso, 24 h (HU-18b).
         */
        post: operations["verificarCorreo"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/auth/recuperar": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Pedir un enlace para recuperar la contraseña
         * @description Rol: público. Responde 202 SIEMPRE, exista o no la cuenta (HU-25). Solo envía si el correo existe, está verificado y no se superó el límite por correo e IP (RF-32).
         */
        post: operations["solicitarRecuperacion"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/auth/fijar-contrasena": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Fijar una contraseña con un enlace de recuperación o de invitación
         * @description Rol: público. Cierra todas las sesiones, desbloquea la cuenta y se audita (HU-25). Con una invitación, además verifica el correo.
         */
        post: operations["fijarContrasenaConEnlace"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/me": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Ver todos mis datos personales
         * @description Rol: conductor o administrador (HU-24, RF-31).
         */
        get: operations["obtenerMisDatos"];
        put?: never;
        post?: never;
        /**
         * Eliminar mi cuenta
         * @description Rol: conductor. Confirma con la contraseña. Borra correo, credenciales, consentimientos y tokens; revoca sesiones; audita sin correo (HU-23, RF-30).
         */
        delete: operations["eliminarMiCuenta"];
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/me/aceptaciones": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Aceptar una versión nueva de la política o de los términos
         * @description Rol: conductor. Solo acepta la versión vigente (RF-37).
         */
        post: operations["aceptarVersion"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/me/reenviar-verificacion": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Reenviar el correo de verificación
         * @description Rol: conductor o administrador. Invalida el enlace anterior. Derivado del prototipo («Revisa tu correo»).
         */
        post: operations["reenviarMiVerificacion"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/usuarios": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Listar usuarios
         * @description Rol: administrador. Búsqueda por correo y filtros por rol y estado (prototipo «Usuarios»).
         *     Con `ids` devuelve solo esas cuentas (para mostrar actores de la auditoría; máx. 200).
         *     Se audita como `AccesoAdministrativoDatos` con `RegistrarLecturaAsync`, también con `ids` (RF-15, D15).
         *
         */
        get: operations["listarUsuarios"];
        put?: never;
        /**
         * Crear (invitar) un usuario
         * @description Rol: administrador. Crea la cuenta sin contraseña y envía una invitación de 24 h (RF-19; invitación derivada del prototipo). Audita `UsuarioCreado`.
         */
        post: operations["crearUsuario"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/usuarios/{id}/rol": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        /**
         * Cambiar el rol de un usuario
         * @description Rol: administrador. Revoca las sesiones del usuario. Audita `RolCambiado` (acción de riesgo, RNF-29).
         */
        put: operations["cambiarRol"];
        post?: never;
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/usuarios/{id}/desactivar": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Desactivar una cuenta
         * @description Rol: administrador. Revoca sesiones; pierde el acceso en ≤ 5 min (HU-14, RNF-26). Audita `CuentaDesactivada`.
         */
        post: operations["desactivarUsuario"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/usuarios/{id}/activar": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Activar una cuenta desactivada
         * @description Rol: administrador. Derivado del prototipo («Activar cuenta»). Audita `CuentaActivada`.
         */
        post: operations["activarUsuario"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/usuarios/{id}/reenviar-correo": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        get?: never;
        put?: never;
        /**
         * Reenviar la verificación o la invitación
         * @description Rol: administrador. Si la invitación está pendiente, reenvía la invitación; si no, la verificación. Derivado del prototipo («Reenviar verificación»).
         */
        post: operations["reenviarCorreoUsuario"];
        delete?: never;
        options?: never;
        head?: never;
        patch?: never;
        trace?: never;
    };
    "/api/v1/usuarios/revision-privilegios": {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        /**
         * Estado de la revisión mensual de privilegios
         * @description Rol: administrador. Última revisión, fecha de vencimiento y administradores actuales (RNF-30). Se audita como `AccesoAdministrativoDatos` con `RegistrarLecturaAsync` (D15).
         */
        get: operations["consultarRevisionPrivilegios"];
        put?: never;
        /**
         * Registrar la revisión mensual
         * @description Rol: administrador. Confirma o retira (desactiva) a cada administrador. Audita `RevisionPrivilegios` (RF-19, RNF-30).
         */
        post: operations["registrarRevisionPrivilegios"];
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
        ProblemaBloqueo: components["schemas"]["Problema"] & {
            /** Format: date-time */
            bloqueadoHastaUtc: string;
        };
        /** @enum {string} */
        Rol: "ConductorRegistrado" | "Administrador";
        /** @enum {string} */
        EstadoUsuario: "Activa" | "Desactivada" | "InvitacionPendiente";
        /** @enum {string} */
        DocumentoLegal: "PoliticaPrivacidad" | "TerminosUso";
        VersionDocumento: {
            version: string;
            /** Format: uri */
            url: string;
        };
        VersionesLegales: {
            politicaPrivacidad: components["schemas"]["VersionDocumento"];
            terminosUso: components["schemas"]["VersionDocumento"];
        };
        RegistroSolicitud: {
            /** Format: email */
            correo: string;
            /** @description Mínimo 10 (D13) */
            contrasena: string;
            versionPoliticaAceptada: string;
            versionTerminosAceptada: string;
        };
        InicioSesionSolicitud: {
            /** Format: email */
            correo: string;
            contrasena: string;
            /** @enum {string} */
            canal: "app" | "panel";
            /** @description Obligatorio con canal = panel (R-008) */
            turnstileToken?: string;
        };
        RenovarSolicitud: {
            /** @description Solo la app; el panel usa la cookie ku_renovacion */
            tokenRenovacion?: string;
        };
        SesionApp: {
            /** @description JWT de 5 min con sub, rol y verificado (D8) */
            tokenAcceso: string;
            /** Format: date-time */
            expiraAccesoUtc: string;
            tokenRenovacion: string;
            /**
             * Format: date-time
             * @description La app usa esta fecha para el permiso local (RN-21 a)
             */
            sesionVenceUtc: string;
            rol: components["schemas"]["Rol"];
        };
        SesionPanel: {
            /** Format: date-time */
            expiraAccesoUtc: string;
            /**
             * Format: date-time
             * @description Prototipo «Sesión hasta 17:12»
             */
            sesionVenceUtc: string;
            rol: components["schemas"]["Rol"];
        };
        TokenEnlace: {
            token: string;
        };
        MisDatos: {
            /** Format: email */
            correo: string;
            rol: components["schemas"]["Rol"];
            /** Format: date-time */
            fechaAltaUtc: string;
            correoVerificado: boolean;
            aceptaciones: {
                documento: components["schemas"]["DocumentoLegal"];
                version: string;
                /** Format: date-time */
                aceptadoUtc: string;
            }[];
        };
        Usuario: {
            /** Format: uuid */
            id: string;
            /** Format: email */
            correo: string;
            rol: components["schemas"]["Rol"];
            estado: components["schemas"]["EstadoUsuario"];
            correoVerificado: boolean;
            /** Format: date-time */
            fechaAltaUtc: string;
            /** Format: date-time */
            ultimaSesionUtc?: string | null;
        };
        PaginaUsuarios: {
            elementos: components["schemas"]["Usuario"][];
            pagina: number;
            tamano: number;
            total: number;
        };
        EstadoRevision: {
            /** Format: date-time */
            ultimaRevisionUtc?: string | null;
            /** Format: date-time */
            venceUtc: string;
            administradores: components["schemas"]["Usuario"][];
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
        /** @description Falta aceptar o la versión no es la vigente (`aceptacion_requerida`), o datos inválidos (`datos_invalidos`) */
        AceptacionOValidacion: {
            headers: {
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["Problema"];
            };
        };
        /** @description `accion_sobre_si_mismo` o `ultimo_administrador` */
        ConflictoAdministrador: {
            headers: {
                [name: string]: unknown;
            };
            content: {
                "application/problem+json": components["schemas"]["Problema"];
            };
        };
    };
    parameters: {
        IdUsuario: string;
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
    obtenerVersionesLegales: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Versiones vigentes */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["VersionesLegales"];
                };
            };
            429: components["responses"]["LimiteExcedido"];
        };
    };
    registrarConductor: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                /** @example {
                 *       "correo": "conductor@ejemplo.pe",
                 *       "contrasena": "una-clave-larga",
                 *       "versionPoliticaAceptada": "1.2",
                 *       "versionTerminosAceptada": "1.2"
                 *     } */
                "application/json": components["schemas"]["RegistroSolicitud"];
            };
        };
        responses: {
            /** @description Cuenta creada con sesión iniciada */
            201: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["SesionApp"];
                };
            };
            400: components["responses"]["AceptacionOValidacion"];
            /** @description El correo ya tiene cuenta (`codigo = correo_en_uso`). Riesgo de enumeración aceptado, mitigado por el límite por IP (D8) */
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
    iniciarSesion: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["InicioSesionSolicitud"];
            };
        };
        responses: {
            /** @description Sesión iniciada. En `canal = panel` además vienen las cookies `Set-Cookie`. */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["SesionApp"] | components["schemas"]["SesionPanel"];
                };
            };
            /** @description Credenciales inválidas (`codigo = credenciales_invalidas`), igual exista o no la cuenta */
            401: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Problema"];
                };
            };
            /** @description Un conductor intentó entrar al panel (`codigo = solo_administradores`) o falló la verificación anti-bot (`codigo = antibot_fallido`) */
            403: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["Problema"];
                };
            };
            /** @description Solo `canal = panel`. Cuenta bloqueada (`codigo = cuenta_bloqueada`) con la hora de desbloqueo (RN-19; prototipo) */
            423: {
                headers: {
                    "Retry-After": components["headers"]["RetryAfter"];
                    [name: string]: unknown;
                };
                content: {
                    "application/problem+json": components["schemas"]["ProblemaBloqueo"];
                };
            };
            429: components["responses"]["LimiteExcedido"];
        };
    };
    renovarSesion: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: {
            content: {
                "application/json": components["schemas"]["RenovarSolicitud"];
            };
        };
        responses: {
            /** @description Par nuevo (o el mismo, dentro de la gracia) */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["SesionApp"] | components["schemas"]["SesionPanel"];
                };
            };
            /** @description `codigo = sesion_invalida` (no existe, vencida, revocada, reutilizada o cuenta eliminada) o `cuenta_desactivada` */
            401: {
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
    cerrarSesion: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: {
            content: {
                "application/json": components["schemas"]["RenovarSolicitud"];
            };
        };
        responses: {
            /** @description Sesión cerrada */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            401: components["responses"]["NoAutenticado"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    verificarCorreo: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": components["schemas"]["TokenEnlace"];
            };
        };
        responses: {
            /** @description Correo verificado */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            /** @description Enlace inválido, vencido o ya usado (`codigo = enlace_invalido`) */
            400: {
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
    solicitarRecuperacion: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": {
                    /** Format: email */
                    correo: string;
                };
            };
        };
        responses: {
            /** @description Si el correo está registrado y verificado */
            202: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            400: components["responses"]["Validacion"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    fijarContrasenaConEnlace: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": {
                    token: string;
                    nuevaContrasena: string;
                };
            };
        };
        responses: {
            /** @description Contraseña fijada */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            /** @description Enlace inválido, vencido o usado (`enlace_invalido`) o contraseña fuera de la política (`datos_invalidos`) */
            400: {
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
    obtenerMisDatos: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Datos personales guardados */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["MisDatos"];
                };
            };
            401: components["responses"]["NoAutenticado"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    eliminarMiCuenta: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": {
                    contrasena: string;
                };
            };
        };
        responses: {
            /** @description Cuenta eliminada */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            401: components["responses"]["NoAutenticado"];
            /** @description Contraseña incorrecta (`codigo = confirmacion_invalida`) o rol no permitido */
            403: {
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
    aceptarVersion: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": {
                    documento: components["schemas"]["DocumentoLegal"];
                    /** @example 1.3 */
                    version: string;
                };
            };
        };
        responses: {
            /** @description Aceptación guardada */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            400: components["responses"]["AceptacionOValidacion"];
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    reenviarMiVerificacion: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Enviado (si el correo aún no estaba verificado) */
            202: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            401: components["responses"]["NoAutenticado"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    listarUsuarios: {
        parameters: {
            query?: {
                busqueda?: string;
                rol?: components["schemas"]["Rol"];
                estado?: components["schemas"]["EstadoUsuario"];
                ids?: string[];
                pagina?: number;
                tamano?: number;
            };
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Página de usuarios */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["PaginaUsuarios"];
                };
            };
            400: components["responses"]["Validacion"];
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    crearUsuario: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": {
                    /** Format: email */
                    correo: string;
                    rol: components["schemas"]["Rol"];
                };
            };
        };
        responses: {
            /** @description Usuario creado e invitación enviada */
            201: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["Usuario"];
                };
            };
            400: components["responses"]["Validacion"];
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            /** @description `correo_en_uso` */
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
    cambiarRol: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["parameters"]["IdUsuario"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": {
                    rol: components["schemas"]["Rol"];
                };
            };
        };
        responses: {
            /** @description Rol cambiado */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            404: components["responses"]["NoEncontrado"];
            409: components["responses"]["ConflictoAdministrador"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    desactivarUsuario: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["parameters"]["IdUsuario"];
            };
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": {
                    motivo: string;
                };
            };
        };
        responses: {
            /** @description Cuenta desactivada */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            404: components["responses"]["NoEncontrado"];
            409: components["responses"]["ConflictoAdministrador"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    activarUsuario: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["parameters"]["IdUsuario"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Cuenta activada */
            204: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            404: components["responses"]["NoEncontrado"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    reenviarCorreoUsuario: {
        parameters: {
            query?: never;
            header?: never;
            path: {
                id: components["parameters"]["IdUsuario"];
            };
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Correo enviado */
            202: {
                headers: {
                    [name: string]: unknown;
                };
                content?: never;
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            404: components["responses"]["NoEncontrado"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    consultarRevisionPrivilegios: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody?: never;
        responses: {
            /** @description Estado de la revisión */
            200: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["EstadoRevision"];
                };
            };
            401: components["responses"]["NoAutenticado"];
            403: components["responses"]["Prohibido"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
    registrarRevisionPrivilegios: {
        parameters: {
            query?: never;
            header?: never;
            path?: never;
            cookie?: never;
        };
        requestBody: {
            content: {
                "application/json": {
                    confirmados: string[];
                    retirados: string[];
                };
            };
        };
        responses: {
            /** @description Revisión registrada */
            201: {
                headers: {
                    [name: string]: unknown;
                };
                content: {
                    "application/json": components["schemas"]["EstadoRevision"];
                };
            };
            /** @description La lista no coincide con los administradores actuales (`codigo = revision_incompleta`) */
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
            409: components["responses"]["ConflictoAdministrador"];
            429: components["responses"]["LimiteExcedido"];
        };
    };
}
