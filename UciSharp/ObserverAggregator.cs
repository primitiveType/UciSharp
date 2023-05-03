using System.Net.Http.Headers;
using CliWrap;
using CliWrap.EventStream;

namespace UciSharp;

public class ObserverAggregator : IObserver<CommandEvent>, IObservable<CommandEvent>
{
    private List<IObserver<CommandEvent>> Observers { get; } = new();

    public IDisposable Subscribe(IObserver<CommandEvent> observer)
    {
        return new SubscribeHandle(observer, this);
    }

    public void OnCompleted()
    {
        Console.WriteLine($"Chess Engine process completed.");

        lock (Observers)
        {
            foreach (IObserver<CommandEvent> observer in Observers.ToList())
            {
                observer.OnCompleted();
            }
        }
    }

    public void OnError(Exception error)
    {
        Console.WriteLine($"+_{error}_");

        lock (Observers)
        {
            foreach (IObserver<CommandEvent> observer in Observers.ToList())
            {
                observer.OnError(error);
            }
        }
    }

    public void OnNext(CommandEvent value)
    {
        lock (Observers)
        {
            foreach (IObserver<CommandEvent> observer in Observers.ToList())
            {
                observer.OnNext(value);
            }
        }
    }

    private class SubscribeHandle : IDisposable
    {
        public SubscribeHandle(IObserver<CommandEvent> observer, ObserverAggregator aggregator)
        {
            Aggregator = aggregator;
            lock (Aggregator.Observers)
            {
                aggregator.Observers.Add(observer);
            }
        }

        public ObserverAggregator Aggregator { get; }


        public void Dispose()
        {
            lock (Aggregator.Observers)
            {
                Aggregator.Observers.Remove(Aggregator);
            }
        }
    }
}
