using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs.DefaultChannel;

class unsubscription_context : bus_context
{
    [Subject("Unsubscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_null_when_calling_unsubscribe
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => unsubscription = () => bus.Unsubscribe(subscriberName: null);

        It should_throw_argument_null_exception = () => unsubscription.Should().Throw<ArgumentNullException>();

        static Action unsubscription;
    }

    [Subject("Unsubscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_empty_when_calling_unsubscribe
    {
        Because of = () => unsubscription = () => bus.Unsubscribe(subscriberName: "");

        It should_throw_argument_exception_with_expected_message =
            () => unsubscription.Should().Throw<ArgumentException>()
                .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

        static Action unsubscription;
    }

    [Subject("Unsubscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespaces_when_calling_unsubscribe
    {
        Because of = () => unsubscription = () => bus.Unsubscribe(subscriberName: "  ");

        It should_throw_argument_exception_with_expected_message =
            () => unsubscription.Should().Throw<ArgumentException>()
                .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

        static Action unsubscription;
    }

    [Subject("Unsubscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespace_and_newline_when_calling_unsubscribe
    {
        Because of = () => unsubscription = () => bus.Unsubscribe("  \r\n  ");

        It should_throw_argument_exception_with_expected_message =
            () => unsubscription.Should().Throw<ArgumentException>()
                .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

        static Action unsubscription;
    }

    [Subject("Unsubscription")]
    [Tags(tag.validation)]
    class given_a_subscriber_whose_name_does_not_exist_when_calling_unsubscribe
    {
        Because of = () => unsubscription = () => bus.Unsubscribe("unknown");

        It should_not_throw = () => unsubscription.Should().NotThrow();

        static Action unsubscription;
    }

    [Subject("Unsubscription")]
    [Tags(tag.async)]
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
                bus.Subscribe(new Subscriber(name, _ => { }));
            }

            unsubscriptions = subscriberNames.Select(name => Task.Factory.StartNew(() => bus.Unsubscribe(name)));
        };

        Because of = () => aggregated_unsubscription = () => Task.WhenAll(unsubscriptions);

        It should_succeed =
            // ReSharper disable once AsyncVoidLambda
            async () => await aggregated_unsubscription.Should().NotThrowAsync();

        It should_there_be_no_subscriber_to_handle_a_message_published_to_it = () =>
        {
            bus.CountPending().Should().Be(0);

            bus.Publish("a message");

            bus.CountPending().Should().Be(1);
        };

        static IEnumerable<Task> unsubscriptions;
        static Func<Task> aggregated_unsubscription;
    }
}
