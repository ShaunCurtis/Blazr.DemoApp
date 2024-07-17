/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public class NewWeatherForecastProvider : INewRecordProvider<WeatherForecast>
{
    public WeatherForecast NewRecord()
    {
        return new WeatherForecast() { WeatherForecastId = Guid.NewGuid(), Date =DateTime.Now };
    }
}

