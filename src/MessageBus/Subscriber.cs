using System;
using EnsureThat;

namespace MessageBus;

public readonly record struct Subscriber
{
    public Subscriber(string name, Action<object> messageHandler)
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));

        Name = name;
        MessageHandler = messageHandler;
    }

    public string Name { get; init; }
    public Action<object> MessageHandler { get; init; }
}
