using System.Text.Json;
using StsDigital.OrderBook.Models;

namespace StsDigital.OrderBook.Exchanges.Binance;

public sealed class BinanceMessageParser : MessageParserBase
{
    protected override ParseResult Parse(JsonElement root)
    {
        if (!root.TryGetProperty("data", out JsonElement data))
        {
            return ParseResult.Skip;
        }

        return CreateSnapshot(
            data,
            Exchange.Binance,
            DateTimeOffset.UtcNow);
    }
}
