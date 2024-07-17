/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
using Blazr.EditStateTracker;

namespace Blazr.App.Presentation;

public sealed class WeatherForecastEditContext
{
    public WeatherForecast BaseRecord { get; }

    [TrackState] public string? Summary { get; set; }
    [TrackState] public int Temperature { get; set; }
    [TrackState] public DateTimeOffset? Date { get; set; }

    public Guid Id => this.BaseRecord.WeatherForecastId;
    public bool IsDirty => this.BaseRecord != this.AsRecord;

    public WeatherForecast AsRecord =>
        this.BaseRecord with
        {
            Date = this.Date ?? DateTime.Now,
            Summary = this.Summary,
            TemperatureC = this.Temperature
        };

    public WeatherForecastEditContext(WeatherForecast record)
    {
        this.BaseRecord = record;
        this.Load(record);
    }

    public void Load(WeatherForecast record)
    {
        this.Summary = record.Summary;
        this.Temperature = record.TemperatureC;
        this.Date = record.Date;
    }

    public void Reset()
        => this.Load(this.BaseRecord);
}
