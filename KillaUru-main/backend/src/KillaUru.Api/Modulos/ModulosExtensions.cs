using System.Reflection;
using KillaUru.Anomalias;
using KillaUru.Auditoria;
using KillaUru.Dashboard;
using KillaUru.Identidad;
using KillaUru.Ingesta;
using KillaUru.Mapa;
using KillaUru.Shared.Eventos;
using KillaUru.Shared.Persistencia;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace KillaUru.Api.Modulos;

/// <summary>Raíz de composición: el host es el único que conoce a los 6 módulos (enfoque §7).</summary>
internal static class ModulosExtensions
{
    private static readonly Assembly[] Ensamblados =
    [
        typeof(AuditoriaModule).Assembly, typeof(IdentidadModule).Assembly, typeof(AnomaliasModule).Assembly,
        typeof(IngestaModule).Assembly, typeof(MapaModule).Assembly, typeof(DashboardModule).Assembly,
    ];

    /// <summary>Kernel compartido, los 6 módulos y sus controladores internos.</summary>
    internal static IServiceCollection AgregarModulos(this IServiceCollection services, IConfiguration configuracion)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IContextoTransaccion, ContextoTransaccion>();
        services.AddSingleton<IPublicadorEventos, PublicadorEnMemoria>();

        services
            .AddAuditoriaModule(configuracion)
            .AddIdentidadModule(configuracion)
            .AddAnomaliasModule(configuracion)
            .AddIngestaModule(configuracion)
            .AddMapaModule(configuracion)
            .AddDashboardModule(configuracion);

        AgregarControladores(services);
        return services;
    }

    /// <summary>
    /// Registra los ensamblados de los módulos y reemplaza el proveedor de controladores.
    /// ASP.NET solo descubre controladores públicos por defecto; se reemplaza el proveedor
    /// para que también encuentre los internos (ADR-001: los módulos no exponen tipos públicos).
    /// </summary>
    private static void AgregarControladores(IServiceCollection services) =>
        services.AddControllers().ConfigureApplicationPartManager(partes =>
        {
            foreach (var ensamblado in Ensamblados)
            {
                partes.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(ensamblado));
            }

            // Reemplazar ControllerFeatureProvider estándar por uno que incluye clases internal
            partes.FeatureProviders.Remove(partes.FeatureProviders.OfType<ControllerFeatureProvider>().Single());
            partes.FeatureProviders.Add(new ControladoresInternosFeatureProvider());
        });
}
