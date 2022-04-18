using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;

namespace MessageBus.Specs.UserDefinedChannel;

class subscription_context : user_defined_channel_context
{
    [Subject("User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_null_when_calling_subscribe
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => subscription = () => bus.Subscribe(new Subscriber(null, _ => { }), user_defined_channel);

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should().Be("Value can not be null. (Parameter 'name')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Subscription")]
    class given_subscriber_name_is_empty_when_calling_subscribe
    {
        Because of = () => subscription = () => bus.Subscribe(new Subscriber("", _ => { }), user_defined_channel);

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'name')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespaces_when_calling_subscribe
    {
        Because of = () => subscription = () => bus.Subscribe(new Subscriber("  ", _ => { }), user_defined_channel);

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'name')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespace_and_newline_when_calling_subscribe
    {
        Because of = () =>
            subscription = () => bus.Subscribe(new Subscriber("  \r\n  ", _ => { }), user_defined_channel);

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'name')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_channel_name_is_null_when_calling_subscribe
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => subscription = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }), channelName: null);

        It should_throw_argument_null_exception = () => subscription.Should().Throw<ArgumentNullException>();

        static Action subscription;
    }

    [Subject("User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_channel_name_is_empty_when_calling_subscribe
    {
        Because of = () =>
            subscription = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }), channelName: "");

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_channel_name_is_whitespaces_when_calling_subscribe
    {
        Because of = () =>
            subscription = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }), channelName: "  ");

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_channel_name_is_whitespace_and_newline_when_calling_subscribe
    {
        Because of = () =>
            subscription = () => bus.Subscribe(new Subscriber("a subscriber", _ => { }), channelName: "  \r\n  ");

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'channelName')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("User-defined Channel: Subscription")]
    [Tags(tag.concurrency)]
    class when_multiple_subscribers_subscribe_concurrently
    {
        Establish context = () =>
        {
            subscriptions = Enumerable.Range(1, 10_000)
                .Select(_ => Guid.NewGuid().ToString("N"))
                .Select(name => new Subscriber(name, _ => { }))
                .Select(subscriber => Task.Factory.StartNew(() => bus.Subscribe(subscriber, user_defined_channel)));
        };

        Because of = () => aggregated_subscription = () => Task.WaitAll(subscriptions.ToArray());

        It should_succeed = () => aggregated_subscription.Should().NotThrow();

        It should_be_able_to_handle_message_published_to_it = () =>
        {
            bus.Publish("a message", user_defined_channel);
            WaitForMessageToBeConsumed();
            bus.CountPending(user_defined_channel).Should().Be(0);
        };

        static IEnumerable<Task> subscriptions;
        static Action aggregated_subscription;
    }
}
