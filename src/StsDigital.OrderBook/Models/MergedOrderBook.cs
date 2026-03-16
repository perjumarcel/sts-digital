namespace StsDigital.OrderBook.Models;

public sealed record MergedOrderBook(
    IReadOnlyList<PriceLevel> Bids,
    IReadOnlyList<PriceLevel> Asks,
    string Symbol,
    DateTimeOffset Timestamp);
