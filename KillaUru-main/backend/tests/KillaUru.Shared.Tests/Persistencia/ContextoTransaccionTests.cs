using System.Data.Common;
using KillaUru.Shared.Persistencia;
using NSubstitute;

namespace KillaUru.Shared.Tests.Persistencia;

public sealed class ContextoTransaccionTests
{
    private readonly ContextoTransaccion _contexto = new();
    private readonly DbConnection _conexion = Substitute.For<DbConnection>();
    private readonly DbTransaction _transaccion = Substitute.For<DbTransaction>();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task AlConfirmar_corre_solo_despues_del_commit()
    {
        var orden = new List<string>();
        _transaccion.CommitAsync(Arg.Any<CancellationToken>()).Returns(_ => { orden.Add("commit"); return Task.CompletedTask; });
        _contexto.Unir(_conexion, _transaccion);
        _contexto.AlConfirmar(() => { orden.Add("accion"); return Task.CompletedTask; });

        await _contexto.ConfirmarAsync(Ct);

        Assert.Equal(["commit", "accion"], orden);
    }

    [Fact]
    public async Task AlConfirmar_nunca_corre_tras_revertir()
    {
        var corrio = false;
        _contexto.Unir(_conexion, _transaccion);
        _contexto.AlConfirmar(() => { corrio = true; return Task.CompletedTask; });

        await _contexto.RevertirAsync(Ct);

        Assert.False(corrio);
        await _transaccion.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Tras_confirmar_queda_libre_para_otra_transaccion()
    {
        _contexto.Unir(_conexion, _transaccion);

        await _contexto.ConfirmarAsync(Ct);

        Assert.Null(_contexto.Conexion);
        Assert.Null(_contexto.Transaccion);
        _contexto.Unir(_conexion, _transaccion);
    }

    [Fact]
    public async Task Tras_revertir_queda_libre()
    {
        _contexto.Unir(_conexion, _transaccion);

        await _contexto.RevertirAsync(Ct);

        Assert.Null(_contexto.Transaccion);
    }

    [Fact]
    public void Unir_dos_veces_sin_liberar_es_un_error_de_programacion()
    {
        _contexto.Unir(_conexion, _transaccion);

        Assert.Throws<InvalidOperationException>(() => _contexto.Unir(_conexion, _transaccion));
    }

    [Fact]
    public void AlConfirmar_sin_transaccion_es_un_error_de_programacion() =>
        Assert.Throws<InvalidOperationException>(() => _contexto.AlConfirmar(() => Task.CompletedTask));

    [Fact]
    public async Task Confirmar_sin_transaccion_es_un_error_de_programacion() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() => _contexto.ConfirmarAsync(Ct));
}
