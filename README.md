# sts-digital order book
Merged order book (Binance | Bitstamp)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Usage

```bash
dotnet build StsDigital.OrderBook.slnx
dotnet run --project src/StsDigital.OrderBook/StsDigital.OrderBook.csproj

# override the default symbol (BTC/USDT)
dotnet run --project src/StsDigital.OrderBook/StsDigital.OrderBook.csproj -- --OrderBook:Symbol="ETH/BTC"

# query the HTTP endpoint (project should be running)
curl http://localhost:5107/orderbook

dotnet test StsDigital.OrderBook.slnx
```

## Thoughts and decisions made for the demo

* Console output was decided to be done on timer refresh. It was also considered using event based mechanism which would trigger the console to update only when necessary, but that would be much more complex for the purpose of this exercise. Would also require adding a throttling mechanism in case of too frequent events, adding also a way to trigger the console if there are no new fresh events from either exhange to prevent displaying stale data in console, etc. So the decision was to use just a simple refresh timer, which although comes with its own drowbacks, like refreshing unnecessarily without any new changes, it solves the issue with evicting the stale entries when no new messages are incoming.
* As a deliberate decision the processing of each exchange stream is done in separate background service in case one fails/stops, it doesn't affect the other. It is also easy to add a new exchange client, by just registering the new background service which would follow the same approach on pushing the snapshot to the IOrderBookCollector.
* `OrderBookAggregator` has 2 interfaces specifically for ease of introducing a different, more sophisticated approach later, maybe event driven, so that the read/write is separate and can be replaced independently.
* Reconnection is just a fixed delay with infinite retries. Not the best, but just to highlight the reconnection mechanism. A proper reconnection policy should probably have an exponential backoff, should add jiter, max number of retries, etc.
* Validation was added for options, for example for depth levels BINANCE only supports 5, 10, 20. That limitation was applied for entire order book as well as display in console. Should be applied only to BINANCE options in production app, but here this was a deliverate decision to simplify the demo.
* A decision was made to add a stale snapshot eviction mechanism to prevent displaying old data from an exchange in case that exchange client goes down.
* For tests the decision was to cover the most important/critical part `OrderBookMergeEngine`. Ideally it should cover much more, but if to choose to limit to the critical part, this would be it.
* `Bitstamp` has a special event when a connection will be closed, to inform the client to reconnect that was added in consideration as well.
* Used the `.editorConfig` from a dotnet public repo to apply the project configuration/formating/styling :) Did not commit it though.

### Known limitations & issues

* `MessageParser` ignores channel/stream validation for confirmation that it is reading for the right symbol. Would required that for multiple symbols support.
* Console output service does a clear which creates flickering effect during runtime. While for real use case that would be really frustrating and annoying, for the purpose of the demo it was left as it is.
* There is no throttling mechanism in place to guard `OrderBookAggregator`, with just 2 exchanges and only depth level of 10, that is not currently an issue, but it would become one when the number of exchanges or/and depth level increased and if the frequency of events are increased.
* There is no delayed connections to the exchanges both happening almost simultaniously, with more exchanges that could become a hammering problem, especially if the frequency of incoming messages(100ms for ex.) will be the same. Same issue on multiple exchanges reconnection.
* `Bitstamp` returns always 100 entries, so the truncation happens after message is received. But it is done before creating the snapshot itself to prevent storing unnecessary data in the merge engine.
* There should be a validation that the stale eviction timeout is not less then the frequency of the messages received(from the slowes exchange), as there would be a high risk of snapshot eviction from a slower exchange client.
* There is no real validation for the provided symbol, just for its format. Also there should be validation for supported symbols per exchange, as some exchanges may not provide information for certain symbols.
* Missing some other important tests coverage for message parser and websocket connection/reconnection client base.
* The timestamp for the snapshot is generated at the time of the message parsing to have a unified approach accross all exchanges, even though Bitstamp provides a timestamp which could be used.

There were of course other considerations during this exercise, but I hope sharing some of them would help.
