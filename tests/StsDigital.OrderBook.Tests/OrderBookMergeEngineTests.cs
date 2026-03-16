using Microsoft.Extensions.Time.Testing;
using StsDigital.OrderBook.Models;
using StsDigital.OrderBook.Services;

namespace StsDigital.OrderBook.Tests;

public class OrderBookMergeEngineTests
{
    private const string DefaultSymbol = "ETH/BTC";

    [Test]
    public void ShouldReturnEmpty_When_NoSnapshots()
    {
        OrderBookMergeEngine engine = CreateEngine();
        MergedOrderBook book = engine.GetMergedBook();

        AssertEmptyBook(book);
    }

    [Test]
    public void ShouldReturnEmpty_When_AllSnapshots_AreEmpty()
    {
        OrderBookMergeEngine engine = CreateEngine();

        engine.ApplySnapshot(MakeSnapshot(bids: [], asks: []));
        engine.ApplySnapshot(MakeSnapshot(bids: [], asks: []));

        MergedOrderBook book = engine.GetMergedBook();

        AssertEmptyBook(book);
    }

    [Test]
    public void ShoulReturnEmpty_When_AllSNapshots_Expired()
    {
        FakeTimeProvider time = new();
        time.SetUtcNow(DateTimeOffset.UtcNow);

        OrderBookMergeEngine engine = CreateEngine(staleThresholdMs: 5000, timeProvider: time);

        engine.ApplySnapshot(MakeSnapshot(
            bids: [(0.06m, 2m)], asks: [(0.07m, 1m)],
            timestamp: time.GetUtcNow()));
        engine.ApplySnapshot(MakeSnapshot(Exchange.Bitstamp,
            bids: [(0.05m, 3m)], asks: [(0.08m, 4m)],
            timestamp: time.GetUtcNow()));

        time.Advance(TimeSpan.FromMilliseconds(6000));

        MergedOrderBook book = engine.GetMergedBook();

        AssertEmptyBook(book);
    }

    [Test]
    public void ShouldSort_BidsDesc_AsksAsc()
    {
        OrderBookMergeEngine engine = CreateEngine();

        engine.ApplySnapshot(MakeSnapshot(
            bids: [(0.04m, 1m), (0.06m, 2m), (0.05m, 3m)],
            asks: [(0.09m, 1m), (0.07m, 2m), (0.08m, 3m)]));

        MergedOrderBook book = engine.GetMergedBook();

        Assert.Multiple(() =>
        {
            Assert.That(book.Bids, Has.Count.EqualTo(3));
            Assert.That(book.Bids[0].Price, Is.EqualTo(0.06m));
            Assert.That(book.Bids[1].Price, Is.EqualTo(0.05m));
            Assert.That(book.Bids[2].Price, Is.EqualTo(0.04m));

            Assert.That(book.Asks, Has.Count.EqualTo(3));
            Assert.That(book.Asks[0].Price, Is.EqualTo(0.07m));
            Assert.That(book.Asks[1].Price, Is.EqualTo(0.08m));
            Assert.That(book.Asks[2].Price, Is.EqualTo(0.09m));
        });
    }

    [Test]
    public void ShouldTakeTop_FromBothExchanges_WithNoMatchingPrices()
    {
        OrderBookMergeEngine engine = CreateEngine();

        engine.ApplySnapshot(MakeSnapshot(
            bids: [
                (0.01m, 2m),
                (0.02m, 2m),
                (0.03m, 2m)
            ],
            asks: [
                (0.015m, 1m),
                (0.025m, 1m),
                (0.035m, 1m)
            ]));

        engine.ApplySnapshot(MakeSnapshot(Exchange.Bitstamp,
            bids: [
                (0.015m, 11m),
                (0.025m, 12m),
                (0.035m, 13m)
            ],
            asks: [
                (0.01m, 21m),
                (0.02m, 22m),
                (0.03m, 23m)
            ]));

        MergedOrderBook book = engine.GetMergedBook();

        Assert.Multiple(() =>
        {
            Assert.That(book.Bids, Has.Count.EqualTo(3));
            Assert.That(book.Bids[0], Is.EqualTo(new PriceLevel(0.035m, 13m)));
            Assert.That(book.Bids[1], Is.EqualTo(new PriceLevel(0.03m, 2m)));
            Assert.That(book.Bids[2], Is.EqualTo(new PriceLevel(0.025m, 12m)));

            Assert.That(book.Asks, Has.Count.EqualTo(3));
            Assert.That(book.Asks[0], Is.EqualTo(new PriceLevel(0.01m, 21m)));
            Assert.That(book.Asks[1], Is.EqualTo(new PriceLevel(0.015m, 1m)));
            Assert.That(book.Asks[2], Is.EqualTo(new PriceLevel(0.02m, 22m)));
        });
    }

    [Test]
    public void ShouldTakeTop_FromBothExchanges_WithMatchingPricesSummed()
    {
        OrderBookMergeEngine engine = CreateEngine();

        engine.ApplySnapshot(MakeSnapshot(
            bids: [
                (0.01m, 2m),
                (0.02m, 2m),
                (0.03m, 2m)
            ],
            asks: [
                (0.01m, 1m),
                (0.025m, 1m),
                (0.03m, 1m)
            ]));

        engine.ApplySnapshot(MakeSnapshot(Exchange.Bitstamp,
            bids: [
                (0.015m, 11m),
                (0.02m, 12m),
                (0.03m, 13m)
            ],
            asks: [
                (0.01m, 21m),
                (0.02m, 22m),
                (0.03m, 23m)
            ]));

        MergedOrderBook book = engine.GetMergedBook();

        Assert.Multiple(() =>
        {
            Assert.That(book.Bids, Has.Count.EqualTo(3));
            Assert.That(book.Bids[0], Is.EqualTo(new PriceLevel(0.03m, 15m)));
            Assert.That(book.Bids[1], Is.EqualTo(new PriceLevel(0.02m, 14m)));
            Assert.That(book.Bids[2], Is.EqualTo(new PriceLevel(0.015m, 11m)));

            Assert.That(book.Asks, Has.Count.EqualTo(3));
            Assert.That(book.Asks[0], Is.EqualTo(new PriceLevel(0.01m, 22m)));
            Assert.That(book.Asks[1], Is.EqualTo(new PriceLevel(0.02m, 22m)));
            Assert.That(book.Asks[2], Is.EqualTo(new PriceLevel(0.025m, 1m)));
        });
    }

