namespace BlazorApp1.Messaging;

public interface IMessageChannel<T>
{
    void HandleMessage(T msg);
}

public interface ISubscriber<T>
{
    void OnMessage(T msg);
}

public interface ISubscribable<T>
{
    void Subscribe(ISubscriber<T> subscriber);
    void Unsubscribe(ISubscriber<T> subscriber);
}

public interface IBroadcast<T> : IMessageChannel<T>, ISubscribable<T>
{
}

public class Broadcast<T> : IBroadcast<T>
{
    private readonly List<ISubscriber<T>> _subscribers = [];

    public void HandleMessage(T msg)
    {
        _subscribers.ForEach(subscriber => subscriber.OnMessage(msg));
    }

    public void Subscribe(ISubscriber<T> subscriber)
    {
        this._subscribers.Add(subscriber);
    }

    public void Unsubscribe(ISubscriber<T> subscriber)
    {
        this._subscribers.Remove(subscriber);
    }
}