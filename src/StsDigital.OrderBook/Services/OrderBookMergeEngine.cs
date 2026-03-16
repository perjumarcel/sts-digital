using System.Runtime.InteropServices;
using StsDigital.OrderBook.Models;

namespace StsDigital.OrderBook.Services;

public sealed class OrderBookMergeEngine(
    string symbol,
    int levels,
    TimeSpan staleThreshold,
    TimeProvider timeProvider)
{
    private readonly Dictionary<decimal, decimal> _askTotals = new();
    private readonly Dictionary<decimal, decimal> _bidTotals = new();
    private readonly Dictionary<Exchange, OrderBookSnapshot> _snapshotsByExchange = new();
    private MergedOrderBook _mergedBook = new([], [], symbol, timeProvider.GetUtcNow());

    private PriceLevel[] _sortBuffer = new PriceLevel[levels];

    public void ApplySnapshot(OrderBookSnapshot snapshot)
    {
        _snapshotsByExchange[snapshot.Exchange] = snapshot;
        _mergedBook = RebuildMergedBook();
    }

    public MergedOrderBook GetMergedBook()
    {
        if (staleThreshold > TimeSpan.Zero && HasStaleSnapshots())
        {
            _mergedBook = RebuildMergedBook();
        }

        return _mergedBook;
    }

    private DateTimeOffset GetCutoff(DateTimeOffset now) =>
        staleThreshold > TimeSpan.Zero
            ? now - staleThreshold
            : DateTimeOffset.MinValue;

    private bool HasStaleSnapshots()
    {
        DateTimeOffset cutoff = GetCutoff(timeProvider.GetUtcNow());
        return _snapshotsByExchange.Any(kv => kv.Value.Timestamp < cutoff);
    }

    private MergedOrderBook RebuildMergedBook()
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset cutoff = GetCutoff(now);

        _bidTotals.Clear();
        _askTotals.Clear();

        List<Exchange>? staleExchanges = null;

        foreach (KeyValuePair<Exchange, OrderBookSnapshot> kv in _snapshotsByExchange)
        {
            if (kv.Value.Timestamp < cutoff)
            {
                (staleExchanges ??= []).Add(kv.Key);
                continue;
            }

            AccumulateLevels(kv.Value.Bids, _bidTotals);
            AccumulateLevels(kv.Value.Asks, _askTotals);
        }

        if (staleExchanges is not null)
        {
            foreach (Exchange id in staleExchanges)
            {
                _snapshotsByExchange.Remove(id);
            }
        }

        PriceLevel[] mergedBids = BuildTopLevels<BidComparer>(_bidTotals);
        PriceLevel[] mergedAsks = BuildTopLevels<AskComparer>(_askTotals);

        return new MergedOrderBook(mergedBids, mergedAsks, symbol, now);
    }

    private static void AccumulateLevels(IReadOnlyList<PriceLevel> priceLevels, Dictionary<decimal, decimal> totals)
    {
        foreach (PriceLevel level in priceLevels)
        {
            ref decimal qty = ref CollectionsMarshal.GetValueRefOrAddDefault(totals, level.Price, out _);
            qty += level.Quantity;
        }
    }

    private PriceLevel[] BuildTopLevels<TComparer>(Dictionary<decimal, decimal> totals)
        where TComparer : struct, IComparer<PriceLevel>
    {
        if (totals.Count == 0)
        {
            return [];
        }

        if (_sortBuffer.Length < totals.Count)
        {
            _sortBuffer = new PriceLevel[totals.Count];
        }

        int index = 0;
        foreach ((decimal price, decimal quantity) in totals)
        {
            _sortBuffer[index++] = new PriceLevel(price, quantity);
        }

        Span<PriceLevel> sortedLevels = _sortBuffer.AsSpan(0, index);
        sortedLevels.Sort(default(TComparer));

        return sortedLevels[..Math.Min(index, levels)].ToArray();
    }

    private readonly struct BidComparer : IComparer<PriceLevel>
    {
        public int Compare(PriceLevel a, PriceLevel b) => b.Price.CompareTo(a.Price);
    }

    private readonly struct AskComparer : IComparer<PriceLevel>
    {
        public int Compare(PriceLevel a, PriceLevel b) => a.Price.CompareTo(b.Price);
    }
}
