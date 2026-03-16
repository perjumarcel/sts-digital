using System.Text.Json;
using StsDigital.OrderBook.Models;

namespace StsDigital.OrderBook.Exchanges.Bitstamp;

public sealed class BitstampMessageParser(int maxLevels = int.MaxValue) : MessageParserBase
{
    private const string EventReconnect = "bts:request_reconnect";
    private const string EventData = "data";

    protected override ParseResult Parse(JsonElement root)
    {
        string? eventType = root.GetProperty("event").GetString();
        if (eventType == EventReconnect)
        {
            return ParseResult.ReconnectRequestedByExchange;
        }

        if (eventType != EventData)
        {
            return ParseResult.Skip;
        }

        JsonElement data = root.GetProperty("data");

        return CreateSnapshot(
            data,
            Exchange.Bitstamp,
            DateTimeOffset.UtcNow,
            maxLevels);
    }
}
