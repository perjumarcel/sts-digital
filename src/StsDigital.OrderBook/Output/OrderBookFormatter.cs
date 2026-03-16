using System.Globalization;
using System.Text;
using StsDigital.OrderBook.Models;

namespace StsDigital.OrderBook.Output;

public sealed class OrderBookFormatter
{
    private const string DecimalFormat = "F8";

    public string Format(MergedOrderBook book)
    {
        StringBuilder sb = new();

        sb.AppendLine($"Order Book({book.Symbol})");
        sb.AppendLine($"Updated: {book.Timestamp:HH:mm:ss.fff}");

        if (book.Bids.Count == 0 && book.Asks.Count == 0)
        {
            sb.AppendLine();
            sb.AppendLine("Waiting for data from exchanges...");
            sb.AppendLine();
            return sb.ToString();
        }

        sb.AppendLine();

        sb.AppendLine(string.Format("  {0,-4} {1,14} {2,14}  │  {3,14} {4,14}",
            "#", "Bid Qty", "Bid Price", "Ask Price", "Ask Qty"));

        sb.AppendLine(string.Format("  {0,-4} {1,14} {2,14}  │  {3,14} {4,14}",
            "─", "──────────────", "──────────────", "──────────────", "──────────────"));

        int maxRows = Math.Max(book.Bids.Count, book.Asks.Count);

        for (int i = 0; i < maxRows; i++)
        {
            string bidQty = i < book.Bids.Count
                ? book.Bids[i].Quantity.ToString(DecimalFormat, CultureInfo.InvariantCulture)
                : "";
            string bidPrice = i < book.Bids.Count
                ? book.Bids[i].Price.ToString(DecimalFormat, CultureInfo.InvariantCulture)
                : "";
            string askPrice = i < book.Asks.Count
                ? book.Asks[i].Price.ToString(DecimalFormat, CultureInfo.InvariantCulture)
                : "";
            string askQty = i < book.Asks.Count
                ? book.Asks[i].Quantity.ToString(DecimalFormat, CultureInfo.InvariantCulture)
                : "";

            sb.AppendLine(string.Format("  {0,-4} {1,14} {2,14}  │  {3,14} {4,14}",
                i + 1, bidQty, bidPrice, askPrice, askQty));
        }

        return sb.ToString();
    }
}
