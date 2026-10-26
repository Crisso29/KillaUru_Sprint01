using System.Diagnostics;
using System.Text.Json;
using KillaUru.Identidad.Application.Puertos;
using KillaUru.Identidad.Domain;
using KillaUru.Identidad.Infrastructure;
using KillaUru.Integracion.Tests.Infraestructura;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace KillaUru.Integracion.Tests.Identidad;

/// <summary>
/// Carga con k6 sobre Kestrel y PostgreSQL reales: 300 cuentas haciendo login y renovación deben
/// cumplir los umbrales del script. Es explícita porque requiere k6 instalado y tarda minutos.
/// Spec 003 · SC (rendimiento de login y renovación).
/// </summary>
public sealed class RendimientoIdentidadTests(BaseDatosPrueba bd)
{
    private const int CuentasSinteticas = 300;

    [Fact(Explicit = true)]
    public async Task K6_con_300_cuentas_cumple_login_y_renovacion()
    {
        // Arrange
        using var api = new FabricaApi(bd, "Pruebas");
        api.UseKestrel(0);
        using var cliente = api.CreateClient();
        var backend = Directory.GetParent(Rutas.Migraciones)!.Parent!.FullName;
        var resultados = Directory.CreateDirectory(Path.Combine(backend, "TestResults")).FullName;
        var cuentas = Path.Combine(resultados, "identidad-cuentas-sinteticas.json");
        await SembrarCuentasAsync(api.Services, cuentas);
        var direccion = api.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();

        // Act
        var codigoSalida = await EjecutarK6Async(backend, resultados, direccion, cuentas);

        // Assert
        Assert.True(codigoSalida == 0, "k6 no cumplió los umbrales; revisar TestResults/identidad-k6.log.");
    }

    /// <summary>Corre k6 y guarda su salida en TestResults para revisar los percentiles.</summary>
    private static async Task<int> EjecutarK6Async(string backend, string resultados, string direccion, string cuentas)
    {
        var inicio = new ProcessStartInfo("k6") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argumento in new[] { "run", "--summary-export", Path.Combine(resultados, "identidad-k6.json"), Path.Combine(backend, "tests", "carga", "identidad.k6.js") })
            inicio.ArgumentList.Add(argumento);
        inicio.Environment["BASE_URL"] = direccion;
        inicio.Environment["USUARIOS_ARCHIVO"] = cuentas;
        inicio.Environment["CONFIRMAR_PRUEBAS"] = "si"; // el script se niega a correr sin esta confirmación
        using var proceso = Process.Start(inicio)!;
        var salida = proceso.StandardOutput.ReadToEndAsync(EscenarioIdentidad.Ct);
        var errores = proceso.StandardError.ReadToEndAsync(EscenarioIdentidad.Ct);
        await proceso.WaitForExitAsync(EscenarioIdentidad.Ct);
        await File.WriteAllTextAsync(Path.Combine(resultados, "identidad-k6.log"), await salida + await errores, EscenarioIdentidad.Ct);
        return proceso.ExitCode;
    }

    /// <summary>Inserta las cuentas directo en la base: registrarlas por la API chocaría con el límite de 5/h por IP.</summary>
    private static async Task SembrarCuentasAsync(IServiceProvider servicios, string archivo)
    {
        await using var scope = servicios.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var hash = scope.ServiceProvider.GetRequiredService<IHashContrasenas>().Hash(EscenarioIdentidad.Contrasena);
        var ahora = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow();
        var cuentas = Enumerable.Range(0, CuentasSinteticas)
            .Select(_ => new { correo = EscenarioIdentidad.CorreoNuevo(), contrasena = EscenarioIdentidad.Contrasena }).ToArray();
        db.Usuarios.AddRange(cuentas.Select(c => Usuario.RegistrarConductor(Correo.Crear(c.correo)!, hash, ahora)));
        await db.SaveChangesAsync(EscenarioIdentidad.Ct);
        await File.WriteAllTextAsync(archivo, JsonSerializer.Serialize(cuentas), EscenarioIdentidad.Ct);
    }
}
