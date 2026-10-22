using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace KillaUru.Api.Modulos;

/// <summary>
/// ASP.NET solo descubre controladores públicos, pero en cada módulo todo es <c>internal</c> salvo
/// Contracts/ (constitución II). Este proveedor acepta también los internos marcados con [ApiController] (R-003).
/// </summary>
internal sealed class ControladoresInternosFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo) =>
        typeInfo.IsClass
        && !typeInfo.IsAbstract
        && !typeInfo.ContainsGenericParameters
        && typeInfo.IsDefined(typeof(ApiControllerAttribute), inherit: true)
        && typeof(ControllerBase).IsAssignableFrom(typeInfo);
}
