namespace KillaUru.Identidad.Infrastructure.Correo;

/// <summary>Mensajes de texto en español; los enlaces usan fragmentos y no parámetros de consulta.</summary>
internal static class PlantillasCorreo
{
    internal static string Texto(string finalidad, string enlace) =>
        $"Hola,\n\nPara {finalidad} tu cuenta de KillaUru, abre este enlace:\n{enlace}\n\n"
        + "El enlace es personal, vence y solo puede utilizarse una vez. Si no lo solicitaste, ignora este mensaje.";
}
