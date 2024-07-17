/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazr.App.Presentation.QuickGrid;
using Microsoft.Extensions.DependencyInjection;

namespace Blazr.App.Presentation;

public static class ApplicationPresentationServices
{
    public static void AddAppBootstrapPresentationServices(this IServiceCollection services)
    {
        AddWeatherForecastBootstrapServices(services);
    }

    private static void AddWeatherForecastBootstrapServices(IServiceCollection services)
    {
        services.AddTransient<IQuickGridPresenter<WeatherForecast>, QuickGridPresenter<WeatherForecast>>();
        services.AddTransient<IViewPresenter<WeatherForecast>, ViewPresenter<WeatherForecast>>();
        services.AddTransient<WeatherForecastEditPresenter>();
    }
}
