namespace StsDigital.OrderBook.Models;

public abstract record ParseResult
{
    private ParseResult()
    {
    }

    public static ParseResult Skip { get; } = new Ignored();
    public static ParseResult ReconnectRequestedByExchange { get; } = new ExchangeReconnectRequested();

    public sealed record Snapshot(OrderBookSnapshot Value) : ParseResult;

    public sealed record Ignored : ParseResult;

    public sealed record ExchangeReconnectRequested : ParseResult;
}
