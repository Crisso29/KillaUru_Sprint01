/*
 * KillaUru · Modelo C4 en Structurizr DSL
 * Versión 3.0 · Revisión 2026-10-14 · Alineado con docs/01, docs/02 y docs/03
 * Vistas: C1_Contexto · C2_Contenedores · C3_API · C3_App · Despliegue
 * Exportar los SVG con Structurizr Lite a img/fuentes-svg/ (mismo nombre que la vista).
 */
workspace "KillaUru" "Detección colaborativa de baches y hundimientos en la carretera PE-28A" {

    !identifiers hierarchical

    model {
        conductorNR = person "Conductor No Registrado" "Ve el tramo demo y aporta datos sin cuenta." "Usuario Externo"
        conductorR  = person "Conductor Registrado" "Ve los 333 km, recibe alertas y gestiona su cuenta." "Usuario Externo"
        admin       = person "Administrador" "Opera la plataforma." "Personal Interno"
        empresa     = person "Empresa suscriptora" "Analítica de tramos (v1.5+)." "Futuro"
        autoridad   = person "Autoridad Vial" "Acceso bajo convenio (v2.0+)." "Futuro"

        cloudflare = softwareSystem "Cloudflare" "DNS, TLS, proxy, caché de borde, Tunnel y Pages." "Infraestructura Externa"
        r2         = softwareSystem "Cloudflare R2" "cdn.killauru.pe: PMTiles, polilínea, modelos y backups cifrados." "Infraestructura Externa"
        uptime     = softwareSystem "UptimeRobot" "Monitoreo de disponibilidad y página de estado pública." "Infraestructura Externa"
        correo     = softwareSystem "Proveedor de correo" "Correo transaccional." "Infraestructura Externa"
        grafana    = softwareSystem "Grafana Cloud" "Métricas, trazas, logs y alertas." "Infraestructura Externa"

        killauru = softwareSystem "KillaUru" "Detecta, valida y muestra las anomalías viales de la PE-28A." {

            app = container "App KillaUru" "Detecta, guarda, sincroniza y alerta sin señal." "Kotlin, Compose, LiteRT, MapLibre, WorkManager" "Mobile" {
                sesion        = component "Sesión y primer uso" "Términos, privacidad, categoría, permisos, login y permiso local (RN-21)." "Kotlin"
                sensores      = component "Sensores" "50 Hz en lote; indicador de estado (RF-36)." "Servicio en primer plano"
                detector      = component "Detector" "Prefiltro de energía y clasificación local." "LiteRT"
                seudonimo     = component "Seudónimo" "UUID rotativo de instalación." "Kotlin"
                sincronizador = component "Sincronizador" "Lotes, estados locales y backoff." "WorkManager"
                mapa          = component "Mapa" "Mapa base offline y anomalías locales." "MapLibre Native"
                corredor      = component "Corredor" "Polilínea con km acumulado; proyección del GPS." "Kotlin"
                proximidad    = component "Proximidad y Alertas" "Rango por km, una alerta por pasada (RN-16, RN-20)." "Kotlin"
                historial     = component "Historial" "Recorridos de 6 meses, solo locales." "Room"
                actualizador  = component "Actualizador" "Modelo y mapa base con manifiesto y SHA-256." "WorkManager"
            }
            baseLocal = container "Base local" "Eventos, anomalías con su km y recorridos." "Room / SQLite" "Database"
            panel     = container "Panel Admin" "Interfaz del administrador, sin reglas de negocio." "React, MapLibre GL JS" "Web Browser"

            api = container "API KillaUru" "Monolito modular: recibe lotes, aplica las reglas y sirve el mapa." "ASP.NET Core 10" {
                host       = component "API REST (host)" "Sesión, RBAC, límite de tasa, caché de borde por rol, /health." "ASP.NET Core"
                identidad  = component "Identidad y Accesos" "Cuentas, tokens, bloqueo, consentimientos." "Módulo"
                ingesta    = component "Ingesta" "Recepción durable, cola acotada y trabajadores." "Módulo"
                anomalias  = component "Anomalías" "Reglas RN-05…RN-15, ciclo de vida y exportación." "Módulo"
                consulta   = component "Consulta de Mapa" "Recorte por rol, caché por celda y ETag." "Módulo"
                dashboard  = component "Dashboard" "Indicadores de negocio." "Módulo"
                auditoria  = component "Auditoría" "Registro append-only." "Módulo"
                eventos    = component "Eventos de dominio" "Observer en proceso." "Shared" "Compartido"
                metricas   = component "Métricas" "IMetricas sobre OpenTelemetry." "Shared" "Compartido"
            }
            db     = container "Base de datos" "Un esquema por módulo." "PostgreSQL + PostGIS" "Database"
            backup = container "Tarea de backup" "Backup diario cifrado fuera de la VM." "pg_dump + cron"
        }

        # Personas
        conductorNR -> killauru.app "Consulta el tramo demo y aporta eventos"
        conductorR  -> killauru.app "Consulta el corredor, recibe alertas"
        admin       -> killauru.panel "Gestiona la plataforma"
        empresa     -> killauru "Consumirá analítica de tramos" "" "Futuro"
        autoridad   -> killauru "Accederá bajo convenio" "" "Futuro"

        # App (componentes)
        killauru.app.sesion -> killauru.app.sensores "Habilita la detección"
        killauru.app.sensores -> killauru.app.detector "Ventanas de 500 ms"
        killauru.app.detector -> killauru.baseLocal "Guarda eventos PENDING"
        killauru.app.seudonimo -> killauru.baseLocal "Marca cada evento"
        killauru.app.sincronizador -> killauru.baseLocal "Lee pendientes y escribe anomalías"
        killauru.app.sincronizador -> killauru.api.host "Lotes y updatedSince" "HTTPS/JSON vía Cloudflare"
        killauru.app.sesion -> killauru.api.host "Login, renovación y cierre de sesión" "HTTPS/JSON"
        killauru.app.mapa -> killauru.baseLocal "Lee anomalías"
        killauru.app.corredor -> killauru.baseLocal "Calcula el km de cada anomalía"
        killauru.app.proximidad -> killauru.app.corredor "Proyecta el GPS"
        killauru.app.proximidad -> killauru.baseLocal "Rango por km"
        killauru.app.historial -> killauru.baseLocal "Lee y escribe recorridos"
        killauru.app.actualizador -> r2 "Descarga manifiestos, modelo y mapa base" "HTTPS"
        killauru.app.actualizador -> killauru.app.detector "Activa un modelo verificado"

        # Panel
        killauru.panel -> killauru.api.host "Gestión, mapa, moderación, exportación" "HTTPS/JSON con cookie"
        killauru.panel -> r2 "Lee PMTiles por rangos" "HTTPS"

        # API (componentes)
        killauru.api.host -> killauru.api.identidad "Invoca"
        killauru.api.host -> killauru.api.ingesta "Invoca"
        killauru.api.host -> killauru.api.anomalias "Invoca (moderación, exportación)"
        killauru.api.host -> killauru.api.consulta "Invoca"
        killauru.api.host -> killauru.api.dashboard "Invoca"
        killauru.api.host -> killauru.api.auditoria "Invoca (consulta)"
        killauru.api.ingesta -> killauru.api.anomalias "IAnomaliasService"
        killauru.api.consulta -> killauru.api.anomalias "IAnomaliaLectura"
        killauru.api.dashboard -> killauru.api.anomalias "IAnomaliaLectura"
        killauru.api.dashboard -> killauru.api.identidad "IIdentidadConsultas"
        killauru.api.anomalias -> killauru.api.auditoria "IAuditoriaService"
        killauru.api.identidad -> killauru.api.auditoria "IAuditoriaService"
        killauru.api.anomalias -> killauru.api.eventos "Publica AnomaliaCambiada"
        killauru.api.eventos -> killauru.api.consulta "Notifica: invalida celdas"
        killauru.api.ingesta -> killauru.api.metricas "Registra"
        killauru.api.anomalias -> killauru.api.metricas "Registra descartes"
        killauru.api.metricas -> grafana "Telemetría" "OTLP/HTTPS"
        killauru.api.identidad -> correo "Verificación y recuperación" "HTTPS"
        killauru.api.identidad -> killauru.db "Esquema identidad" "Npgsql"
        killauru.api.ingesta -> killauru.db "Esquema ingesta" "Npgsql"
        killauru.api.anomalias -> killauru.db "Esquema anomalias" "Npgsql"
        killauru.api.auditoria -> killauru.db "Esquema auditoria" "Npgsql"
        killauru.api.dashboard -> killauru.db "Esquema dashboard" "Npgsql"

        # Infraestructura
        killauru -> cloudflare "Publica la API y el panel" "HTTPS"
        uptime -> killauru.api.host "GET /health/ready cada 5 min" "HTTPS"
        killauru.backup -> killauru.db "pg_dump"
        killauru.backup -> r2 "Backup cifrado" "API S3"

        deploymentEnvironment "Produccion" {
            deploymentNode "Celular Android" "" "Android 8.0+ (minSdk 26)" {
                containerInstance killauru.app
                containerInstance killauru.baseLocal
            }
            deploymentNode "Cloudflare" "" "Plan gratuito" {
                deploymentNode "Pages" "" "Estáticos" {
                    containerInstance killauru.panel
                }
            }
            deploymentNode "Oracle Cloud ARM" "Always Free · Santiago de Chile" "Ubuntu + Docker Compose" {
                deploymentNode "api" "" "Contenedor Docker" {
                    containerInstance killauru.api
                }
                deploymentNode "postgres" "" "Contenedor Docker con volumen" {
                    containerInstance killauru.db
                }
                deploymentNode "backup" "" "Contenedor Docker con cron" {
                    containerInstance killauru.backup
                }
                infrastructureNode "cloudflared" "Tunnel hacia Cloudflare, sin puertos abiertos." "Contenedor Docker"
            }
        }
    }

    views {
        systemContext killauru "C1_Contexto" {
            include *
            autoLayout tb
        }
        container killauru "C2_Contenedores" {
            include *
            autoLayout tb
        }
        component killauru.api "C3_API" {
            include *
            autoLayout tb
        }
        component killauru.app "C3_App" {
            include *
            autoLayout lr
        }
        deployment killauru "Produccion" "Despliegue" {
            include *
            autoLayout lr
        }

        styles {
            element "Person" {
                shape Person
            }
            element "Usuario Externo" {
                background #0F766E
                color #ffffff
            }
            element "Personal Interno" {
                background #6D28D9
                color #ffffff
            }
            element "Futuro" {
                background #ffffff
                color #475569
                stroke #94A3B8
                border dashed
            }
            element "Software System" {
                background #1565C0
                color #ffffff
            }
            element "Infraestructura Externa" {
                background #475569
                color #ffffff
            }
            element "Container" {
                background #1565C0
                color #ffffff
            }
            element "Database" {
                shape Cylinder
                background #1E88E5
                color #ffffff
            }
            element "Mobile" {
                shape MobileDevicePortrait
            }
            element "Web Browser" {
                shape WebBrowser
            }
            element "Component" {
                background #A9C9F5
                color #0F2744
            }
            element "Compartido" {
                background #E3F2FD
                color #0F2744
                border dashed
            }
            relationship "Relationship" {
                color #64748B
                thickness 2
            }
            relationship "Futuro" {
                style dashed
            }
        }
    }
}
