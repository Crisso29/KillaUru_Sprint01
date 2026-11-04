using System.Text.Json;
using KillaUru.Anomalias.Contracts;
using KillaUru.Ingesta.Application.Recepcion;

namespace KillaUru.Ingesta.Tests.Application;

/// <summary>
/// LectorEvento deserializa un JsonElement de forma tolerante: un campo ausente o de tipo
/// incorrecto convierte ese campo en null sin lanzar excepciones (FR-002, R-001).
/// Spec 005 · T007.
/// </summary>
public sealed class LectorEventoTests
{
    private static JsonElement Parsear(string json) =>
        JsonDocument.Parse(json).RootElement;

    private static string EventoCompleto(
        string? eventoId = "\"0192f1b5-0a44-7d20-8c1e-5e3b9f2d6a70\"",
        string? detectadoUtc = "\"2026-10-20T14:32:05Z\"",
        string? latitud = "-13.2105", string? longitud = "-74.2581",
        string? precisionMetros = "6.5", string? tipo = "\"Bache\"",
        string? severidad = "4", string? confidence = "0.91",
        string? modeloVersion = "\"1.2.0\"",
        string? seudonimo = "\"6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f\"") =>
        $$"""
        {
          "eventoId": {{eventoId ?? "null"}},
          "detectadoUtc": {{detectadoUtc ?? "null"}},
          "latitud": {{latitud ?? "null"}}, "longitud": {{longitud ?? "null"}},
          "precisionMetros": {{precisionMetros ?? "null"}},
          "tipo": {{tipo ?? "null"}}, "severidad": {{severidad ?? "null"}},
          "confidence": {{confidence ?? "null"}}, "modeloVersion": {{modeloVersion ?? "null"}},
          "seudonimo": {{seudonimo ?? "null"}}
        }
        """;

    /// <summary>Un evento bien formado devuelve todos los campos correctamente.</summary>
    [Fact]
    public void Evento_completo_se_lee_sin_errores()
    {
        // Arrange
        var elemento = Parsear(EventoCompleto());

        // Act
        var (id, evento) = LectorEvento.Leer(elemento, 0);

        // Assert
        Assert.NotNull(id);
        Assert.NotNull(evento);
        Assert.Equal("Bache", evento.Tipo);
        Assert.Equal(4, evento.Severidad);
    }

    /// <summary>Campo ausente del JSON produce null en el campo correspondiente, sin excepción.</summary>
    [Fact]
    public void Campo_ausente_produce_null_sin_excepcion()
    {
        // Arrange — se omite "severidad" del JSON
        var json = """
            {
              "eventoId": "0192f1b5-0a44-7d20-8c1e-5e3b9f2d6a70",
              "detectadoUtc": "2026-10-20T14:32:05Z",
              "latitud": -13.2105, "longitud": -74.2581,
              "precisionMetros": 6.5, "tipo": "Bache",
              "confidence": 0.91, "modeloVersion": "1.2.0",
              "seudonimo": "6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f"
            }
            """;

        // Act
        var (_, evento) = LectorEvento.Leer(Parsear(json), 0);

        // Assert
        Assert.Null(evento?.Severidad);
    }

    /// <summary>"severidad": "alta" (string en lugar de int) produce null, sin excepción (R-001).</summary>
    [Fact]
    public void Tipo_incorrecto_produce_null_sin_excepcion()
    {
        // Arrange
        var elemento = Parsear(EventoCompleto(severidad: "\"alta\""));

        // Act
        var (_, evento) = LectorEvento.Leer(elemento, 0);

        // Assert
        Assert.Null(evento?.Severidad);
    }

    /// <summary>eventoId ausente devuelve id=null con su índice (D5).</summary>
    [Fact]
    public void EventoId_ausente_devuelve_id_null_con_indice()
    {
        // Arrange
        var json = """
            {
              "detectadoUtc": "2026-10-20T14:32:05Z",
              "latitud": -13.2105, "longitud": -74.2581,
              "precisionMetros": 6.5, "tipo": "Bache", "severidad": 4,
              "confidence": 0.91, "modeloVersion": "1.2.0",
              "seudonimo": "6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f"
            }
            """;

        // Act
        var (id, _) = LectorEvento.Leer(Parsear(json), 7);

        // Assert
        Assert.Null(id);
    }

    /// <summary>eventoId con texto que no es UUID devuelve id=null (D5).</summary>
    [Fact]
    public void EventoId_no_uuid_devuelve_id_null()
    {
        // Arrange
        var elemento = Parsear(EventoCompleto(eventoId: "\"no-es-un-uuid\""));

        // Act
        var (id, _) = LectorEvento.Leer(elemento, 3);

        // Assert
        Assert.Null(id);
    }

    /// <summary>El payload resultante no incluye el seudónimo (RN-09, FR-008).</summary>
    [Fact]
    public void Payload_no_incluye_seudonimo()
    {
        // Arrange
        var elemento = Parsear(EventoCompleto());

        // Act
        var (_, evento) = LectorEvento.Leer(elemento, 0);
        var payload = LectorEvento.ObtenerPayloadSinSeudonimo(elemento);

        // Assert
        Assert.False(payload.RootElement.TryGetProperty("seudonimo", out _));
        Assert.False(payload.RootElement.TryGetProperty("eventoId", out _));
    }
}
