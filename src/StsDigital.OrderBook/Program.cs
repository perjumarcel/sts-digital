using StsDigital.OrderBook.Exchanges.Binance;
using StsDigital.OrderBook.Exchanges.Bitstamp;
using StsDigital.OrderBook.Exchanges.Connection;
using StsDigital.OrderBook.Models;
using StsDigital.OrderBook.Output;
using StsDigital.OrderBook.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<OrderBookOptions>()
    .BindConfiguration(OrderBookOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<OrderBookAggregator>();
builder.Services.AddSingleton<IOrderBookCollector>(sp => sp.GetRequiredService<OrderBookAggregator>());
builder.Services.AddSingleton<IOrderBookReader>(sp => sp.GetRequiredService<OrderBookAggregator>());

builder.Services.AddOptions<ReconnectionOptions>().BindConfiguration(ReconnectionOptions.SectionName);

builder.Services.AddOptions<BinanceOptions>()
    .BindConfiguration(BinanceOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHostedService<BinanceOrderBookClient>();
builder.Services.AddOptions<BitstampOptions>().BindConfiguration(BitstampOptions.SectionName);
builder.Services.AddHostedService<BitstampOrderBookClient>();

builder.Services.AddOptions<ConsoleOutputOptions>().BindConfiguration(ConsoleOutputOptions.SectionName);
builder.Services.AddSingleton<OrderBookFormatter>();
builder.Services.AddHostedService<ConsoleOutputService>();

var app = builder.Build();

app.MapGet("/orderbook", (IOrderBookReader reader) =>
{
    MergedOrderBook book = reader.GetMergedBook();
    return Results.Ok(book);
});

app.Run();
