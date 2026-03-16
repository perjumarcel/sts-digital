using Microsoft.Extensions.Options;
using StsDigital.OrderBook.Models;
using StsDigital.OrderBook.Services;

namespace StsDigital.OrderBook.Output;

public sealed class ConsoleOutputService(
    IOrderBookReader reader,
    OrderBookFormatter formatter,
    IOptions<ConsoleOutputOptions> consoleOptions,
    ILogger<ConsoleOutputService> logger)
    : BackgroundService
{
    private readonly ConsoleOutputOptions _consoleOptions = consoleOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await RenderLoopAsync(stoppingToken);

    private async Task RenderLoopAsync(CancellationToken ct)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(_consoleOptions.IntervalMs));

        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                MergedOrderBook book = reader.GetMergedBook();

                if (!Console.IsOutputRedirected)
                {
                    Console.Clear();
                }

                Console.Write(formatter.Format(book));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rendering order book");
            }
        }
    }
}
