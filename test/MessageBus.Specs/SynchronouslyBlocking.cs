using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs;

class synchronously_blocking_context : bus_context
{
    [Subject("Synchronously Blocking")]
    [Tags(tag.async)]
    class given_a_message_when_subscribing_a_slow_handler
    {
        Establish context = () =>
        {
            bus.Publish("a message");
            WaitForMessageToBeConsumed();
            subscription = () => bus.Subscribe(new Subscriber("slow subscriber", _ => Thread.Sleep(TimeSpan.FromSeconds(10))));
        };

        Because of = () => stopwatch = RecordTime(subscription);

        It should_not_synchronously_block =
            () => stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(5));

        static Stopwatch stopwatch;
        static Action subscription;
    }

    static Stopwatch RecordTime(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();

        return stopwatch;
    }
}
