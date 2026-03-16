using StsDigital.OrderBook.Models;

namespace StsDigital.OrderBook.Services;

public interface IOrderBookCollector
{
    void Collect(OrderBookSnapshot snapshot);
}
