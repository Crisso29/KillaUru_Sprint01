namespace KillaUru.Identidad.Application.Resultados;

/// <summary>Versión y fecha que prueban una aceptación explícita.</summary>
internal sealed record AceptacionVista(string Documento, string Version, DateTimeOffset AceptadoUtc);
