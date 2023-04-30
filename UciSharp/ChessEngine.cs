using System.IO.Pipes;
using CliWrap;
using CliWrap.EventStream;
using Nerdbank.Streams;

namespace UciSharp;

public class ChessEngine : IObserver<CommandEvent>, IAsyncDisposable, IDisposable
{
    private StreamWriter _writer;

    public ChessEngine(string path)
    {
        Path = path;
    }

    public string Path { get; }
    private Dictionary<string, Option> AvailableOptionsInternal { get; } = new();
    public IReadOnlyList<Option> AvailableOptions => AvailableOptionsInternal.Values.ToList();
    private CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();

    private TaskCompletionSource? ReadyOk;
    private object ReadyLock { get; } = new object();

    public async Task StartAsync()
    {
        AnonymousPipeServerStream outputStream = new AnonymousPipeServerStream(PipeDirection.Out);
        SimplexStream inputStream = new SimplexStream();
        _writer = new(inputStream);
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
        observable.Subscribe(this);

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


    private void UpdateAvailableOptions(string line)
    {
        Option option = new Option(line);
        AvailableOptionsInternal[option.Name] = option;
    }

    public void OnCompleted()
    {
        Console.WriteLine("Process completed.");
    }

    public void OnError(Exception error)
    {
        Console.WriteLine($"Process error {error}");
    }

    public void OnNext(CommandEvent value)
    {
        switch (value)
        {
            case StartedCommandEvent started:
                Console.WriteLine($"Process started; ID: {started.ProcessId}");
                break;
            case StandardOutputCommandEvent stdOut:
                HandleChessEngineResponse(stdOut.Text);
                break;
            case StandardErrorCommandEvent stdErr:
                Console.WriteLine($"Err> {stdErr.Text}");
                break;
            case ExitedCommandEvent exited:
                Console.WriteLine($"Process exited; Code: {exited.ExitCode}");
                break;
        }
    }

    private void HandleChessEngineResponse(string stdOutText)
    {
        Console.WriteLine(stdOutText);
        if (stdOutText.StartsWith("option"))
        {
            UpdateAvailableOptions(stdOutText);
        }

        if (stdOutText.StartsWith("readyok"))
        {
            FireReadyOk();
        }
    }

    private void FireReadyOk()
    {
        lock (ReadyLock)
        {
            if (ReadyOk != null)
            {
                ReadyOk.TrySetResult();
            }
        }
    }


    public void Dispose()
    {
        _writer.Dispose();
        TokenSource.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        TokenSource.Dispose();
        await _writer.DisposeAsync();
    }

    public async Task WaitForReadyAsync()
    {
        Task readyTask;
        lock (ReadyLock)
        {
            if (ReadyOk == null || ReadyOk.Task.IsCompleted)
            {
                ReadyOk = new TaskCompletionSource();
            }

            readyTask = ReadyOk.Task;
        }

        await SendCommandAsync("isready");
        await readyTask;
    }

    public async Task StartGameAsync()
    {
        await SendCommandAsync($"ucinewgame");
    }

    public async Task GoAsync()
    {
        await SendCommandAsync("go");
    }
}
