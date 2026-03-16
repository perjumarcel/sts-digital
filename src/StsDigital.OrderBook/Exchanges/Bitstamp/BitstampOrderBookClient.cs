using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StsDigital.OrderBook.Exchanges.Connection;
using StsDigital.OrderBook.Models;
using StsDigital.OrderBook.Services;
using StsDigital.OrderBook.Utils;

namespace StsDigital.OrderBook.Exchanges.Bitstamp;

public sealed class BitstampOrderBookClient(
    IOptions<BitstampOptions> bitstampOptions,
    IOptions<OrderBookOptions> orderBookOptions,
    IOptions<ReconnectionOptions> reconnectionOptions,
    IOrderBookCollector collector)
    : OrderBookClientBase(new BitstampMessageParser(orderBookOptions.Value.DepthLevels), collector,
        reconnectionOptions)
{
    private const string ChannelPrefix = "order_book";

    protected override string GetUri() => bitstampOptions.Value.WebSocketUrl;

    protected override bool IsEnabled() => bitstampOptions.Value.Enabled;

    protected override async Task AfterConnectionAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        string symbol = SymbolFormats.ToNormalised(orderBookOptions.Value.Symbol);
        string channel = $"{ChannelPrefix}_{symbol}";

        string payload = JsonSerializer.Serialize(new { @event = "bts:subscribe", data = new { channel } });

        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }
}
