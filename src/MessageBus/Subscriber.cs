using System;

namespace MessageBus;

// TODO: Be a struct?
public record Subscriber(string Name, Action<object> MessageHandler);
