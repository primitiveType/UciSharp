namespace UciSharp;

public class ReadyCommand : CommandObserver<bool>
{
    public ReadyCommand(UciBridge uciBridge) : base(uciBridge) { }


    protected override void HandleChessEngineResponse(string stdOutText)
    {
        if (stdOutText == "readyok")
        {
            ResolveResponse(true);
        }
    }

    protected override async Task SendCommandAsync()
    {
        await UciBridge.SendCommandAsync("isready");
    }
}
