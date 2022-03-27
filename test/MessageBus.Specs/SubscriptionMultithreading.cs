using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs;

[Subject("Subscription: Multithreading")]
class multithreading_subscription_context : bus_context
{
}

[Tags(tag.async)]
[Tags(tag.concurrency)]
class when_many_subscribers_subscribe_concurrently : multithreading_subscription_context
{
    Establish context = () =>
    {
        subscriptions = Enumerable.Range(1, 10_000)
            .Select(_ => Guid.NewGuid().ToString("N"))
            .Select(name => new Subscriber(name, _ => { }))
            .Select(subscriber => Task.Factory.StartNew(() => bus.Subscribe(subscriber)));
    };

    Because of = () => aggregated_subscription = () => Task.WhenAll(subscriptions);

    It should_succeed = async () => await aggregated_subscription.Should().NotThrowAsync();

    static Func<Task> aggregated_subscription;
    static IEnumerable<Task> subscriptions;
}
