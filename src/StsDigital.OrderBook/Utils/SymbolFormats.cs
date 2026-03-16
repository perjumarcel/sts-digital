namespace StsDigital.OrderBook.Utils;

public static class SymbolFormats
{
    public static string ToCanonical(string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        int slashIndex = symbol.IndexOf('/');
        if (slashIndex < 1 || slashIndex == symbol.Length - 1)
        {
            throw new FormatException($"'{symbol}' is not in BASE/QUOTE format (e.g. ETH/BTC).");
        }

        string baseSymbol = symbol[..slashIndex].Trim().ToUpperInvariant();
        string quoteSymbol = symbol[(slashIndex + 1)..].Trim().ToUpperInvariant();
        return $"{baseSymbol}/{quoteSymbol}";
    }

    public static string ToNormalised(string symbol)
        => ToCanonical(symbol).Replace("/", "", StringComparison.Ordinal).ToLowerInvariant();
}
