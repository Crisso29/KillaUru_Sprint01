"""Plataforma, identidad pública, la cuenta del conductor y su cierre (specs 001 y 003).

El recorrido es el de un conductor real: consulta las versiones legales, se registra, entra, renueva, usa su cuenta,
cierra sesión y la borra. Correo y contraseña se generan al correr (únicos y en memoria); nada se guarda en archivos.
"""
from __future__ import annotations

from peticion import Peticion, carpeta
from scripts_postman import campos, codigo_de_error, estado, guardar, no_cachear

_SESION = campos("tokenAcceso", "tokenRenovacion", "sesionVenceUtc", "rol")
_GUARDAR_SESION = (guardar("tokenConductor", "pm.response.json().tokenAcceso")
                   + guardar("tokenRenovacion", "pm.response.json().tokenRenovacion"))
_CREDENCIALES = {"correo": "{{correo}}", "contrasena": "{{contrasena}}", "canal": "app"}
_CUENTA_NUEVA = [
    "// Cuenta única en cada corrida; la contraseña es un GUID (36 caracteres) y solo vive en memoria",
    "pm.collectionVariables.set('correo', `conductor-${Date.now()}@example.com`);",
    "pm.collectionVariables.set('contrasena', 'Ku-' + pm.variables.replaceIn('{{$guid}}'));",
]


def plataforma() -> dict:
    return carpeta("01 Plataforma", "Salud de la API (001, RF-28): la usa UptimeRobot cada 5 min.", [
        Peticion("La API está viva", "GET", "/health/live", estado(200)),
        Peticion("La API puede atender (BD, migraciones, cola y corredor)", "GET", "/health/ready", estado(200)),
    ])


def identidad_publica() -> dict:
    return carpeta("02 Identidad pública", "Registro e inicio de sesión del conductor por la app (003).", [
        Peticion("Versiones legales vigentes", "GET", "/api/v1/legal/versiones",
                 estado(200) + campos("politicaPrivacidad", "terminosUso")
                 + guardar("versionPolitica", "pm.response.json().politicaPrivacidad.version")
                 + guardar("versionTerminos", "pm.response.json().terminosUso.version")),
        Peticion("Registro de un conductor", "POST", "/api/v1/auth/registro", estado(201) + _SESION + _GUARDAR_SESION,
                 cuerpo=_registro("{{contrasena}}"), antes=_CUENTA_NUEVA),
        Peticion("Registro con un correo ya usado", "POST", "/api/v1/auth/registro",
                 estado(409) + codigo_de_error("correo_en_uso"), cuerpo=_registro("{{contrasena}}")),
        Peticion("Registro con contraseña de 9 caracteres (mínimo 10, D13)", "POST", "/api/v1/auth/registro",
                 estado(400) + codigo_de_error("datos_invalidos"), cuerpo=_registro("123456789")),
        Peticion("Inicio de sesión por la app", "POST", "/api/v1/auth/inicio-sesion",
                 estado(200) + _SESION + _GUARDAR_SESION, cuerpo=_CREDENCIALES),
        Peticion("Inicio de sesión con contraseña incorrecta", "POST", "/api/v1/auth/inicio-sesion", estado(401),
                 cuerpo=_CREDENCIALES | {"contrasena": "otra-contrasena-incorrecta"}),
        Peticion("Renovar la sesión", "POST", "/api/v1/auth/renovar", estado(200) + _SESION + _GUARDAR_SESION,
                 cuerpo={"tokenRenovacion": "{{tokenRenovacion}}"}),
        Peticion("Renovar con un token inventado", "POST", "/api/v1/auth/renovar",
                 estado(401) + codigo_de_error("sesion_invalida"), cuerpo={"tokenRenovacion": "inventado"}),
        Peticion("Verificar correo con un enlace inventado", "POST", "/api/v1/auth/verificar-correo",
                 estado(400) + codigo_de_error("enlace_invalido"), cuerpo={"token": "inventado"}),
        Peticion("Recuperar contraseña de un correo que no existe (misma respuesta, RN-19)", "POST",
                 "/api/v1/auth/recuperar", estado(202), cuerpo={"correo": "nadie-{{$timestamp}}@example.com"}),
        Peticion("Recuperar contraseña de un correo que sí existe", "POST", "/api/v1/auth/recuperar", estado(202),
                 cuerpo={"correo": "{{correo}}"}),
        Peticion("Fijar contraseña con un enlace inventado", "POST", "/api/v1/auth/fijar-contrasena",
                 estado(400) + codigo_de_error("enlace_invalido"),
                 cuerpo={"token": "inventado", "nuevaContrasena": "una-contrasena-larga"}),
    ])


def mi_cuenta() -> dict:
    return carpeta("03 Mi cuenta (conductor)", "Datos propios y aceptación de versiones (HU-24, RF-31).", [
        Peticion("Mis datos", "GET", "/api/v1/me", estado(200) + campos("correo", "rol", "correoVerificado", "aceptaciones")
                 + ["pm.test('son los datos del conductor que entró', () =>",
                    "  pm.expect(pm.response.json().correo).to.eql(pm.collectionVariables.get('correo')));"]
                 + no_cachear(), token="tokenConductor"),
        Peticion("Mis datos sin sesión", "GET", "/api/v1/me", estado(401) + codigo_de_error("token_invalido")),
        Peticion("Aceptar la versión vigente de la política", "POST", "/api/v1/me/aceptaciones", estado(204),
                 cuerpo={"documento": "PoliticaPrivacidad", "version": "{{versionPolitica}}"}, token="tokenConductor"),
        Peticion("Reenviar el correo de verificación", "POST", "/api/v1/me/reenviar-verificacion", estado(202),
                 token="tokenConductor"),
    ])


def cierre() -> dict:
    return carpeta("07 Cierre de la cuenta", "Cerrar sesión y borrar la cuenta (HU-23, RF-30): deja la base limpia.", [
        Peticion("Cerrar sesión", "POST", "/api/v1/auth/cerrar-sesion", estado(204),
                 cuerpo={"tokenRenovacion": "{{tokenRenovacion}}"}, token="tokenConductor"),
        Peticion("Volver a entrar para borrar la cuenta", "POST", "/api/v1/auth/inicio-sesion",
                 estado(200) + _GUARDAR_SESION, cuerpo=_CREDENCIALES),
        Peticion("Borrar mi cuenta", "DELETE", "/api/v1/me", estado(204), cuerpo={"contrasena": "{{contrasena}}"},
                 token="tokenConductor"),
        Peticion("La cuenta borrada ya no puede entrar", "POST", "/api/v1/auth/inicio-sesion", estado(401),
                 cuerpo=_CREDENCIALES),
    ])


def _registro(contrasena: str) -> dict:
    return {"correo": "{{correo}}", "contrasena": contrasena, "versionPoliticaAceptada": "{{versionPolitica}}",
            "versionTerminosAceptada": "{{versionTerminos}}"}
