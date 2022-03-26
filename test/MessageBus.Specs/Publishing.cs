using System;
using FluentAssertions;
using Machine.Specifications;
using MessageBus.Specs.Context;

namespace MessageBus.Specs;

[Subject("Publishing: Validation")]
class publishing_validation_context : bus_context
{
}

class given_null_message : publishing_validation_context
{
    Because of = () => publishing = () => bus.Publish(null);

    It should_throw_argument_null_exception = () => publishing.Should().Throw<ArgumentNullException>();

    static Action publishing;
}
