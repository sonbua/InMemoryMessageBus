using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;

namespace MessageBus.Specs.UserDefinedChannel;

class unsubscription_context : user_defined_channel_context
{
    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_null_when_calling_unsubscribe
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => unsubscription = () => bus.Unsubscribe(subscriberName: null, user_defined_channel);

        It should_throw_argument_null_exception = () => unsubscription.Should().Throw<ArgumentNullException>();

        static Action unsubscription;
    }

    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_empty_when_calling_unsubscribe
    {
        Because of = () => unsubscription = () => bus.Unsubscribe(subscriberName: "", user_defined_channel);

        It should_throw_argument_exception =
            () => exception = unsubscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should().Be(
                "The string can't be left empty, null or consist of only whitespaces. (Parameter 'subscriberName')");

        static Action unsubscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespaces_when_calling_unsubscribe
    {
        Because of = () => unsubscription = () => bus.Unsubscribe(subscriberName: "  ", user_defined_channel);

        It should_throw_argument_exception =
            () => exception = unsubscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should().Be(
                "The string can't be left empty, null or consist of only whitespaces. (Parameter 'subscriberName')");

        static Action unsubscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespace_and_newline_when_calling_unsubscribe
    {
        Because of = () => unsubscription = () => bus.Unsubscribe(subscriberName: "  \r\n  ", user_defined_channel);

        It should_throw_argument_exception =
            () => exception = unsubscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should().Be(
                "The string can't be left empty, null or consist of only whitespaces. (Parameter 'subscriberName')");

        static Action unsubscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.validation)]
    class given_a_subscriber_whose_name_does_not_exist_when_calling_unsubscribe
    {
        Because of = () => unsubscription = () => bus.Unsubscribe(subscriberName: "unknown", user_defined_channel);

        It should_not_throw = () => unsubscription.Should().NotThrow();

        static Action unsubscription;
    }

    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.validation)]
    class given_a_subscriber_whose_channel_name_is_null_when_calling_unsubscribe
    {
        Establish context = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }));

        Because of = () => unsubscription =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => bus.Unsubscribe("a subscriber", channelName: null);

        It should_throw_argument_null_exception = () => unsubscription.Should().Throw<ArgumentNullException>();

        static Action unsubscription;
    }

    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.validation)]
    class given_a_subscriber_whose_channel_name_is_empty_when_calling_unsubscribe
    {
        Establish context = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }));

        Because of = () => unsubscription = () => bus.Unsubscribe("a subscriber", channelName: "");

        It should_throw_argument_exception =
            () => exception = unsubscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action unsubscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.validation)]
    class given_a_subscriber_whose_channel_name_is_whitespaces_when_calling_unsubscribe
    {
        Establish context = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }));

        Because of = () => unsubscription = () => bus.Unsubscribe("a subscriber", channelName: "  ");

        It should_throw_argument_exception =
            () => exception = unsubscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action unsubscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.validation)]
    class given_a_subscriber_whose_channel_name_is_whitespace_and_newline_when_calling_unsubscribe
    {
        Establish context = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }));

        Because of = () => unsubscription = () => bus.Unsubscribe("a subscriber", channelName: "  \r\n  ");

        It should_throw_argument_exception =
            () => exception = unsubscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action unsubscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Unsubscription")]
    [Tags(tag.concurrency)]
    class when_multiple_subscribers_are_unsubscribed_concurrently
    {
        Establish context = () =>
        {
            var subscriberNames = Enumerable.Range(1, 10_000)
                .Select(_ => Guid.NewGuid().ToString("N"))
                .ToList();

            foreach (var name in subscriberNames)
            {
                bus.Subscribe(new Subscriber(name, _ => { }), user_defined_channel);
            }

            unsubscriptions = subscriberNames.Select(
                name => Task.Factory.StartNew(() => bus.Unsubscribe(name, user_defined_channel)));
        };

        Because of = () => aggregated_unsubscription = () => Task.WaitAll(unsubscriptions.ToArray());

        It should_succeed = () => aggregated_unsubscription.Should().NotThrow();

        It should_there_be_no_subscriber_to_handle_a_message_published_to_it = () =>
        {
            bus.CountPending(user_defined_channel).Should().Be(0);

            bus.Publish("a message", user_defined_channel);

            bus.CountPending(user_defined_channel).Should().Be(1);
        };

        static IEnumerable<Task> unsubscriptions;
        static Action aggregated_unsubscription;
    }
}
