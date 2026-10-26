namespace KillaUru.Identidad.Application.Legal;

/// <summary>Documentos vigentes que la app debe presentar antes de solicitar aceptación.</summary>
internal sealed record VersionesLegales(VersionDocumento PoliticaPrivacidad, VersionDocumento TerminosUso);
