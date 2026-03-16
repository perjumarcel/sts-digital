namespace StsDigital.OrderBook.Output;

public sealed class ConsoleOutputOptions
{
    public const string SectionName = "ConsoleOutput";

    public int IntervalMs { get; set; } = 100;
}
