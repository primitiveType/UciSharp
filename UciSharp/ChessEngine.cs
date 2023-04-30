namespace UciSharp;

public class ChessEngine : IAsyncDisposable, IDisposable
{
    private UciOptionsCommandObserver OptionsCommandObserver { get; }

    public ChessEngine(string path)
    {
        UciBridge = new UciBridge(path, ObserverAggregator);
        ReadyObserver = new ReadyObserver(UciBridge);
        OptionsCommandObserver = new UciOptionsCommandObserver(UciBridge);
        ObserverAggregator.Subscribe(ReadyObserver);
        ObserverAggregator.Subscribe(OptionsCommandObserver);
    }

    private ReadyObserver ReadyObserver { get; }
    
    //it is not possible to attach multiple observers to CliWrap, so this one aggregates our observers and forwards all messages.
    private ObserverAggregator ObserverAggregator { get; } = new();

    public UciBridge UciBridge { get; }

    public async ValueTask DisposeAsync()
    {
        await UciBridge.DisposeAsync();
    }

    public void Dispose()
    {
        UciBridge.Dispose();
    }

    public async Task<IReadOnlyList<Option>> StartAsync()
    {
        return await OptionsCommandObserver.InvokeAsync();
    }

    public async Task WaitForReadyAsync()
    {
        await ReadyObserver.InvokeAsync();
    }

    public async Task SetOptions(IList<Option> options)
    {
        foreach (Option option in options)
        {
            UciBridge.SendCommandAsync(option.SetCommandString);
        }
    }
}
