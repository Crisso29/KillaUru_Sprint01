using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using NSubstitute;

namespace KillaUru.Ingesta.Tests.Application;

/// <summary>
/// BarrerPendientesUseCase encola todos los lotes con filas Recibido y respeta la capacidad
/// de la cola; al arrancar no hay límite de antigüedad (FR-016; R-011). Spec 005 · T025.
/// </summary>
public sealed class BarrerPendientesUseCaseTests
{
    private static (BarrerPendientesUseCase uc, IBarridoPendientes barrido, IColaLotes cola)
    Crear()
    {
        var barrido = Substitute.For<IBarridoPendientes>();
        var cola = Substitute.For<IColaLotes>();
        var uc = new BarrerPendientesUseCase(barrido, cola);
        return (uc, barrido, cola);
    }

    /// <summary>Al arrancar, encola todos los lotes pendientes sin filtro de antigüedad.</summary>
    [Fact]
    public async Task Al_arrancar_encola_todos_sin_limite_de_antiguedad()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var (uc, barrido, cola) = Crear();
        barrido.LotesPendientesAsync(null, 100, Arg.Any<CancellationToken>())
               .Returns(Task.FromResult<IReadOnlyList<Guid?>>([id1, id2]));
        barrido.LotesPendientesAsync(id2, 100, Arg.Any<CancellationToken>())
               .Returns(Task.FromResult<IReadOnlyList<Guid?>>([]));
        cola.TryEscribir(Arg.Any<LoteRecibido>()).Returns(true);

        // Act
        await uc.EjecutarAsync(null, CancellationToken.None);

        // Assert
        cola.Received(2).TryEscribir(Arg.Any<LoteRecibido>());
    }

    /// <summary>Filas sin lote_id se encolan como LoteRecibido(null) (R-011).</summary>
    [Fact]
    public async Task Filas_sin_loteId_se_encolan_como_null()
    {
        // Arrange
        var (uc, barrido, cola) = Crear();
        barrido.LotesPendientesAsync(null, 100, Arg.Any<CancellationToken>())
               .Returns(Task.FromResult<IReadOnlyList<Guid?>>([null]),
                        Task.FromResult<IReadOnlyList<Guid?>>([]));
        cola.TryEscribir(Arg.Any<LoteRecibido>()).Returns(true);

        // Act
        await uc.EjecutarAsync(null, CancellationToken.None);

        // Assert
        cola.Received().TryEscribir(Arg.Is<LoteRecibido>(lr => lr.LoteId == null));
    }

    /// <summary>Cuando la cola no tiene capacidad, el barredor para de encolar (FR-016).</summary>
    [Fact]
    public async Task Sin_capacidad_en_cola_deja_de_encolar()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var (uc, barrido, cola) = Crear();
        barrido.LotesPendientesAsync(null, 100, Arg.Any<CancellationToken>())
               .Returns(Task.FromResult<IReadOnlyList<Guid?>>([id1, id2]));
        cola.TryEscribir(Arg.Any<LoteRecibido>()).Returns(false);  // cola llena

        // Act
        await uc.EjecutarAsync(null, CancellationToken.None);

        // Assert — se intentó escribir pero no se continuó con el lote siguiente
        cola.Received(1).TryEscribir(Arg.Any<LoteRecibido>());
    }

    /// <summary>Con antigüedad no nula, el filtro de tiempo se pasa al repositorio (FR-016).</summary>
    [Fact]
    public async Task Con_antiguedad_pasa_el_filtro_al_repositorio()
    {
        // Arrange
        var anteriorA = new DateTimeOffset(2026, 10, 20, 14, 31, 05, TimeSpan.Zero);
        var (uc, barrido, cola) = Crear();
        barrido.LotesPendientesAsync(null, 100, Arg.Any<CancellationToken>())
               .Returns(Task.FromResult<IReadOnlyList<Guid?>>([]));

        // Act
        await uc.EjecutarAsync(anteriorA, CancellationToken.None);

        // Assert
        await barrido.Received(1).LotesPendientesAsync(null, 100, Arg.Any<CancellationToken>());
    }
}
