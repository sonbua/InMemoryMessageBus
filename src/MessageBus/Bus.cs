using System;
using System.Collections.Generic;
using System.Linq;

[assembly: CLSCompliant(true)]

namespace MessageBus;

public class Bus
{
    private readonly Queue<object> _pendingMessages;
    private readonly Dictionary<string, Action<object>> _subscribers;

    public Bus()
    {
        _pendingMessages = new Queue<object>();
        _subscribers = new Dictionary<string, Action<object>>();
    }

    public long PendingCount => _pendingMessages.Count;

    public void Publish(object message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        _pendingMessages.Enqueue(message);

        StartConsuming();
    }

    public void Subscribe(string subscriberName, Action<object> action)
    {
        if (string.IsNullOrEmpty(subscriberName))
        {
            throw new ArgumentException("", nameof(subscriberName));
        }

        _subscribers.Add(subscriberName, action);

        StartConsuming();
    }

    public void Unsubscribe(string subscriberName) => _subscribers.Remove(subscriberName);

    private void StartConsuming()
    {
        while (_pendingMessages.Any() && _subscribers.Any())
        {
            var message = _pendingMessages.Dequeue();

            foreach (var subscriber in _subscribers)
            {
                subscriber.Value.Invoke(message);
            }
        }
    }
}
