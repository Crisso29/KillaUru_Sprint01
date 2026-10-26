using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Sesion;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
namespace KillaUru.Identidad.Tests.Application;

public sealed class RegistroTests
{
    [Theory]
    [InlineData("123456789", "1.2", "datos_invalidos")]
    [InlineData("1234567890", "1.1", "aceptacion_requerida")]
    [InlineData("1234567890", null, "aceptacion_requerida")]
    [InlineData("1234567890", "1.2", null)]
    public async Task Registro_valida_y_audita_sin_emitir_antes_del_commit(string clave, string? version, string? error)
    {
        var usuarios = Substitute.For<IUsuarioRepositorio>();
        var transaccion = Substitute.For<ITransaccionIdentidad>();
        transaccion.EjecutarAsync(Arg.Any<Func<Task<ResultadoLogin>>>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<Func<Task<ResultadoLogin>>>()());
        var auditoria = Substitute.For<IAuditoriaService>();
        var correo = Substitute.For<IEnviadorCorreo>();
        var secretos = Substitute.For<IGeneradorSecretos>();
        secretos.Aleatorio32().Returns("secreto");
        secretos.Hash(Arg.Any<string>()).Returns([1]);
        var opciones = Options.Create(new IdentidadOptions());
        var reloj = new FakeTimeProvider();
        var emision = new EmisionSesion(Substitute.For<ISesionRepositorio>(), secretos, Substitute.For<IEmisorJwt>(), opciones, reloj);
        var enlaces = new EmitirEnlaceCorreo(Substitute.For<ITokenCorreoRepositorio>(), secretos, correo, transaccion, opciones, reloj);
        var caso = new RegistrarConductorUseCase(usuarios, Substitute.For<IHashContrasenas>(), transaccion,
            emision, enlaces, new AuditarIdentidad(auditoria), opciones, reloj);
        var resultado = await caso.EjecutarAsync(new("a@example.com", clave, version, "1.2"), TestContext.Current.CancellationToken);
        Assert.Equal(error, resultado.Error);
        if (error is null)
        {
            usuarios.Received(1).Agregar(Arg.Is<Usuario>(u => u.Rol == RolUsuario.ConductorRegistrado && !u.CorreoVerificado));
            await auditoria.Received(1).RegistrarAsync(AccionAuditada.UsuarioCreado, ActorAuditoria.Sistema,
                Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, object?>>(), Arg.Any<CancellationToken>());
            transaccion.Received(1).AlConfirmar(Arg.Any<Func<Task>>());
        }
        await correo.DidNotReceiveWithAnyArgs().VerificacionAsync(default!, default!, TestContext.Current.CancellationToken);
    }
}
