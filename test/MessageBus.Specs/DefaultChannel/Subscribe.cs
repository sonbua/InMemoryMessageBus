using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel;

class subscription_context : bus_context
{
    [Subject("Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_null_when_calling_subscribe
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => subscription = () => bus.Subscribe(new Subscriber(null, _ => { }));

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should().Be("Value can not be null. (Parameter 'name')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_empty_when_calling_subscribe
    {
        Because of = () => subscription = () => bus.Subscribe(new Subscriber("", _ => { }));

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'name')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespaces_when_calling_subscribe
    {
        Because of = () => subscription = () => bus.Subscribe(new Subscriber("  ", _ => { }));

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'name')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespace_and_newline_when_calling_subscribe
    {
        Because of = () => subscription = () => bus.Subscribe(new Subscriber("  \r\n  ", _ => { }));

        It should_throw_argument_exception =
            () => exception = subscription.Should().Throw<ArgumentException>().Which;

        It should_have_set_exception_message =
            () => exception.Message.Should()
                .Be("The string can't be left empty, null or consist of only whitespaces. (Parameter 'name')");

        static Action subscription;
        static ArgumentException exception;
    }

    [Subject("Subscription")]
    [Tags(tag.async)]
    [Tags(tag.concurrency)]
    class when_multiple_subscribers_subscribe_concurrently
    {
        Establish context = () =>
        {
            subscriptions = Enumerable.Range(1, 10_000)
                .Select(_ => Guid.NewGuid().ToString("N"))
                .Select(name => new Subscriber(name, _ => { }))
                .Select(subscriber => Task.Factory.StartNew(() => bus.Subscribe(subscriber)));
        };

        Because of = () => aggregated_subscription = () => Task.WaitAll(subscriptions.ToArray());

        It should_succeed = () => aggregated_subscription.Should().NotThrow();

        It should_be_able_to_handle_message_published_to_it = () =>
        {
            bus.Publish("a message");
            WaitForMessageToBeConsumed();
            bus.CountPending().Should().Be(0);
        };

        static IEnumerable<Task> subscriptions;
        static Action aggregated_subscription;
    }
}
