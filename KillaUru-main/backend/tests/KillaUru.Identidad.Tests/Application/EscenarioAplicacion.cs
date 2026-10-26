using KillaUru.Auditoria.Contracts;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Cuenta;
using KillaUru.Identidad.Application.Privilegios;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Application.Resultados;
using KillaUru.Identidad.Application.Sesion;
using KillaUru.Identidad.Application.Usuarios;
using KillaUru.Identidad.Domain;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace KillaUru.Identidad.Tests.Application;

/// <summary>
/// Puertos sustituidos para observar decisiones y efectos de los casos de uso sin base de datos.
/// La transacción ejecuta la acción al instante, como si siempre confirmara.
/// </summary>
internal sealed class EscenarioAplicacion
{
    internal IUsuarioRepositorio Usuarios { get; } = Substitute.For<IUsuarioRepositorio>();
    internal ISesionRepositorio Sesiones { get; } = Substitute.For<ISesionRepositorio>();
    internal ITokenCorreoRepositorio Tokens { get; } = Substitute.For<ITokenCorreoRepositorio>();
    internal IRevisionRepositorio Revisiones { get; } = Substitute.For<IRevisionRepositorio>();
    internal ITransaccionIdentidad Transaccion { get; } = Substitute.For<ITransaccionIdentidad>();
    internal IEnviadorCorreo Correo { get; } = Substitute.For<IEnviadorCorreo>();
    internal IAuditoriaService Auditoria { get; } = Substitute.For<IAuditoriaService>();
    internal IGeneradorSecretos Secretos { get; } = Substitute.For<IGeneradorSecretos>();
    internal IHashContrasenas Hash { get; } = Substitute.For<IHashContrasenas>();
    internal IOptions<IdentidadOptions> Opciones { get; } = Options.Create(new IdentidadOptions());
    internal FakeTimeProvider Reloj { get; } = new();
    internal static CancellationToken Ct => TestContext.Current.CancellationToken;
    internal ActorIdentidad Administrador { get; } = new(Guid.NewGuid(), "Administrador");

    internal AuditarIdentidad Auditar => new(Auditoria);
    internal EmitirEnlaceCorreo Enlaces => new(Tokens, Secretos, Correo, Transaccion, Opciones, Reloj);
    internal EmisionSesion Emision => new(Sesiones, Secretos, Substitute.For<IEmisorJwt>(), Opciones, Reloj);
    internal ProteccionAdministradores Proteccion => new(Usuarios);

    internal EscenarioAplicacion()
    {
        Secretos.Aleatorio32().Returns("secreto-sintetico");
        Secretos.Hash(Arg.Any<string>()).Returns([1]);
        TransaccionConfirmaSiempre<ResultadoLogin>();
        TransaccionConfirmaSiempre<ResultadoGestion>();
        TransaccionConfirmaSiempre<ResultadoUsuario>();
        TransaccionConfirmaSiempre<ResultadoEnlace>();
        TransaccionConfirmaSiempre<ResultadoRevision>();
    }

    internal Usuario Conductor(string correo = "conductor@example.com") =>
        Usuario.RegistrarConductor(CorreoDe(correo), "hash", Reloj.GetUtcNow());

    internal Usuario AdministradorRegistrado(string correo) =>
        Usuario.Invitar(CorreoDe(correo), RolUsuario.Administrador, Reloj.GetUtcNow());

    private static KillaUru.Identidad.Domain.Correo CorreoDe(string direccion) => KillaUru.Identidad.Domain.Correo.Crear(direccion)!;

    /// <summary>Hace que el repositorio encuentre a estos usuarios por su id.</summary>
    internal void Existen(params Usuario[] usuarios)
    {
        foreach (var usuario in usuarios) Usuarios.PorIdAsync(usuario.Id, Arg.Any<CancellationToken>()).Returns(usuario);
    }

    private void TransaccionConfirmaSiempre<T>() =>
        Transaccion.EjecutarAsync(Arg.Any<Func<Task<T>>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(llamada => llamada.Arg<Func<Task<T>>>()());
}
