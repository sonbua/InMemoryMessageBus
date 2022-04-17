using System;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.UserDefinedChannel;

class count_pending_context : bus_context
{
    [Subject("User-defined Channel: CountPending")]
    [Tags(tag.validation)]
    class given_channel_name_is_null_when_calling_countpending
    {
        // ReSharper disable once AssignNullToNotNullAttribute
        Because of = () => count_action = () => bus.CountPending(channelName: null);

        It should_throw_argument_null_exception = () => count_action.Should().Throw<ArgumentNullException>();

        static Func<long> count_action;
    }

    [Subject("User-defined Channel: CountPending")]
    [Tags(tag.validation)]
    class given_channel_name_is_empty_when_calling_countpending
    {
        Because of = () => count_action = () => bus.CountPending(channelName: "");

        It should_throw_argument_exception =
            () => exception = count_action.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action count_action;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: CountPending")]
    [Tags(tag.validation)]
    class given_channel_name_is_whitespaces_when_calling_countpending
    {
        Because of = () => count_action = () => bus.CountPending(channelName: "  ");

        It should_throw_argument_exception =
            () => exception = count_action.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action count_action;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: CountPending")]
    [Tags(tag.validation)]
    class given_channel_name_is_whitespace_and_newline_when_calling_countpending
    {
        Because of = () => count_action = () => bus.CountPending(channelName: "  \r\n  ");

        It should_throw_argument_exception =
            () => exception = count_action.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action count_action;
        static ArgumentException exception;
    }
}
