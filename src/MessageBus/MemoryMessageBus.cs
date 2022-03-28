using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// ReSharper disable MemberCanBePrivate.Global

namespace MessageBus;

public class MemoryMessageBus
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<object?>> _queue = new();
    private readonly ConcurrentDictionary<string, ConcurrentStack<object?>> _stack = new();

    private readonly ConcurrentDictionary<string, object?> _cache = new();

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Action<object?>>> _channelSubscriber =
        new();

    private readonly ConcurrentDictionary<string, bool> _channelTypeIsQueue = new();
    private readonly ConcurrentDictionary<string, DateTime?> _keyExpire = new();
    private readonly ConcurrentDictionary<string, KeyValuePair<DateTime, TimeSpan?>> _keySlideExpire = new();

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object?>> _hashSet = new();

    private MemoryMessageBus()
    {
        var channelThread = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        List<KeyValuePair<string, ConcurrentDictionary<string, Action<object?>>>> tempChannel;
                        lock (_channelSubscriber)
                        {
                            tempChannel = _channelSubscriber.ToList();
                        }

                        foreach (var itm in tempChannel)
                        {
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                if (_channelTypeIsQueue.ContainsKey(itm.Key) && !itm.Value.IsEmpty)
                                {
                                    var channelType = _channelTypeIsQueue[itm.Key];
                                    var data = channelType ? Dequeue(itm.Key) : Pop(itm.Key);
                                    if (data != null)
                                    {
                                        List<KeyValuePair<string, Action<object?>>> tempSubscribe;
                                        lock (itm.Value)
                                        {
                                            tempSubscribe = itm.Value.ToList();
                                        }

                                        foreach (var subscriber in tempSubscribe)
                                        {
                                            ThreadPool.QueueUserWorkItem(_ =>
                                            {
                                                subscriber.Value(data);
                                            });
                                        }
                                    }
                                }
                            });
                        }
                    });

                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        List<KeyValuePair<string, DateTime?>> tempExpired;

                        lock (_keyExpire)
                        {
                            tempExpired = _keyExpire.ToList();
                        }

                        var listExpired = tempExpired.Where(i => i.Value != null && i.Value.Value < DateTime.Now)
                            .ToList();

                        foreach (var item in listExpired)
                        {
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                Clear(item.Key);
                            });
                        }
                    });
                }
#pragma warning disable CA1031
                catch
