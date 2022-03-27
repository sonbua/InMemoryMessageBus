using System;

namespace MessageBus;

public record Subscriber(string Name, Action<object> MessageHandler);
