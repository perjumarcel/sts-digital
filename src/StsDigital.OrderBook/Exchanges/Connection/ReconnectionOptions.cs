namespace StsDigital.OrderBook.Exchanges.Connection;

public sealed class ReconnectionOptions
{
    public const string SectionName = "Reconnection";

    public int DelayMs { get; set; } = 3000;
}
