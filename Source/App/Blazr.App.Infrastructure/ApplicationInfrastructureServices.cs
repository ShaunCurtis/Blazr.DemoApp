/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

public static class ApplicationInfrastructureServices
{
    /// <summary>
    /// Adds the server side Infrastructure services
    /// and generic handlers
    /// </summary>
    /// <param name="services"></param>
    public static void AddAppServerInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContextFactory<InMemoryTestDbContext>(options
            => options.UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid().ToString()}"));

        services.AddScoped<IDataBroker, DataBroker>();

        // Add the standard handlers
        services.AddScoped<IListRequestHandler, ListRequestServerHandler<InMemoryTestDbContext>>();
        services.AddScoped<IItemRequestHandler, ItemRequestServerHandler<InMemoryTestDbContext>>();
        services.AddScoped<ICommandHandler, CommandServerHandler<InMemoryTestDbContext>>();

        // Add any individual entity services
        services.AddWeatherForecastServerInfrastructureServices();
    }

    /// <summary>
    /// Adds specific WeatherForecast API call implementations
    /// </summary>
    /// <param name="services"></param>
    /// <param name="baseHostEnvironmentAddress"></param>
    public static void AddWeatherForecastServerInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IItemRequestHandler<WeatherForecast>, WeatherForecastItemRequestServerHandler<InMemoryTestDbContext>>();
        services.AddScoped<INewRecordProvider<WeatherForecast>, NewWeatherForecastProvider>();
    }

    /// <summary>
    ///  Adds the test data to the in-memory DB context
    /// </summary>
    /// <param name="provider"></param>
    public static void AddWeatherTestData(this IServiceProvider provider)
    {
        var factory = provider.GetService<IDbContextFactory<InMemoryTestDbContext>>();

        if (factory is not null)
            TestDataProvider.Instance().LoadDbContext<InMemoryTestDbContext>(factory);
    }
}
