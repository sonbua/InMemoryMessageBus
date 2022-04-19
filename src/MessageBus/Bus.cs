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

        return GetChannel(channelName).Count();
    }

    public void Publish(object message) => Publish(message, _defaultChannel);

    public void Publish(object message, string channelName)
    {
        EnsureArg.IsNotNull(message, nameof(message));
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        GetChannel(channelName).Publish(message);
    }

    public void Subscribe(Subscriber subscriber) => Subscribe(subscriber, _defaultChannel);

    public void Subscribe(Subscriber subscriber, string channelName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        GetChannel(channelName).Subscribe(subscriber);
    }

    public void Unsubscribe(string subscriberName) => Unsubscribe(subscriberName, _defaultChannel);

    public void Unsubscribe(string subscriberName, string channelName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(subscriberName, nameof(subscriberName));
        EnsureArg.IsNotNullOrWhiteSpace(channelName, nameof(channelName));

        GetChannel(channelName).Unsubscribe(subscriberName);
    }

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

    private Channel GetChannel(string channelName) =>
        _channelRouter.GetOrAdd(
            channelName,
            _ => new Channel(
                    MessageQueue: new BlockingCollection<object>(),
                    Subscriptions: new ConcurrentDictionary<string, Subscriber>(),
                    ManualResetEvent: new ManualResetEventSlim())
                .Initialize());

    private record Channel(
        BlockingCollection<object> MessageQueue,
        ConcurrentDictionary<string, Subscriber> Subscriptions,
        ManualResetEventSlim ManualResetEvent) : IDisposable
    {
        private BlockingCollection<object> MessageQueue { get; } = MessageQueue;
        private ConcurrentDictionary<string, Subscriber> Subscriptions { get; } = Subscriptions;
        private ManualResetEventSlim ManualResetEvent { get; } = ManualResetEvent;

        public Channel Initialize()
        {
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
                state: this,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            return this;
        }

        public long Count() => MessageQueue.Count;

        public void Publish(object message) => MessageQueue.TryAdd(message);

        public void Subscribe(Subscriber subscriber)
        {
            // Modification of subscriptions and manualResetEvent should be an atomic operation
            lock (Subscriptions)
            {
                if (Subscriptions.TryAdd(subscriber.Name, subscriber))
                {
                    ManualResetEvent.Set();
                }
            }
        }

        public void Unsubscribe(string subscriberName)
        {
            // Modification of subscriptions and manualResetEvent should be an atomic operation
            lock (Subscriptions)
            {
                if (Subscriptions.TryRemove(subscriberName, out _) &&
                    Subscriptions.IsEmpty)
                {
                    ManualResetEvent.Reset();
                }
            }
        }

        public void Dispose()
        {
            MessageQueue.Dispose();
            ManualResetEvent.Dispose();
        }
    }
}
