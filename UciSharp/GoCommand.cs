namespace UciSharp;

public class GoCommand : CommandObserver<string>
{
    public GoCommand(UciBridge uciBridge) : base(uciBridge) { }
    protected override void HandleChessEngineResponse(string stdOutText)
    {
        if (stdOutText.StartsWith("bestmove"))
        {
            ResolveResponse(stdOutText.Replace("bestmove ", ""));
        }
    }

    protected override async Task SendCommandAsync()
    {
        await UciBridge.SendCommandAsync("go");
    }
}
