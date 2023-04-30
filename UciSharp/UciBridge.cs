using System.IO.Pipes;
using CliWrap;
using CliWrap.EventStream;
using Nerdbank.Streams;

namespace UciSharp;

public class UciBridge
{
    public string Path { get; }
    public IObserver<CommandEvent> Observer { get; }
    private StreamWriter _writer;
    private CancellationTokenSource TokenSource { get; } = new();

    public UciBridge(string path, IObserver<CommandEvent> observer)
    {
        Path = path;
        Observer = observer;
    }

    public async ValueTask DisposeAsync()
    {
        TokenSource.Dispose();
        await _writer.DisposeAsync();
    }

    public void Dispose()
    {
        _writer.Dispose();
        TokenSource.Dispose();
    }

    public async Task StartAsync()
    {
        AnonymousPipeServerStream outputStream = new(PipeDirection.Out);
        SimplexStream inputStream = new();
        _writer = new StreamWriter(inputStream);
        await StartAsync(outputStream, inputStream);
    }

    public async Task StartAsync(Stream processOutputStream, Stream processInputStream)
    {
        Command command = Cli.Wrap(Path)
                // .WithStandardOutputPipe(PipeTarget.ToStream(fileOutput))
                .WithStandardInputPipe(PipeSource.FromStream(processInputStream))
                .WithStandardOutputPipe(PipeTarget.ToStream(processOutputStream))
            ;


        IObservable<CommandEvent> observable = command.Observe();
        observable.Subscribe(Observer);
        await StartUciAsync();
    }

    public async Task SendCommandAsync(string command)
    {
        await _writer.WriteLineAsync(command);
        await _writer.FlushAsync();
    }

    public async Task StartUciAsync()
    {
        await SendCommandAsync("uci");
    }

    public async Task StartGameAsync()
    {
        await SendCommandAsync("ucinewgame");
    }

    public async Task GoAsync()
    {
        await SendCommandAsync("go");
    }
}