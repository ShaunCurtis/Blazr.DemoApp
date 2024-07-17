/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public readonly record struct ItemQueryRequest(Guid Key, CancellationToken? Cancellation = null)
{
    public static ItemQueryRequest Create(Guid key)
        => new(key);
}