#pragma warning restore CA1031
                {
                    // ignored
                }
                finally
                {
                    Thread.Sleep(1);
                }
            }
        });

        channelThread.Start();
    }

    public void CacheSet<T>(string key, T data, DateTime? expireAt = null)
    {
        _cache[key] = data;
        SetExpire(key, expireAt);
    }

    public void CacheRemove(string key) => _cache.TryRemove(key, out _);

    public void CacheSetUseSlideExpire<T>(string key, T data, TimeSpan? slideInterval = null)
    {
        _cache[key] = data;
        SetExpireUseSlide(key, slideInterval);
    }

    public T? CacheGet<T>(string key)
    {
        if (_keySlideExpire.ContainsKey(key))
        {
            var slideInterval = _keySlideExpire[key].Value;
            SetExpireUseSlide(key, slideInterval);
        }

        return _cache.TryGetValue(key, out var val) ? (T?)val : default;
    }

    public void HashSet<T>(string key, string field, T data, DateTime? expireAt = null)
    {
        _hashSet.GetOrAdd(key, _ =>
        {
            var value = new ConcurrentDictionary<string, object?>();
            value.TryAdd(field, data);
            return value;
        });

        SetExpire(key, expireAt);
    }

    public void HashSetUseSlideExpire<T>(string key, string field, T data, TimeSpan? slideInterval = null)
    {
        _hashSet.GetOrAdd(key, _ =>
        {
            var value = new ConcurrentDictionary<string, object?>();
            value.TryAdd(field, data);
            return value;
        });

        SetExpireUseSlide(key, slideInterval);
    }

    public T? HashGet<T>(string key, string field)
    {
        if (_keySlideExpire.ContainsKey(key))
        {
            var slideInterval = _keySlideExpire[key].Value;
            SetExpireUseSlide(key, slideInterval);
        }

        return _hashSet.TryGetValue(key, out var filedData) &&
            filedData.TryGetValue(field, out var data)
                ? (T?)data
                : default;
    }

    public ConcurrentDictionary<string, object?> HashGetAll(string key)
    {
        _hashSet.TryGetValue(key, out var val);

        return val!;
    }

    public void SetExpire(string key, DateTime? expireAt = null)
    {
        if (expireAt == null)
        {
            _keyExpire.TryRemove(key, out expireAt);

            _keySlideExpire.TryRemove(key, out _);
        }
        else
        {
            _keyExpire[key] = expireAt;
        }
    }

    public void SetExpireUseSlide(string key, TimeSpan? slideInterval = null)
    {
        DateTime? expireAt = null;
        if (slideInterval != null)
        {
            expireAt = DateTime.Now.Add(slideInterval.Value);

            _keySlideExpire[key] = new KeyValuePair<DateTime, TimeSpan?>(expireAt.Value, slideInterval);
        }

        SetExpire(key, expireAt);
    }

    public void Enqueue<T>(string queueName, T data, DateTime? expireAt = null)
    {
        _queue.GetOrAdd(queueName, _ => new ConcurrentQueue<object?>(new object?[] { data }));

        SetExpire(queueName, expireAt);
    }

    private object? Dequeue(string queueName) =>
        _queue.TryGetValue(queueName, out var queueData) &&
        queueData.TryDequeue(out var data)
            ? data
            : null;

    public T? Dequeue<T>(string queueName) => (T?)Dequeue(queueName);

    public void Push<T>(string stackName, T data, DateTime? expireAt = null)
    {
        _stack.GetOrAdd(stackName, _ => new ConcurrentStack<object?>(new object?[] { data }));

        SetExpire(stackName, expireAt);
    }

    private object? Pop(string stackName) =>
        _stack.TryGetValue(stackName, out var stackData) &&
        stackData.TryPop(out var data)
            ? data
            : null;

    public T? Pop<T>(string stackName) => (T?)Pop(stackName);

    private static string ScopedChannelName(string channelName) => $"Channel:{channelName}";

    public void Publish<T>(string channelName, T data, DateTime? expireAt = null)
    {
        channelName = ScopedChannelName(channelName);
        _channelTypeIsQueue[channelName] = true;
        Enqueue(channelName, data, expireAt);
    }

    public void PublishUseStack<T>(string channelName, T data, DateTime? expireAt = null)
    {
        channelName = ScopedChannelName(channelName);
        _channelTypeIsQueue[channelName] = false;
        Push(channelName, data, expireAt);
    }

    public void Subscribe<T>(string channelName, string subscribeName, Action<T?> onMessage)
    {
        channelName = ScopedChannelName(channelName);

        _channelSubscriber.GetOrAdd(
            channelName,
            _ => new ConcurrentDictionary<string, Action<object?>>(
                new KeyValuePair<string, Action<object?>>[]
                {
                    new(subscribeName, o =>
                    {
                        if (o is T t)
                        {
                            onMessage(t);
                        }
                        else
                        {
                            onMessage((T?)(object?)null);
                        }
                    })
                }));
    }

    public void Unsubscribe(string channelName, string subscribeName)
    {
        channelName = ScopedChannelName(channelName);

        if (_channelSubscriber.TryGetValue(channelName, out var subscribers))
        {
            subscribers.TryRemove(subscribeName, out _);
        }
    }

    public int CountQueue(string queueName) =>
        _queue.TryGetValue(queueName, out var q)
            ? q.Count
            : 0;

    public int CountStack(string stackName) =>
        _stack.TryGetValue(stackName, out _) &&
        _queue.TryGetValue(stackName, out var q)
            ? q.Count
            : 0;

    public int CountDataInChannel(string channelName)
    {
        channelName = ScopedChannelName(channelName);
        return CountQueue(channelName);
    }

    public int CountDataInChannelUseStack(string channelName)
    {
        channelName = ScopedChannelName(channelName);
        return CountStack(channelName);
    }

    public int CountSubscribeInChannel(string channelName) =>
        _channelSubscriber.ContainsKey(channelName) &&
        _queue.TryGetValue(channelName, out var q)
            ? q.Count
            : 0;

#pragma warning disable CA1002
    public List<string> ListSubscriberInChannel(string channelName) =>
#pragma warning restore CA1002
        _channelSubscriber.TryGetValue(channelName, out var subscriber)
            ? subscriber.Keys.ToList()
            : new List<string>();

    public void Clear(string key)
    {
        _cache.TryRemove(key, out _);

        _queue.TryRemove(key, out _);

        _stack.TryRemove(key, out _);

        var channelName = key;
        _channelSubscriber.TryRemove(channelName, out _);
        _channelTypeIsQueue.TryRemove(channelName, out _);

        _keyExpire.TryRemove(key, out _);
        _keySlideExpire.TryRemove(key, out _);

        _hashSet.TryRemove(key, out _);
    }

#pragma warning disable CA1002
    public List<string> ListAllKey()
#pragma warning restore CA1002
    {
        var keys = new List<string>();

        lock (_cache)
        {
            keys.AddRange(_cache.Select(i => i.Key));
        }

        lock (_queue)
        {
            keys.AddRange(_cache.Select(i => i.Key));
        }

        lock (_stack)
        {
            keys.AddRange(_cache.Select(i => i.Key));
        }

        lock (_channelSubscriber)
        {
            keys.AddRange(_cache.Select(i => i.Key));
        }

        lock (_channelTypeIsQueue)
        {
            keys.AddRange(_cache.Select(i => i.Key));
        }

        lock (_keyExpire)
        {
            keys.AddRange(_cache.Select(i => i.Key));
        }

        lock (_keySlideExpire)
        {
            keys.AddRange(_cache.Select(i => i.Key));
        }

        lock (_hashSet)
        {
            keys.AddRange(_hashSet.Select(i => i.Key));
        }

        return keys.Distinct().ToList();
    }

    public void ClearAll()
    {
        var keys = ListAllKey();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            foreach (var k in keys)
            {
                Clear(k);
            }
        });
    }
}
