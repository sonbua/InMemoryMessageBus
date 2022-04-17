using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using EnsureThat;

[assembly: CLSCompliant(true)]

namespace MessageBus;

public class Bus
{
    private readonly string _defaultChannel;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<object>> _routingQueue;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Subscriber>> _channelToSubscriptionsMap;

    public Bus()
    {
        _defaultChannel = "_default_channel";
        _routingQueue = new ConcurrentDictionary<string, ConcurrentQueue<object>>();
        _routingQueue.TryAdd(_defaultChannel, new ConcurrentQueue<object>());
        _channelToSubscriptionsMap = new ConcurrentDictionary<string, ConcurrentDictionary<string, Subscriber>>();
    }

    public long CountPending() => CountPending(_defaultChannel);

    public long CountPending(string channelName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        return GetQueue(channelName).Count;
    }

    public void Publish(object message) => Publish(message, _defaultChannel);

    public void Publish(object message, string channelName)
    {
        EnsureArg.IsNotNull(message, nameof(message));
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        var queue = GetQueue(channelName);

        queue.Enqueue(message);

        StartConsuming(channelName);
    }

    public void Subscribe(Subscriber subscriber) => Subscribe(subscriber, _defaultChannel);

    public void Subscribe(Subscriber subscriber, string channelName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        var subscriptions = _channelToSubscriptionsMap.GetOrAdd(
            channelName,
            static _ => new ConcurrentDictionary<string, Subscriber>());

        subscriptions.TryAdd(subscriber.Name, subscriber);

        StartConsuming(channelName, subscriptions);
    }

    public void Unsubscribe(string subscriberName) => Unsubscribe(subscriberName, _defaultChannel);

    public void Unsubscribe(string subscriberName, string channelName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(subscriberName, nameof(subscriberName));
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        if (_channelToSubscriptionsMap.TryGetValue(channelName, out var subscriptions)
            && !subscriptions.IsEmpty)
        {
            subscriptions.TryRemove(subscriberName, out _);
        }
    }

    private void StartConsuming(string channelName)
    {
        Debug.Assert(string.IsNullOrWhiteSpace(channelName));

        if (_channelToSubscriptionsMap.TryGetValue(channelName, out var subscriptions)
            && !subscriptions.IsEmpty)
        {
            StartConsuming(channelName, subscriptions);
        }
    }

    private void StartConsuming(string channelName, ConcurrentDictionary<string, Subscriber> subscriptions)
    {
        Debug.Assert(string.IsNullOrWhiteSpace(channelName));
        Debug.Assert(!subscriptions.IsEmpty);

        var queue = GetQueue(channelName);

        if (queue.IsEmpty)
        {
            return;
        }

        StartConsuming(queue, subscriptions);
    }

    private static void StartConsuming(
        ConcurrentQueue<object> queue,
        ConcurrentDictionary<string, Subscriber> subscriptions)
    {
        Debug.Assert(!queue.IsEmpty);
        Debug.Assert(!subscriptions.IsEmpty);

        while (queue.TryDequeue(out var message))
        {
            foreach (var subscriber in subscriptions)
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

    private ConcurrentQueue<object> GetQueue(string channelName)
    {
        Debug.Assert(string.IsNullOrWhiteSpace(channelName));

        return _routingQueue.GetOrAdd(channelName, _ => new ConcurrentQueue<object>());
    }
}
