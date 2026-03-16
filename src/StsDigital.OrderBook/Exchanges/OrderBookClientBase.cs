using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Options;
using StsDigital.OrderBook.Exchanges.Connection;
using StsDigital.OrderBook.Models;
using StsDigital.OrderBook.Services;

namespace StsDigital.OrderBook.Exchanges;

public abstract class OrderBookClientBase(
    IMessageParser parser,
    IOrderBookCollector collector,
    IOptions<ReconnectionOptions> reconnectionOptions) : BackgroundService
{
    private const int ReceiveBufferSize = 8192;

    protected virtual bool IsEnabled() => true;
    protected abstract string GetUri();

    protected abstract Task AfterConnectionAsync(ClientWebSocket socket, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!IsEnabled())
        {
            return;
        }

        await ConnectAsync(cancellationToken);
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RunConnectionAsync(GetUri(), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                await Task.Delay(reconnectionOptions.Value.DelayMs, cancellationToken);
            }
        }
    }

    private async Task RunConnectionAsync(string uri, CancellationToken cancellationToken)
    {
        using ClientWebSocket socket = new();

        await socket.ConnectAsync(new Uri(uri), cancellationToken);
        await AfterConnectionAsync(socket, cancellationToken);

        byte[] buffer = new byte[ReceiveBufferSize];

        while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
        {
            string? json = await ReceiveMessageAsync(socket, buffer, cancellationToken);
            if (json is null)
            {
                break;
            }

            ParseResult result = parser.Parse(json);
            switch (result)
            {
                case ParseResult.Snapshot snapshot:
                    collector.Collect(snapshot.Value);
                    break;

                case ParseResult.ExchangeReconnectRequested:
                    return;

                case ParseResult.Ignored:
                    break;
            }
        }
    }

    private static async Task<string?> ReceiveMessageAsync(
        ClientWebSocket socket,
        byte[] buffer,
        CancellationToken ct)
    {
        using MemoryStream message = new();

        while (true)
        {
            WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, ct);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            message.Write(buffer, 0, result.Count);

            if (result.EndOfMessage)
            {
                return Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length);
            }
        }
    }
}
