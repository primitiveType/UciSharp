namespace UciSharp;

public class NewGameCommand : CommandWaitForReadyObserver
{
    public NewGameCommand(UciBridge uciBridge) : base(uciBridge) { }

    protected override async Task SendInitialCommandAsync()
    {
        await UciBridge.SendCommandAsync("ucinewgame");
    }
}
