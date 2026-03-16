using Microsoft.Extensions.Options;
using StsDigital.OrderBook.Models;
using StsDigital.OrderBook.Utils;

namespace StsDigital.OrderBook.Services;

public sealed class OrderBookAggregator(
    IOptions<OrderBookOptions> options,
    TimeProvider? timeProvider = null)
    : IOrderBookCollector, IOrderBookReader
{
    private readonly Lock _lock = new();

    private readonly OrderBookMergeEngine _mergeEngine = new(
        SymbolFormats.ToCanonical(options.Value.Symbol),
        options.Value.DepthLevels,
        TimeSpan.FromMilliseconds(options.Value.StaleThresholdMs),
        timeProvider ?? TimeProvider.System);

    public void Collect(OrderBookSnapshot snapshot)
    {
        lock (_lock)
        {
            _mergeEngine.ApplySnapshot(snapshot);
        }
    }

    public MergedOrderBook GetMergedBook()
    {
        lock (_lock)
        {
            return _mergeEngine.GetMergedBook();
        }
    }
}
