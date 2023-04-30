namespace UciSharp;

public class ReadyObserver : CommandObserver<bool>
{
    public ReadyObserver(UciBridge uciBridge) : base(uciBridge) { }


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
