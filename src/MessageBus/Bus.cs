using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

[assembly: CLSCompliant(true)]

namespace MessageBus;

public class Bus
{
    private readonly Queue<object> _pendingMessages;
    private readonly ConcurrentDictionary<string, Subscriber> _subscribers;

    public Bus()
    {
        _pendingMessages = new Queue<object>();
        _subscribers = new ConcurrentDictionary<string, Subscriber>();
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

    public void Subscribe(Subscriber subscriber)
    {
        if (subscriber is null)
        {
            throw new ArgumentNullException(nameof(subscriber));
        }

        if (string.IsNullOrWhiteSpace(subscriber.Name))
        {
            throw new ArgumentException("Subscriber name should not be empty or whitespace(s).", nameof(subscriber));
        }

        _subscribers.TryAdd(subscriber.Name, subscriber);

        StartConsuming();
    }

    public void Unsubscribe(string subscriberName)
    {
        if (subscriberName is null)
        {
            throw new ArgumentNullException(nameof(subscriberName));
        }

        if (string.IsNullOrWhiteSpace(subscriberName))
        {
            throw new ArgumentException(
                "Subscriber name should not be empty or whitespace(s).",
                nameof(subscriberName));
        }

        _subscribers.TryRemove(subscriberName, out _);
    }

    private void StartConsuming()
    {
        while (_pendingMessages.Any() && _subscribers.Any())
        {
            var message = _pendingMessages.Dequeue();

            foreach (var subscriber in _subscribers)
            {
                try
                {
                    subscriber.Value.MessageHandler(message);
                }
// Ensures exception from a subscriber won't affect next subscriber to receive the message.
#pragma warning disable CA1031
                catch
#pragma warning restore CA1031
                {
                    // ignored
                }
            }
        }
    }
}
