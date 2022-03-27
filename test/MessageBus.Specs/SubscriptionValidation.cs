using System;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs;

[Subject("Subscription: Validation")]
class subscription_validation_context : bus_context
{
}

[Tags(tag.validation)]
class given_subscriber_is_null_when_calling_subscribe : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe(subscriber: null);

    It should_throw_argument_null_exception = () => subscription.Should().Throw<ArgumentNullException>();

    static Action subscription;
}

[Tags(tag.validation)]
class given_subscriber_name_is_null_when_calling_subscribe : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe(new Subscriber(null, _ => { }));

    It should_throw_argument_exception_with_expected_message =
        () => subscription.Should().Throw<ArgumentException>()
            .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

    static Action subscription;
}

[Tags(tag.validation)]
class given_subscriber_name_is_empty_when_calling_subscribe : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe(new Subscriber("", _ => { }));

    It should_throw_argument_exception_with_expected_message =
        () => subscription.Should().Throw<ArgumentException>()
            .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

    static Action subscription;
}

[Tags(tag.validation)]
class given_subscriber_name_is_whitespaces_when_calling_subscribe : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe(new Subscriber("  ", _ => { }));

    It should_throw_argument_exception_with_expected_message =
        () => subscription.Should().Throw<ArgumentException>()
            .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

    static Action subscription;
}

[Tags(tag.validation)]
class given_subscriber_name_is_whitespace_and_newline_when_calling_subscribe : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe(new Subscriber("  \r\n  ", _ => { }));

    It should_throw_argument_exception_with_expected_message =
        () => subscription.Should().Throw<ArgumentException>()
            .And.Message.Should().StartWith("Subscriber name should not be empty or whitespace(s).");

    static Action subscription;
}
