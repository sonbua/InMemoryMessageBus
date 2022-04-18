using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;

[assembly: CLSCompliant(true)]

namespace MessageBus;

public class Bus : IDisposable
{
    private readonly string _defaultChannel;
    private readonly ConcurrentDictionary<string, Channel> _channelRouter;

    public Bus()
    {
        _defaultChannel = "_default_channel";
        _channelRouter = new ConcurrentDictionary<string, Channel>();

        GetChannel(_defaultChannel);
    }

    public long CountPending() => CountPending(_defaultChannel);

    public long CountPending(string channelName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        return GetChannel(channelName).MessageQueue.Count;
    }

    public void Publish(object message) => Publish(message, _defaultChannel);

    public void Publish(object message, string channelName)
    {
        EnsureArg.IsNotNull(message, nameof(message));
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        GetChannel(channelName).MessageQueue.TryAdd(message);
    }

    public void Subscribe(Subscriber subscriber) => Subscribe(subscriber, _defaultChannel);

    public void Subscribe(Subscriber subscriber, string channelName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        var (_, subscriptions, manualResetEvent) = GetChannel(channelName);

        // Modification of subscriptions and manualResetEvent should be an atomic operation
        lock (subscriptions)
        {
            if (subscriptions.TryAdd(subscriber.Name, subscriber))
            {
                manualResetEvent.Set();
            }
        }
    }

    public void Unsubscribe(string subscriberName) => Unsubscribe(subscriberName, _defaultChannel);

    public void Unsubscribe(string subscriberName, string channelName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(subscriberName, nameof(subscriberName));
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        var (_, subscriptions, manualResetEvent) = GetChannel(channelName);

        // Modification of subscriptions and manualResetEvent should be an atomic operation
        lock (subscriptions)
        {
            if (subscriptions.TryRemove(subscriberName, out _) &&
                subscriptions.IsEmpty)
            {
                manualResetEvent.Reset();
            }
        }
    }

    private Channel GetChannel(string channelName) =>
        _channelRouter.GetOrAdd(
            channelName,
            _ =>
            {
                var channel = new Channel(
                    new BlockingCollection<object>(),
                    new ConcurrentDictionary<string, Subscriber>(),
                    new ManualResetEventSlim());

                Task.Factory.StartNew(
                    action: c =>
                    {
                        Debug.Assert(c is Channel);

                        var (queue, subscriptions, manualResetEvent) = (Channel)c;

                        manualResetEvent.Wait();

                        foreach (var message in queue.GetConsumingEnumerable())
                        {
                            if (subscriptions.IsEmpty)
                            {
                                queue.TryAdd(message);
                                manualResetEvent.Wait();
                            }

                            foreach (var (_, subscriber) in subscriptions)
                            {
                                try
                                {
                                    subscriber.MessageHandler(message);
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
                    },
                    state: channel,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                return channel;
            });

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var (_, channel) in _channelRouter)
            {
                channel.Dispose();
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private record Channel(
        BlockingCollection<object> MessageQueue,
        ConcurrentDictionary<string, Subscriber> Subscriptions,
        ManualResetEventSlim ManualResetEvent) : IDisposable
    {
        public void Dispose()
        {
            MessageQueue.Dispose();
            ManualResetEvent.Dispose();
        }
    }
}
