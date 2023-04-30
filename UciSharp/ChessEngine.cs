using Nerdbank.Streams;

namespace UciSharp;

public class ChessEngine : IAsyncDisposable, IDisposable
{
    //it is not possible to attach multiple observers to CliWrap, so this one aggregates our observers and forwards all messages.
    private ObserverAggregator ObserverAggregator { get; } = new();
    private UciOptionsCommand OptionsCommand { get; }
    private ReadyCommand ReadyCommand { get; }
    public UciBridge UciBridge { get; }
    private NewGameCommand NewGameCommand { get; }
    private GoCommand GoCommand { get; }


    public ChessEngine(string path)
    {
        UciBridge = new UciBridge(path, ObserverAggregator);

        GoCommand = new GoCommand(UciBridge);
        NewGameCommand = new NewGameCommand(UciBridge);
        ReadyCommand = new ReadyCommand(UciBridge);
        OptionsCommand = new UciOptionsCommand(UciBridge);
        ObserverAggregator.Subscribe(ReadyCommand);
        ObserverAggregator.Subscribe(OptionsCommand);
        ObserverAggregator.Subscribe(NewGameCommand);
        ObserverAggregator.Subscribe(GoCommand);
    }


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
        await UciBridge.StartAsync();
        return await OptionsCommand.InvokeAsync();
    }

    public async Task WaitForReadyAsync()
    {
        await ReadyCommand.InvokeAsync();
    }

    public async Task SetOptions(IList<Option> options)
    {
        foreach (Option option in options)
        {
            await UciBridge.SendCommandAsync(option.SetCommandString);
        }
    }

    public async Task<IReadOnlyList<Option>> GetOptionsAsync()
    {
        return await OptionsCommand.InvokeAsync();
    }

    public async Task StartGameAsync()
    {
        await NewGameCommand.InvokeAsync();
    }

    public async Task<string> GoAsync()
    {
        return await GoCommand.InvokeAsync();
    }
}
