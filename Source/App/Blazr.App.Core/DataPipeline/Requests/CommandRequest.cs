/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public record struct CommandRequest<TRecord>(TRecord Item, CommandState State, CancellationToken Cancellation = new());

public record CommandState
{
    public int Index { get; private init; } = 0;
    public string Value { get; private init; } = "None";

    public CommandState() { }

    public CommandState(int index, string value)
    {
        Index = index;
        Value = value;
    }

    public static CommandState None = new CommandState(0, "None");
    public static CommandState Add = new CommandState(1, "Add");
    public static CommandState Update = new CommandState(2, "Update");
    public static CommandState Delete = new CommandState(-1, "Delete");

    public static CommandState GetState(int index)
        => (index) switch
        {
            1 => CommandState.Add,
            2 => CommandState.Update,
            -1 => CommandState.Delete,
            _ => CommandState.None,
        };
}
