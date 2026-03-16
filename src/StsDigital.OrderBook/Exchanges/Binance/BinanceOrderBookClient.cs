using System.Net.WebSockets;
using Microsoft.Extensions.Options;
using StsDigital.OrderBook.Exchanges.Connection;
using StsDigital.OrderBook.Models;
using StsDigital.OrderBook.Services;
using StsDigital.OrderBook.Utils;

namespace StsDigital.OrderBook.Exchanges.Binance;

public sealed class BinanceOrderBookClient(
    IOptions<BinanceOptions> binanceOptions,
    IOptions<OrderBookOptions> orderBookOptions,
    IOptions<ReconnectionOptions> reconnectionOptions,
    IOrderBookCollector collector)
    : OrderBookClientBase(new BinanceMessageParser(), collector, reconnectionOptions)
{
    protected override bool IsEnabled() => binanceOptions.Value.Enabled;

    protected override string GetUri()
    {
        string streamName =
            $"{SymbolFormats.ToNormalised(orderBookOptions.Value.Symbol)}@depth10@{binanceOptions.Value.UpdateSpeed}";
        return $"{binanceOptions.Value.WebSocketUrl}?streams={streamName}";
    }

    protected override Task AfterConnectionAsync(ClientWebSocket socket, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
