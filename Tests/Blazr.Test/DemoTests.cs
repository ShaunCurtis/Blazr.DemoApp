/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazr.App.Core;
using Blazr.App.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazr.Test;

public class DemoTests
{
    private TestDataProvider _testDataProvider;

    public DemoTests()
        => _testDataProvider = TestDataProvider.Instance();

    private ServiceProvider GetServiceProvider()
    {
        // Creates a new root level DI Services container
        var services = new ServiceCollection();

        // Adds the debug logger
        //services.AddLogging(builder => builder.AddDebug());

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


    //[Theory]
    //[InlineData(0, 10)]
    //[InlineData(0, 50)]
    //[InlineData(5, 10)]
    //public async Task GetForecastList(int startIndex, int pageSize)
    //{
    //    var provider = GetServiceProvider();
    //    var broker = provider.GetService<IDataBroker>()!;

    //    // Get the total count and the first paged item from the test provider
    //    var testCount = _testDataProvider.WeatherForecasts.Count();
    //    var testFirstItem = DboWeatherForecastMap.Map(_testDataProvider.WeatherForecasts.Skip(startIndex).First());

    //    // Build the ListQueryRequest
    //    var request = new ListQueryRequest { PageSize = pageSize, StartIndex = startIndex };
    //    var loadResult = await broker.ExecuteQueryAsync<DmoWeatherForecast>(request);

    //    // Test we have success
    //    Assert.True(loadResult.Successful);
    //    // Total number of items
    //    Assert.Equal(testCount, loadResult.TotalCount);
    //    // The correct page size
    //    Assert.Equal(pageSize, loadResult.Items.Count());
    //    // The correct first item in the pages list
    //    Assert.Equal(testFirstItem, loadResult.Items.First());
    //}

    //[Fact]
    //public async Task GetAFilteredForecastList()
    //{
    //    var provider = GetServiceProvider();
    //    var broker = provider.GetService<IDataBroker>()!;

    //    var pageSize = 2;
    //    var testSummary = "Warm";
    //    var testQuery = _testDataProvider.WeatherForecasts.Where(item => testSummary.Equals(item.Summary, StringComparison.CurrentCultureIgnoreCase));

    //    var testCount = testQuery.Count();
    //    var testFirstItem = DboWeatherForecastMap.Map(testQuery.First());

    //    var filterDefinition = new FilterDefinition(AppDictionary.WeatherForecast.WeatherForecastFilterBySummarySpecification, "Warm");
    //    var filters = new List<FilterDefinition>() { filterDefinition };
    //    var request = new ListQueryRequest { PageSize = pageSize, StartIndex = 0, Filters = filters };

    //    var loadResult = await broker.ExecuteQueryAsync<DmoWeatherForecast>(request);

    //    Assert.True(loadResult.Successful);
    //    Assert.Equal(testCount, loadResult.TotalCount);
    //    Assert.Equal(pageSize, loadResult.Items.Count());
    //    Assert.Equal(testFirstItem, loadResult.Items.First());
    //}

    //[Fact]
    //public async Task GetASortedForecastList()
    //{
    //    var provider = GetServiceProvider();
    //    var broker = provider.GetService<IDataBroker>()!;

    //    var testCount = _testDataProvider.WeatherForecasts.Count();
    //    var testFirstItem = DboWeatherForecastMap.Map(_testDataProvider.WeatherForecasts.Last());

    //    SortDefinition sort = new("Date", true);
    //    var sortList = new List<SortDefinition>() { sort };

    //    var request = new ListQueryRequest { PageSize = 10000, StartIndex = 0, Sorters = sortList };
    //    var loadResult = await broker.ExecuteQueryAsync<DmoWeatherForecast>(request);
    //    Assert.True(loadResult.Successful);

    //    Assert.Equal(testFirstItem, loadResult.Items.First());

    //    sort = new("Date", false);
    //    sortList = new List<SortDefinition>() { sort };

    //    request = new ListQueryRequest { PageSize = 100000, StartIndex = 0, Sorters = sortList };
    //    loadResult = await broker.ExecuteQueryAsync<DmoWeatherForecast>(request);
    //    Assert.True(loadResult.Successful);

    //    Assert.Equal(testFirstItem, loadResult.Items.Last());
    //}

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

    //[Fact]
    //public async Task DeleteAForecast()
    //{
    //    // Get a fully stocked DI container
    //    var provider = GetServiceProvider();
    //    var broker = provider.GetService<IDataBroker>()!;

    //    var testDboItem = _testDataProvider.WeatherForecasts.First();
    //    var testUid = new WeatherForecastId(testDboItem.WeatherForecastID);

    //    var testItem = DboWeatherForecastMap.Map(testDboItem);

    //    var command = new CommandRequest<DmoWeatherForecast>(testItem, CommandState.Delete);
    //    var commandResult = await broker.ExecuteCommandAsync<DmoWeatherForecast>(command);
    //    Assert.True(commandResult.Successful);

    //    var testCount = _testDataProvider.WeatherForecasts.Count() - 1;

    //    var queryRequest = new ListQueryRequest { PageSize = 10, StartIndex = 0 };
    //    var queryResult = await broker.ExecuteQueryAsync<DmoWeatherForecast>(queryRequest);
    //    Assert.True(queryResult.Successful);

    //    Assert.Equal(testCount, queryResult.TotalCount);
    //}

    //[Fact]
    //public async Task AddAForecast()
    //{
    //    // Get a fully stocked DI container
    //    var provider = GetServiceProvider();
    //    var broker = provider.GetService<IDataBroker>()!;

    //    var newItemGuid = Guid.NewGuid();
    //    var newItem = new DmoWeatherForecast { WeatherForecastId = new(newItemGuid), Date = DateOnly.FromDateTime(DateTime.Now), Summary = "Testing", Temperature = new(30) };

    //    var command = new CommandRequest<DmoWeatherForecast>(newItem, CommandState.Add);
    //    var commandResult = await broker.ExecuteCommandAsync<DmoWeatherForecast>(command);
    //    Assert.True(commandResult.Successful);

    //    var request = ItemQueryRequest<WeatherForecastId>.Create(newItem.WeatherForecastId);
    //    var loadResult = await broker.ExecuteQueryAsync<DmoWeatherForecast, WeatherForecastId>(request);
    //    Assert.True(loadResult.Successful);

    //    var dbNewItem = loadResult.Item!;

    //    Assert.Equal(newItem, dbNewItem);

    //    var testCount = _testDataProvider.WeatherForecasts.Count() + 1;

    //    var queryRequest = new ListQueryRequest { PageSize = 10, StartIndex = 0 };
    //    var queryResult = await broker.ExecuteQueryAsync<DmoWeatherForecast>(queryRequest);
    //    Assert.True(queryResult.Successful);

    //    Assert.Equal(testCount, queryResult.TotalCount);
    //}
}