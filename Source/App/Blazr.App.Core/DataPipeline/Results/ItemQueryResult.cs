/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public sealed record ItemQueryResult<TRecord> : DataResult<TRecord>
{
    public new static ItemQueryResult<TRecord> Success(TRecord Item, string? message = null)
    => new () { Successful = true, Item = Item, Message = message };

    public new static ItemQueryResult<TRecord> Failure(string message)
        => new () { Message = message };

}
