namespace StsDigital.OrderBook.Exchanges.Bitstamp;

public sealed class BitstampOptions
{
    public const string SectionName = "Exchanges:Bitstamp";

    public bool Enabled { get; set; } = true;

    public string WebSocketUrl { get; set; } = "wss://ws.bitstamp.net";
}
