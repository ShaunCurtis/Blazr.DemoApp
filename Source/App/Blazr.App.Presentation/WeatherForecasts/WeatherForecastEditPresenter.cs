/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
using Microsoft.AspNetCore.Components.Forms;

namespace Blazr.App.Presentation;

public class WeatherForecastEditPresenter
{
    private readonly IDataBroker _dataBroker;
    private readonly INewRecordProvider<WeatherForecast> _newProvider;

    public IDataResult LastDataResult { get; private set; } = DataResult.Success();
    public EditContext? EditContext { get; private set; }
    public WeatherForecastEditContext RecordEditContext { get; private set; }
    public bool IsNew { get; private set; }

    public bool IsInvalid => this.EditContext?.GetValidationMessages().Any() ?? false;

    public WeatherForecastEditPresenter(IDataBroker dataBroker, INewRecordProvider<WeatherForecast> newProvider)
    {
        _dataBroker = dataBroker;
        this.RecordEditContext = new(new());
        _newProvider = newProvider;
    }

    public async Task LoadAsync(Guid id)
    {
        this.LastDataResult = DataResult.Success();
        this.IsNew = false;

        // The Update Path.  Get the requested record if it exists
        if (id != Guid.Empty)
        {
            var request = ItemQueryRequest.Create(id);
            var result = await _dataBroker.ExecuteQueryAsync<WeatherForecast>(request);
            LastDataResult = result;
            if (this.LastDataResult.Successful)
            {
                RecordEditContext = new(result.Item!);
                this.EditContext = new(this.RecordEditContext);
            }
            return;
        }

        // The new path.  Get a new record using the NewRecordProvider service
        this.RecordEditContext = new(_newProvider.NewRecord());
        this.EditContext = new(this.RecordEditContext);
        this.IsNew = true;
    }

    public async Task<IDataResult> SaveItemAsync()
    {

        if (!this.RecordEditContext.IsDirty)
        {
            this.LastDataResult = DataResult.Failure("The record has not changed and therefore has not been updated.");
            return this.LastDataResult;
        }

        var record = RecordEditContext.AsRecord;
        var command = new CommandRequest<WeatherForecast>(record, this.IsNew ? CommandState.Add : CommandState.Update);
        var result = await _dataBroker.ExecuteCommandAsync<WeatherForecast>(command);

        if (result.Successful)
        {
            //var outcome = this.IsNew ? "added" : "updated";
            //_toastService.ShowSuccess($"The Weather Forecast was {outcome}.");
        }
        else
            //_toastService.ShowError(result.Message ?? "The Weather Forecast could not be saved.");

        this.LastDataResult = result;
        return result;
    }
}
