using System;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs;

[Subject("Subscription: Validation")]
class subscription_validation_context : bus_context
{
}

class given_subscriber_is_null_when_subscribing : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe(null);

    It should_throw_argument_null_exception = () => subscription.Should().Throw<ArgumentNullException>();

    static Action subscription;
}

class given_subscriber_name_is_null_when_subscribing : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe(new Subscriber(null, _ => { }));

    It should_throw_argument_exception = () => subscription.Should().Throw<ArgumentException>();

    It should_throw_expected_message = () =>
    {
        var message = subscription.Should().Throw<Exception>().Which.Message;
        message.Should().StartWith("Subscriber name should not be null or empty.");
    };

    static Action subscription;
}

class given_subscriber_name_is_empty_when_subscribing : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe(new Subscriber("", _ => { }));

    It should_throw_argument_exception = () => subscription.Should().Throw<ArgumentException>();

    It should_throw_expected_message = () =>
    {
        var message = subscription.Should().Throw<ArgumentException>().Which.Message;
        message.Should().StartWith("Subscriber name should not be null or empty.");
    };

    static Action subscription;
}
