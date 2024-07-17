/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
using Microsoft.AspNetCore.Components.QuickGrid;

namespace Blazr.App.Presentation.QuickGrid;

public class QuickGridPresenter<TRecord> : IQuickGridPresenter<TRecord>
    where TRecord : class, new()
{
    private readonly IDataBroker _dataBroker;
    public IDataResult LastDataResult { get; private set; } = DataResult.Success();
    public int DefaultPageSize { get; set; } = 20;

    public QuickGridPresenter(IDataBroker dataBroker)
    {
        _dataBroker = dataBroker;
    }

    /// <summary>
    /// QuickGrid delegate method
    /// Maps the QuickGrid request and result objects to the internal objects to make the request into the data pipeline 
    /// </summary>
    /// <typeparam name="TGridItem"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    public async ValueTask<GridItemsProviderResult<TRecord>> GetItemsAsync<TGridItem>(GridItemsProviderRequest<TRecord> request)
    {
        // Define the Query Request
        var listRequest = new ListQueryRequest(request.StartIndex, request.Count ?? this.DefaultPageSize);

        var result = await _dataBroker.ExecuteQueryAsync<TRecord>(listRequest);
        this.LastDataResult = result;

        return new GridItemsProviderResult<TRecord>() { Items = result.Items.ToList(), TotalItemCount = result.TotalCount };
    }
}
