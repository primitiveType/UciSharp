using System.Runtime.CompilerServices;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;

namespace UciSharp;

public class ChessEngine : IObserver<CommandEvent>
{
    private Task chessEngineTask;
    private Task _readTask;

    public ChessEngine(string path)
    {
        Path = path;
    }

    public string Path { get; }
    private Dictionary<string, Option> AvailableOptionsInternal { get; } = new();
    public IReadOnlyList<Option> AvailableOptions => AvailableOptionsInternal.Values.ToList();
    private CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();

    public async Task Start()
    {
        await using var inputStream = Console.OpenStandardInput();
        await using var outputStream = Console.OpenStandardOutput();
        await Start(inputStream, outputStream);
    }

    public async Task Start(Stream processOutputStream, Stream processInputStream)
    {
        // MemoryStream stringStream = new();


        // _readTask = Task.Run(async () => await ReadStreamAsync(processOutputStream, TokenSource.Token), TokenSource.Token);

        // await using FileStream fileOutput = File.Create("output.txt");
        // Command command = processInputStream | Cli.Wrap(Path) | processOutputStream;
        Command command = Cli.Wrap(Path)
                // .WithStandardOutputPipe(PipeTarget.ToStream(fileOutput))
                .WithStandardInputPipe(PipeSource.FromStream(processInputStream))
                .WithStandardOutputPipe(PipeTarget.ToStream(processOutputStream))
            ;

        IObservable<CommandEvent> observable = command.Observe();
        observable.Subscribe(this);

        // chessEngineTask = Task.Run(async () => await ReadCommands(command));
        // await using StreamWriter writer = new StreamWriter(processInputStream);
        // await writer.WriteLineAsync("uci");


        // chessEngineTask = command.ExecuteAsync(default, TokenSource.Token);
        // chessEngineTask.ContinueWith(Done);
    }

    private void Done(Task task)
    {
        Console.WriteLine($"process exited... {chessEngineTask.IsCompleted}");
    }

    private async Task ReadCommands(Command cmd)
    {
        await foreach (var cmdEvent in cmd.ListenAsync())
        {
            switch (cmdEvent)
            {
                case StartedCommandEvent started:
                    Console.WriteLine($"Process started; ID: {started.ProcessId}");
                    break;
                case StandardOutputCommandEvent stdOut:
                    Console.WriteLine($"Out> {stdOut.Text}");
                    break;
                case StandardErrorCommandEvent stdErr:
                    Console.WriteLine($"Err> {stdErr.Text}");
                    break;
                case ExitedCommandEvent exited:
                    Console.WriteLine($"Process exited; Code: {exited.ExitCode}");
                    break;
            }
        }
    }

    public async Task Stop()
    {
        TokenSource.Cancel();
        // await chessEngineTask;
        // await Task.WhenAll(chessEngineTask, _readTask);
    }


    private async Task ReadStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        long readerPos = -1;
        StreamReader reader = new(stream);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string? line = await reader.ReadLineAsync(cancellationToken);
                if (readerPos != reader.BaseStream.Position)
                {
                    Console.WriteLine(reader.BaseStream.Position);
                    readerPos = reader.BaseStream.Position;
                }

                if (line == null)
                {
                    await Task.Delay(10, cancellationToken);
                    continue;
                }

                Console.WriteLine(line);

                if (line.StartsWith("option"))
                {
                    UpdateAvailableOptions(line);
                }

                // byte[] buffer = new byte[256];
                // var response = await stream.ReadAsync(buffer);
                // if (response > 0)
                // {
                //     Console.WriteLine($"got {response} bytes!");
                Console.Write(line);

                // }
            }
        }
        catch (TaskCanceledException) { }
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
    }
}

class Observer : IObserver<CommandEvent>
{
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
                Console.WriteLine($"Out> {stdOut.Text}");
                break;
            case StandardErrorCommandEvent stdErr:
                Console.WriteLine($"Err> {stdErr.Text}");
                break;
            case ExitedCommandEvent exited:
                Console.WriteLine($"Process exited; Code: {exited.ExitCode}");
                break;
        }
    }
}
