/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public sealed record ListQueryRequest(int StartIndex, int PageSize, CancellationToken? Cancellation = null)
{
    public static ListQueryRequest Empty()
        => new(0, 1000);
}
