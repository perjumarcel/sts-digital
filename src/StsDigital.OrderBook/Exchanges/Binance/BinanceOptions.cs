using System.ComponentModel.DataAnnotations;

namespace StsDigital.OrderBook.Exchanges.Binance;

public sealed class BinanceOptions : IValidatableObject
{
    public const string SectionName = "Exchanges:Binance";

    public bool Enabled { get; set; } = true;

    [Required(AllowEmptyStrings = false)]
    public string WebSocketUrl { get; set; } = "wss://stream.binance.com:9443/stream";

    [Required(AllowEmptyStrings = false)] public string UpdateSpeed { get; set; } = "100ms";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UpdateSpeed.ToLowerInvariant() is not ("100ms" or "1000ms"))
        {
            yield return new ValidationResult(
                "UpdateSpeed must be 100ms or 1000ms.",
                [nameof(UpdateSpeed)]);
        }
    }
}