    [Test]
    public void ShouldReplace_When_SnapshotFromSameExchange()
    {
        OrderBookMergeEngine engine = CreateEngine();

        engine.ApplySnapshot(MakeSnapshot(
            bids: [
                (0.01m, 2m),
                (0.02m, 2m),
                (0.03m, 2m)
            ],
            asks: [
                (0.01m, 1m),
                (0.025m, 1m),
                (0.03m, 1m)
            ]));

        engine.ApplySnapshot(MakeSnapshot(
            bids: [
                (0.015m, 11m),
                (0.02m, 12m),
                (0.03m, 13m)
            ],
            asks: [
                (0.01m, 21m),
                (0.02m, 22m),
                (0.03m, 23m)
            ]));

        MergedOrderBook book = engine.GetMergedBook();

        Assert.Multiple(() =>
        {
            Assert.That(book.Bids, Has.Count.EqualTo(3));
            Assert.That(book.Bids[0], Is.EqualTo(new PriceLevel(0.03m, 13m)));
            Assert.That(book.Bids[1], Is.EqualTo(new PriceLevel(0.02m, 12m)));
            Assert.That(book.Bids[2], Is.EqualTo(new PriceLevel(0.015m, 11m)));

            Assert.That(book.Asks, Has.Count.EqualTo(3));
            Assert.That(book.Asks[0], Is.EqualTo(new PriceLevel(0.01m, 21m)));
            Assert.That(book.Asks[1], Is.EqualTo(new PriceLevel(0.02m, 22m)));
            Assert.That(book.Asks[2], Is.EqualTo(new PriceLevel(0.03m, 23m)));
        });
    }

    [Test]
    public void ShouldClearExpiredSnapshot_When_Rebuilt()
    {
        FakeTimeProvider time = new();
        time.SetUtcNow(DateTimeOffset.UtcNow);

        OrderBookMergeEngine engine = CreateEngine(depthLevels: 1, staleThresholdMs: 5000, timeProvider: time);

        engine.ApplySnapshot(MakeSnapshot(
            bids: [(0.05m, 2m)], asks: [(0.07m, 1m)],
            timestamp: time.GetUtcNow()));

        time.Advance(TimeSpan.FromMilliseconds(3000));

        engine.ApplySnapshot(MakeSnapshot(Exchange.Bitstamp,
            bids: [(0.05m, 3m)], asks: [(0.07m, 4m)],
            timestamp: time.GetUtcNow()));

        MergedOrderBook book = engine.GetMergedBook();

        Assert.Multiple(() =>
        {
            Assert.That(book.Bids, Has.Count.EqualTo(1));
            Assert.That(book.Bids[0], Is.EqualTo(new PriceLevel(0.05m, 5m)));

            Assert.That(book.Asks, Has.Count.EqualTo(1));
            Assert.That(book.Asks[0], Is.EqualTo(new PriceLevel(0.07m, 5m)));
        });

        time.Advance(TimeSpan.FromMilliseconds(4000));

        MergedOrderBook freshbook = engine.GetMergedBook();

        Assert.Multiple(() =>
        {
            Assert.That(freshbook.Bids, Has.Count.EqualTo(1));
            Assert.That(freshbook.Bids[0], Is.EqualTo(new PriceLevel(0.05m, 3m)));

            Assert.That(freshbook.Asks, Has.Count.EqualTo(1));
            Assert.That(freshbook.Asks[0], Is.EqualTo(new PriceLevel(0.07m, 4m)));
        });
    }

    private static void AssertEmptyBook(MergedOrderBook book) =>
        Assert.Multiple(() =>
        {
            Assert.That(book.Bids, Is.Empty);
            Assert.That(book.Asks, Is.Empty);
            Assert.That(book.Symbol, Is.EqualTo(DefaultSymbol));
        });

    private static OrderBookMergeEngine CreateEngine(
        int depthLevels = 3,
        int staleThresholdMs = 0,
        FakeTimeProvider? timeProvider = null) =>
        new(DefaultSymbol, depthLevels, TimeSpan.FromMilliseconds(staleThresholdMs),
            timeProvider ?? TimeProvider.System);

    private static OrderBookSnapshot MakeSnapshot(
        Exchange exchange = Exchange.Binance,
        (decimal price, decimal qty)[]? bids = null,
        (decimal price, decimal qty)[]? asks = null,
        DateTimeOffset? timestamp = null)
    {
        List<PriceLevel> bidLevels = bids is not null
            ? bids.Select(b => new PriceLevel(b.price, b.qty)).ToList()
            : [new PriceLevel(0.06m, 10m)];

        List<PriceLevel> askLevels = asks is not null
            ? asks.Select(a => new PriceLevel(a.price, a.qty)).ToList()
            : [new PriceLevel(0.061m, 8m)];

        return new OrderBookSnapshot(bidLevels, askLevels, exchange, timestamp ?? DateTimeOffset.UtcNow);
    }
}
