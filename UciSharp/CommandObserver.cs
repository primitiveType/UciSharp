using CliWrap.EventStream;
using Nerdbank.Streams;

namespace UciSharp;

public abstract class CommandObserver<T> : IObserver<CommandEvent>
{
    protected CommandObserver(UciBridge uciBridge)
    {
        UciBridge = uciBridge;
    }

    public void OnCompleted() { }

    public void OnError(Exception error) { }

    public void OnNext(CommandEvent value)
    {
        if (value is StandardOutputCommandEvent stdOut)
        {
            HandleChessEngineResponse(stdOut.Text);
        }
    }

    protected abstract void HandleChessEngineResponse(string stdOutText);
    private TaskCompletionSource<T>? ResponseReceived { get; set; }
    protected UciBridge UciBridge { get; }
    private object ReadyLock { get; } = new();

    protected void ResolveResponse(T value)
    {
        lock (ReadyLock)
        {
            if (ResponseReceived != null)
            {
                ResponseReceived.TrySetResult(value);
            }
        }
    }

    protected abstract Task SendCommandAsync(string? options = null);

    public async Task<T> InvokeAsync(string? options = null)
    {
        Task<T>? task = ResponseReceived?.Task;
        lock (ReadyLock)
        {
            if (ResponseReceived == null || ResponseReceived.Task.IsCompleted)
            {
                ResponseReceived = new TaskCompletionSource<T>();
                task = ResponseReceived.Task;
            }
        }

        await SendCommandAsync(options);

        return await task!;
    }
}
