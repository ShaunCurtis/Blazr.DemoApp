/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
using System.ComponentModel.DataAnnotations;

namespace Blazr.App.Core;

public record WeatherForecast : ICommandEntity
{
    [Key] public Guid WeatherForecastId { get; init; } = Guid.Empty;
    public DateTimeOffset Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
    public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);
    public DateOnly DateOnly => DateOnly.FromDateTime(this.Date.DateTime);
}
