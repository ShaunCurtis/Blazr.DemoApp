/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

public sealed class WeatherForecastItemRequestServerHandler<TDbContext>
    : IItemRequestHandler<WeatherForecast>
    where TDbContext : DbContext
{
    private readonly IDbContextFactory<TDbContext> _factory;

    public WeatherForecastItemRequestServerHandler(IDbContextFactory<TDbContext> factory)
    {
        _factory = factory;
    }

    public async ValueTask<ItemQueryResult<WeatherForecast>> ExecuteAsync(ItemQueryRequest request)
    {
        using var dbContext = _factory.CreateDbContext();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var record = await dbContext.Set<WeatherForecast>().SingleOrDefaultAsync(item => item.WeatherForecastId == request.Key, request.Cancellation ?? new());

        if (record is null)
            return ItemQueryResult<WeatherForecast>.Failure($"No record retrieved with the Key provided");

        return ItemQueryResult<WeatherForecast>.Success(record);
    }
}
