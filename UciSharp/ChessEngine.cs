using Microsoft.VisualBasic;
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

    // private PositionCommand PositionCommand { get; }
    // private GoCommand GoCommand { get; }


    public ChessEngine(string path)
    {
        UciBridge = new UciBridge(path, ObserverAggregator);

        // GoCommand = new GoCommand(UciBridge);
        NewGameCommand = new NewGameCommand(UciBridge);
        // PositionCommand = new PositionCommand(UciBridge);
        ReadyCommand = new ReadyCommand(UciBridge);
        OptionsCommand = new UciOptionsCommand(UciBridge);
        // ObserverAggregator.Subscribe(UciBridge.GetFlusher());
        ObserverAggregator.Subscribe(ReadyCommand);
        ObserverAggregator.Subscribe(OptionsCommand);
        ObserverAggregator.Subscribe(NewGameCommand);
        // ObserverAggregator.Subscribe(GoCommand);
        // ObserverAggregator.Subscribe(PositionCommand);
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

    public async Task<string> GoAsync(int msLimit = -1)
    {
        GoCommand goCommand = new(UciBridge);
        using (ObserverAggregator.Subscribe(goCommand))
        {
            if (msLimit >= 0)
            {
                return await goCommand.InvokeAsync($"movetime {msLimit}");
            }
            return await goCommand.InvokeAsync();
        }
    }

    public async Task SetPositionAsync(string fenString)
    {
        PositionCommand positionCommand = new(UciBridge);
        using (ObserverAggregator.Subscribe(positionCommand))
        {
            await positionCommand.InvokeAsync(fenString);
        }
    }
}
