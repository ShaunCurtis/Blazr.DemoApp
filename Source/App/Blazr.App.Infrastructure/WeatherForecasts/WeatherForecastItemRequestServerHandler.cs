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
        // Gets a transactional DbContext from the factory - this will be scoped to the lifetime of this method
        using var dbContext = _factory.CreateDbContext();

        // Turns off change tracking - there's no mutation occuring 
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        // Gets the record asynchronously from the EF context
        var record = await dbContext.Set<WeatherForecast>().SingleOrDefaultAsync(item => item.WeatherForecastId == request.Key, request.Cancellation ?? new());

        // Returns a failure result if there's no record returned
        if (record is null)
            return ItemQueryResult<WeatherForecast>.Failure($"No record retrieved with the Key provided");

        // Returns the record in a ItemQueryResult
        return ItemQueryResult<WeatherForecast>.Success(record);
    }
}
