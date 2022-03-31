using System;
using System.Collections.Concurrent;

[assembly: CLSCompliant(true)]

namespace MessageBus;

public class Bus
{
    private readonly string _defaultChannel;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<object>> _routingQueue;
    private readonly ConcurrentDictionary<string, Subscriber> _subscribers;

    public Bus()
    {
        _defaultChannel = "_default_channel";
        _routingQueue = new ConcurrentDictionary<string, ConcurrentQueue<object>>();
        _routingQueue.TryAdd(_defaultChannel, new ConcurrentQueue<object>());
        _subscribers = new ConcurrentDictionary<string, Subscriber>();
    }

    public long PendingCount => DefaultQueue().Count;

    public void Publish(object message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        DefaultQueue().Enqueue(message);

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
        if (_subscribers.IsEmpty)
        {
            return;
        }

        var defaultQueue = DefaultQueue();

        while (defaultQueue.TryDequeue(out var message))
        {
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

    private ConcurrentQueue<object> DefaultQueue()
    {
        var hasDefaultQueue = _routingQueue.TryGetValue(_defaultChannel, out var defaultQueue);

        if (!hasDefaultQueue)
        {
            throw new InvalidOperationException($"Fatal: There is no default channel '{_defaultChannel}'.");
        }

        if (defaultQueue is null)
        {
            throw new InvalidOperationException(
                $"Fatal: There is default channel '{_defaultChannel}', but queue is null.");
        }

        return defaultQueue;
    }
}
