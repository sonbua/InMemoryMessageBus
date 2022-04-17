﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Machine.Specifications;

namespace MessageBus.Specs.UserDefinedChannel;

class subscription_context : user_defined_channel_context
{
    [Subject(typeof(Bus), "User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_is_null_when_calling_subscribe
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => subscription = () => bus.Subscribe(subscriber: null, user_defined_channel);

        It should_throw_argument_null_exception = () => subscription.Should().Throw<ArgumentNullException>();

        static Action subscription;
    }

    [Subject(typeof(Bus), "User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_null_when_calling_subscribe
    {
        Because of =
            // ReSharper disable once AssignNullToNotNullAttribute
            () => subscription = () => bus.Subscribe(new Subscriber(null, _ => { }), user_defined_channel);

        It should_throw_argument_exception_with_expected_message =
            () => subscription.Should().Throw<ArgumentException>()
                .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

        static Action subscription;
    }

    [Subject(typeof(Bus), "User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_empty_when_calling_subscribe
    {
        Because of = () => subscription = () => bus.Subscribe(new Subscriber("", _ => { }), user_defined_channel);

        It should_throw_argument_exception_with_expected_message =
            () => subscription.Should().Throw<ArgumentException>()
                .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

        static Action subscription;
    }

    [Subject(typeof(Bus), "User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespaces_when_calling_subscribe
    {
        Because of = () => subscription = () => bus.Subscribe(new Subscriber("  ", _ => { }), user_defined_channel);

        It should_throw_argument_exception_with_expected_message =
            () => subscription.Should().Throw<ArgumentException>()
                .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

        static Action subscription;
    }

    [Subject(typeof(Bus), "User-defined Channel: Subscription")]
    [Tags(tag.validation)]
    class given_subscriber_name_is_whitespace_and_newline_when_calling_subscribe
    {
        Because of = () =>
            subscription = () => bus.Subscribe(new Subscriber("  \r\n  ", _ => { }), user_defined_channel);

        It should_throw_argument_exception_with_expected_message =
            () => subscription.Should().Throw<ArgumentException>()
                .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

        static Action subscription;
    }

    [Subject(typeof(Bus), "User-defined Channel: Subscription")]
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

        Because of = () => aggregated_subscription = () => Task.WhenAll(subscriptions);

        It should_succeed =
            // ReSharper disable once AsyncVoidLambda
            async () => await aggregated_subscription.Should().NotThrowAsync();

        It should_be_able_to_handle_message_published_to_it = () =>
        {
            bus.Publish("a message", user_defined_channel);

            bus.CountPending(user_defined_channel).Should().Be(0);
        };

        static IEnumerable<Task> subscriptions;
        static Func<Task> aggregated_subscription;
    }
}
