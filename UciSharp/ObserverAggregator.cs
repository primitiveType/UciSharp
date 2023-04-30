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
        lock (Observers)
        {
            foreach (IObserver<CommandEvent> observer in Observers)
            {
                observer.OnCompleted();
            }
        }
    }

    public void OnError(Exception error)
    {
        lock (Observers)
        {
            foreach (IObserver<CommandEvent> observer in Observers)
            {
                observer.OnError(error);
            }
        }
    }

    public void OnNext(CommandEvent value)
    {
        lock (Observers)
        {
            foreach (IObserver<CommandEvent> observer in Observers)
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