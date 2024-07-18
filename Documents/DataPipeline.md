#  Data Pipeline

The data pipeline is based on the CQS pattern.  While you can access the handlers directly, the simplest way to use it is through the `IDataBroker` facade.

```csharp
public interface IDataBroker
{
    public ValueTask<ListQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ListQueryRequest request) 
        where TRecord : class;

    public ValueTask<ItemQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ItemQueryRequest request) 
        where TRecord : class;

    public ValueTask<CommandResult> ExecuteCommandAsync<TRecord>(CommandRequest<TRecord> request) 
        where TRecord : class;
}
```

## Configuring Test Environment

I use XUnit for testing so here's how I set up a test.

This method gets and sets up the test DI environment with an In-Memory database loaded with test data.

```csharp
private ServiceProvider GetServiceProvider()
{
    // Creates a new root level DI Services container
    var services = new ServiceCollection();

    // Add the data pipeline services
    services.AddAppServerInfrastructureServices();

    // creates the DI container
    var provider = services.BuildServiceProvider();

    // get the DbContext factory and add the test data
    var factory = provider.GetService<IDbContextFactory<InMemoryTestDbContext>>();
    if (factory is not null)
        TestDataProvider.Instance().LoadDbContext<InMemoryTestDbContext>(factory);

    return provider!;
}
```

The `AddAppServerInfrastructureServices` extension method adds all the necessary services.

It:

1. Adds the DbContext factory.
1. Adds the data broker.
1. Adds the generic handlers.
1. Adds an specific entity based handlers.

```csharp
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

public static void AddWeatherForecastServerInfrastructureServices(this IServiceCollection services)
{
    services.AddScoped<IItemRequestHandler<WeatherForecast>, WeatherForecastItemRequestServerHandler<InMemoryTestDbContext>>();
    services.AddScoped<INewRecordProvider<WeatherForecast>, NewWeatherForecastProvider>();
}
```

We can now write a test to get a record:

```csharp
[Fact]
public async Task GetAForecast()
{
    // Get a fully stocked DI container
    var provider = GetServiceProvider();

    //Get the data broker
    var broker = provider.GetService<IDataBroker>()!;

    // Get the test item from the Test Provider
    var testItem = _testDataProvider.WeatherForecasts.First();
        
    // Gets the raw Id to retrieve
    var testId = testItem.WeatherForecastId;

    // Builds an item request instance
    var request = ItemQueryRequest.Create(testId);

    // Executes the query against the broker
    var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        
    // checks the query was successful
    Assert.True(loadResult.Successful);
        
    // gets the returned record 
    var dbItem = loadResult.Item;

    // checks it matches the test record
    Assert.Equal(testItem, dbItem);
}
```

The broker passes the call to the generic registered `IListRequestHandler` [`ListRequestServerHandler`].  It finds a entity specific registered handler `WeatherForecastItemRequestServerHandler` for `IItemRequestHandler<WeatherForecast>` which executes the query.  Here it is:

```csharp
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
```

And a test a record update:

```csharp
    [Fact]
    public async Task UpdateAForecast()
    {
        // Get a fully stocked DI container
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        // Get a test item id
        var testItem = _testDataProvider.WeatherForecasts.First();
        var testUid = testItem.WeatherForecastId;

        // Get the test item from the data store
        var request = ItemQueryRequest.Create(testUid);
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);
        var dbItem = loadResult.Item!;

        // Get the total number of records in the data store
        // we want to assert we are not adding or deleting.
        var queryRequest = new ListQueryRequest(0, 10);
        var queryResult = await broker.ExecuteQueryAsync<WeatherForecast>(queryRequest);
        Assert.True(queryResult.Successful);
        var initialCount = queryResult.TotalCount;

        //Mutate the returned record.  In this case create a new record with an updated temperature
        var newItem = dbItem with { TemperatureC =dbItem.TemperatureC + 10 };

        // Create a CommandRequest and execute it against the data store
        var command = new CommandRequest<WeatherForecast>(newItem, CommandState.Update);
        var commandResult = await broker.ExecuteCommandAsync<WeatherForecast>(command);
        Assert.True(commandResult.Successful);

        // Get the update record from the data store
        var updateRequest = ItemQueryRequest.Create(testUid);
        loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(updateRequest);
        Assert.True(loadResult.Successful);
        var dbNewItem = loadResult.Item!;

        // Check the record has been updated
        Assert.Equal(newItem, dbNewItem);

        // Get the total count after the test and chack it hasn't changed
        var afterQueryRequest = new ListQueryRequest(0,10);
        var afterQueryResult = await broker.ExecuteQueryAsync<WeatherForecast>(afterQueryRequest);
        Assert.True(queryResult.Successful);
        Assert.Equal(initialCount, afterQueryResult.TotalCount);
    }
```


