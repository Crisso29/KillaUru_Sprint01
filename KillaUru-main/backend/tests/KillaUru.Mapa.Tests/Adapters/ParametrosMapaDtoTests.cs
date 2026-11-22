using KillaUru.Anomalias.Contracts;
using KillaUru.Mapa.Adapters.Dtos;

namespace KillaUru.Mapa.Tests.Adapters;

/// <summary>
/// `severidad` y `estado` llegan como listas separadas por comas (`?severidad=4,5`), como dicen el contrato
/// `mapa.openapi.yaml` y el panel. Una lista mal formada se rechaza entera. Spec 006 · T029 · FR-001.
/// </summary>
public sealed class ParametrosMapaDtoTests
{
    private static readonly int[] SeveridadesAltas = [4, 5];
    private static readonly EstadoAnomalia[] EstadosDeModeracion = [EstadoAnomalia.Activa, EstadoAnomalia.PendienteConfirmacion];

    /// <summary>Sin filtros de lista, ambos quedan sin valor: «sin filtro», no «lista vacía».</summary>
    [Fact]
    public void Sin_severidad_ni_estado_las_listas_quedan_nulas()
    {
        // Arrange
        var dto = new ParametrosMapaDto();

        // Act
        var listas = dto.LeerListas();

        // Assert
        Assert.NotNull(listas);
        Assert.Null(listas.Severidades);
        Assert.Null(listas.Estados);
    }

    /// <summary>`4,5` es la severidad 4 y la 5, en el orden recibido.</summary>
    [Fact]
    public void Severidad_separada_por_comas_se_lee_como_lista()
    {
        // Arrange
        var dto = new ParametrosMapaDto { Severidad = "4,5" };

        // Act
        var listas = dto.LeerListas();

        // Assert
        Assert.Equal(SeveridadesAltas, listas!.Severidades);
    }

    /// <summary>El administrador pide varios estados en un solo parámetro, como lo envía el panel.</summary>
    [Fact]
    public void Estado_separado_por_comas_se_lee_como_lista()
    {
        // Arrange
        var dto = new ParametrosMapaDto { Estado = "Activa,PendienteConfirmacion" };

        // Act
        var listas = dto.LeerListas();

        // Assert
        Assert.Equal(EstadosDeModeracion, listas!.Estados);
    }

    /// <summary>
    /// Un elemento vacío, no numérico o fuera del catálogo invalida la lista (el controlador responde 400
    /// `datos_invalidos`). Los estados se nombran; su número interno no es parte del contrato.
    /// </summary>
    [Theory]
    [InlineData("4,,5", null)]
    [InlineData("cuatro", null)]
    [InlineData(null, "Activa,Rota")]
    [InlineData(null, "1")]
    [InlineData(null, "")]
    public void Lista_mal_formada_se_rechaza(string? severidad, string? estado)
    {
        // Arrange
        var dto = new ParametrosMapaDto { Severidad = severidad, Estado = estado };

        // Act
        var listas = dto.LeerListas();

        // Assert
        Assert.Null(listas);
    }
}
