using System.ComponentModel.DataAnnotations;

namespace StsDigital.OrderBook.Models;

public sealed class OrderBookOptions : IValidatableObject
{
    public const string SectionName = "OrderBook";

    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"^[A-Za-z0-9]+/[A-Za-z0-9]+$",
        ErrorMessage = "Symbol must be in BASE/QUOTE format")]
    public string Symbol { get; set; } = "";

    public int DepthLevels { get; set; } = 10;

    [Range(1000, 60000)] public int StaleThresholdMs { get; set; } = 5000;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DepthLevels is not (5 or 10 or 20))
        {
            yield return new ValidationResult(
                "DepthLevels must be: 5, 10 or 20.",
                [nameof(DepthLevels)]);
        }
    }
}
