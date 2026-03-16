using StsDigital.OrderBook.Models;

namespace StsDigital.OrderBook.Exchanges;

public interface IMessageParser
{
    ParseResult Parse(string json);
}
