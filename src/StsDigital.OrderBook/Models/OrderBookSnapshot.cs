namespace StsDigital.OrderBook.Models;

public sealed record OrderBookSnapshot(
    IReadOnlyList<PriceLevel> Bids,
    IReadOnlyList<PriceLevel> Asks,
    Exchange Exchange,
    DateTimeOffset Timestamp);
