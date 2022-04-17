using System;
using EnsureThat;

namespace MessageBus;

// TODO: Be a struct?
public record Subscriber
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
