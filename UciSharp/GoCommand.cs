namespace UciSharp;

public class GoCommand : CommandObserver<string>
{
    public GoCommand(UciBridge uciBridge) : base(uciBridge) { }
    protected override void HandleChessEngineResponse(string stdOutText)
    {
        if (stdOutText.Contains("bestmove"))
        {
            ResolveResponse(stdOutText.Replace("bestmove ", ""));
        }
    }

    protected override async Task SendCommandAsync(string? options)
    {
        await UciBridge.SendCommandAsync("go");
    }
}

public struct GoResponse
{
    
}
