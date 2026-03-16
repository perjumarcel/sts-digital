using StsDigital.OrderBook.Models;

namespace StsDigital.OrderBook.Services;

public interface IOrderBookReader
{
    MergedOrderBook GetMergedBook();
}
