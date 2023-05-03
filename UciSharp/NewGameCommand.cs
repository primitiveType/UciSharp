namespace UciSharp;

public class NewGameCommand : CommandWaitForReadyObserver
{
    public NewGameCommand(UciBridge uciBridge) : base(uciBridge) { }

    protected override async Task SendInitialCommandAsync(string? options = null)
    {
        await UciBridge.SendCommandAsync("ucinewgame");
    }
}


public class PositionCommand : CommandWaitForReadyObserver
{
    public PositionCommand(UciBridge uciBridge) : base(uciBridge) { }

    protected override async Task SendInitialCommandAsync(string? options = null)
    {
        await UciBridge.SendCommandAsync($"position fen {options}");
    }
}
