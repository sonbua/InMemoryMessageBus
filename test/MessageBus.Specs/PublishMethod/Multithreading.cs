using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.PublishMethod;

[Subject("Publishing: Multithreading")]
class multithreading_publishing_context : bus_context
{
}

[Tags(tag.async)]
[Tags(tag.concurrency)]
class when_multiple_messages_are_published_concurrently : multithreading_publishing_context
{
    Establish context = () =>
    {
        publishings = Enumerable.Range(1, message_count)
            .Select(_ => Guid.NewGuid().ToString("N"))
            .Select(message => Task.Factory.StartNew(() => bus.Publish(message)));
    };

    Because of = () => aggregated_publishing = () => Task.WhenAll(publishings);

    It should_succeed = async () => await aggregated_publishing.Should().NotThrowAsync();

    It should_all_messages_be_pending_since_there_is_no_subscriber =
        () => bus.PendingCount.Should().Be(message_count);

    static IEnumerable<Task> publishings;
    static Func<Task> aggregated_publishing;
    static int message_count = 10_000;
}
