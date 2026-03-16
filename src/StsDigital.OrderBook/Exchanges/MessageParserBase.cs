using System.Globalization;
using System.Text.Json;
using StsDigital.OrderBook.Models;

namespace StsDigital.OrderBook.Exchanges;

public abstract class MessageParserBase : IMessageParser
{
    public ParseResult Parse(string json)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            return Parse(root);
        }
        catch (Exception)
        {
            return ParseResult.Skip;
        }
    }

    protected abstract ParseResult Parse(JsonElement root);

    protected ParseResult.Snapshot CreateSnapshot(
        JsonElement data,
        Exchange exchange,
        DateTimeOffset timestamp,
        int maxLevels = int.MaxValue)
    {
        PriceLevel[] bids = ParseLevels(data.GetProperty("bids"), maxLevels);
        PriceLevel[] asks = ParseLevels(data.GetProperty("asks"), maxLevels);

        return new ParseResult.Snapshot(
            new OrderBookSnapshot(bids, asks, exchange, timestamp));
    }

    private PriceLevel[] ParseLevels(JsonElement array, int maxLevels = int.MaxValue) =>
        (from entry in array.EnumerateArray().Take(maxLevels)
            let price = decimal.Parse(entry[0].GetString()!, CultureInfo.InvariantCulture)
            let qty = decimal.Parse(entry[1].GetString()!, CultureInfo.InvariantCulture)
            select new PriceLevel(price, qty)).ToArray();
}
