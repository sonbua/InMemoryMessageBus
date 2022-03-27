using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.Unsubscribe;

[Subject("Unsubscription: Multithreading")]
class multithreading_unsubscription_context : bus_context
{
}

[Tags(tag.async)]
[Tags(tag.concurrency)]
class when_multiple_subscribers_are_unsubscribed_concurrently : multithreading_unsubscription_context
{
    Establish context = () =>
    {
        var subscriberNames = Enumerable.Range(1, 10_000)
            .Select(_ => Guid.NewGuid().ToString("N"))
            .ToList();

        foreach (var name in subscriberNames)
        {
            bus.Subscribe(new Subscriber(name, _ => { }));
        }

        unsubscriptions = subscriberNames.Select(name => Task.Factory.StartNew(() => bus.Unsubscribe(name)));
    };

    Because of = () => aggregated_unsubscription = () => Task.WhenAll(unsubscriptions);

    It should_succeed = async () => await aggregated_unsubscription.Should().NotThrowAsync();

    It should_there_be_no_subscriber_to_handle_a_message_published_to_it = () =>
    {
        bus.PendingCount.Should().Be(0);

        bus.Publish("a message");

        bus.PendingCount.Should().Be(1);
    };

    static IEnumerable<Task> unsubscriptions;
    static Func<Task> aggregated_unsubscription;
}
