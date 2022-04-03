using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;

namespace MessageBus.Specs.UserDefinedChannel;

class publishing_context : user_defined_channel_context
{
    [Subject(typeof(Bus), "User-defined Channel: Publishing")]
    [Tags(tag.validation)]
    class given_null_message_when_calling_publish
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => publishing = () => bus.Publish(null, user_defined_channel);

        It should_throw_argument_null_exception = () => publishing.Should().Throw<ArgumentNullException>();

        static Action publishing;
    }

    [Subject(typeof(Bus), "User-defined Channel: Publishing")]
    [Tags(tag.concurrency)]
    class when_multiple_messages_are_published_concurrently
    {
        Establish context = () =>
        {
            message_count = 10_000;
            publishings = Enumerable.Range(1, message_count)
                .Select(_ => Guid.NewGuid().ToString("N"))
                .Select(message => Task.Factory.StartNew(() => bus.Publish(message, user_defined_channel)));
        };

        Because of = () => aggregated_publishing = () => Task.WhenAll(publishings);

        It should_succeed =
            // ReSharper disable once AsyncVoidLambda
            async () => await aggregated_publishing.Should().NotThrowAsync();

        It should_all_messages_be_pending_since_there_is_no_subscriber =
            () => bus.CountPending(user_defined_channel).Should().Be(message_count);

        It should_default_channel_be_empty = () => bus.CountPending().Should().Be(0);

        static int message_count;
        static IEnumerable<Task> publishings;
        static Func<Task> aggregated_publishing;
    }
}
