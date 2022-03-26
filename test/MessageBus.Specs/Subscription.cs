using System;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs;

[Subject("Subscription: Validation")]
class subscription_validation_context : bus_context
{
}

class given_subscriber_name_is_null_when_subscribing : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe(null, _ => { });

    It should_throw_argument_exception = () => subscription.Should().Throw<ArgumentException>();

    static Action subscription;
}

class given_subscriber_name_is_empty_when_subscribing : subscription_validation_context
{
    Because of = () => subscription = () => bus.Subscribe("", _ => { });

    It should_throw_argument_exception = () => subscription.Should().Throw<ArgumentException>();

    static Action subscription;
}
