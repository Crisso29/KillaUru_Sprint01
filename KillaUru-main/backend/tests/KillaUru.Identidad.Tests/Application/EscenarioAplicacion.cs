using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Application.Privilegios;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Usuarios;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
namespace KillaUru.Identidad.Tests.Application;

/// <summary>Puertos sustituidos para observar decisiones y efectos sin base de datos.</summary>
internal sealed class EscenarioAplicacion
{
    internal IUsuarioRepositorio Usuarios { get; } = Substitute.For<IUsuarioRepositorio>();
    internal ISesionRepositorio Sesiones { get; } = Substitute.For<ISesionRepositorio>();
    internal ITokenCorreoRepositorio Tokens { get; } = Substitute.For<ITokenCorreoRepositorio>();
    internal ITransaccionIdentidad Transaccion { get; } = Substitute.For<ITransaccionIdentidad>();
    internal IEnviadorCorreo Correo { get; } = Substitute.For<IEnviadorCorreo>();
    internal IAuditoriaService Auditoria { get; } = Substitute.For<IAuditoriaService>();
    internal IGeneradorSecretos Secretos { get; } = Substitute.For<IGeneradorSecretos>();
    internal FakeTimeProvider Reloj { get; } = new();
    internal AuditarIdentidad Auditar => new(Auditoria);
    internal EmitirEnlaceCorreo Enlaces => new(Tokens, Secretos, Correo, Transaccion, Options.Create(new IdentidadOptions()), Reloj);

    internal EscenarioAplicacion()
    {
        Secretos.Aleatorio32().Returns("secreto-sintetico");
        Secretos.Hash(Arg.Any<string>()).Returns([1]);
        Preparar<ResultadoGestion>(); Preparar<ResultadoUsuario>(); Preparar<ResultadoEnlace>(); Preparar<ResultadoRevision>();
    }

    private void Preparar<T>() => Transaccion.EjecutarAsync(Arg.Any<Func<Task<T>>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
        .Returns(x => x.Arg<Func<Task<T>>>()());
}
