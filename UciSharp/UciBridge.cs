using System.IO.Pipes;
using System.Text;
using CliWrap;
using CliWrap.EventStream;
using Nerdbank.Streams;

namespace UciSharp;

public class UciBridge
{
    public string Path { get; }
    public IObserver<CommandEvent> Observer { get; }
    private StreamWriter _writer;
    private StringBuilder _outputStream;
    private SimplexStream _inputStream;
    private CancellationTokenSource TokenSource { get; } = new();

    public UciBridge(string path, IObserver<CommandEvent> observer)
    {
        Path = path;
        Observer = observer;
        _outputStream = new StringBuilder();
        // _outputStream = new(PipeDirection.Out, HandleInheritability.None, 4098 * 4 );
        _inputStream = new(16, 32);
        _writer = new StreamWriter(_inputStream);
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
      
        await StartAsync(_outputStream, _inputStream);
    }

    public async Task StartAsync(StringBuilder processOutputStream, Stream processInputStream)
    {
        Command command = Cli.Wrap(Path)
                // .WithStandardOutputPipe(PipeTarget.ToStream(fileOutput))
                .WithStandardInputPipe(PipeSource.FromStream(processInputStream))
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(processOutputStream))
            ;


        IObservable<CommandEvent> observable = command.Observe();
        observable.Subscribe(Observer);
    }

    public async Task SendCommandAsync(string command)
    {
        await _writer.WriteLineAsync(command);
        await _writer.FlushAsync();
    }

    public async Task WaitForReadyAsync()
    {
        await SendCommandAsync("isready");
    }
    
}
