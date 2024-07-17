/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Infrastructure;

public sealed class TestDataProvider
{
    public IEnumerable<WeatherForecast> WeatherForecasts => _weatherForecasts.AsEnumerable();

    private List<WeatherForecast> _weatherForecasts = new List<WeatherForecast>();
    private int _recordCount = 8;

    public TestDataProvider()
    {
        this.Load();
    }

    private void Load()
    {
        var startDate = DateTime.Now;
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        _weatherForecasts = Enumerable.Range(1, _recordCount).Select(index => new WeatherForecast
        {
            WeatherForecastId = Guid.NewGuid(),
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToList();

    }

    public void LoadDbContext<TDbContext>(IDbContextFactory<TDbContext> factory) where TDbContext : DbContext
    {
        using var dbContext = factory.CreateDbContext();

        var dboWeatherForecasts = dbContext.Set<WeatherForecast>();

        // Check if we already have a full data set
        // If not clear down any existing data and start again

        if (dboWeatherForecasts.Count() == 0)
            dbContext.AddRange(_weatherForecasts);

        dbContext.SaveChanges();
    }

    private static TestDataProvider? _provider;

    public static TestDataProvider Instance()
    {
        if (_provider is null)
            _provider = new TestDataProvider();

        return _provider;
    }
}