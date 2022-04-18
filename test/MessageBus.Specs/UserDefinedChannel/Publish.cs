using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;

namespace MessageBus.Specs.UserDefinedChannel;

class publishing_context : user_defined_channel_context
{
    [Subject("User-defined Channel: Publishing")]
    [Tags(tag.validation)]
    class given_message_is_null_when_calling_publish
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => publishing = () => bus.Publish(message: null, user_defined_channel);

        It should_throw_argument_null_exception = () => publishing.Should().Throw<ArgumentNullException>();

        static Action publishing;
    }

    [Subject("User-defined Channel: Publishing")]
    [Tags(tag.validation)]
    class given_channel_name_is_null_when_calling_publish
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => publishing = () => bus.Publish("a message", channelName: null);

        It should_throw_argument_null_exception = () => publishing.Should().Throw<ArgumentNullException>();

        static Action publishing;
    }

    [Subject("User-defined Channel: Publishing")]
    [Tags(tag.validation)]
    class given_channel_name_is_empty_when_calling_publish
    {
        Because of =
            () => publishing = () => bus.Publish("a message", channelName: "");

        It should_throw_argument_exception =
            () => exception = publishing.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action publishing;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Publishing")]
    [Tags(tag.validation)]
    class given_channel_name_is_whitespaces_when_calling_publish
    {
        Because of =
            () => publishing = () => bus.Publish("a message", channelName: "  ");

        It should_throw_argument_exception =
            () => exception = publishing.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action publishing;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Publishing")]
    [Tags(tag.validation)]
    class given_channel_name_is_whitespace_and_newline_when_calling_publish
    {
        Because of =
            () => publishing = () => bus.Publish("a message", channelName: "  \r\n  ");

        It should_throw_argument_exception =
            () => exception = publishing.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action publishing;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Publishing")]
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

        Because of = () => aggregated_publishing = () => Task.WaitAll(publishings.ToArray());

        It should_succeed = () => aggregated_publishing.Should().NotThrow();

        It should_all_messages_be_pending_since_there_is_no_subscriber =
            () => bus.CountPending(user_defined_channel).Should().Be(message_count);

        It should_default_channel_be_empty = () => bus.CountPending().Should().Be(0);

        static int message_count;
        static IEnumerable<Task> publishings;
        static Action aggregated_publishing;
    }
}
