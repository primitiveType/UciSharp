namespace UciSharp;

public abstract class CommandWaitForReadyObserver : CommandObserver<bool>
{
    public CommandWaitForReadyObserver(UciBridge uciBridge) : base(uciBridge) { }

    protected override void HandleChessEngineResponse(string stdOutText)
    {
        if (stdOutText == "readyok")
        {
            ResolveResponse(true);
        }
    }

    protected override async Task SendCommandAsync(string? options = null)
    {
        await SendInitialCommandAsync(options);
        await UciBridge.WaitForReadyAsync();
    }

    protected abstract Task SendInitialCommandAsync(string? options = null);
}
