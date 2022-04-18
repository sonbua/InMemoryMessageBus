using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel;

class publishing_context : bus_context
{
    [Subject("Publishing")]
    [Tags(tag.validation)]
    class given_null_message_when_calling_publish
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => publishing = () => bus.Publish(null);

        It should_throw_argument_null_exception = () => publishing.Should().Throw<ArgumentNullException>();

        static Action publishing;
    }

    [Subject("Publishing")]
    [Tags(tag.async)]
    [Tags(tag.concurrency)]
    class when_multiple_messages_are_published_concurrently
    {
        Establish context = () =>
        {
            message_count = 10_000;
            publishings = Enumerable.Range(1, message_count)
                .Select(_ => Guid.NewGuid().ToString("N"))
                .Select(message => Task.Factory.StartNew(() => bus.Publish(message)));
        };

        Because of = () => aggregated_publishing = () => Task.WaitAll(publishings.ToArray());

        It should_succeed = () => aggregated_publishing.Should().NotThrow();

        It should_all_messages_be_pending_since_there_is_no_subscriber =
            () =>
            {
                WaitForMessageToBeConsumed();
                bus.CountPending().Should().Be(message_count);
            };

        static int message_count;
        static IEnumerable<Task> publishings;
        static Action aggregated_publishing;
    }
}
