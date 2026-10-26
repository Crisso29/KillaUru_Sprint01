using System.Text.Json.Nodes;
using Microsoft.OpenApi;
namespace KillaUru.Identidad.Adapters.Documentacion;

/// <summary>Ejemplos sintéticos: nunca documentos, tokens ni usuarios reales.</summary>
internal static class EjemplosIdentidad
{
    internal static void Completar(OpenApiOperation operacion)
    {
        var ejemplo = Ejemplo(operacion.OperationId);
        if (ejemplo is not null && operacion.RequestBody?.Content?.TryGetValue("application/json", out var contenido) == true)
            contenido.Example = JsonNode.Parse(ejemplo);
    }

    private static string? Ejemplo(string? id) => id switch
        {
            "registrarConductor" => """{"correo":"conductor@ejemplo.pe","contrasena":"una-clave-larga","versionPoliticaAceptada":"1.2","versionTerminosAceptada":"1.2"}""",
            "iniciarSesion" => """{"correo":"conductor@ejemplo.pe","contrasena":"una-clave-larga","canal":"app"}""",
            "renovarSesion" or "cerrarSesion" => """{"tokenRenovacion":"token-base64url"}""",
            "verificarCorreo" => """{"token":"token-del-fragmento"}""",
            "solicitarRecuperacion" => """{"correo":"conductor@ejemplo.pe"}""",
            "fijarContrasenaConEnlace" => """{"token":"token-del-fragmento","nuevaContrasena":"nueva-clave-larga"}""",
            "eliminarMiCuenta" => """{"contrasena":"una-clave-larga"}""",
            "aceptarVersion" => """{"documento":"PoliticaPrivacidad","version":"1.2"}""",
            "crearUsuario" => """{"correo":"administrador@ejemplo.pe","rol":"Administrador"}""",
            "cambiarRol" => """{"rol":"ConductorRegistrado"}""",
            "desactivarUsuario" => """{"motivo":"Fin de funciones"}""",
            "registrarRevisionPrivilegios" => """{"confirmados":["019a10e0-0000-7000-8000-000000000001"],"retirados":[]}""",
            _ => null,
        };
}
