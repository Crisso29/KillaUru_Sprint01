using System.Threading.Channels;
using KillaUru.Ingesta.Application.Procesamiento;
using KillaUru.Ingesta.Application.Puertos;
using KillaUru.Shared.Metricas;
using Microsoft.Extensions.Options;

namespace KillaUru.Ingesta.Infrastructure;

/// <summary>
/// Cola en memoria acotada usando System.Threading.Channels (ADR-005, FR-011).
/// DropWrite: TryEscribir devuelve false en vez de bloquear cuando la cola está llena (DA-05).
/// </summary>
internal sealed class ColaLotesChannel : IColaLotes
{
    private readonly Channel<LoteRecibido> _channel;
    private readonly int _capacidad;

    public ColaLotesChannel(IOptions<Application.IngestaOptions> opciones, IMetricas metricas)
    {
        _capacidad = opciones.Value.CapacidadCola;
        _channel = Channel.CreateBounded<LoteRecibido>(
            new BoundedChannelOptions(_capacidad)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleWriter = false,
                SingleReader = false,
            });
        metricas.ObservarCola(() => (_channel.Reader.Count, 0));
    }

    /// <summary>True si aún hay espacio antes de que el próximo TryEscribir descarte el aviso.</summary>
    public bool HayCapacidad() => _channel.Reader.Count < _capacidad;

    /// <summary>Escribe si hay espacio; DropWrite hace que devuelva false cuando la cola está llena.</summary>
    public bool TryEscribir(LoteRecibido aviso) => _channel.Writer.TryWrite(aviso);

    public IAsyncEnumerable<LoteRecibido> LeerAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
